global using odl;
global using amethyst;
global using rubydotnet;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Versioning;
using NativeLibraryLoader;
using System.Threading;

namespace RPGStudioMK;

public class Program
{
    /// <summary>
    /// Whether or not exceptions should be caught and displayed, and whether unsaved changes messages should be given.
    /// If false, crashes will use a native (and undescriptive) console of some sort - or nothing at all and simply close.
    /// </summary>
    public static bool ReleaseMode = false;
    public static string ProjectFile = null;
    public static bool ThrownError = false;

    [STAThread]
    static void Main(params string[] args)
    {
        if (!ReleaseMode) TestSuite.RunAll();
        InitializeProgram();
        if (args.Length == 1) ProjectFile = args[0];
        MainEditorWindow win = new MainEditorWindow();
        Widget.DefaultContextMenuFont = Fonts.Paragraph;
        //win.Show();
        Graphics.Update();
        win.Load(ProjectFile);
        //win.Hide();
        win.Prepare();
        win.UI.Widgets.ForEach(e => e.UpdateBounds());
        Graphics.Update();
        win.Show();
        Widgets.MessageBox ErrorBox = null;
        win.OnSizeChanged += delegate (BaseEventArgs e)
        {
            if (ErrorBox != null && !ErrorBox.Disposed) ErrorBox.SetSize(win.Width, win.Height);
        };
        
        // Amethyst's main UI loop
        Amethyst.Run(() =>
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
                // Updates graphics
                Graphics.Update();
            }
        });

        // Stops amethyst
        Amethyst.Stop();
    }

    private static void InitializeProgram()
    {
        // Ensures the working directory becomes the editor directory
        Directory.SetCurrentDirectory(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName);
        Console.WriteLine("Launching RPG Studio MK.");
        Console.WriteLine($"Editor Version: {Editor.GetVersionString()}");
        if (!ReleaseMode)
        {
            Console.WriteLine("===============================\nProgram launched in Debug mode.\n===============================");
        }
        PrintPlatformInfo();
        Config.Setup();
        InitializeAmethyst();
        InitializeRuby();
    }

    private static void InitializeAmethyst()
    {
        Amethyst.Start(Config.PathInfo, true, true);
        int Handle = Audio.LoadSoundfont("assets/soundfont.sf2");
        if (Handle == 0) throw new Exception("Failed to load soundfont.");
    }

    private static void InitializeRuby()
    {
        Ruby.Initialize(Config.PathInfo);
        IntPtr ruby_load_path = Ruby.GetGlobal("$LOAD_PATH");
        Ruby.Array.Push(ruby_load_path, Ruby.String.ToPtr("./lib/ruby/2.7.0"));
        if (NativeLibrary.Platform == NativeLibraryLoader.Platform.Windows)
        {
            Ruby.Array.Push(ruby_load_path, Ruby.String.ToPtr("./lib/ruby/2.7.0/x64-mingw32"));
        }
        else if (NativeLibrary.Platform == NativeLibraryLoader.Platform.Linux)
        {
            Ruby.Array.Push(ruby_load_path, Ruby.String.ToPtr("./lib/ruby/2.7.0/x86_64-linux"));
        }
        Ruby.Require("zlib");
    }

    private static void PrintPlatformInfo()
    {
        OperatingSystem os = Environment.OSVersion;
        string Framework = "";
        string fw = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
        if (fw.Contains(".NETCoreApp,Version"))
        {
            if (Convert.ToInt32(fw[".NETCoreApp,Version".Length]) <= 3) Framework = ".NET Core ";
            else Framework = ".NET ";
        }
        else if (fw.Contains(".NETFrameworkApp")) Framework = ".NET Framework ";
        else Framework = "Unknown ";
        Framework += Environment.Version.ToString();
        Console.WriteLine($"Framework: {Framework}");
        Console.WriteLine($"OS Platform: {os.Platform} ({Graphics.Platform}) {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}");
        Console.WriteLine($"OS Version: {os.VersionString}");
    }
}
