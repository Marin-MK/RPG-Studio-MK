using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class MoveCommand
    {
        public int Code;
        public List<object> Parameters = new List<object>();

        public MoveCommand(IntPtr data)
        {
            this.Code = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@code"));
            IntPtr parameters = Ruby.GetIVar(data, "@parameters");
            for (int i = 0; i < Ruby.Array.Length(parameters); i++)
            {
                IntPtr obj = Ruby.Array.Get(parameters, i);
                this.Parameters.Add(obj);
            }
        }
    }
}
