using System;
using System.Collections.Generic;

namespace RPGStudioMK.Game;

[Serializable]
public class MoveRoute : ICloneable
{
    public int Type;
    public int Frequency;
    public int Speed;
    public bool Skippable;
    public bool Repeat;
    public List<MoveCommand> Commands;

    public MoveRoute()
    {
        this.Type = 0;
        this.Frequency = 3;
        this.Speed = 3;
        this.Skippable = false;
        this.Repeat = false;
        this.Commands = new List<MoveCommand>() { new MoveCommand(MoveCode.None, new List<object>()) };
    }

    public MoveRoute(IntPtr data)
    {
        this.Skippable = Ruby.GetIVar(data, "@skippable") == Ruby.True;
        this.Repeat = Ruby.GetIVar(data, "@repeat") == Ruby.True;
        IntPtr list = Ruby.GetIVar(data, "@list");
        this.Commands = new List<MoveCommand>();
        for (int i = 0; i < Ruby.Array.Length(list); i++)
        {
            IntPtr cmd = Ruby.Array.Get(list, i);
            MoveCommand movecmd = new MoveCommand(cmd);
            this.Commands.Add(movecmd);
        }
    }

    public IntPtr Save()
    {
        IntPtr moveroute = Ruby.Funcall(Compatibility.RMXP.MoveRoute.Class, "new");
        Ruby.Pin(moveroute);
        Ruby.SetIVar(moveroute, "@skippable", this.Skippable ? Ruby.True : Ruby.False);
        Ruby.SetIVar(moveroute, "@repeat", this.Repeat ? Ruby.True : Ruby.False);
        IntPtr list = Ruby.Array.Create();
        Ruby.SetIVar(moveroute, "@list", list);
        for (int i = 0; i < this.Commands.Count; i++)
        {
            IntPtr cmd = this.Commands[i].Save();
            Ruby.Array.Set(list, i, cmd);
        }
        Ruby.Unpin(moveroute);
        return moveroute;
    }

    public object Clone()
    {
        MoveRoute r = new MoveRoute();
        r.Type = this.Type;
        r.Frequency = this.Frequency;
        r.Speed = this.Speed;
        r.Skippable = this.Skippable;
        r.Repeat = this.Repeat;
        r.Commands = new List<MoveCommand>();
        this.Commands.ForEach(c => r.Commands.Add((MoveCommand)c.Clone()));
        return r;
    }
}
