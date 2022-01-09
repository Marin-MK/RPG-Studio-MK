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

    public static void Start()
    {
        Process = new Process();
        Process.StartInfo.FileName = Data.ProjectPath + "/Game.exe";
        Process.StartInfo.Arguments = "debug";
        Process.Start();
        if (Server == null)
        {
            Server = new Server(59995);
            Console.WriteLine("Server started.");
            Server.OnClientAccepted += delegate (Server.Socket Socket)
            {
                Console.WriteLine($"Socket {Socket.ID} connected.");
            };
            Server.OnClientMessaged += delegate (Server.Socket Socket, string Message)
            {
                Console.WriteLine($"Socket {Socket.ID} :: {Message}");
            };
            Server.OnClientTimedOut += delegate (Server.Socket Socket)
            {
                Console.WriteLine($"Socket {Socket.ID} timed out.");
            };
            Server.OnClientClosed += delegate (Server.Socket Socket)
            {
                Console.WriteLine($"Socket {Socket.ID} disconnected.");
                Server.Stop();
            };
            Server.OnServerClosed += delegate (BaseEventArgs e)
            {
                Console.WriteLine("Server closed.");
                Server = null;
            };
        }
    }
}
