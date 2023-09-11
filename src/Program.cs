global using odl;
global using amethyst;
global using rubydotnet;

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Collections.Generic;
using NativeLibraryLoader;
using System.Diagnostics;
using MKUtils;

namespace RPGStudioMK;

public class Program
{
    /// <summary>
    /// Whether or not exceptions should be caught and displayed, and whether unsaved changes messages should be given.
    /// If true, crashes will use a native (and undescriptive) console of some sort - or nothing at all and simply close.
    /// </summary>
    public static bool ThrownError = false;
    public static bool ProgramUpdateAvailable = false;
    public static bool InstallerUpdateAvailable = false;
    public static bool PromptedUpdate = false;
    public static string CurrentProgramVersion;
    public static string? LatestProgramVersion;
    public static string CurrentInstallerVersion;
    public static string LatestInstallerVersion;
    public static string PendingInstallerVersion;
    public static bool MetadataLoadSuccessful = false;

	private delegate uint GetEUID();
    private static GetEUID geteuid;

	[STAThread]
    static void Main(params string[] args)
    {
        // Calculate SHA of new Essentials releases
        //FileStream fs = File.OpenRead("link-to-file.zip");
        //string sha = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(fs));
        //Console.WriteLine(sha);
        //Console.ReadKey();
        //return;

        if (!Directory.Exists(Editor.AppDataFolder))
        {
            Directory.CreateDirectory(Editor.AppDataFolder);
			if (ODL.OnLinux && IsLinuxAdmin())
			{
                // Ensure the App Data folder permits non-root access
                SetNonRoot(Editor.AppDataFolder);
			}
		}
        Widgets.MessageBox ErrorBox = null;
        MainEditorWindow win = null;

#if DEBUG
        try
        {
            Logger.Start();
#elif RELEASE
            string logPath = Path.Combine(Editor.AppDataFolder, "log.txt").Replace('\\', '/');
            Logger.Start(logPath);
#endif

			ODL.Logger = Logger.Instance;
            MKUtils.Logger.Instance = Logger.Instance;
			// Ensures the working directory becomes the editor directory
			Logger.WriteLine("Process Path: {0}", Environment.ProcessPath);
			Directory.SetCurrentDirectory(Path.GetDirectoryName(Environment.ProcessPath));
            PrintPlatformInfo();

#if DEBUG
            TestSuite.RunAll();
#endif

            VerifyVersions();

            if (!InitializeProgram())
            {
                Logger.WriteLine("The program failed to setup properly.");
                Logger.Stop();
                return;
            }

            if (ODL.OnWindows)
            {
				NativeLibraryLoader.NativeLibrary shell = NativeLibraryLoader.NativeLibrary.Load("shell32.dll");
				IsUserAnAdmin = shell.GetFunction<IsUserAnAdminFunc>("IsUserAnAdmin");
			}
            else if (ODL.OnLinux)
			{
				NativeLibrary libc = NativeLibrary.Load("libc.so.6");
				geteuid = libc.GetFunction<GetEUID>("geteuid");
                string tempUpdaterPath = Path.Combine(Editor.AppDataFolder, "updater").Replace('\\', '/');
                string desiredUpdaterPath = Path.Combine(MKUtils.MKUtils.ProgramFilesPath, VersionMetadata.InstallerInstallPath, VersionMetadata.InstallerInstallFilename["linux"]).Replace('\\', '/');
				string tempVersionPath = Path.Combine(Editor.AppDataFolder, "VERSION").Replace('\\', '/');
				string desiredVersionPath = Path.Combine(MKUtils.MKUtils.ProgramFilesPath, VersionMetadata.InstallerInstallPath, "VERSION").Replace('\\', '/');
				if (File.Exists(tempUpdaterPath) && File.Exists(tempVersionPath))
                {
                    Logger.WriteLine("A pending installer update needs to be completed.");
                    if (IsLinuxAdmin())
                    {
                        Logger.WriteLine("Root user, move updater from its temporary location to the desired updater path.");
                        if (!Directory.Exists(Editor.AppDataFolder)) Directory.CreateDirectory(Editor.AppDataFolder);
                        File.Move(tempUpdaterPath, desiredUpdaterPath, true);
                        File.Move(tempVersionPath, desiredVersionPath, true);
                        Logger.WriteLine("Installation complete.");
                        Popup popup = new Popup("Success", "The updater was successfully installed. Please re-run the application as a normal user, as RPG Studio MK cannot be used as a root user.");
                        popup.Show();
                        return;
                    }
                    else
                    {
                        Logger.WriteLine("Non-root user, updater cannot be moved from its temporary location to the desired updater path.");
                        odl.Popup popup = new Popup("Warning", "The recently installed update to the installer could not be completed. Please re-run the application as a root user (using 'sudo') so it can be completed.");
                        popup.Show();
                        PendingInstallerVersion = MKUtils.MKUtils.TrimVersion(File.ReadAllText(tempVersionPath));
                        if (string.IsNullOrEmpty(PendingInstallerVersion)) PendingInstallerVersion = "0";
                    }
                }
			}

#if RELEASE
			if (MetadataLoadSuccessful) VerifyInstallerVersions();
#endif

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
#if DEBUG
        }
        catch (Exception ex)
        {
            if (Logger.Instance is not null)
            {
                Logger.Error("Setup failed!");
                Logger.Error(ex);
            }
            else
            {
                Console.WriteLine("Setup failed!");
                Console.WriteLine(ex);
            }
			throw;
		}
#endif

        // Amethyst's main UI loop
        Amethyst.Run(() =>
        {
#if RELEASE
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
#else
			Graphics.Update();
#endif
        });

        // Stops amethyst
        Amethyst.Stop();
        Logger.Stop();
    }

	private static void SetNonRoot(string path)
	{
		Process p = new Process();
		p.StartInfo = new ProcessStartInfo("chmod");
		p.StartInfo.ArgumentList.Add("0777");
		p.StartInfo.ArgumentList.Add(path);
		p.StartInfo.UseShellExecute = false;
		p.StartInfo.CreateNoWindow = true;
		p.Start();
	}

    delegate bool IsUserAnAdminFunc();
    static IsUserAnAdminFunc IsUserAnAdmin;

    public static bool IsWindowsAdmin()
    {
        return IsUserAnAdmin();
    }

	public static bool IsLinuxAdmin()
	{
		return geteuid() == 0;
	}

	public static void VerifyInstallerVersions()
    {
        LatestInstallerVersion = VersionMetadata.InstallerVersion;
        Logger.WriteLine("Latest installer version: {0}", LatestInstallerVersion);
        string installerPath = Path.Combine(MKUtils.MKUtils.ProgramFilesPath, VersionMetadata.InstallerInstallPath, VersionMetadata.InstallerInstallFilename[ODL.Platform switch
        {
            odl.Platform.Windows => "windows",
            odl.Platform.Linux => "linux",
            odl.Platform.MacOS => "macos",
            _ => throw new NotImplementedException()
        }]).Replace('\\', '/');
        bool installUpdater = false;
        if (File.Exists(installerPath))
        {
            Logger.WriteLine("Found an installer at {0}", installerPath);
			// Check existing version
            if (ODL.OnWindows)
            {
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(installerPath);
                CurrentInstallerVersion = MKUtils.MKUtils.TrimVersion(fvi.ProductVersion);
            }
            else if (ODL.OnLinux || ODL.OnMacOS)
            {
                string versionFile = Path.Combine(MKUtils.MKUtils.ProgramFilesPath, VersionMetadata.InstallerInstallPath, "VERSION");
                if (File.Exists(versionFile))
                {
                    CurrentInstallerVersion = File.ReadAllText(versionFile);
                    if (!string.IsNullOrEmpty(CurrentInstallerVersion)) CurrentInstallerVersion = MKUtils.MKUtils.TrimVersion(CurrentInstallerVersion);
                }
                if (string.IsNullOrEmpty(CurrentInstallerVersion)) CurrentInstallerVersion = "0";
            }
            else throw new NotImplementedException();

            Logger.WriteLine("Current installer version: {0}", CurrentInstallerVersion);

            if (!string.IsNullOrEmpty(PendingInstallerVersion))
            {
                int cmpPending = VersionMetadata.CompareVersions(PendingInstallerVersion, LatestInstallerVersion);
                if (cmpPending >= 0)
                {
                    // If the pending installer is newer or the same as the latest installer version, do not update.
                    Logger.WriteLine($"A pending installer update exists with the same version as the latest installer version. There is no need to redownload; the pending installation should be completed instead.");
                    InstallerUpdateAvailable = false;
                    return;
                }
            }

            int cmp = VersionMetadata.CompareVersions(CurrentInstallerVersion, LatestInstallerVersion);
            if (cmp == -1)
            {
                // An installer update exists
                installUpdater = true;
                Logger.WriteLine("A newer installer exists; download it.");
            }
        }
        else
        {
            // No existing updater; download it.
            installUpdater = true;
            Logger.WriteLine("No installer was found at {0}. One will be downloaded.", installerPath);
        }
        if (!installUpdater) return;
        InstallerUpdateAvailable = true;
	}

    public static void VerifyVersions()
    {
        // Load current version
        // Changed in Project Settings -> Package -> Package Version (stored in .csproj)
        // Try getting the version from the assembly first (debug)
        Logger.WriteLine("Determining current version...");
        if (ODL.OnWindows)
        {
            CurrentProgramVersion = FileVersionInfo.GetVersionInfo(Environment.ProcessPath).ProductVersion;
            CurrentProgramVersion = MKUtils.MKUtils.TrimVersion(CurrentProgramVersion);
        }
        else if (ODL.OnLinux || ODL.OnMacOS)
        {
            CurrentProgramVersion = File.Exists("VERSION") ? File.ReadAllText("VERSION") : "0";
            if (string.IsNullOrEmpty(CurrentProgramVersion)) CurrentProgramVersion = "0";
            CurrentProgramVersion = MKUtils.MKUtils.TrimVersion(CurrentProgramVersion);
        }
        else throw new NotImplementedException();
        Logger.WriteLine("Current version: {0}", CurrentProgramVersion);
        // Load latest version
        Logger.WriteLine("Downloading version metadata...");
        if (MKUtils.VersionMetadata.Load())
        {
#if DEBUG
			Logger.WriteLine("Skipped latest version check in Debug Mode");
			return;
#endif
			LatestProgramVersion = MKUtils.VersionMetadata.ProgramVersion;
            // Compare versions
            int cmp = MKUtils.VersionMetadata.CompareVersions(CurrentProgramVersion, LatestProgramVersion);
            if (!string.IsNullOrEmpty(LatestProgramVersion) && cmp == -1)
            {
                // LatestVersion > CurrentVersion, so there is an update available.
                ProgramUpdateAvailable = true;
                Logger.WriteLine($"Version {CurrentProgramVersion} is outdated. Update {LatestProgramVersion} is available.");
            }
            else Logger.WriteLine($"Version {CurrentProgramVersion} is up-to-date.");
            MetadataLoadSuccessful = true;
        }
        else Logger.WriteLine($"Failed to download metadata.");
    }

    private static bool InitializeProgram()
    {
        Logger.WriteLine("Launching RPG Studio MK.");
        Logger.WriteLine($"Editor Version: {Editor.GetVersionString()}");
#if DEBUG
        Logger.WriteLine("===============================\nProgram launched in Debug mode.\n===============================");
#endif
        Config.Setup();
        InitializeAmethyst();
        return InitializeRuby();
    }

    private static void InitializeAmethyst()
    {
        Amethyst.Start(Config.PathInfo, true, true);
        int Handle = Audio.LoadSoundfont("assets/soundfont.sf2");
        if (Handle == 0) throw new Exception("Failed to load soundfont.");
    }

    private static bool InitializeRuby()
    {
        Logger.WriteLine("Loading Ruby...");
        string rubyVersion = Ruby.Initialize(Config.PathInfo);
        Logger.WriteLine("Loaded Ruby ({0})", rubyVersion);
        IntPtr ruby_load_path = Ruby.GetGlobal("$LOAD_PATH");
        
        Ruby.Array.Push(ruby_load_path, Ruby.String.ToPtr("./lib/ruby/2.7.0"));
        if (ODL.OnWindows)
        {
            Ruby.Array.Push(ruby_load_path, Ruby.String.ToPtr("./lib/ruby/2.7.0/x64-mingw32"));
        }
        else if (ODL.OnLinux)
        {
            Ruby.Array.Push(ruby_load_path, Ruby.String.ToPtr("./lib/ruby/2.7.0/x86_64-linux"));
        }
        else if (ODL.OnMacOS)
        {
            Ruby.Array.Push(ruby_load_path, Ruby.String.ToPtr("./lib/ruby/2.7.0/arm64-darwin22"));
        }

        if (!Ruby.Require("zlib"))
        {
            Logger.WriteLine("Ruby failed to initialize zlib. The program cannot continue.");
            odl.Popup popup = new Popup("Error", "Ruby failed to initialize zlib. The program cannot continue.");
            popup.Show();
            return false;
        }
        return true;
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
        Logger.WriteLine($"OS Platform: {ODL.Platform} ({os.Platform}) {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}");
        Logger.WriteLine($"OS Version: {os.VersionString}");
    }
}
