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
        public List<Layer> Layers;
        public List<int> Tilesets;
        public List<Event> Events;

        public static Map CreateTemp()
        {
            Map m = new Map();
            m.Name = "MT. MOON";
            m.Width = 25;
            m.Height = 25;
            m.Tilesets = new List<int>() { 1 };
            m.Layers = new List<Layer>() { new Layer(), new Layer(), new Layer(), new Layer(), new Layer() };
            m.Layers[0].Tiles = new List<TileData>();
            for (int i = 0; i < m.Width * m.Height; i++)
            {
                m.Layers[0].Tiles.Add(new TileData { TilesetIndex = 0, TileID = 0 });
            }
            // Makes a diagonal line of grass on the second layer
            m.Layers[1].Tiles = new List<TileData>();
            for (int i = 0; i < m.Width * m.Height; i++)
            {
                if ((i - (int) Math.Floor((double) i / m.Width)) % m.Width == 0)
                {
                    m.Layers[1].Tiles.Add(new TileData { TilesetIndex = 0, TileID = 9 });
                }
                else
                {
                    m.Layers[1].Tiles.Add(null);
                }
            }
            m.Events = new List<Event>();
            return m;
        }
    }

    public class Layer
    {
        public string Name = "Unnamed Layer";
        public List<TileData> Tiles;
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
