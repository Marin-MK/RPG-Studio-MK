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
        public int Width;
        public int Height;
        public List<EventPage> Pages = new List<EventPage>();
        public EventSettings Settings;

        public Event(int ID)
        {
            this.ID = ID;
            this.Name = "Untitled Event";
            this.Width = 1;
            this.Height = 1;
            this.Pages = new List<EventPage>();
            this.Pages.Add(new EventPage());
            this.Settings = new EventSettings();
        }

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
            this.Width = Convert.ToInt32(Data["@width"]);
            this.Height = Convert.ToInt32(Data["@height"]);
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
            Data["@width"] = Width;
            Data["@height"] = Height;
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
        public List<IEventCommand> Commands;
        public List<IEventCondition> Conditions;
        public EventGraphic Graphic;
        public TriggerMode TriggerMode;
        public object TriggerParam;
        public AutoMoveRoute AutoMoveRoute;

        public EventPage()
        {
            this.Commands = new List<IEventCommand>();
            this.Conditions = new List<IEventCondition>();
            this.Graphic = new EventGraphic();
            this.TriggerMode = TriggerMode.Action;
            this.AutoMoveRoute = new AutoMoveRoute();
        }

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
            string mode = (string) Data["@trigger_mode"];
            if (mode == ":action") TriggerMode = TriggerMode.Action;
            else if (mode == ":player_touch") TriggerMode = TriggerMode.PlayerTouch;
            else if (mode == ":event_touch") TriggerMode = TriggerMode.EventTouch;
            else if (mode == ":autorun") TriggerMode = TriggerMode.Autorun;
            else if (mode == ":parallel_process") TriggerMode = TriggerMode.ParallelProcess;
            this.TriggerParam = Data["@trigger_param"];
            this.AutoMoveRoute = new AutoMoveRoute(((JObject) Data["@automoveroute"]).ToObject<Dictionary<string, object>>());
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::Event::Page";
            Data["@graphic"] = Graphic.ToJSON();
            Data["@automoveroute"] = AutoMoveRoute.ToJSON();
            string TriggerString = null;
            if (TriggerMode == TriggerMode.Action) TriggerString = ":action";
            else if (TriggerMode == TriggerMode.PlayerTouch) TriggerString = ":player_touch";
            else if (TriggerMode == TriggerMode.EventTouch) TriggerString = ":event_touch";
            else if (TriggerMode == TriggerMode.Autorun) TriggerString = ":autorun";
            else if (TriggerMode == TriggerMode.ParallelProcess) TriggerString = ":parallel_process";
            Data["@trigger_mode"] = TriggerString;
            Data["@trigger_param"] = TriggerParam;
            Data["@commands"] = new List<string>();
            Data["@conditions"] = new List<string>();
            // Add commands
            // Add conditions
            return Data;
        }
    }

    public enum TriggerMode
    {
        Action,
        PlayerTouch,
        EventTouch,
        Autorun,
        ParallelProcess
    }

    public class EventGraphic
    {
        public string Type;
        public object Param;
        public int Direction;
        public int FrameUpdateInterval;

        public EventGraphic()
        {
            this.Type = "blank";
            this.Direction = 2;
            this.FrameUpdateInterval = 16;
        }

        public EventGraphic(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "MKD::Event::Graphic") throw new Exception("Invalid class - Expected class of type MKD::Event::Graphic but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            this.Type = ((string) Data["@type"]).Replace(":", "");
            this.Direction = Convert.ToInt32(Data["@direction"]);
            if (Data["@param"] is int) this.Param = Convert.ToInt32(Data["@param"]);
            else if (Data["@param"] is string) this.Param = (string) Data["@param"];
            else this.Param = Data["@param"];
            this.FrameUpdateInterval = Convert.ToInt32(Data["@frame_update_interval"]);
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::Event::Graphic";
            Data["@type"] = ":" + Type;
            Data["@direction"] = Direction;
            Data["@param"] = Param;
            Data["@frame_update_interval"] = FrameUpdateInterval;
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
        public double Frequency;
        public List<string> Commands;

        public AutoMoveRoute()
        {
            this.Frequency = 0;
            this.Commands = new List<string>();
        }

        public AutoMoveRoute(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "MKD::Event::AutoMoveRoute") throw new Exception("Invalid class - Expected class of type MKD::Event::AutoMoveRoute but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            this.Frequency = Convert.ToDouble(Data["@frequency"]);
            foreach (object o in ((JArray) Data["@commands"]).ToObject<List<object>>())
            {
                this.Commands.Add(((string) o).Replace(":", ""));
            }
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::Event::AutoMoveRoute";
            Data["@frequency"] = Frequency;
            List<string> commands = new List<string>();
            foreach (string command in Commands) commands.Add(":" + command);
            Data["@commands"] = commands;
            return Data;
        }
    }

    public class EventSettings
    {
        public bool Passable;
        public bool SavePosition;
        public float Speed;

        public EventSettings()
        {
            this.Passable = false;
            this.SavePosition = true;
            this.Speed = 0.25f;
        }

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
            this.Passable = (bool) Data["@passable"];
            this.SavePosition = (bool) Data["@save_position"];
            this.Speed = (float) Convert.ToDouble(Data["@speed"]);
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::Event::Settings";
            Data["@passable"] = Passable;
            Data["@save_position"] = SavePosition;
            Data["@speed"] = Speed;
            return Data;
        }
    }
}
