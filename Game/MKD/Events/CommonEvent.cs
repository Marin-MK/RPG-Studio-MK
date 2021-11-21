using System;
using System.Collections.Generic;
using System.Text;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class CommonEvent
    {
        public int ID;
        public string Name;
        public int Trigger;
        public int SwitchID;
        public List<EventCommand> List = new List<EventCommand>();

        public CommonEvent()
        {

        }

        public CommonEvent(IntPtr data)
        {
            this.ID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@id"));
            this.Trigger = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@trigger"));
            this.SwitchID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@switch_id"));
            this.Name = Ruby.String.FromPtr(Ruby.GetIVar(data, "@name"));

            IntPtr list = Ruby.GetIVar(data, "@list");
            for (int i = 0; i < Ruby.Array.Length(list); i++)
            {
                EventCommand c = new EventCommand(Ruby.Array.Get(list, i));
                this.List.Add(c);
            }
        }

        public IntPtr Save()
        {
            IntPtr c = Ruby.Funcall(Compatibility.RMXP.CommonEvent.Class, "new");
            Ruby.SetIVar(c, "@id", Ruby.Integer.ToPtr(this.ID));
            Ruby.SetIVar(c, "@switch_id", Ruby.Integer.ToPtr(this.SwitchID));
            Ruby.SetIVar(c, "@trigger", Ruby.Integer.ToPtr(this.Trigger));
            Ruby.SetIVar(c, "@name", Ruby.String.ToPtr(this.Name));

            IntPtr list = Ruby.Array.Create();
            Ruby.SetIVar(c, "@list", list);
            for (int i = 0; i < this.List.Count; i++)
            {
                IntPtr cmd = this.List[i].Save();
                Ruby.Array.Set(list, i, cmd);
            }

            return c;
        }
    }
}
