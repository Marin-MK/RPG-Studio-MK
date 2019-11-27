﻿using System;
using MKEditor.Game;
using ODL;

namespace MKEditor
{
    public class Program
    {
        static void Main(params string[] args)
        {
            Console.WriteLine($"Platform: {Graphics.GetPlatform()}");
            if (args.Length == 0) args = new string[1] { "D:\\Desktop\\MK\\mk\\project.mkproj" };
            if (args.Length > 0) Data.SetProjectPath(args[0]);
            Graphics.Start();
            MainEditorWindow win = new MainEditorWindow();
            win.Show();
            Utilities.PickFile();
            win.OnClosing += delegate (object sender, CancelEventArgs e)
            {
                int x, y;
                SDL2.SDL.SDL_GetWindowPosition(win.SDL_Window, out x, out y);
                int w, h;
                SDL2.SDL.SDL_GetWindowSize(win.SDL_Window, out w, out h);
                Editor.GeneralSettings.LastX = x;
                Editor.GeneralSettings.LastY = y;
                Editor.GeneralSettings.LastWidth = w;
                Editor.GeneralSettings.LastHeight = h;
                SDL2.SDL.SDL_WindowFlags flags = (SDL2.SDL.SDL_WindowFlags)SDL2.SDL.SDL_GetWindowFlags(win.SDL_Window);
                Editor.GeneralSettings.WasMaximized = (flags & SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) == SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;
                Editor.DumpGeneralSettings();
            };
            while (Graphics.CanUpdate())
            {
                Graphics.Update();
            }
            Graphics.Stop();
        }
    }
}