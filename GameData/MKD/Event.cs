using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor.Data
{
    public class Event
    {
        public int ID;
        public string Name;
        public int X;
        public int Y;
        public List<EventPage> Pages;
        public EventSettings Settings;
    }

    public class EventPage
    {
        public List<IEventCommand> Commands;
        public List<IEventCondition> Conditions;
        public EventGraphic Graphics;
        public List<string> Triggers;
        public AutoMoveRoute AutoMoveRoute;
    }

    public class EventGraphic
    {
        public string Type;
        public object Param;
    }

    public class AutoMoveRoute
    {
        public int Frequency;
        public List<object> Commands;
    }

    public class EventSettings
    {
        public int Priority;
        public bool Passable;
        public bool CanStartSurfingHere;
        public bool ResetPositionOnTransfer;
        public float Speed;
    }
}
