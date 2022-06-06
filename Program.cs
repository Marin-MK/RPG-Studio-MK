global using odl;
global using amethyst;
global using rubydotnet;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Versioning;
using NativeLibraryLoader;

namespace RPGStudioMK;

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

    [STAThread]
    static void Main(params string[] args)
    {
        InitializeProgram();
        if (args.Length == 1) ProjectFile = args[0];
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
        InitializeODL();
        InitializeRuby();
    }

    private static void InitializeODL()
    {
        InitializeGraphics();
        InitializeAudio();
    }

    private static void InitializeGraphics()
    {
        PathPlatformInfo windows = new PathPlatformInfo(NativeLibraryLoader.Platform.Windows);
        windows.AddPath("libsdl2", "./lib/windows/SDL2.dll");
        windows.AddPath("libz", "./lib/windows/zlib1.dll");
        windows.AddPath("libsdl2_image", "./lib/windows/SDL2_image.dll");
        windows.AddPath("libpng", "./lib/windows/libpng16-16.dll");
        if (File.Exists("lib/windows/libjpeg-9.dll")) windows.AddPath("libjpeg", "./lib/windows/libjpeg-9.dll");
        windows.AddPath("libsdl2_ttf", "./lib/windows/SDL2_ttf.dll");
        windows.AddPath("libfreetype", "./lib/windows/libfreetype-6.dll");

        PathPlatformInfo linux = new PathPlatformInfo(NativeLibraryLoader.Platform.Linux);
        linux.AddPath("libsdl2", "./lib/linux/SDL2.so");
        linux.AddPath("libz", "./lib/linux/libz.so");
        linux.AddPath("libsdl2_image", "./lib/linux/SDL2_image.so");
        linux.AddPath("libpng", "./lib/linux/libpng16-16.so");
        if (File.Exists("lib/linux/libjpeg-9.so")) linux.AddPath("libjpeg", "./lib/linux/libjpeg-9.so");
        linux.AddPath("libsdl2_ttf", "./lib/linux/SDL2_ttf.so");
        linux.AddPath("libfreetype", "./lib/linux/libfreetype-6.so");

        Graphics.Start(PathInfo.Create(windows, linux));
    }

    private static void InitializeAudio()
    {
        PathPlatformInfo windows = new PathPlatformInfo(NativeLibraryLoader.Platform.Windows);
        windows.AddPath("bass", "./lib/windows/bass.dll");
        windows.AddPath("bass_fx", "./lib/windows/bass_fx.dll");
        windows.AddPath("bass_midi", "./lib/windows/bassmidi.dll");

        PathPlatformInfo linux = new PathPlatformInfo(NativeLibraryLoader.Platform.Linux);
        linux.AddPath("bass", "./lib/linux/libbass.so");
        linux.AddPath("bass_fx", "./lib/linux/libbass_fx.so");
        linux.AddPath("bass_midi", "./lib/linux/libbassmidi.so");

        Audio.Start(PathInfo.Create(windows, linux));

        int Handle = Audio.LoadSoundfont("assets/soundfont.sf2");
        if (Handle == 0) throw new Exception("Failed to load soundfont.");
    }

    private static void InitializeRuby()
    {
        PathPlatformInfo windows = new PathPlatformInfo(NativeLibraryLoader.Platform.Windows);
        windows.AddPath("ruby", "./lib/windows/x64-msvcrt-ruby270.dll");
        windows.AddPath("libgmp", "./lib/windows/libgmp-10.dll");
        windows.AddPath("libssp", "./lib/windows/libssp-0.dll");
        windows.AddPath("libwinpthread", "./lib/windows/libwinpthread-1.dll");

        PathPlatformInfo linux = new PathPlatformInfo(NativeLibraryLoader.Platform.Linux);
        linux.AddPath("ruby", "./lib/linux/libruby.so");

        Ruby.Initialize(PathInfo.Create(windows, linux));

        IntPtr ruby_load_path = Ruby.GetGlobal("$LOAD_PATH");
        Ruby.Funcall(ruby_load_path, "push", Ruby.String.ToPtr("./lib/ruby/2.7.0"));
        if (NativeLibrary.Platform == NativeLibraryLoader.Platform.Windows)
        {
            Ruby.Funcall(ruby_load_path, "push", Ruby.String.ToPtr("./lib/ruby/2.7.0/x64-mingw32"));
        }
        else if (NativeLibrary.Platform == NativeLibraryLoader.Platform.Linux)
        {
            Ruby.Funcall(ruby_load_path, "push", Ruby.String.ToPtr("./lib/ruby/2.7.0/x86_64-linux"));
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
