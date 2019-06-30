using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor.Data
{
    public class Event : Serializable
    {
        public int ID;
        public string Name;
        public int X;
        public int Y;
        public List<EventPage> Pages = new List<EventPage>();
        public EventSettings Settings;

        public Event(string path)
            : base(path)
        {
            this.Name = GetVar<string>("name");
            this.X = GetVar<int>("x");
            this.Y = GetVar<int>("y");
            int pagecount = GetCount("pages");
            for (int i = 0; i < pagecount; i++)
            {
                this.Pages.Add(new EventPage(GetPath($"pages[{i}]")));
            }
            this.Settings = new EventSettings(GetPath("settings"));
        }
    }

    public class EventPage : Serializable
    {
        public List<IEventCommand> Commands { get { throw new NotImplementedException(); } }
        public List<IEventCondition> Conditions { get { throw new NotImplementedException(); } }
        public EventGraphic Graphic;
        public List<EventTrigger> Triggers = new List<EventTrigger>();
        public AutoMoveRoute AutoMoveRoute;

        public EventPage(string path)
            : base(path)
        {
            this.Graphic = new EventGraphic(GetPath("graphic"));
            int triggercount = GetCount("triggers");
            for (int i = 0; i < triggercount; i++)
            {
                this.Triggers.Add(new EventTrigger(GetPath($"triggers[{i}]")));
            }
            this.AutoMoveRoute = new AutoMoveRoute(GetPath("automoveroute"));
        }
    }

    public class EventGraphic : Serializable
    {
        public string Type;
        public object Param;
        public int Direction;

        public EventGraphic(string path)
            : base(path)
        {
            this.Type = GetVar<string>("type", VariableType.HashSymbol);
            if (!Nil("param", VariableType.HashSymbol)) this.Param = GetVar<string>("param", VariableType.HashSymbol);
            this.Direction = GetVar<int>("direction", VariableType.HashSymbol);
        }
    }

    public class EventTrigger : Serializable
    {
        public string Type;
        public object Param;

        public EventTrigger(string path)
            : base(path)
        {
            this.Type = GetVar<string>("0", VariableType.ArrayElement);
            bool hasparam = (bool) GameData.Exec($"!{GetPath("1", VariableType.ArrayElement)}.nil?");
            if (hasparam)
            {
                this.Param = GetVar<object>("1", VariableType.ArrayElement);
            }
        }
    }

    public class AutoMoveRoute: Serializable
    {
        public int Frequency;
        public List<string> Commands;

        public AutoMoveRoute(string path)
            : base(path)
        {
            this.Frequency = GetVar<int>("frequency", VariableType.HashSymbol);
            this.Commands = GetList<string>("commands", VariableType.HashSymbol);
        }
    }

    public class EventSettings : Serializable
    {
        public int Priority;
        public bool Passable;
        public bool CanStartSurfingHere;
        public bool ResetPositionOnTransfer;
        public float Speed;

        public EventSettings(string path)
            : base(path)
        {
            this.Priority = GetVar<int>("priority");
            this.Passable = GetVar<bool>("passable");
            this.CanStartSurfingHere = GetVar<bool>("can_start_surfing_here");
            this.ResetPositionOnTransfer = GetVar<bool>("reset_position_on_transfer");
            this.Speed = GetVar<float>("speed");
        }
    }
}
