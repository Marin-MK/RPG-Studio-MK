using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using odl;

namespace RPGStudioMK
{
    public class Program
    {
        /// <summary>
        /// Whether or not exceptions should be caught and displayed, and whether unsaved changes messages should be given.
        /// If false, crashes will use a native (and undescriptive) console of some sort - or nothing at all and simply close.
        /// </summary>
        public static bool ReleaseMode = false;
        public static bool Verbose = false;
        public static string ProjectFile = null;
        public static bool ThrownError = false;

        static void Main(params string[] args)
        {
            // Ensures the working directory becomes the editor directory
            Directory.SetCurrentDirectory(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName);
            Console.WriteLine("Launching RPG Studio MK.");
            if (!ReleaseMode)
            {
                Console.WriteLine("===============================\nProgram launched in Debug mode.\n===============================");
            }
            OperatingSystem os = Editor.GetOperatingSystem();
            string Framework = "";
            string fw = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
            if (fw.Contains(".NETCoreApp")) Framework = ".NET Core ";
            else if (fw.Contains(".NETFrameworkApp")) Framework = ".NET Framework ";
            else Framework = "Unknown ";
            Framework += Environment.Version.ToString();
            Console.WriteLine($"Editor Version: {Editor.GetVersionString()}");
            Console.WriteLine($"Framework: {Framework}");
            Console.WriteLine($"SDL Version: {odl.SDL2.SDL.SDL_MAJOR_VERSION}.{odl.SDL2.SDL.SDL_MINOR_VERSION}.{odl.SDL2.SDL.SDL_PATCHLEVEL}");
            Console.WriteLine($"SDL_image Version: {odl.SDL2.SDL_image.SDL_IMAGE_MAJOR_VERSION}.{odl.SDL2.SDL_image.SDL_IMAGE_MINOR_VERSION}.{odl.SDL2.SDL_image.SDL_IMAGE_PATCHLEVEL}");
            Console.WriteLine($"SDL_ttf Version: {odl.SDL2.SDL_ttf.SDL_TTF_MAJOR_VERSION}.{odl.SDL2.SDL_ttf.SDL_TTF_MINOR_VERSION}.{odl.SDL2.SDL_ttf.SDL_TTF_PATCHLEVEL}");
            Graphics.Start();
            Audio.Start();
            int Handle = Audio.LoadSoundfont("soundfont.sf2");
            if (Handle == 0) throw new Exception("Failed to load soundfont.");
            Console.WriteLine($"OS Platform: {os.Platform} ({odl.Graphics.Platform}) {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}");
            Console.WriteLine($"OS Version: {os.VersionString}");
            Framework = null;
            fw = null;
            MainEditorWindow win = new MainEditorWindow(ProjectFile);
            win.Show();
            Widgets.MessageBox ErrorBox = null;
            win.OnSizeChanged += delegate (BaseEventArgs e)
            {
                if (ErrorBox != null && !ErrorBox.Disposed) ErrorBox.SetSize(win.Width, win.Height);
            };
            // Update all top-level widgets to make sure they're the right size.
            win.UI.Widgets.ForEach(e => e.UpdateBounds());
            // While there's at least one window open,
            while (Graphics.CanUpdate())
            {
                if (ReleaseMode)
                {
                    // Catch all errors and show them in a message box
                    try
                    {
                        if (ErrorBox != null && !ErrorBox.Disposed)
                        {
                            ErrorBox.MakePriorityWindow();
                        }
                        Graphics.Update(ThrownError);
                    }
                    catch (Exception ex)
                    {
                        if (!ThrownError)
                        {
                            string msg = ex.GetType() + " : " + ex.Message + "\n\n" + ex.StackTrace;
                            ErrorBox = new Widgets.MessageBox("Error!", msg, new List<string>() { "Quit" }, Widgets.IconType.Error);
                            ErrorBox.SetSize(win.Width, win.Height);
                            ErrorBox.OnDisposed += delegate (BaseEventArgs e)
                            {
                                Editor.ExitEditor();
                            };
                            ThrownError = true;
                        }
                    }
                }
                else
                {
                    // Update all renderers
                    Graphics.Update();
                }
            }
            // Stops all windows
            Graphics.Stop();
            Audio.Stop();
        }
    }
}
