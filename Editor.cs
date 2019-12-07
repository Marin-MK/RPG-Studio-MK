using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using ODL;

namespace MKEditor
{
    public static class Editor
    {
        public static bool UnsavedChanges = false;
        private static Platform? _platform;
        /// <summary>
        /// The current OS.
        /// </summary>
        public static Platform Platform
        {
            get
            {
                if (_platform != null) return (Platform)_platform;
                string p = SDL2.SDL.SDL_GetPlatform();
                if (p == "Windows") _platform = Platform.Windows;
                if (p == "Linux") _platform = Platform.Linux;
                if (p == "Mac OS X") _platform = Platform.MacOS;
                if (p == "iOS") _platform = Platform.IOS;
                if (p == "Android") _platform = Platform.Android;
                return (Platform)_platform;
            }
        }

        public static MainEditorWindow MainWindow;
        public static bool InProject { get { return !string.IsNullOrEmpty(ProjectFilePath); } }
        public static string ProjectFilePath;
        public static ProjectSettings ProjectSettings;
        public static GeneralSettings GeneralSettings;

        public static string GetVersionString()
        {
            // Changed in Project Settings -> Package -> Package Version (stored in .csproj)
            Assembly assembly = Assembly.GetExecutingAssembly();
            string Version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            if (string.IsNullOrEmpty(Version)) Version = "0.0.1";
            string VersionName = "Version";
            if (Version[0] == '0') VersionName = "Alpha";
            return VersionName + " " + Version;
        }

        public static OperatingSystem GetOperatingSystem()
        {
            return Environment.OSVersion;
        }

        public static void DumpProjectSettings()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(ProjectFilePath, FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, ProjectSettings);
            stream.Close();
        }

        public static void LoadProjectSettings()
        {
            if (File.Exists(ProjectFilePath))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(ProjectFilePath, FileMode.Open, FileAccess.Read);
                ProjectSettings = formatter.Deserialize(stream) as ProjectSettings;
                stream.Close();
            }
            else
            {
                ProjectSettings = new ProjectSettings();
            }
            if (ProjectSettings.LastZoomFactor == 0) ProjectSettings.LastZoomFactor = 1;
            if (ProjectSettings.ProjectName.Length == 0) ProjectSettings.ProjectName = "Untitled Game";
        }

        public static void ClearProjectData()
        {
            ProjectFilePath = null;
            ProjectSettings = null;
            UnsavedChanges = false;
        }

        public static void CloseProject()
        {
            if (!InProject) return;
            if (MainWindow.MainEditorWidget != null) MainWindow.MainEditorWidget.Dispose();
            MainWindow.MainEditorWidget = null;
            MainWindow.StatusBar.SetVisible(false);
            MainWindow.ToolBar.SetVisible(false);
            MainWindow.HomeScreen = new Widgets.HomeScreen(MainWindow.MainGridLayout);
            MainWindow.HomeScreen.SetGridRow(3);
            MainWindow.MainGridLayout.UpdateLayout();
            Game.Data.ClearProjectData();
            ClearProjectData();
        }

        public static void NewProject()
        {
            new Widgets.MessageBox("Oops!", "This feature has not been implemented yet.\nTo get started, please use the \"Open Project\" feature and choose the MK Starter Kit.");
        }

        public static void OpenProject()
        {
            OpenFile of = new OpenFile();
            of.SetFilters(new List<FileFilter>()
            {
                new FileFilter("MK Project File", "mkproj")
            });
            string lastfolder = "";
            if (GeneralSettings.RecentFiles.Count > 0)
            {
                string path = GeneralSettings.RecentFiles[0][1];
                while (path.Contains("/")) path = path.Replace("/", "\\");
                List<string> folders = path.Split('\\').ToList();
                for (int i = 0; i < folders.Count - 1; i++)
                {
                    lastfolder += folders[i];
                    if (i != folders.Count - 2) lastfolder += "\\";
                }
            }
            of.SetInitialDirectory(lastfolder);
            of.SetTitle("Choose a project file...");
            string result = of.Show();
            if (!string.IsNullOrEmpty(result))
            {
                CloseProject();
                Game.Data.SetProjectPath(result);
                MainWindow.CreateEditor();
                MakeRecentProject();
            }
        }

        public static void SaveProject()
        {
            DateTime t1 = DateTime.Now;
            DumpProjectSettings();
            Game.Data.SaveTilesets();
            Game.Data.SaveMaps();
            Game.Data.SaveSpecies();
            UnsavedChanges = false;
            long time = (long) Math.Round((DateTime.Now - t1).TotalMilliseconds);
            MainWindow.StatusBar.QueueMessage($"Saved project ({time}ms)");
        }

        public static void StartGame()
        {
            MainWindow.StatusBar.QueueMessage("Game starting...");
            Process.Start(Game.Data.ProjectPath + "/mkxp.exe");
        }

        public static void OpenGameFolder()
        {
            Utilities.OpenFolder(Game.Data.ProjectPath);
        }

        public static void ExitEditor()
        {
            MainWindow.Dispose();
        }

        public static void DumpGeneralSettings()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("editor.mkd", FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, GeneralSettings);
            stream.Close();
        }

        public static void LoadGeneralSettings()
        {
            if (File.Exists("editor.mkd"))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream("editor.mkd", FileMode.Open, FileAccess.Read);
                GeneralSettings = formatter.Deserialize(stream) as GeneralSettings;
                stream.Close();
            }
            else
            {
                GeneralSettings = new GeneralSettings();
            }
            if (GeneralSettings.LastWidth < MainWindow.MinimumSize.Width) GeneralSettings.LastWidth = MainWindow.MinimumSize.Width;
            if (GeneralSettings.LastHeight < MainWindow.MinimumSize.Height) GeneralSettings.LastHeight = MainWindow.MinimumSize.Height;
            if (GeneralSettings.LastX < 0) GeneralSettings.LastX = 0;
            if (GeneralSettings.LastY < 0) GeneralSettings.LastY = 0;
        }

        public static void MakeRecentProject()
        {
            for (int i = 0; i < GeneralSettings.RecentFiles.Count; i++)
            {
                if (GeneralSettings.RecentFiles[i][1] == ProjectFilePath) // Project file paths match - same project
                {
                    GeneralSettings.RecentFiles.RemoveAt(i);
                }
            }
            GeneralSettings.RecentFiles.Add(new List<string>() { ProjectSettings.ProjectName, ProjectFilePath });
        }
    }

    [Serializable]
    public class ProjectSettings
    {
        public List<object> MapOrder = new List<object>();
        public string ProjectName = "Untitled Game";
        public int LastMapID = 1;
        public int LastLayer = 1;
        public double LastZoomFactor = 1;
    }

    [Serializable]
    public class GeneralSettings
    {
        public bool WasMaximized = true;
        public int LastWidth = 600;
        public int LastHeight = 600;
        public int LastX = 50;
        public int LastY = 50;
        public List<List<string>> RecentFiles = new List<List<string>>();
    }

    public enum Platform
    {
        Unknown,
        Windows,
        Linux,
        MacOS,
        IOS,
        Android
    }
}
