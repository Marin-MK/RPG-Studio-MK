using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor.Data
{
    public class Map : Serializable
    {
        public int ID;
        public string Name;
        public int Width;
        public int Height;
        public List<Layer> Layers;
        public List<int> TilesetIDs;
        public Dictionary<int, Event> Events = new Dictionary<int, Event>();

        public Map() { }

        public Map(string path)
            : base(path)
        {
            this.ID = GetVar<int>("id");
            this.Name = GetVar<string>("name");
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

        public override string ToString()
        {
            return this.Name;
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
