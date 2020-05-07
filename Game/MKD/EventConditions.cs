using System;
using System.Collections.Generic;

namespace MKEditor.Game
{
    public class BasicCondition
    {
        public string Identifier;
        public Dictionary<string, object> Parameters;

        public BasicCondition(string Identifier, Dictionary<string, object> Parameters)
        {
            this.Identifier = Identifier;
            this.Parameters = Parameters;
        }

        public static BasicCondition IDToCondition(string Identifier, Dictionary<string, object> Parameters)
        {
            switch (Identifier)
            {
                case ":script":
                    return new ScriptCondition(Identifier, Parameters);
                case ":switch":
                    return new SwitchCondition(Identifier, Parameters);
                case ":variable":
                    return new VariableCondition(Identifier, Parameters);
                default:
                    throw new Exception($"Unknown condition Identifier: '{Identifier}'");
            }
        }

        public List<object> ToJSON()
        {
            List<object> Data = new List<object>();
            Data.Add(Identifier);
            Data.Add(Parameters);
            return Data;
        }
    }

    public class ScriptCondition : BasicCondition
    {
        public string Code { get { return (string) Parameters[":code"]?? ""; } }

        public ScriptCondition(string Identifier, Dictionary<string, object> Parameters)
            : base(Identifier, Parameters) { }
    }

    public class SwitchCondition : BasicCondition
    {
        public int GroupID { get { return (int) Parameters[":group_id"]; } }
        public int SwitchID { get { return (int) Parameters[":switch_id"]; } }
        public bool Value { get { return Convert.ToBoolean(Parameters[":value"]); } }

        public SwitchCondition(string Identifier, Dictionary<string, object> Parameters)
            : base(Identifier, Parameters) { }
    }

    public class VariableCondition : BasicCondition
    {
        public int GroupID { get { return (int) Parameters[":group_id"]; } }
        public int VariableID { get { return (int) Parameters[":variable_id"]; } }
        public object Value { get { return Parameters[":value"]; } }

        public VariableCondition(string Identifier, Dictionary<string, object> Parameters)
            : base(Identifier, Parameters) { }
    }
}
