using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class EventCommand
    {
        public int Indent;
        public int Code;
        public List<object> Parameters = new List<object>();

        public EventCommand(IntPtr data)
        {
            this.Indent = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@indent"));
            this.Code = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@code"));
            IntPtr parameters = Ruby.GetIVar(data, "@parameters");
            for (int i = 0; i < Ruby.Array.Length(parameters); i++)
            {
                IntPtr param = Ruby.Array.Get(parameters, i);
                object obj = Utilities.RubyToNative(param);
                this.Parameters.Add(obj);
            }
        }
    }
}
