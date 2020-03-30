using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MKEditor
{
    public static class CommandLineHandler
    {
        public static List<Command> Commands = new List<Command>();
        public static int ArgIndex = 0;
        public static List<string> CurrentArgs;
        public static Command CurrentCommand;
        public static bool? OpenEditor = true;

        public static void Init()
        {
            Register(new Command("--help", Help, "Shows this help menu.", "-h"));
            Register(new Command("--debug", DebugMode, "Enables debug mode, which means unsafe errors and no unsaved changes confirmation.", "-d"));
            Register(new Command("--version", Version, "Displays the version.", "-v"));
            Register(new Command("--generate-project-file", GenProjectFile, "Generates a new project file in <dir>.") { HelpArg = "<dir>" });
            Register(new Command("--test-sdl", TestSDL, "Tests the SDL libraries by initializing them."));
            Register(new Command("--test-ruby", TestRuby, "Tests the Ruby libraries."));
            Register(new Command("--reset-editor", ResetEditor, "Resets all the editor's internal settings and preferences.", "-r"));
            Register(new Command("--verbose", Verbose, "Provides more debug information in the console."));
            Commands.Sort(delegate (Command c1, Command c2) { return c1.Name.CompareTo(c2.Name); });
        }

        public static void Register(Command c)
        {
            Commands.Add(c);
        }

        public static bool Parse(List<string> Args)
        {
            CurrentArgs = new List<string>();
            for (int i = 0; i < Args.Count; i++)
            {
                if (Args[i].Length < 2 || Args[i][0] != '-' || Args[i][1] == '-') CurrentArgs.Add(Args[i]);
                else
                {
                    for (int j = 1; j < Args[i].Length; j++)
                    {
                        CurrentArgs.Add("-" + Args[i][j]);
                    }
                }
            }
            for (ArgIndex = 0; ArgIndex < CurrentArgs.Count; ArgIndex++)
            {
                bool Found = false;
                for (int i = 0; i < Commands.Count; i++)
                {
                    if (Commands[i].Aliases.Contains(CurrentArgs[ArgIndex]))
                    {
                        CurrentCommand = Commands[i];
                        bool Success = Commands[i].Trigger(CurrentArgs[ArgIndex]);
                        Found = true;
                        if (!Success) return false;
                    }
                }
                if (!Found)
                {
                    if (ArgIndex == 0)
                    {
                        if (File.Exists(CurrentArgs[0]))
                        {
                            if (!CurrentArgs[0].Contains(".mkproj"))
                            {
                                Error($"File is not a .mkproj file: {CurrentArgs[0]}");
                                return false;
                            }
                            Program.ProjectFile = CurrentArgs[0];
                            return true;
                        }
                        else
                        {
                            Error($"Unknown command or project file: '{CurrentArgs[0]}'");
                            return false;
                        }
                    }
                    CurrentCommand = null;
                    Error($"Unknown command: '{CurrentArgs[ArgIndex]}'");
                    return false;
                }
            }
            return OpenEditor != false;
        }

        public static string GetNextArg(bool SkipNextArg)
        {
            string Arg = ArgIndex < CurrentArgs.Count - 1 ? CurrentArgs[ArgIndex + 1] : null;
            if (SkipNextArg) ArgIndex++;
            return Arg;
        }

        public static bool Error(string Message)
        {
            Console.WriteLine($"ERROR: {(CurrentCommand == null ? "" : CurrentCommand.Name + ": ")}{Message}");
            return false;
        }

        public static bool Help(string Arg)
        {
            OpenEditor = false;
            Console.WriteLine($"All known commands:");
            int MaxLength = 4;
            foreach (Command c in Commands)
            {
                string Str = "    ";
                for (int i = 0; i < c.Aliases.Count; i++)
                {
                    Str += c.Aliases[i];
                    if (i != c.Aliases.Count - 1) Str += ", ";
                }
                if (!string.IsNullOrEmpty(c.HelpArg)) Str += " " + c.HelpArg;
                if (Str.Length > MaxLength) MaxLength = Str.Length;
            }
            Commands.Insert(0, new Command("...", null, "Opens the home screen of the editor."));
            Commands.Insert(1, new Command("<projectfile>", null, "Directly opens the project."));
            foreach (Command c in Commands)
            {
                string Str = "    ";
                for (int i = 0; i < c.Aliases.Count; i++)
                {
                    Str += c.Aliases[i];
                    if (i != c.Aliases.Count - 1) Str += ", ";
                }
                if (!string.IsNullOrEmpty(c.HelpArg)) Str += " " + c.HelpArg;
                for (int i = Str.Length; i < MaxLength; i++) Str += " ";
                Str += " : " + c.HelpDescription;
                Console.WriteLine(Str);
            }
            Commands.RemoveRange(0, 2);
            return true;
        }

        public static bool DebugMode(string Arg)
        {
            if (OpenEditor == null) OpenEditor = true;
            Program.ReleaseMode = false;
            Console.WriteLine($"Editor will start in debug mode.");
            return true;
        }

        public static bool Version(string Arg)
        {
            OpenEditor = false;
            Console.WriteLine($"RPG Studio MK ({Editor.GetVersionString()})");
            return true;
        }

        public static bool GenProjectFile(string Arg)
        {
            OpenEditor = false;
            string NextArg = GetNextArg(true);
            if (string.IsNullOrEmpty(NextArg)) return Error("Expected a folder path to generate a project file.");
            else
            {
                Editor.ProjectSettings = new ProjectSettings();
                Editor.ProjectFilePath = Path.Combine(NextArg, "project.mkproj");
                try
                {
                    Editor.DumpProjectSettings();
                    Console.WriteLine($"Created project file at {Editor.ProjectFilePath}.");
                }
                catch (UnauthorizedAccessException)
                {
                    return Error($"No permission to write in {Path.GetFullPath(NextArg)}.");
                }
                catch (DirectoryNotFoundException)
                {
                    return Error($"Directory {Path.GetFullPath(NextArg)} not found.");
                }
                catch (IOException)
                {
                    return Error($"Could not write to {Path.GetFullPath(NextArg)}");
                }
            }
            return true;
        }

        public static bool TestSDL(string Arg)
        {
            OpenEditor = false;
            SDL2.SDL.SDL_Init(SDL2.SDL.SDL_INIT_EVERYTHING);
            string Err = SDL2.SDL.SDL_GetError();
            if (!string.IsNullOrEmpty(Err)) return Error(Err);
            SDL2.SDL_image.IMG_Init(SDL2.SDL_image.IMG_InitFlags.IMG_INIT_PNG);
            Err = SDL2.SDL.SDL_GetError();
            if (!string.IsNullOrEmpty(Err)) return Error(Err);
            SDL2.SDL_ttf.TTF_Init();
            Err = SDL2.SDL.SDL_GetError();
            if (!string.IsNullOrEmpty(Err)) return Error(Err);
            SDL2.SDL_ttf.TTF_Quit();
            SDL2.SDL_image.IMG_Quit();
            SDL2.SDL.SDL_Quit();
            Console.WriteLine("SDL libraries initialized successfully.");
            Console.WriteLine($"SDL Version: {SDL2.SDL.SDL_MAJOR_VERSION}.{SDL2.SDL.SDL_MINOR_VERSION}.{SDL2.SDL.SDL_PATCHLEVEL}");
            Console.WriteLine($"SDL_image Version: {SDL2.SDL_image.SDL_IMAGE_MAJOR_VERSION}.{SDL2.SDL_image.SDL_IMAGE_MINOR_VERSION}.{SDL2.SDL_image.SDL_IMAGE_PATCHLEVEL}");
            Console.WriteLine($"SDL_ttf Version: {SDL2.SDL_ttf.SDL_TTF_MAJOR_VERSION}.{SDL2.SDL_ttf.SDL_TTF_MINOR_VERSION}.{SDL2.SDL_ttf.SDL_TTF_PATCHLEVEL}");
            return true;
        }

        public static bool TestRuby(string Arg)
        {
            OpenEditor = false;
            try
            {
                RubyDotNET.Internal.Initialize();
                string Version = new RubyDotNET.RubyString(RubyDotNET.Internal.Eval("RUBY_VERSION", false)).ToString();
                Console.WriteLine("Ruby libraries successfully tested.");
                Console.WriteLine($"Ruby version: {Version}");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
            return true;
        }

        public static bool ResetEditor(string Arg)
        {
            OpenEditor = false;
            if (File.Exists("editor.mkd"))
            {
                Editor.LoadGeneralSettings();
                Editor.GeneralSettings = new GeneralSettings();
                Editor.DumpGeneralSettings();
                Console.WriteLine($"Successfully reset the editor's settings and preferences.");
            }
            else
            {
                Console.WriteLine($"No settings and preferences file could be found. The editor is already using default settings.");
            }
            return true;
        }

        public static bool Verbose(string Arg)
        {
            if (OpenEditor == null) OpenEditor = true;
            Program.Verbose = true;
            Console.WriteLine($"Editor will start in verbose mode.");
            return true;
        }
    }

    public class Command
    {
        public string Name;
        public List<string> Aliases = new List<string>();
        public CommandCallBack Callback;
        public string HelpDescription;
        public string HelpArg;

        public delegate bool CommandCallBack(string Arg);

        public Command(string Name, CommandCallBack Callback, string HelpDescription, params string[] Aliases)
        {
            this.Name = Name;
            this.Aliases.Add(Name);
            this.Aliases.AddRange(Aliases);
            this.HelpDescription = HelpDescription;
            this.Callback = Callback;
        }

        public bool Trigger(string Arg)
        {
            return Callback(Arg);
        }
    }
}
