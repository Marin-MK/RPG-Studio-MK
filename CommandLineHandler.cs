using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RPGStudioMK
{
    public class CommandLineHandler : IDisposable
    {
        public List<Command> Commands = new List<Command>();
        public int ArgIndex = 0;
        public List<string> CurrentArgs;
        public Command CurrentCommand;
        public string BaseCommand;
        public bool Success = false;

        public void Initialize()
        {
            Commands.Sort(delegate (Command c1, Command c2) { return c1.Name.CompareTo(c2.Name); });
        }

        public static List<List<string>> InputToArgs(string Input)
        {
            List<List<string>> Lines = new List<List<string>>();
            List<string> Args = new List<string>();
            bool InString = false;
            string CurrentWord = "";
            for (int i = 0; i < Input.Length; i++)
            {
                char? p = i > 0 ? Input[i - 1] : (char?) null;
                char c = Input[i];
                char? n = i < Input.Length - 1 ? Input[i + 1] : (char?) null;
                if (c == '\\')
                {
                    if (n == '"' || n == '\\')
                    {
                        CurrentWord += n;
                        i++;
                    }
                    else CurrentWord += c;
                }
                else if (c == '"') // Unescaped quote as escaped quote is handled above
                {
                    InString = !InString;
                }
                else if (c == ' ')
                {
                    if (InString) CurrentWord += c;
                    else if (!string.IsNullOrEmpty(CurrentWord))
                    {
                        Args.Add(CurrentWord);
                        CurrentWord = "";
                    }
                }
                else if (c == '=')
                {
                    if (InString) CurrentWord += c;
                    else
                    {
                        Args.Add(CurrentWord);
                        CurrentWord = "";
                    }
                }
                else if (!InString && (c == '&' && n == '&' || c == ';'))
                {
                    if (c != ';') i++;
                    if (!string.IsNullOrEmpty(CurrentWord)) Args.Add(CurrentWord);
                    Lines.Add(new List<string>(Args));
                    Args.Clear();
                    CurrentWord = "";
                }
                else
                {
                    CurrentWord += c;
                }
            }
            if (!string.IsNullOrEmpty(CurrentWord)) Args.Add(CurrentWord);
            Lines.Add(Args);
            if (InString) throw new CommandLineException("Unterminated string.");
            return Lines;
        }

        public void Register(Command c)
        {
            Commands.Add(c);
        }

        public virtual bool Parse(List<string> Args)
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
                        if (Commands[i].Condition != null && !Commands[i].Condition.Invoke()) continue;
                        CurrentCommand = Commands[i];
                        Success = Commands[i].Trigger();
                        Found = true;
                        if (!Success)
                        {
                            Finalize();
                            return false;
                        }
                    }
                }
                if (!Found)
                {
                    CurrentCommand = null;
                    Error($"Unknown command: '{CurrentArgs[ArgIndex]}'");
                    return false;
                }
            }
            return Success = Finalize();
        }

        public virtual bool Finalize()
        {
            return true;
        }

        public string GetNextArg(bool SkipNextArg)
        {
            string Arg = ArgIndex < CurrentArgs.Count - 1 ? CurrentArgs[ArgIndex + 1] : null;
            if (SkipNextArg) ArgIndex++;
            return Arg;
        }

        public virtual bool Error(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"ERROR: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Message);
            return false;
        }

        public virtual bool Help()
        {
            Console.WriteLine($"All known commands:");
            int MaxLength = 4;
            foreach (Command c in Commands)
            {
                if (c.Condition != null && !c.Condition.Invoke()) continue;
                string Str = "    " + (string.IsNullOrEmpty(BaseCommand) ? "" : BaseCommand);
                for (int i = 0; i < c.Aliases.Count; i++)
                {
                    Str += c.Aliases[i];
                    if (i != c.Aliases.Count - 1) Str += ", ";
                }
                if (!string.IsNullOrEmpty(c.HelpArg)) Str += " " + c.HelpArg;
                if (Str.Length > MaxLength) MaxLength = Str.Length;
            }
            foreach (Command c in Commands)
            {
                if (c.Condition != null && !c.Condition.Invoke()) continue;
                string Str = "    " + (string.IsNullOrEmpty(BaseCommand) ? "" : BaseCommand);
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
            return false;
        }

        public void Dispose()
        {
            this.Commands.Clear();
            this.Commands = null;
            this.CurrentArgs.Clear();
            this.CurrentArgs = null;
            this.CurrentCommand = null;
        }
    }

    public class Command
    {
        public string Name;
        public List<string> Aliases = new List<string>();
        public CommandCallBack Callback;
        public ConditionCallBack Condition;
        public string HelpDescription;
        public string HelpArg;

        public delegate bool CommandCallBack();
        public delegate bool ConditionCallBack();

        public Command(string Name, CommandCallBack Callback, string HelpDescription, params string[] Aliases)
        {
            this.Name = Name;
            this.Aliases.Add(Name);
            this.Aliases.AddRange(Aliases);
            this.HelpDescription = HelpDescription;
            this.Callback = Callback;
        }

        public bool Trigger()
        {
            return Callback();
        }
    }

    public class CommandLineException : Exception
    {
        public CommandLineException(string Message) : base(Message) { }
    }
}
