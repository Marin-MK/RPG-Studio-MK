using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MKEditor.Game
{
    public class BasicCommand
    {
        public CommandType Type;
        public int Indent;
        public string Identifier;
        public Dictionary<string, object> Parameters;

        public BasicCommand()
        {

        }

        public BasicCommand(CommandType Type, int Indent, string Identifier, Dictionary<string, object> Parameters)
        {
            this.Type = Type;
            this.Indent = Indent;
            this.Identifier = Identifier;
            this.Parameters = Parameters;
        }

        public static BasicCommand IDToCommand(int Indent, string Identifier, Dictionary<string, object> Parameters)
        {
            string nativeid = Identifier.Substring(1);
            CommandType type = CommandParser.Types.Find(t => t.Identifier == nativeid);
            if (type == null) throw new Exception($"Invalid command identifier: '{nativeid}'");
            return new BasicCommand(type, Indent, Identifier, Parameters);
        }

        public List<object> ToJSON()
        {
            List<object> Data = new List<object>();
            Data.Add(Indent);
            Data.Add(Identifier);
            Data.Add(Parameters);
            return Data;
        }

        public BasicCommand Clone()
        {
            BasicCommand bc = new BasicCommand();
            bc.Type = this.Type;
            bc.Indent = this.Indent;
            bc.Identifier = this.Identifier;
            bc.Parameters = new Dictionary<string, object>(this.Parameters);
            return bc;
        }
    }
}
