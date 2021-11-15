using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class EventPage
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
            this.TriggerMode = (TriggerMode)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@trigger"));
            this.Graphic = new EventGraphic(Ruby.GetIVar(data, "@graphic"));
            this.Condition = new EventCondition(Ruby.GetIVar(data, "@condition"));
            IntPtr list = Ruby.GetIVar(data, "@list");
            for (int i = 0; i < Ruby.Array.Length(list); i++)
            {
                IntPtr cmd = Ruby.Array.Get(list, i);
                this.Commands.Add(new EventCommand(cmd));
            }
        }

        public EventPage Clone()
        {
            throw new NotImplementedException();
            /*EventPage p = new EventPage();
            p.Name = this.Name;
            p.Commands = new List<BasicCommand>(this.Commands);
            p.Conditions = new List<BasicCondition>(this.Conditions);
            p.Graphic = this.Graphic.Clone();
            p.TriggerMode = this.TriggerMode;
            p.TriggerParam = this.TriggerParam;
            p.AutoMoveRoute = this.AutoMoveRoute.Clone();
            p.Settings = this.Settings.Clone();
            return p;*/
        }
    }
}
