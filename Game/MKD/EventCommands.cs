using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RPGStudioMK.Game
{
    public class BasicCommand
    {
        public int Indent;
        public string Identifier;
        public Dictionary<string, object> Parameters;

        public BasicCommand()
        {

        }

        public BasicCommand(int Indent, string Identifier, Dictionary<string, object> Parameters)
        {
            this.Indent = Indent;
            this.Identifier = Identifier;
            this.Parameters = Parameters;
        }

        public static BasicCommand IDToCommand(int Indent, string Identifier, Dictionary<string, object> Parameters)
        {
            throw new NotImplementedException("Legacy event commands");
            /*string nativeid = Identifier.Substring(1);
            if (CommandPlugins.CommandTypes.Find(t => t.Identifier == nativeid) == null)
                throw new Exception($"Invalid command identifier: '{nativeid}'");
            return new BasicCommand(Indent, Identifier, Parameters);*/
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
            bc.Indent = this.Indent;
            bc.Identifier = this.Identifier;
            bc.Parameters = new Dictionary<string, object>(this.Parameters);
            return bc;
        }
    }
}
