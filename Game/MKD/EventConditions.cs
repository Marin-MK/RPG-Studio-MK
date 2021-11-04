using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RPGStudioMK.Game
{
    public class BasicCondition
    {
        public string Type;
        public string Identifier;
        public Dictionary<string, object> Parameters;

        public BasicCondition()
        {

        }

        public BasicCondition(string Type, string Identifier, Dictionary<string, object> Parameters)
        {
            this.Type = Type;
            this.Identifier = Identifier;
            this.Parameters = Parameters;
        }

        public static BasicCondition IDToCondition(string Identifier, Dictionary<string, object> Parameters)
        {
            throw new NotImplementedException("Legacy event conditions");
            /*string nativeid = Identifier.Substring(1);
            ConditionType type = ConditionParser.Types.Find(t => t.Identifier == nativeid);
            if (type == null) throw new Exception($"Invalid condition identifier: '{nativeid}'");
            return new BasicCondition(type, Identifier, Parameters);*/
        }

        public List<object> ToJSON()
        {
            List<object> Data = new List<object>();
            Data.Add(Identifier);
            Data.Add(Parameters);
            return Data;
        }

        public BasicCondition Clone()
        {
            BasicCondition bc = new BasicCondition();
            bc.Type = this.Type;
            bc.Identifier = this.Identifier;
            bc.Parameters = new Dictionary<string, object>(this.Parameters);
            return bc;
        }
    }
}
