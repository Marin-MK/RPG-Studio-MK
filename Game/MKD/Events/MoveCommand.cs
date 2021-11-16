using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class MoveCommand : ICloneable
    {
        public int Code;
        public List<object> Parameters = new List<object>();

        public MoveCommand()
        {

        }

        public MoveCommand(IntPtr data)
        {
            this.Code = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@code"));
            IntPtr parameters = Ruby.GetIVar(data, "@parameters");
            for (int i = 0; i < Ruby.Array.Length(parameters); i++)
            {
                IntPtr param = Ruby.Array.Get(parameters, i);
                object obj = Utilities.RubyToNative(param);
                this.Parameters.Add(obj);
            }
        }

        public IntPtr Save()
        {
            IntPtr cmd = Ruby.Funcall(Compatibility.RMXP.MoveCommand.Class, "new");
            Ruby.Pin(cmd);
            Ruby.SetIVar(cmd, "@code", Ruby.Integer.ToPtr(Code));
            IntPtr parameters = Ruby.Array.Create();
            Ruby.SetIVar(cmd, "@parameters", parameters);
            for (int i = 0; i < this.Parameters.Count; i++)
            {
                IntPtr param = Utilities.NativeToRuby(this.Parameters[i]);
                Ruby.Array.Set(parameters, i, param);
            }
            Ruby.Unpin(cmd);
            return cmd;
        }

        public object Clone()
        {
            MoveCommand c = new MoveCommand();
            c.Code = this.Code;
            c.Parameters = new List<object>();
            this.Parameters.ForEach(p =>
            {
                if (p is int || p is long || p is string || p is true || p is false || p is null) c.Parameters.Add(p);
                else if (p is ICloneable) c.Parameters.Add(((ICloneable) p).Clone());
                else throw new Exception("Uncloneable object: " + p.GetType().ToString());
            });
            return c;
        }
    }
}
