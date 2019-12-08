using System;
using MKEditor.Game;
using ODL;

namespace MKEditor
{
    public class Program
    {
        static void Main(params string[] args)
        {
            OperatingSystem os = Editor.GetOperatingSystem();
            Console.WriteLine($"Platform: {os.Platform} ({Editor.Platform})");
            Console.WriteLine($"Version: {os.VersionString}");
            Graphics.Start();
            MainEditorWindow win = new MainEditorWindow(args);
            bool CatchErrors = false;
            win.Show();
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
            bool Error = false;
            Widgets.MessageBox ErrorBox = null;
            win.OnWindowSizeChanged += delegate (object sender, WindowEventArgs e)
            {
                if (ErrorBox != null && !ErrorBox.Disposed) ErrorBox.SetSize(win.Width, win.Height);
            };
            while (Graphics.CanUpdate())
            {
                if (CatchErrors)
                {
                    try
                    {
                        if (ErrorBox != null && !ErrorBox.Disposed)
                        {
                            ErrorBox.MakePriorityWindow();
                        }
                        Graphics.Update();
                    }
                    catch (Exception ex)
                    {
                        if (!Error)
                        {
                            string msg = ex.GetType() + " : " + ex.Message + "\n\n" + ex.StackTrace;
                            ErrorBox = new Widgets.MessageBox("Error!", msg, new System.Collections.Generic.List<string>() { "Quit" });
                            ErrorBox.SetSize(win.Width, win.Height);
                            ErrorBox.OnDisposed += delegate (object sender, EventArgs e)
                            {
                                Editor.ExitEditor();
                            };
                        }
                    }
                }
                else Graphics.Update();
            }
            Graphics.Stop();
        }
    }
}