using MKEditor.Game;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MKEditor
{
    public class CommandUtility
    {
        public List<BasicCommand> Commands;
        public int CommandIndex;
        public Dictionary<string, object> Parameters;

        public CommandUtility(List<BasicCommand> Commands, int CommandIndex, Dictionary<string, object> Parameters)
        {
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
            string identifier = this.Commands[this.CommandIndex].Identifier;
            for (int i = this.CommandIndex - 1; i >= 0; i--)
            {
                BasicCommand cmd = this.Commands[i];
                if (cmd.Identifier == ":choice" ||
                    cmd.Identifier == ":choice_branch" && identifier != ":choice_branch")
                {
                    return new CommandUtility(this.Commands, i, this.Commands[i].Parameters);
                }
            }
            throw new Exception($"Failed to find a branch parent.");
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
    }
}
