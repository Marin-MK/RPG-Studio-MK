﻿using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class Map : ICloneable
    {
        public int ID;
        public string Name = "";
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
        public string BGMName = "";
        public int BGMVolume = 100;
        public int BGMPitch = 100;
        public bool AutoplayBGM = false;
        public string BGSName = "";
        public int BGSVolume = 100;
        public int BGSPitch = 100;
        public bool AutoplayBGS = false;
        public int EncounterStep = 0;

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

            IntPtr events = Ruby.GetIVar(data, "@events");
            IntPtr keys = Ruby.Hash.Keys(events);
            Ruby.Pin(keys);
            for (int i = 0; i < Ruby.Array.Length(keys); i++)
            {
                IntPtr key = Ruby.Array.Get(keys, i);
                IntPtr eventdata = Ruby.Hash.Get(events, key);
                int id = (int) Ruby.Integer.FromPtr(key);
                this.Events.Add(id, new Event(eventdata));
            }
            Ruby.Unpin(keys);
        }

        public IntPtr Save()
        {
            IntPtr map = Ruby.Funcall(Compatibility.RMXP.Map.Class, "new");
            Ruby.Pin(map);
            Ruby.SetIVar(map, "@bgm", Ruby.Funcall(Compatibility.RMXP.AudioFile.Class, "new"));
            Ruby.SetIVar(Ruby.GetIVar(map, "@bgm"), "@name", Ruby.String.ToPtr(this.BGMName));
            Ruby.SetIVar(Ruby.GetIVar(map, "@bgm"), "@volume", Ruby.Integer.ToPtr(this.BGMVolume));
            Ruby.SetIVar(Ruby.GetIVar(map, "@bgm"), "@pitch", Ruby.Integer.ToPtr(this.BGMPitch));
            Ruby.SetIVar(map, "@bgs", Ruby.Funcall(Compatibility.RMXP.AudioFile.Class, "new"));
            Ruby.SetIVar(Ruby.GetIVar(map, "@bgs"), "@name", Ruby.String.ToPtr(this.BGSName));
            Ruby.SetIVar(Ruby.GetIVar(map, "@bgs"), "@volume", Ruby.Integer.ToPtr(this.BGSVolume));
            Ruby.SetIVar(Ruby.GetIVar(map, "@bgs"), "@pitch", Ruby.Integer.ToPtr(this.BGSPitch));
            Ruby.SetIVar(map, "@autoplay_bgm", this.AutoplayBGM ? Ruby.True : Ruby.False);
            Ruby.SetIVar(map, "@autoplay_bgs", this.AutoplayBGS ? Ruby.True : Ruby.False);
            Ruby.SetIVar(map, "@width", Ruby.Integer.ToPtr(this.Width));
            Ruby.SetIVar(map, "@height", Ruby.Integer.ToPtr(this.Height));
            Ruby.SetIVar(map, "@tileset_id", Ruby.Integer.ToPtr(this.TilesetIDs[0]));
            Ruby.SetIVar(map, "@encounter_step", Ruby.Integer.ToPtr(this.EncounterStep));
            Ruby.SetIVar(map, "@encounter_list", Ruby.Array.Create());
            IntPtr data = Ruby.Funcall(Compatibility.RMXP.Table.Class, "new", Ruby.Integer.ToPtr(this.Width), Ruby.Integer.ToPtr(this.Height), Ruby.Integer.ToPtr(3));
            Ruby.SetIVar(map, "@data", data);
            for (int z = 0; z < this.Layers.Count; z++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    for (int x = 0; x < this.Width; x++)
                    {
                        TileData tile = this.Layers[z].Tiles[y * this.Width + x];
                        int value = 0;
                        if (tile != null)
                        {
                            if (tile.TileType == TileType.Autotile) value = 48 + tile.Index * 48 + tile.ID;
                            else value = 384 + tile.ID;
                        }
                        int index = z * this.Width * this.Height + y * this.Width + x;
                        Compatibility.RMXP.Table.Set(data, index, Ruby.Integer.ToPtr(value));
                    }
                }
            }
            IntPtr events = Ruby.Hash.Create();
            Ruby.SetIVar(map, "@events", events);
            foreach (Event e in this.Events.Values)
            {
                IntPtr eventdata = e.Save();
                Ruby.Hash.Set(events, Ruby.Integer.ToPtr(e.ID), eventdata);
            }
            Ruby.Unpin(map);
            return map;
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
            return $"({Order}): {Name}";
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
            Map m = new Map();
            m.ID = this.ID;
            m.Name = this.Name;
            m.Width = this.Width;
            m.Height = this.Height;
            m.BGMName = this.BGMName;
            m.BGMVolume = this.BGMVolume;
            m.BGMPitch = this.BGMPitch;
            m.BGSName = this.BGSName;
            m.BGSVolume = this.BGSVolume;
            m.BGSPitch = this.BGSPitch;
            m.EncounterStep = this.EncounterStep;
            m.ScrollX = this.ScrollX;
            m.ScrollY = this.ScrollY;
            m.Expanded = this.Expanded;
            m.Order = this.Order;
            m.ParentID = this.ParentID;
            m.AutoplayBGM = this.AutoplayBGM;
            m.AutoplayBGS = this.AutoplayBGS;
            m.Layers = new List<Layer>();
            this.Layers.ForEach(l => m.Layers.Add((Layer) l.Clone()));
            m.TilesetIDs = new List<int>(this.TilesetIDs);
            m.AutotileIDs = new List<int>(this.AutotileIDs);
            m.Events = new Dictionary<int, Event>();
            foreach (Event e in this.Events.Values) m.Events.Add(e.ID, (Event) e.Clone());
            return m;
        }
    }

    public enum TileType
    {
        Tileset = 0,
        Autotile = 1
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
