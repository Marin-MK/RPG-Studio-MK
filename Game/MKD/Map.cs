using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class Map
    {
        public int ID;
        public string Name;
        public int Width;
        public int Height;
        public List<Layer> Layers = new List<Layer>();
        public List<int> TilesetIDs = new List<int>();
        public List<int> AutotileIDs = new List<int>();
        public Dictionary<int, Event> Events = new Dictionary<int, Event>();

        // RMXP MapInfo Properties
        public int ScrollX;
        public int ScrollY;
        public bool Expanded;
        public int Order;
        public int ParentID;

        // RMXP Map Properties
        public string BGMName;
        public int BGMVolume;
        public int BGMPitch;
        public bool AutoplayBGM;
        public string BGSName;
        public int BGSVolume;
        public int BGSPitch;
        public bool AutoplayBGS;
        public int EncounterStep;

        public int SaveX = 0;
        public int SaveY = 0;

        // Used only for the editor to track which maps exist in the order list
        public bool Added = false;

        public Map() 
        {
            
        }

        public Map(int ID, IntPtr data, IntPtr mapinfo)
        {
            this.ID = ID;
            this.Name = Ruby.String.FromPtr(Ruby.GetIVar(mapinfo, "@name"));
            this.ScrollX = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(mapinfo, "@scroll_x"));
            this.ScrollY = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(mapinfo, "@scroll_y"));
            this.Order = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(mapinfo, "@order"));
            this.ParentID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(mapinfo, "@parent_id"));
            this.Expanded = Ruby.GetIVar(mapinfo, "@expanded") == Ruby.True;

            int tilesetid = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@tileset_id"));
            this.TilesetIDs.Add(tilesetid);
            this.BGMName = Ruby.String.FromPtr(Ruby.GetIVar(Ruby.GetIVar(data, "@bgm"), "@name"));
            this.BGMVolume = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Ruby.GetIVar(data, "@bgm"), "@volume"));
            this.BGMPitch = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Ruby.GetIVar(data, "@bgm"), "@pitch"));
            this.BGSName = Ruby.String.FromPtr(Ruby.GetIVar(Ruby.GetIVar(data, "@bgs"), "@name"));
            this.BGSVolume = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Ruby.GetIVar(data, "@bgs"), "@volume"));
            this.BGSPitch = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(Ruby.GetIVar(data, "@bgs"), "@pitch"));
            this.AutoplayBGM = Ruby.GetIVar(data, "@autoplay_bgm") == Ruby.True;
            this.AutoplayBGS = Ruby.GetIVar(data, "@autoplay_bgs") == Ruby.True;
            this.EncounterStep = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@encounter_step"));

            IntPtr table = Ruby.GetIVar(data, "@data");
            this.Width = Compatibility.RMXP.Table.XSize(table);
            this.Height = Compatibility.RMXP.Table.YSize(table);
            for (int layer = 0; layer < 3; layer++)
            {
                Layer l = new Layer($"Layer {layer + 1}");
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int index = layer * Width * Height + y * Width + x;
                        int id = (int) Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(table, index));
                        if (id == 0)
                        {
                            l.Tiles.Add(null);
                            continue;
                        }
                        TileData tile = new TileData();
                        tile.TileType = id < 384 ? TileType.Autotile : TileType.Tileset;
                        if (tile.TileType == TileType.Autotile)
                        {
                            tile.Index = (int) Math.Floor(id / 48d) - 1;
                            tile.ID = id % 48;
                        }
                        else
                        {
                            tile.Index = 0; // Tileset 0, since only 1 tileset.
                            tile.ID = id - 384;
                        }
                        l.Tiles.Add(tile);
                    }
                }
                this.Layers.Add(l);
            }

            // Now add all autotile IDs from the tileset to the list of autotile ids
            foreach (Autotile autotile in Data.Tilesets[tilesetid].Autotiles)
            {
                this.AutotileIDs.Add(autotile.ID);
            }
        }

        public Dictionary<string, object> ToJSON()
        {
            /*Dictionary<string, object> Data = new Dictionary<string, object>();
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
            return Data;*/
            throw new NotImplementedException();
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
            return this.Name;
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

        public Map Clone()
        {
            /*Map o = new Map();
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
            return o;*/
            throw new NotImplementedException();
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

        public Layer Clone()
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

        public override string ToString()
        {
            return this.Name;
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
        /// <summary>
        /// Regular tile or autotile
        /// </summary>
        public TileType TileType;
        /// <summary>
        /// Tileset or autotile index
        /// </summary>
        public int Index;
        /// <summary>
        /// Specific tile or autotile
        /// </summary>
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
