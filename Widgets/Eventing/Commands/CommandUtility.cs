using MKEditor.Game;
using MKEditor.Widgets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MKEditor
{
    public class CommandUtility
    {
        public CommandBox CommandBox;
        public List<BasicCommand> Commands;
        public int CommandIndex;
        public Dictionary<string, object> Parameters;
        public bool AwaitCloseFlag = false;
        public bool CanClose = true;
        public bool RedrawAll = false;

        public CommandUtility(CommandBox CommandBox, List<BasicCommand> Commands, int CommandIndex, Dictionary<string, object> Parameters)
        {
            this.CommandBox = CommandBox;
            this.Commands = Commands;
            this.CommandIndex = CommandIndex;
            this.Parameters = Parameters;
        }

        public object Param(string Name)
        {
            return Parameters[":" + Name];
        }

        public void CreateParam(string Name, object Value)
        {
            Parameters.Add(":" + Name, Value);
        }

        public bool HasParam(string Name)
        {
            return Parameters.ContainsKey(":" + Name);
        }

        public void RemoveParam(string Name)
        {
            Parameters.Remove(":" + Name);
        }

        public void SetParam(string Name, object Value)
        {
            Parameters[":" + Name] = Value;
        }

        public bool ParamIsString(string Name)
        {
            return Param(Name) is string;
        }

        public string ParamAsString(string Name)
        {
            return Param(Name) as string;
        }

        public bool ParamIsInt(string Name)
        {
            return Param(Name) is int || Param(Name) is long;
        }

        public int ParamAsInt(string Name)
        {
            return Convert.ToInt32(Param(Name));
        }

        public bool ParamIsBool(string Name)
        {
            return Param(Name) is bool;
        }

        public bool ParamAsBool(string Name)
        {
            return Convert.ToBoolean(Param(Name));
        }

        public bool ParamIsHash(string Name)
        {
            return Param(Name) is Dictionary<string, object> || Param(Name) is JObject;
        }

        public Dictionary<string, object> ParamAsHash(string Name)
        {
            if (Param(Name) is JObject) return ((JObject) Param(Name)).ToObject<Dictionary<string, object>>();
            else if (Param(Name) is Dictionary<string, object>) return (Dictionary<string, object>) Param(Name);
            return null;
        }

        public bool ParamIsArray(string Name)
        {
            return Param(Name) is List<object> || Param(Name) is JArray;
        }

        public List<object> ParamAsArray(string Name)
        {
            if (Param(Name) is JArray) return ((JArray) Param(Name)).ToObject<List<object>>();
            else if (Param(Name) is List<object>) return (List<object>) Param(Name);
            return null;
        }



        public CommandUtility GetBranchParent()
        {
            BasicCommand main = Commands[CommandIndex];
            DynamicCommandType maintype = CommandBox.GetCommandType(main);
            string identifier = this.Commands[this.CommandIndex].Identifier;
            for (int i = this.CommandIndex - 1; i >= 0; i--)
            {
                BasicCommand cmd = this.Commands[i];
                DynamicCommandType cmdtype = CommandBox.GetCommandType(cmd);
                if (maintype.IsSubBranch && cmdtype.HasBranches && cmd.Indent == main.Indent ||
                    !maintype.IsSubBranch && cmd.Indent < main.Indent)
                {
                    return new CommandUtility(this.CommandBox, this.Commands, i, this.Commands[i].Parameters);
                }
            }
            throw new Exception($"Failed to find a branch parent.");
        }

        public List<CommandUtility> GetAllBranches()
        {
            if (!CommandBox.GetCommandType(this.Commands[this.CommandIndex]).HasBranches) throw new Exception($"Cannot return all branches for a branchless command.");
            List<CommandUtility> Utilities = new List<CommandUtility>();
            for (int i = this.CommandIndex + 1; i < Commands.Count; i++)
            {
                BasicCommand cmd = this.Commands[i];
                if (Commands[CommandIndex].Indent == cmd.Indent && CommandBox.GetCommandType(cmd).IsSubBranch)
                {
                    Utilities.Add(new CommandUtility(this.CommandBox, this.Commands, i, this.Commands[i].Parameters));
                }
            }
            return Utilities;
        }

        public bool IsBranchEmpty()
        {
            BasicCommand main = Commands[CommandIndex];
            BasicCommand next = Commands[CommandIndex + 1];
            return next.Indent <= main.Indent;
        }

        public void UpdateBranchCommands(int BranchCount, dynamic NewBranchCallback)
        {
            List<dynamic> BranchUtilities = new List<dynamic>();
            dynamic branches = GetAllBranches();
            for (int i = 0; i < branches.Count; i++) BranchUtilities.Add(branches[i]);
            if (BranchUtilities.Count < BranchCount)
            {
                // Add new branches
                for (int i = BranchUtilities.Count; i < BranchCount; i++)
                {
                    AddNewBranch(NewBranchCallback(i, Commands[CommandIndex].Indent));
                }
            }
            else if (BranchUtilities.Count > BranchCount)
            {
                // Remove existing branches
                // Removing a command at index X will affect indexes and lists with an index HIGHER than X,
                // So by removing the last branch to delete down to the first branch to delete,
                // we never affect/offset an entry we've yet to delete.
                for (int i = BranchUtilities.Count - 1; i >= BranchCount; i--)
                {
                    RemoveBranch(BranchUtilities[i].Commands[BranchUtilities[i].CommandIndex]);
                }
            }
        }

        public BasicCommand CreateCommand(int Indent, string Identifier, Dictionary<string, object> Parameters)
        {
            return new BasicCommand(Indent, ":" + Identifier, Parameters);
        }

        public void AddNewBranch(BasicCommand Command)
        {
            for (int i = this.CommandIndex + 1; i < Commands.Count; i++)
            {
                BasicCommand cmd = this.Commands[i];
                if (Commands[CommandIndex].Indent == cmd.Indent && !CommandBox.GetCommandType(cmd).IsSubBranch)
                {
                    this.Commands.Insert(i, Command);
                    break;
                }
            }
        }

        public void RemoveBranch(BasicCommand main)
        {
            int StartIndex = -1;
            for (int i = this.CommandIndex; i < Commands.Count; i++)
            {
                BasicCommand cmd = this.Commands[i];
                DynamicCommandType cmdtype = CommandBox.GetCommandType(cmd);
                if (cmd == main)
                {
                    StartIndex = i;
                }
                else if (StartIndex != -1)
                {
                    if (cmd.Indent <= main.Indent)
                    {
                        this.Commands.RemoveRange(StartIndex, i - StartIndex);
                        break;
                    }
                }
            }
        }

        public bool TooManyBranches(int BranchCount)
        {
            List<CommandUtility> branches = GetAllBranches();
            if (BranchCount < branches.Count)
            {
                for (int i = BranchCount; i < branches.Count; i++)
                {
                    if (!branches[i].IsBranchEmpty()) return true;
                }
                return false;
            }
            return false;
        }



        public string GetSwitchName(string GroupID, string SwitchID)
        {
            return GetSwitchName((int) Param(GroupID), (int) Param(SwitchID));
        }

        public string GetSwitchName(int GroupID, int SwitchID)
        {
            return Editor.ProjectSettings.Switches[GroupID - 1].Switches[SwitchID - 1].Name;
        }

        public string Digits(string Name, int DigitCount)
        {
            return Digits(ParamAsInt(Name), DigitCount);
        }

        public string Digits(int Integer, int DigitCount)
        {
            return Utilities.Digits(Integer, DigitCount);
        }



        public void ShowWindow(string Title, string Message, dynamic Callback)
        {
            ShowWindow(Title, Message, 0, 1, Callback);
        }

        public void ShowWindow(string Title, string Message, List<string> Buttons, dynamic Callback)
        {
            ShowWindow(Title, Message, 0, Buttons, Callback);
        }

        public void ShowWindow(string Title, string Message, dynamic IconType, List<string> Buttons, dynamic Callback)
        {
            MessageBox messagebox = new MessageBox(Title, Message, Buttons, (IconType) IconType);
            messagebox.OnClosed += delegate (odl.BaseEventArgs e)
            {
                Callback?.Invoke(messagebox.Result);
            };
        }

        public void ShowWindow(string Title, string Message, dynamic IconType, dynamic ButtonType, dynamic Callback)
        {
            MessageBox messagebox = new MessageBox(Title, Message, (ButtonType) ButtonType, (IconType) IconType);
            messagebox.OnClosed += delegate (odl.BaseEventArgs e)
            {
                Callback?.Invoke(messagebox.Result);
            };
        }
    }
}
