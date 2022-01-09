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
    public List<Socket> Clients = new List<Socket>();

    public SocketEvent OnClientAccepted;
    public SocketTextEvent OnClientMessaged;
    public SocketEvent OnClientTimedOut;
    public SocketEvent OnClientClosed;
    public BaseEvent OnServerClosed;

    int ID = 1;

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
                    Socket client = new Socket(this.ID++, tcp);
                    Thread thread = new Thread(client.Loop);
                    thread.Start();
                    Clients.Add(client);
                    OnClientAccepted?.Invoke(client);
                    client.OnMessaged += e => OnClientMessaged?.Invoke(client, e.Text);
                    client.OnTimedOut += e => OnClientTimedOut?.Invoke(client);
                    client.OnClosed += e => OnClientClosed?.Invoke(client);
                }
            }
            catch (SocketException) { }
            finally
            {
                Listener?.Stop();
                Listener = null;
                this.OnServerClosed?.Invoke(new BaseEventArgs());
            }
        });
        ListenThread.Start();
        this.OnClientClosed += delegate (Socket Socket)
        {
            Clients.Remove(Socket);
        };
    }

    public void Stop()
    {
        Open = false;
        Listener?.Stop();
        Listener = null;
    }

    public class Socket
    {
        public int ID;
        public bool Connected = true;
        public TimeSpan PingInterval;
        public TimeSpan PingTimeout;

        TcpClient Client;
        NetworkStream Stream;
        DateTime LastPing;
        DateTime PingTimeoutTime;
        bool AwaitingPing;

        public TextEvent OnMessaged;
        public BaseEvent OnTimedOut;
        public BaseEvent OnClosed;

        public Socket(int ID, TcpClient Client)
        {
            this.ID = ID;
            this.Client = Client;
            this.Stream = Client.GetStream();
            this.LastPing = DateTime.Now;
            this.AwaitingPing = false;
            this.PingInterval = TimeSpan.FromSeconds(1);
            this.PingTimeout = TimeSpan.FromSeconds(5);
        }

        public void Loop()
        {
            List<byte> Buffer = new List<byte>();
            while (Connected)
            {
                DateTime Now = DateTime.Now;
                try
                {
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
                            OnTimedOut?.Invoke(new BaseEventArgs());
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
                    OnMessaged?.Invoke(new TextEventArgs(received, null));
                    AwaitingPing = false;
                    LastPing = Now;
                }
                catch
                {
                    Connected = false;
                }
            }
            Client.Close();
            OnClosed?.Invoke(new BaseEventArgs());
        }

        public void Write(string Text)
        {
            Stream.Write(Encoding.UTF8.GetBytes(Text + "\n"));
            Stream.Flush();
        }
    }
}