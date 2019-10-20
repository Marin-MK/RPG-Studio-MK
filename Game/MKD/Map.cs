using System;
using System.Collections.Generic;

namespace MKEditor.Game
{
    public class Map : Serializable, ICloneable
    {
        public int ID;
        public string DevName;
        public string DisplayName;
        public int Width;
        public int Height;
        public List<Layer> Layers;
        public List<int> TilesetIDs;
        public Dictionary<int, Event> Events = new Dictionary<int, Event>();

        // Used only for the editor to track which maps exist in the order list
        public bool Added = false;

        public Map() { }

        public Map(string path)
            : base(path)
        {
            this.ID = GetVar<int>("id");
            this.DevName = GetVar<string>("name");
            this.DisplayName = this.DevName;
            this.Width = GetVar<int>("width");
            this.Height = GetVar<int>("height");
            this.Layers = new List<Layer>();
            int layercount = GetCount("tiles");
            for (int i = 0; i < layercount; i++)
            {
                this.Layers.Insert(i, new Layer($"Layer {i + 1}"));
                int tilecount = GetCount($"tiles[{i}]");
                for (int j = 0; j < tilecount; j++)
                {
                    List<object> tile = GetList<object>($"tiles[{i}][{j}]");
                    if (tile == null)
                    {
                        this.Layers[i].Tiles.Insert(j, null);
                    }
                    else
                    {
                        TileData data = new TileData();
                        data.TilesetIndex = (int) tile[0];
                        data.TileID = (int) tile[1];
                        this.Layers[i].Tiles.Insert(j, data);
                    }
                }
            }
            this.TilesetIDs = GetList<int>("tilesets");
            List<int> eventkeys = GetKeys<int>("events");
            foreach (int eventid in eventkeys)
            {
                this.Events[eventid] = new Event(GetPath($"events[{eventid}]"));
                this.Events[eventid].ID = eventid;
            }
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
            return this.DevName;
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
