using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RPGStudioMK;

public class Server
{
    public delegate void SocketEvent(Socket Socket);
    public delegate void SocketTextEvent(Socket Socket, string Text);

    TcpListener Listener;

    public bool Open { get; protected set; } = true;
    public List<Socket> Clients { get; } = new List<Socket>();

    public SocketEvent OnClientAccepted;
    public SocketTextEvent OnClientMessaged;
    public SocketEvent OnClientTimedOut;
    public SocketEvent OnClientClosed;
    public BaseEvent OnServerClosed;

    int NextSocketID = 1;

    public Server(int Port)
    {
        Listener = TcpListener.Create(Port);
        Listener.Start();
        Thread ListenThread = new Thread(_ =>
        {
            try
            {
                while (Open && Listener != null)
                {
                    TcpClient tcp = Listener.AcceptTcpClient();
                    Socket client = new Socket(this.NextSocketID++, tcp);
                    Clients.Add(client);
                    client.OnMessaged += (_, text) => OnClientMessaged?.Invoke(client, text);
                    client.OnTimedOut += _ => OnClientTimedOut?.Invoke(client);
                    client.OnClosed += _ =>
                    {
                        OnClientClosed?.Invoke(client);
                        Clients.Remove(client);
                    };
                    OnClientAccepted?.Invoke(client);
                }
            }
            catch (SocketException) { }
            finally
            {
                Stop();
                Listener?.Stop();
                Listener = null;
                this.OnServerClosed?.Invoke(new BaseEventArgs());
            }
        });
        ListenThread.Start();
    }

    public void Stop()
    {
        if (!Open) return;
        Open = false;
        Listener?.Stop();
        Listener = null;
        while (Clients.Count > 0) Clients[0].Close();
    }

    public class Socket
    {
        public int ID;
        public bool Connected = true;
        public TimeSpan PingInterval;
        public TimeSpan PingTimeout;

        TcpClient Client;
        NetworkStream Stream { get { return Client.GetStream(); } }
        DateTime LastPing;
        DateTime PingTimeoutTime;
        bool AwaitingPing;
        Thread Thread;

        public SocketTextEvent OnMessaged;
        public SocketEvent OnTimedOut;
        public SocketEvent OnClosed;

        public Socket(int ID, TcpClient Client)
        {
            this.ID = ID;
            this.Client = Client;
            this.LastPing = DateTime.Now;
            this.AwaitingPing = false;
            this.PingInterval = TimeSpan.FromSeconds(2);
            this.PingTimeout = TimeSpan.FromSeconds(2);
            this.Thread = new Thread(Loop);
            this.Thread.Start();
        }

        private void Loop()
        {
            try
            {
                List<byte> Buffer = new List<byte>();
                while (Connected)
                {
                    DateTime Now = DateTime.Now;
                    if (!AwaitingPing)
                    {
                        if ((Now - LastPing) > PingInterval)
                        {
                            AwaitingPing = true;
                            PingTimeoutTime = Now + PingTimeout;
                            Write("ping");
                        }
                    }
                    else
                    {
                        if (Now > PingTimeoutTime)
                        {
                            Connected = false;
                            OnTimedOut?.Invoke(this);
                            break;
                        }
                    }
                    if (!Stream.DataAvailable) continue;
                    while (Stream.DataAvailable)
                    {
                        int Data = Stream.ReadByte();
                        if (Data == -1) break;
                        Buffer.Add((byte)Data);
                    }
                    string received = Encoding.UTF8.GetString(Buffer.ToArray()).TrimEnd('\r', '\n');
                    Buffer.Clear();
                    OnMessaged?.Invoke(this, received);
                    AwaitingPing = false;
                    LastPing = Now;
                }
            }
            catch (SocketException) { }
            catch (InvalidOperationException) { }
            finally { Close(); }
        }

        public void Write(string Text)
        {
            Stream.Write(Encoding.UTF8.GetBytes(Text + "\n"));
            Stream.Flush();
        }

        public void Close()
        {
            if (!this.Connected) return;
            try { Write("close"); }
            catch { }
            this.Connected = false;
            Client?.Close();
            Client = null;
            OnClosed?.Invoke(this);
        }
    }
}