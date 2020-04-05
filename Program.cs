using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using ODL;

namespace MKEditor
{
    public class Program
    {
        /// <summary>
        /// Whether or not exceptions should be caught and displayed, and whether unsaved changes messages should be given.
        /// If false, crashes will use a native (and undescriptive) console of some sort - or nothing at all and simply close.
        /// </summary>
        public static bool ReleaseMode = true;
        public static bool Verbose = false;
        public static bool Headless = false;
        public static string ProjectFile = null;
        public static bool ThrownError = false;

        static void Output(List<string> args)
        {
            foreach (string s in args) Console.WriteLine($">> {s}");
        }

        static void Main(params string[] args)
        {
            // Ensures the working directory becomes the editor directory
            Directory.SetCurrentDirectory(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName);
            MainCommandLineHandler cli = new MainCommandLineHandler();
            bool StartProgram = cli.Parse(args.ToList());
            cli.Dispose();
            cli = null;
            if (Headless)
            {
                HeadlessModeManager headless = new HeadlessModeManager();
                headless.Start();
                return;
            }
            if (!StartProgram) return;
            Console.WriteLine("Launching RPG Studio MK.");
            if (!ReleaseMode)
            {
                Console.WriteLine("===============================\nProgram launched in Debug mode.\n===============================\n");
            }
            OperatingSystem os = Editor.GetOperatingSystem();
            string Framework = "";
            string fw = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
            if (fw.Contains(".NETCoreApp")) Framework = ".NET Core ";
            else if (fw.Contains(".NETFrameworkApp")) Framework = ".NET Framework ";
            Framework += Environment.Version.ToString();
            Console.WriteLine($"Framework: {Framework}");
            SDL2.SDL.SDL_version v;
            SDL2.SDL.SDL_GetVersion(out v);
            Console.WriteLine($"SDL Version: {v.major}.{v.minor}.{v.patch}");
            Console.WriteLine($"SDL_image Version: {SDL2.SDL_image.SDL_IMAGE_MAJOR_VERSION}.{SDL2.SDL_image.SDL_IMAGE_MINOR_VERSION}.{SDL2.SDL_image.SDL_IMAGE_PATCHLEVEL}");
            Console.WriteLine($"SDL_ttf Version: {SDL2.SDL_ttf.SDL_TTF_MAJOR_VERSION}.{SDL2.SDL_ttf.SDL_TTF_MINOR_VERSION}.{SDL2.SDL_ttf.SDL_TTF_PATCHLEVEL}");
            Console.WriteLine($"OS Platform: {os.Platform} ({Editor.Platform}) {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}");
            Console.WriteLine($"OS Version: {os.VersionString}");
            Console.WriteLine($"Editor Version: {Editor.GetVersionString()}");
            Graphics.Start();
            MainEditorWindow win = new MainEditorWindow(ProjectFile);
            win.Show();
            Widgets.MessageBox ErrorBox = null;
            win.OnWindowSizeChanged += delegate (object sender, WindowEventArgs e)
            {
                if (ErrorBox != null && !ErrorBox.Disposed) ErrorBox.SetSize(win.Width, win.Height);
            };
            while (Graphics.CanUpdate())
            {
                if (ReleaseMode)
                {
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
                            ErrorBox.OnDisposed += delegate (object sender, EventArgs e)
                            {
                                Editor.ExitEditor();
                            };
                            ThrownError = true;
                        }
                    }
                }
                else Graphics.Update();
            }
            Graphics.Stop();
        }
    }
}