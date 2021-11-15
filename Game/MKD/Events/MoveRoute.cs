using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class MoveRoute
    {
        public int Type;
        public int Frequency;
        public int Speed;
        public bool Skippable;
        public bool Repeat;
        public List<MoveCommand> Commands = new List<MoveCommand>();

        public MoveRoute()
        {
            this.Type = 0;
            this.Frequency = 0;
            this.Speed = 0;
            this.Skippable = false;
            this.Repeat = true;
        }

        public MoveRoute(IntPtr data)
        {
            this.Skippable = Ruby.GetIVar(data, "@skippable") == Ruby.True;
            this.Repeat = Ruby.GetIVar(data, "@repeat") == Ruby.True;
            IntPtr list = Ruby.GetIVar(data, "@list");
            for (int i = 0; i < Ruby.Array.Length(list); i++)
            {
                IntPtr cmd = Ruby.Array.Get(list, i);
                MoveCommand movecmd = new MoveCommand(cmd);
                this.Commands.Add(movecmd);
            }
        }

        public MoveRoute Clone()
        {
            throw new NotImplementedException();
            /*MoveRoute amr = new MoveRoute();
            amr.Type = this.Type;
            amr.Frequency = this.Frequency;
            amr.Commands = new List<MoveCommand>(this.Commands);
            return amr;*/
        }
    }
}
