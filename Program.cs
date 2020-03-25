using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using MKEditor.Game;
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

        static bool ThrownError = false;

        static void Main(params string[] args)
        {
            if (args.Length > 0)
            {
                string ThisFile = Assembly.GetExecutingAssembly().Location;
                string ParentDir = Directory.GetParent(ThisFile).FullName;
                Directory.SetCurrentDirectory(ParentDir);
            }
            Console.WriteLine("Launching RPG Studio MK.");
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
            Console.WriteLine($"OS Platform: {os.Platform} ({Editor.Platform}) {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}");
            Console.WriteLine($"OS Version: {os.VersionString}");
            Console.WriteLine($"Editor Version: {Editor.GetVersionString()}");
            Graphics.Start();
            MainEditorWindow win = new MainEditorWindow(args);
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
                            ErrorBox = new Widgets.MessageBox("Error!", msg, new System.Collections.Generic.List<string>() { "Quit" }, Widgets.IconType.Error);
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