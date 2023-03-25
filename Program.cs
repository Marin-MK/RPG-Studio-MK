global using odl;
global using amethyst;
global using rubydotnet;
global using MKUtils;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Versioning;
using NativeLibraryLoader;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RPGStudioMK;

public class Program
{
    /// <summary>
    /// Whether or not exceptions should be caught and displayed, and whether unsaved changes messages should be given.
    /// If true, crashes will use a native (and undescriptive) console of some sort - or nothing at all and simply close.
    /// </summary>
    public static bool DebugMode = true;
    public static bool ReleaseMode => !DebugMode;
    public static string ProjectFile = null;
    public static bool ThrownError = false;
    public static string? LatestVersion;
    public static bool UpdateAvailable = false;
    public static bool PromptedUpdate = false;
    public static string CurrentVersion;

    [STAThread]
    static void Main(params string[] args)
    {
        Logger.Start(null);// Path.Combine(Editor.AppDataFolder, "log.txt"));
        Logger.Write("Log is active");
        // Ensures the working directory becomes the editor directory
        Directory.SetCurrentDirectory(Path.GetDirectoryName(Environment.ProcessPath));
        if (DebugMode) TestSuite.RunAll();
        VerifyVersions();
        InitializeProgram();
        if (args.Length == 1) ProjectFile = args[0];
        MainEditorWindow win = new MainEditorWindow();
        Widget.DefaultContextMenuFont = Fonts.Paragraph;
        Graphics.Update();
        win.Load(ProjectFile);
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
            else // if (DebugMode)
            {
                // Updates graphics
                Graphics.Update();
            }
        });

        // Stops amethyst
        Amethyst.Stop();
        Logger.Write("Log is inactive");
        Logger.Stop();
    }

    public static void VerifyVersions()
    {
        // Load current version
        // Changed in Project Settings -> Package -> Package Version (stored in .csproj)
        // Try getting the version from the assembly first (debug)
        Assembly assembly = Assembly.GetExecutingAssembly();
        if (assembly is not null && !string.IsNullOrEmpty(assembly.Location))
        {
            CurrentVersion = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            if (!string.IsNullOrEmpty(CurrentVersion)) CurrentVersion = RemoveExcessZeroes(CurrentVersion);
        }
        if (string.IsNullOrEmpty(CurrentVersion))
        {
            // Try getting it from version.txt otherwise (release)
            CurrentVersion = File.ReadAllText("version.txt").TrimEnd();
            if (string.IsNullOrEmpty(CurrentVersion)) CurrentVersion = "Unknown";
            else CurrentVersion = RemoveExcessZeroes(CurrentVersion);
        }
        // Load latest version
        if (DebugMode)
        {
            Logger.Write("Skipped latest version check in Debug Mode");
            Console.WriteLine("Skipped latest version check in Debug Mode");
            return;
        }
        if (VersionMetadata.Load())
        {
            LatestVersion = VersionMetadata.ProgramVersion;
            // Compare versions
            int cmp = VersionMetadata.CompareVersions(CurrentVersion, LatestVersion);
            if (!string.IsNullOrEmpty(LatestVersion) && cmp == -1)
            {
                // LatestVersion > CurrentVersion, so there is an update available.
                UpdateAvailable = true;
                Console.WriteLine($"Version {CurrentVersion} is outdated. Update {LatestVersion} is available.");
            }
            else Console.WriteLine($"Version {CurrentVersion} is up-to-date.");
        }
        else Console.WriteLine($"Failed to look for updates.");
    }

    private static string RemoveExcessZeroes(string version)
    {
        List<string> _split = version.Split('.').ToList();
        while (_split[^1] == "0")
        {
            _split.RemoveAt(_split.Count - 1);
        }
        if (_split.Count == 0) _split.Add("1");
        return _split.GetRange(0, _split.Count).Aggregate((a, b) => a + "." + b);
    }

    private static void InitializeProgram()
    {
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
