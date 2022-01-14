using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using RPGStudioMK.Game;

namespace RPGStudioMK;

public static class GameRunner
{
    private static Process Process;

    public static bool Running { get { return !Process.HasExited; } }
    public static TextEvent OnDataOutput;

    public static Server Server;

    public static int? FirstConnectedID = null;

    public static void Start()
    {
        if (Process != null || Server != null) return;
        Process = new Process();
        Process.StartInfo.FileName = Data.ProjectPath + "/Game.exe";
        Process.StartInfo.Arguments = "debug";
        Process.Start();
        Server = new Server(59995);
        Console.WriteLine("Server started.");
        Server.OnClientAccepted += delegate (Server.Socket Socket)
        {
            if (FirstConnectedID == null)
            {
                FirstConnectedID = Socket.ID;
                Console.WriteLine($"Socket {Socket.ID} connected.");
                Socket.OnMessaged += delegate (Server.Socket Socket, string Message)
                {
                    Console.WriteLine($"Socket {Socket.ID} :: {Message}");
                };
                Socket.OnTimedOut += delegate (Server.Socket Socket)
                {
                    Console.WriteLine($"Socket {Socket.ID} timed out.");
                };
                Socket.OnClosed += delegate (Server.Socket Socket)
                {
                    Console.WriteLine($"Socket {Socket.ID} disconnected.");
                    Server?.Stop();
                    Server = null;
                    FirstConnectedID = null;
                };
            }
            else
            {
                Socket.Write("close_rejected");
                Console.WriteLine($"Socket {Socket.ID} rejected.");
            }
        };
        Server.OnServerClosed += delegate (BaseEventArgs e)
        {
            Console.WriteLine("Server closed.");
            Server = null;
            FirstConnectedID = null;
        };
    }

    public static void Update()
    {
        if (Process == null && Server != null || Process?.HasExited == true)
        {
            Server?.Stop();
            Server = null;
            Process = null;
            FirstConnectedID = null;
        }
    }

    public static void Stop()
    {
        Server?.Stop();
        Server = null;
        Process?.Kill();
        Process = null;
        FirstConnectedID = null;
    }
}
