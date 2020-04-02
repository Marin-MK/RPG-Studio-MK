using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MKEditor.Game
{
    public class Map
    {
        public int ID;
        public string DevName;
        public string DisplayName;
        public int Width;
        public int Height;
        public List<Layer> Layers = new List<Layer>();
        public List<int> TilesetIDs = new List<int>();
        public List<int> AutotileIDs = new List<int>();
        public Dictionary<int, Event> Events = new Dictionary<int, Event>();
        public List<MapConnection> Connections = new List<MapConnection>();

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
                        if (tiledata.Count == 2) tiledata.Insert(0, 0);
                        int TileType = Convert.ToInt32(tiledata[0]);
                        int Index = Convert.ToInt32(tiledata[1]);
                        int ID = Convert.ToInt32(tiledata[2]);
                        l.Tiles.Add(new TileData() { TileType = (TileType) TileType, Index = Index, ID = ID });
                    }
                }
                this.Layers.Add(l);
            }
            this.TilesetIDs = ((JArray) Data["@tilesets"]).ToObject<List<int>>();
            if (Data.ContainsKey("@autotiles")) this.AutotileIDs = ((JArray) Data["@autotiles"]).ToObject<List<int>>();

            foreach(KeyValuePair<string, object> kvp in ((JObject) Data["@events"]).ToObject<Dictionary<string, object>>())
            {
                Event e = new Event(((JObject) kvp.Value).ToObject<Dictionary<string, object>>());
                this.Events[e.ID] = e;
            }

            this.Connections = new List<MapConnection>();
            foreach (object conn in ((JArray) Data["@connections"]).ToObject<List<object>>())
            {
                MapConnection c = new MapConnection(((JObject) conn).ToObject<Dictionary<string, object>>());
                this.Connections.Add(c);
            }
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
            Data["@autotiles"] = AutotileIDs;
            Dictionary<int, Dictionary<string, object>> events = new Dictionary<int, Dictionary<string, object>>();
            foreach (KeyValuePair<int, Event> kvp in Events)
            {
                events[kvp.Key] = kvp.Value.ToJSON();
            }
            Data["@events"] = events;
            List<object> connections = new List<object>();
            foreach (MapConnection c in this.Connections)
            {
                connections.Add(c.ToJSON());
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

        public void Resize(int OldWidth, int NewWidth, int OldHeight, int NewHeight)
        {
            int diffw = NewWidth - OldWidth;
            bool diffwneg = diffw < 0;
            diffw = Math.Abs(diffw);
            int diffh = NewHeight - OldHeight;
            bool diffhneg = diffh < 0;
            diffh = Math.Abs(diffh);
            for (int layer = 0; layer < this.Layers.Count; layer++)
            {
                for (int y = 0; y < OldHeight; y++)
                {
                    for (int i = 0; i < diffw; i++)
                    {
                        if (diffwneg) this.Layers[layer].Tiles.RemoveAt(y * NewWidth + NewWidth);
                        else this.Layers[layer].Tiles.Insert(y * NewWidth + OldWidth, null);
                    }
                }
            }
            for (int layer = 0; layer < this.Layers.Count; layer++)
            {
                if (diffhneg) this.Layers[layer].Tiles.RemoveRange(NewWidth * NewHeight, diffh * NewWidth);
                else
                {
                    for (int i = 0; i < diffh * NewWidth; i++)
                    {
                        this.Layers[layer].Tiles.Add(null);
                    }
                }
            }
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
                    if (tile.TileType == TileType.Tileset && tile.Index == idx) Layers[i].Tiles[j] = null;
                }
            }
            this.TilesetIDs.Remove(TilesetID);
            for (int i = 0; i < Layers.Count; i++)
            {
                for (int j = 0; j < Layers[i].Tiles.Count; j++)
                {
                    TileData tile = Layers[i].Tiles[j];
                    if (tile == null) continue;
                    if (tile.TileType == TileType.Tileset && tile.Index > idx) Layers[i].Tiles[j].Index -= 1;
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
            o.AutotileIDs = new List<int>(this.AutotileIDs);
            o.Events = new Dictionary<int, Event>(this.Events);
            o.Connections = new List<MapConnection>(this.Connections);
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

        public object Clone()
        {
            Layer l = new Layer(this.Name);
            l.Tiles = new List<TileData>(Tiles);
            l.Visible = Visible;
            return l;
        }

        public List<object> ToJSON()
        {
            List<object> Data = new List<object>();
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i] == null) Data.Add(null);
                else Data.Add(new List<object>() { Convert.ToInt32(Tiles[i].TileType), Tiles[i].Index, Tiles[i].ID });
            }
            return Data;
        }

        
    }

    public enum TileType
    {
        Tileset = 0,
        Autotile = 1
    }

    // MKD Data
    public class MapConnection
    {
        public int MapID;
        public int RelativeX;
        public int RelativeY;

        public MapConnection(int MapID, int RelativeX, int RelativeY)
        {
            this.MapID = MapID;
            this.RelativeX = RelativeX;
            this.RelativeY = RelativeY;
        }

        public MapConnection(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "MKD::MapConnection") throw new Exception("Invalid class - Expected class of type MKD::MapConnection but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            this.MapID = Convert.ToInt32(Data["@map_id"]);
            this.RelativeX = Convert.ToInt32(Data["@relative_x"]);
            this.RelativeY = Convert.ToInt32(Data["@relative_y"]);
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::MapConnection";
            Data["@map_id"] = this.MapID;
            Data["@relative_x"] = this.RelativeX;
            Data["@relative_y"] = this.RelativeY;
            return Data;
        }
    }

    public class TileData
    {
        public TileType TileType;
        public int Index;
        public int ID;


        public TileData Clone()
        {
            TileData o = new TileData();
            o.TileType = this.TileType;
            o.Index = this.Index;
            o.ID = this.ID;
            return o;
        }
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
