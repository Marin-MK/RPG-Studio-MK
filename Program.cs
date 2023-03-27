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
    public static bool ThrownError = false;
    public static string? LatestVersion;
    public static bool UpdateAvailable = false;
    public static bool PromptedUpdate = false;
    public static string CurrentVersion;

    [STAThread]
    static void Main(params string[] args)
    {
        Widgets.MessageBox ErrorBox = null;
        MainEditorWindow win = null;
        try
        {
            if (DebugMode) Logger.Start();
            else
            {
                if (!Directory.Exists(Editor.AppDataFolder)) Directory.CreateDirectory(Editor.AppDataFolder);
                Logger.Start(Path.Combine(Editor.AppDataFolder, "log.txt"));
            }
            Graphics.Logger = Logger.Instance;
            MKUtils.Logger.Instance = Logger.Instance;
            // Ensures the working directory becomes the editor directory
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Environment.ProcessPath));
            if (DebugMode) TestSuite.RunAll();

            VerifyVersions();
            InitializeProgram();
            Logger.WriteLine("Initializing data");
            Game.Data.Setup();
            string initialProjectFile = args.Length > 0 ? args[0] : null;
            win = new MainEditorWindow();
            Widget.DefaultContextMenuFont = Fonts.Paragraph;
            Graphics.Update();
            win.Load(initialProjectFile);
            win.Prepare();
            win.UI.Widgets.ForEach(e => e.UpdateBounds());
            Graphics.Update();
            win.Show();
            win.OnSizeChanged += delegate (BaseEventArgs e)
            {
                if (ErrorBox != null && !ErrorBox.Disposed) ErrorBox.SetSize(win.Width, win.Height);
            };
        }
        catch (Exception ex) when (ReleaseMode)
        {
            Logger.Error("Setup failed!");
            Logger.Error(ex);
            throw ex;
        }

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
                        Logger.Error(ex);
                        string msg = ex.GetType() + " : " + ex.Message + "\n" + ex.StackTrace;
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
        Logger.Stop();
    }

    public static void VerifyVersions()
    {
        // Load current version
        // Changed in Project Settings -> Package -> Package Version (stored in .csproj)
        // Try getting the version from the assembly first (debug)
        Logger.WriteLine("Determining current version...");
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
            Logger.WriteLine("Skipped latest version check in Debug Mode");
            return;
        }
        Logger.WriteLine("Downloading version metadata...");
        if (MKUtils.VersionMetadata.Load())
        {
            LatestVersion = MKUtils.VersionMetadata.ProgramVersion;
            // Compare versions
            int cmp = MKUtils.VersionMetadata.CompareVersions(CurrentVersion, LatestVersion);
            if (!string.IsNullOrEmpty(LatestVersion) && cmp == -1)
            {
                // LatestVersion > CurrentVersion, so there is an update available.
                UpdateAvailable = true;
                Logger.WriteLine($"Version {CurrentVersion} is outdated. Update {LatestVersion} is available.");
            }
            else Logger.WriteLine($"Version {CurrentVersion} is up-to-date.");
        }
        else Logger.WriteLine($"Failed to look for updates.");
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
        Logger.WriteLine("Launching RPG Studio MK.");
        Logger.WriteLine($"Editor Version: {Editor.GetVersionString()}");
        if (!ReleaseMode)
        {
            Logger.WriteLine("===============================\nProgram launched in Debug mode.\n===============================");
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
        Logger.WriteLine("Loading Ruby...");
        string rubyVersion = Ruby.Initialize(Config.PathInfo);
        Logger.WriteLine("Loaded Ruby ({0})", rubyVersion);
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
        Logger.WriteLine($"Framework: {Framework}");
        Logger.WriteLine($"OS Platform: {os.Platform} ({Graphics.Platform}) {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}");
        Logger.WriteLine($"OS Version: {os.VersionString}");
    }
}
