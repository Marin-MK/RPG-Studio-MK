using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MKEditor
{
    public class CommandUtility
    {
        public Dictionary<string, object> Parameters;

        public CommandUtility(Dictionary<string, object> Parameters)
        {
            this.Parameters = Parameters;
        }

        public object Param(string Name)
        {
            return Parameters[":" + Name];
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
