using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game;

[Serializable]
public class MoveCommand : ICloneable
{
    public int Code;
    public List<object> Parameters = new List<object>();

    public MoveCommand()
    {

    }

    public MoveCommand(IntPtr data)
    {
        this.Code = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@code"));
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
        c.Parameters = (List<object>)Utilities.CloneUnknown(this.Parameters);
        return c;
    }
}
