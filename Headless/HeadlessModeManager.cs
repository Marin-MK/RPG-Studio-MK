using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MKEditor
{
    public class HeadlessModeManager
    {
        HeadlessCommandLineHandler cli;

        public HeadlessModeManager()
        {
            cli = new HeadlessCommandLineHandler();
            Console.WriteLine("Welcome to RPG Studio MK headless mode. Type 'help' to get started.");
            Console.WriteLine();
            Editor.LoadGeneralSettings();
        }

        public void Start()
        {
            while (Program.Headless)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"$:> ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                string Input = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                foreach (List<string> Args in CommandLineHandler.InputToArgs(Input))
                {
                    if (!cli.Parse(Args)) break;
                }
            }
            Editor.DumpGeneralSettings();
        }
    }

    public class HeadlessCommandLineHandler : CommandLineHandler
    {
        public bool CloseProject = false;

        public HeadlessCommandLineHandler()
        {
            Register(new Command("help", Help, "Shows this help menu."));
            Register(new Command("open-project", OpenProject, "Opens a project.") { HelpArg = "<project.mkproj>", Condition = delegate () { return !Editor.InProject; } });
            Register(new Command("close-project", CloseProjectFunc, "Closes the current project.") { Condition = delegate () { return Editor.InProject; } });
            Register(new Command("show-recents", ShowRecents, "Displays a list of all recently opened projects.") { Condition = delegate () { return !Editor.InProject; } });
            Register(new Command("load-recent", LoadRecent, "Loads a recently opened project.") { Condition = delegate () { return !Editor.InProject; } });
            Register(new Command("map", MapMode, "Perform mapping-related operations.") { Condition = delegate () { return Editor.InProject; } });
            Register(new Command("data", DataMode, "Perform data-related operations.") { Condition = delegate () { return Editor.InProject; } });
            Register(new Command("clear", Clear, "Clears the console window.", "cls"));
            Register(new Command("exit", Exit, "Exit headless mode.", "quit"));
            Register(new Command("save", Save, "Saves all changes to the project.") { Condition = delegate () { return Editor.InProject; } });
            Initialize();
        }

        public override bool Finalize()
        {
            if (CloseProject)
            {
                CloseProject = false;
                if (!CurrentArgs.Contains("-c") && !CurrentArgs.Contains("--confirm"))
                    return Error($"You are about to close the project. Any unsaved changes will be discarded. Please repeat the command with the '--confirm' flag.");
                Game.Data.ClearProjectData();
                Editor.ClearProjectData();
                Console.WriteLine("Successfully closed the project.");
            }
            return true;
        }

        public bool Clear()
        {
            Console.Clear();
            return true;
        }

        private bool LoadProject(string File)
        {
            if (!File.Contains(".mkproj"))
            {
                Error($"File is not a .mkproj file: {File}");
                return false;
            }

            try
            {
                Game.Data.SetProjectPath(File);
            }
            catch (Exception ex)
            {
                Error($"Something went wrong while trying to load the project: {ex.Message}");
                return false;
            }
            try
            {
                Editor.LoadProjectSettings();
            }
            catch (Exception ex)
            {
                Error($"Something went wrong while trying to load project settings: {ex.Message}");
                return false;
            }
            try
            {
                Game.Data.LoadGameData();
            }
            catch (Exception ex)
            {
                Error($"Something went wrong while trying to load the game data: {ex.Message}");
                return false;
            }
            Console.WriteLine($"Project '{Editor.ProjectSettings.ProjectName}' loaded successfully.");
            Editor.MakeRecentProject();
            return true;
        }

        public bool OpenProject()
        {
            string NextArg = GetNextArg(true);
            if (string.IsNullOrEmpty(NextArg)) return Error("Expected a file path.");
            else
            {
                if (File.Exists(NextArg))
                {
                    return LoadProject(NextArg);
                }
                else
                {
                    Error($"Invalid file path: '{NextArg}'");
                    return false;
                }
            }
        }

        public bool CloseProjectFunc()
        {
            CloseProject = true;
            return false;
        }

        public bool ShowRecents()
        {
            Console.WriteLine("To load a recently opened project, you may use the 'load-recent' command, followed by the index of the project.");
            for (int i = 0; i < 10; i++)
            {
                if (i >= Editor.GeneralSettings.RecentFiles.Count) break;
                List<string> Recent = Editor.GeneralSettings.RecentFiles[i];
                string Name = Recent[0];
                string Path = Recent[1];
                Console.WriteLine($"#{i + 1}.) {Name}\n        {Path}");
                Console.WriteLine();
            }
            return true;
        }

        public bool LoadRecent()
        {
            string NextArg = GetNextArg(true);
            if (string.IsNullOrEmpty(NextArg)) return Error("Expected an index.");
            else
            {
                try
                {
                    int Index = Convert.ToInt32(NextArg);
                    if (Index < 1 || Index > 10) return Error("Invalid index.");
                    LoadProject(Editor.GeneralSettings.RecentFiles[Index - 1][1]);
                }
                catch (Exception)
                {
                    return Error("Invalid index.");
                }
            }
            return true;
        }

        public bool MapMode()
        {
            if (ArgIndex >= CurrentArgs.Count - 1)
            {
                Console.WriteLine("For information about the 'map' command, please use 'map --help'.");
            }
            else
            {
                if (!Editor.InProject)
                {
                    Error("You must be in a project to use the 'map' command. Please see the 'help' command for more information.");
                    return false;
                }
                MapCommandLineHandler cli = new MapCommandLineHandler();
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false; // Return false to cancel the rest of the parsing
            }
            return true;
        }

        public bool DataMode()
        {
            if (CurrentArgs.Count == 1)
            {
                Console.WriteLine("For information about the 'data' command, please use 'data --help'.");
            }
            else
            {
                if (!Editor.InProject)
                {
                    Error("You must be in a project to use the 'data' command. Please see the 'help' command for more information.");
                    return false;
                }
                DataCommandLineHandler cli = new DataCommandLineHandler();
                cli.Parse(CurrentArgs.GetRange(ArgIndex + 1, CurrentArgs.Count - ArgIndex - 1));
                return false; // Return false to cancel the rest of the parsing
            }
            return true;
        }

        public bool Exit()
        {
            Program.Headless = false;
            return false;
        }

        public bool Save()
        {
            try
            {
                Editor.SaveProject();
                Console.WriteLine("The project has been saved successfully.");
            }
            catch (Exception ex)
            {
                return Error($"Something went wrong while saving the project: {ex.Message}");
            }
            return true;
        }
    }
}
