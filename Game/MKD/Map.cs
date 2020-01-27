using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MKEditor.Game
{
    public class Map : ICloneable
    {
        public int ID;
        public string DevName;
        public string DisplayName;
        public int Width;
        public int Height;
        public List<Layer> Layers = new List<Layer>();
        public List<int> TilesetIDs = new List<int>();
        public Dictionary<int, Event> Events = new Dictionary<int, Event>();
        public Dictionary<string, List<Connection>> Connections = new Dictionary<string, List<Connection>>();

        public int SaveX = 0;
        public int SaveY = 0;

        // Used only for the editor to track which maps exist in the order list
        public bool Added = false;

        public Map() { }

        public Map(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "MKD::Map") throw new Exception("Invalid class - Expected class of type MKD::Map but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            this.ID = Convert.ToInt32(Data["@id"]);
            this.DevName = (string) Data["@dev_name"];
            this.DisplayName = (string) Data["@display_name"];
            this.Width = Convert.ToInt32(Data["@width"]);
            this.Height = Convert.ToInt32(Data["@height"]);
            foreach (object layer in ((JArray) Data["@tiles"]).ToObject<List<object>>())
            {
                Layer l = new Layer("Layer " + (Layers.Count + 1).ToString());
                foreach (object tile in ((JArray) layer).ToObject<List<object>>())
                {
                    if (tile == null) l.Tiles.Add(null);
                    else
                    {
                        List<object> tiledata = ((JArray) tile).ToObject<List<object>>();
                        int TilesetIndex = Convert.ToInt32(tiledata[0]);
                        int TileID = Convert.ToInt32(tiledata[1]);
                        l.Tiles.Add(new TileData() { TilesetIndex = TilesetIndex, TileID = TileID });
                    }
                }
                this.Layers.Add(l);
            }
            this.TilesetIDs = ((JArray) Data["@tilesets"]).ToObject<List<int>>();

            foreach(KeyValuePair<string, object> kvp in ((JObject) Data["@events"]).ToObject<Dictionary<string, object>>())
            {
                Event e = new Event(((JObject) kvp.Value).ToObject<Dictionary<string, object>>());
                this.Events[e.ID] = e;
            }

            this.Connections = new Dictionary<string, List<Connection>>();
            this.Connections.Add(":north", new List<Connection>());
            this.Connections.Add(":east", new List<Connection>());
            this.Connections.Add(":south", new List<Connection>());
            this.Connections.Add(":west", new List<Connection>());
            if (this.ID == 6)
                this.Connections[":west"].Add(new Connection(5, 5));
            if (this.ID == 5)
                this.Connections[":east"].Add(new Connection(-5, 6));
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::Map";
            Data["@id"] = ID;
            Data["@dev_name"] = DevName;
            Data["@display_name"] = DisplayName;
            Data["@width"] = Width;
            Data["@height"] = Height;
            List<List<object>> layers = new List<List<object>>();
            for (int i = 0; i < Layers.Count; i++)
            {
                layers.Add(Layers[i].ToJSON());
            }
            Data["@tiles"] = layers;
            Data["@tilesets"] = TilesetIDs;
            Dictionary<int, Dictionary<string, object>> events = new Dictionary<int, Dictionary<string, object>>();
            foreach (KeyValuePair<int, Event> kvp in Events)
            {
                events[kvp.Key] = kvp.Value.ToJSON();
            }
            Data["@events"] = events;
            Dictionary<string, List<List<int>>> connections = new Dictionary<string, List<List<int>>>();
            foreach (KeyValuePair<string, List<Connection>> kvp in Connections)
            {
                List<List<int>> l = new List<List<int>>();
                for (int i = 0; i < kvp.Value.Count; i++) l.Add(kvp.Value[i].ToJSON());
                connections[kvp.Key] = l;
            }
            Data["@connections"] = connections;
            return Data;
        }

        public void SetSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Layers = new List<Layer>() { new Layer("Layer 1") };
            for (int i = 0; i < width * height; i++) this.Layers[0].Tiles.Add(null);
            this.TilesetIDs = new List<int>() { 1 };
        }

        public override string ToString()
        {
            return this.DisplayName;
        }

        public void RemoveTileset(int TilesetID)
        {
            int idx = this.TilesetIDs.IndexOf(TilesetID);
            for (int i = 0; i < Layers.Count; i++)
            {
                for (int j = 0; j < Layers[i].Tiles.Count; j++)
                {
                    TileData tile = Layers[i].Tiles[j];
                    if (tile == null) continue;
                    if (tile.TilesetIndex == idx) Layers[i].Tiles[j] = null;
                }
            }
            this.TilesetIDs.Remove(TilesetID);
            for (int i = 0; i < Layers.Count; i++)
            {
                for (int j = 0; j < Layers[i].Tiles.Count; j++)
                {
                    TileData tile = Layers[i].Tiles[j];
                    if (tile == null) continue;
                    if (tile.TilesetIndex > idx) Layers[i].Tiles[j].TilesetIndex -= 1;
                }
            }
        }

        public object Clone()
        {
            Map o = new Map();
            o.ID = this.ID;
            o.DevName = this.DevName;
            o.DisplayName = this.DisplayName;
            o.Width = this.Width;
            o.Height = this.Height;
            o.Layers = new List<Layer>(this.Layers);
            o.TilesetIDs = new List<int>(this.TilesetIDs);
            o.Events = new Dictionary<int, Event>(this.Events);
            o.Connections = new Dictionary<string, List<Connection>>(this.Connections);
            return o;
        }
    }

    public class Layer
    {
        public string Name = "Unnamed Layer";
        public List<TileData> Tiles = new List<TileData>();
        public bool Visible = true;

        public Layer(string Name)
        {
            this.Name = Name;
        }

        public List<object> ToJSON()
        {
            List<object> Data = new List<object>();
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i] == null) Data.Add(null);
                else Data.Add(new List<object>() { Tiles[i].TilesetIndex, Tiles[i].TileID });
            }
            return Data;
        }
    }

    public class MapConnection
    {
        public List<Dictionary<List<int>, int>> Maps = new List<Dictionary<List<int>, int>>();

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::MapConnections";
            List<Dictionary<string, int>> maps = new List<Dictionary<string, int>>();
            foreach (Dictionary<List<int>, int> System in this.Maps)
            {
                Dictionary<string, int> NewSystem = new Dictionary<string, int>();
                foreach (KeyValuePair<List<int>, int> Connection in System)
                {
                    NewSystem.Add($"[{Connection.Key[0]}, {Connection.Key[1]}]", Connection.Value);
                }
                maps.Add(NewSystem);
            }
            Data["@maps"] = maps;
            return Data;
        }
    }

    public class Connection
    {
        public int Offset;
        public int MapID;

        public Connection(int Offset, int MapID)
        {
            this.Offset = Offset;
            this.MapID = MapID;
        }

        public List<int> ToJSON()
        {
            return new List<int>() { Offset, MapID };
        }
    }

    public class TileData
    {
        public int TilesetIndex;
        public int TileID;
    }

    public enum Passability
    {
        None = 0,
        Down = 1,
        Left = 2,
        Right = 4,
        Up = 8,
        All = Down | Left | Right | Up,

        DownLeft = Down | Left,
        DownRight = Down | Right,
        LeftRight = Left | Right,
        DownLeftRight = Down | Left | Right,
        DownUp = Down | Up,
        LeftUp = Left | Up,
        DownLeftUp = Down | Left | Up,
        RightUp = Right | Up,
        DownRightUp = Down | Right | Up,
        LeftRightUp = Left | Right | Up,
        DownLeftRightUp = Down | Left | Right | Up
    }
}
