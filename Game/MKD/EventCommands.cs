using System;
using System.Collections.Generic;

namespace MKEditor.Game
{
    public class BasicCommand
    {
        public int Indent;
        public string Identifier;
        public Dictionary<string, object> Parameters;

        public BasicCommand(int Indent, string Identifier, Dictionary<string, object> Parameters)
        {
            this.Indent = Indent;
            this.Identifier = Identifier;
            this.Parameters = Parameters;
        }

        public static BasicCommand IDToCommand(int Indent, string Identifier, Dictionary<string, object> Parameters)
        {
            switch (Identifier)
            {
                case ":script":
                    return new ScriptCommand(Indent, Identifier, Parameters);
                case ":message":
                    return new MessageCommand(Indent, Identifier, Parameters);
                default:
                    throw new Exception($"Unknown condition Identifier: '{Identifier}'");
            }
        }

        public List<object> ToJSON()
        {
            List<object> Data = new List<object>();
            Data.Add(Indent);
            Data.Add(Identifier);
            Data.Add(Parameters);
            return Data;
        }
    }

    public class ScriptCommand : BasicCommand
    {
        public string Code { get { return (string) Parameters[":code"] ?? ""; } }

        public ScriptCommand(int Indent, string Identifier, Dictionary<string, object> Parameters)
            : base(Indent, Identifier, Parameters) { }
    }

    public class MessageCommand : BasicCommand
    {
        public string Message { get { return (string) Parameters[":text"] ?? ""; } }

        public MessageCommand(int Indent, string Identifier, Dictionary<string, object> Parameters)
            : base(Indent, Identifier, Parameters) { }
    }
}
