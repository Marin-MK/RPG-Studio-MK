using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MKEditor
{
    public static class Editor
    {
        public static bool UnsavedChanges = false;

        public static MainEditorWindow MainWindow;
        public static bool InProject { get { return !string.IsNullOrEmpty(ProjectFilePath); } }
        public static string ProjectFilePath;
        public static ProjectSettings ProjectSettings;
        public static GeneralSettings GeneralSettings;

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
        }

        public static void ClearProjectData()
        {
            ProjectFilePath = null;
            ProjectSettings = null;
            UnsavedChanges = false;
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
            MainWindow.StatusBar.QueueMessage($"Saved project ({time}ms).");
        }

        public static void StartGame()
        {
            MainWindow.StatusBar.QueueMessage("Game starting...");
            Process.Start(Game.Data.ProjectPath + "/mkxp.exe");
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
}
