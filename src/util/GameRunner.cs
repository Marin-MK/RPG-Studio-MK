﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
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
        string filename = "/Game.exe";
        if (ODL.OnWindows) filename = "/Game.exe";
        else if (ODL.OnLinux) filename = "/Game";
        else throw new PlatformNotSupportedException();
        Process.StartInfo.FileName = Data.ProjectPath + filename;
        Process.StartInfo.Arguments = "debug";
        Process.Start();
        Server = new Server(59995);
        Logger.WriteLine("Server started.");
        Server.OnClientAccepted += delegate (Server.Socket Socket)
        {
            if (FirstConnectedID == null)
            {
                FirstConnectedID = Socket.ID;
                Logger.WriteLine($"Socket {Socket.ID} connected.");
                Socket.OnMessaged += delegate (Server.Socket Socket, string Message)
                {
                    Logger.WriteLine($"Socket {Socket.ID} :: {Message}");
                    OnDataOutput?.Invoke(new TextEventArgs(Message, null));
                };
                Socket.OnTimedOut += delegate (Server.Socket Socket)
                {
                    Logger.WriteLine($"Socket {Socket.ID} timed out.");
                };
                Socket.OnClosed += delegate (Server.Socket Socket)
                {
                    Logger.WriteLine($"Socket {Socket.ID} disconnected.");
                    Server?.Stop();
                    Server = null;
                    FirstConnectedID = null;
                };
                Socket.Write("ready");
            }
            else
            {
                Logger.WriteLine($"Socket {Socket.ID} rejected.");
                Socket.Close();
            }
        };
        Server.OnServerClosed += delegate (BaseEventArgs e)
        {
            Logger.WriteLine("Server closed.");
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
