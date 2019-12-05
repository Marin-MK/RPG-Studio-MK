using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MKEditor.Game
{
    public class Event
    {
        public int ID;
        public string Name;
        public int X;
        public int Y;
        public List<EventPage> Pages = new List<EventPage>();
        public EventSettings Settings;

        public Event(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "MKD::Event") throw new Exception("Invalid class - Expected class of type MKD::Event but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            this.ID = Convert.ToInt32(Data["@id"]);
            this.Name = (string) Data["@name"];
            this.X = Convert.ToInt32(Data["@x"]);
            this.Y = Convert.ToInt32(Data["@y"]);
            foreach (object o in ((JArray) Data["@pages"]).ToObject<List<object>>())
            {
                this.Pages.Add(new EventPage(((JObject) o).ToObject<Dictionary<string, object>>()));
            }
            this.Settings = new EventSettings(((JObject) Data["@settings"]).ToObject<Dictionary<string, object>>());
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::Event";
            Data["@id"] = ID;
            Data["@name"] = Name;
            Data["@x"] = X;
            Data["@y"] = Y;
            List<Dictionary<string, object>> pages = new List<Dictionary<string, object>>();
            foreach (EventPage page in Pages)
            {
                pages.Add(page.ToJSON());
            }
            Data["@pages"] = pages;
            Data["@settings"] = Settings.ToJSON();
            return Data;
        }
    }

    public class EventPage
    {
        public List<IEventCommand> Commands { get { throw new NotImplementedException(); } }
        public List<IEventCondition> Conditions { get { throw new NotImplementedException(); } }
        public EventGraphic Graphic;
        public List<EventTrigger> Triggers = new List<EventTrigger>();
        public AutoMoveRoute AutoMoveRoute;

        public EventPage(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "MKD::Event::Page") throw new Exception("Invalid class - Expected class of type MKD::Event::Page but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            foreach (object o in ((JArray) Data["@commands"]).ToObject<List<object>>())
            {
                // o is [0, :cmd, data] as JArray
            }
            foreach (object o in ((JArray) Data["@conditions"]).ToObject<List<object>>())
            {
                // o is [0, :cmd, data] as JArray
            }
            this.Graphic = new EventGraphic(((JObject) Data["@graphic"]).ToObject<Dictionary<string, object>>());
            foreach (object o in ((JArray) Data["@triggers"]).ToObject<List<object>>())
            {
                this.Triggers.Add(new EventTrigger(((JArray) o).ToObject<List<object>>()));
            }
            this.AutoMoveRoute = new AutoMoveRoute(((JObject) Data["@automoveroute"]).ToObject<Dictionary<string, object>>());
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::Event::Page";
            Data["@graphic"] = Graphic.ToJSON();
            Data["@automoveroute"] = AutoMoveRoute.ToJSON();
            List<List<object>> triggers = new List<List<object>>();
            foreach (EventTrigger trigger in Triggers) triggers.Add(trigger.ToJSON());
            Data["@triggers"] = triggers;
            Data["@commands"] = new List<string>();
            Data["@conditions"] = new List<string>();
            // Add commands
            // Add conditions
            return Data;
        }
    }

    public class EventGraphic
    {
        public string Type;
        public object Param;
        public int Direction;

        public EventGraphic(Dictionary<string, object> Data)
        {
            this.Type = ((string) Data[":type"]).Replace(":", "");
            this.Direction = Convert.ToInt32(Data[":direction"]);
            if (Data[":param"] is int) this.Param = Convert.ToInt32(Data[":param"]);
            else if (Data[":param"] is string) this.Param = (string) Data[":param"];
            else this.Param = Data[":param"];
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data[":type"] = ":" + Type;
            Data[":direction"] = Direction;
            Data[":param"] = Param;
            return Data;
        }
    }

    public class EventTrigger
    {
        public string Type;
        public object Param;

        public EventTrigger(List<object> Data)
        {
            this.Type = ((string) Data[0]).Replace(":", "");
            if (Data.Count > 1)
            {
                throw new NotImplementedException();
            }
        }

        public List<object> ToJSON()
        {
            List<object> Data = new List<object>();
            Data.Add(":" + Type);
            return Data;
        }
    }

    public class AutoMoveRoute
    {
        public int Frequency;
        public List<string> Commands = new List<string>();

        public AutoMoveRoute(Dictionary<string, object> Data)
        {
            this.Frequency = Convert.ToInt32(Data[":frequency"]);
            foreach (object o in ((JArray) Data[":commands"]).ToObject<List<object>>())
            {
                this.Commands.Add(((string) o).Replace(":", ""));
            }
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data[":frequency"] = Frequency;
            List<string> commands = new List<string>();
            foreach (string command in Commands) commands.Add(":" + command);
            Data[":commands"] = commands;
            return Data;
        }
    }

    public class EventSettings
    {
        public int Priority;
        public bool Passable;
        public bool CanStartSurfingHere;
        public bool ResetPositionOnTransfer;
        public float Speed;

        public EventSettings(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "MKD::Event::Settings") throw new Exception("Invalid class - Expected class of type MKD::Event::Settings but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            this.Priority = Convert.ToInt32(Data["@priority"]);
            this.Passable = (bool) Data["@passable"];
            this.CanStartSurfingHere = (bool) Data["@can_start_surfing_here"];
            this.ResetPositionOnTransfer = (bool) Data["@reset_position_on_transfer"];
            this.Speed = (float) Convert.ToDouble(Data["@speed"]);
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::Event::Settings";
            Data["@priority"] = Priority;
            Data["@passable"] = Passable;
            Data["@can_start_surfing_here"] = CanStartSurfingHere;
            Data["@reset_position_on_transfer"] = ResetPositionOnTransfer;
            Data["@speed"] = Speed;
            return Data;
        }
    }
}
