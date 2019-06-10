using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor.MKD
{
    public class Map
    {
        public int ID;
        public string Name;
        public int Width;
        public int Height;
        public List<TileData> Tiles;
        public List<Passability> Passabilities;
        public List<int> Tags;
        public List<int> Tilesets;
        public List<Event> Events;

        public static Map CreateTemp()
        {
            Map m = new Map();
            m.Name = "MT. MOON";
            m.Width = 25;
            m.Height = 25;
            m.Tilesets = new List<int>() { 1 };
            m.Tiles = new List<TileData>();
            for (int i = 0; i < m.Width * m.Height; i++)
            {
                int tileid = 0;
                // Temporary condition to make a diagonal line of grass
                if ((i - (int) Math.Floor((double) i / m.Width)) % m.Width == 0) tileid = 9;
                m.Tiles.Add(new TileData { TilesetIndex = 0, TileID = tileid });
            }
            m.Passabilities = new List<Passability>();
            m.Tags = new List<int>();
            m.Events = new List<Event>();
            return m;
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
        All = Down | Left | Right | Up
    }
}
