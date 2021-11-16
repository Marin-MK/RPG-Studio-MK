using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class EventPage : ICloneable
    {
        public string Name;
        public List<EventCommand> Commands = new List<EventCommand>();
        public EventCondition Condition;
        public EventGraphic Graphic;
        public TriggerMode TriggerMode;
        public MoveRoute MoveRoute;
        public EventSettings Settings;

        public EventPage()
        {
            this.Name = "Untitled Page";
            this.Graphic = new EventGraphic();
            this.TriggerMode = TriggerMode.Action;
            this.MoveRoute = new MoveRoute();
            this.Settings = new EventSettings();
        }

        public EventPage(IntPtr data)
        {
            this.Settings = new EventSettings();
            this.Settings.DirectionFix = Ruby.GetIVar(data, "@direction_fix") == Ruby.True;
            this.Settings.StepAnime = Ruby.GetIVar(data, "@step_anime") == Ruby.True;
            this.Settings.AlwaysOnTop = Ruby.GetIVar(data, "@always_on_top") == Ruby.True;
            this.Settings.WalkAnime = Ruby.GetIVar(data, "@walk_anime") == Ruby.True;
            this.Settings.Through = Ruby.GetIVar(data, "@through") == Ruby.True;
            this.MoveRoute = new MoveRoute(Ruby.GetIVar(data, "@move_route"));
            this.MoveRoute.Type = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@move_type"));
            this.MoveRoute.Frequency = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@move_frequency"));
            this.MoveRoute.Speed = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@move_speed"));
            this.TriggerMode = (TriggerMode) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@trigger"));
            this.Graphic = new EventGraphic(Ruby.GetIVar(data, "@graphic"));
            this.Condition = new EventCondition(Ruby.GetIVar(data, "@condition"));
            IntPtr list = Ruby.GetIVar(data, "@list");
            for (int i = 0; i < Ruby.Array.Length(list); i++)
            {
                IntPtr cmd = Ruby.Array.Get(list, i);
                this.Commands.Add(new EventCommand(cmd));
            }
        }

        public IntPtr Save()
        {
            IntPtr page = Ruby.Funcall(Compatibility.RMXP.Page.Class, "new");
            Ruby.Pin(page);
            Ruby.SetIVar(page, "@direction_fix", this.Settings.DirectionFix ? Ruby.True : Ruby.False);
            Ruby.SetIVar(page, "@step_anime", this.Settings.StepAnime ? Ruby.True : Ruby.False);
            Ruby.SetIVar(page, "@always_on_top", this.Settings.AlwaysOnTop ? Ruby.True : Ruby.False);
            Ruby.SetIVar(page, "@walk_anime", this.Settings.WalkAnime ? Ruby.True : Ruby.False);
            Ruby.SetIVar(page, "@through", this.Settings.Through ? Ruby.True : Ruby.False);
            Ruby.SetIVar(page, "@trigger", Ruby.Integer.ToPtr((int) this.TriggerMode));
            Ruby.SetIVar(page, "@move_type", Ruby.Integer.ToPtr(this.MoveRoute.Type));
            Ruby.SetIVar(page, "@move_frequency", Ruby.Integer.ToPtr(this.MoveRoute.Frequency));
            Ruby.SetIVar(page, "@move_speed", Ruby.Integer.ToPtr(this.MoveRoute.Speed));
            Ruby.SetIVar(page, "@move_route", this.MoveRoute.Save());
            Ruby.SetIVar(page, "@condition", this.Condition.Save());
            Ruby.SetIVar(page, "@graphic", this.Graphic.Save());
            IntPtr list = Ruby.Array.Create();
            Ruby.SetIVar(page, "@list", list);
            for (int i = 0; i < this.Commands.Count; i++)
            {
                IntPtr cmd = this.Commands[i].Save();
                Ruby.Array.Set(list, i, cmd);
            }
            Ruby.Unpin(page);
            return page;
        }

        public object Clone()
        {
            EventPage p = new EventPage();
            p.Name = this.Name;
            p.Condition = (EventCondition) this.Condition.Clone();
            p.Graphic = (EventGraphic) this.Graphic.Clone();
            p.TriggerMode = this.TriggerMode;
            p.MoveRoute = (MoveRoute) this.MoveRoute.Clone();
            p.Settings = (EventSettings) this.Settings.Clone();
            p.Commands = new List<EventCommand>();
            this.Commands.ForEach(c => p.Commands.Add((EventCommand) c.Clone()));
            return p;
        }
    }
}
