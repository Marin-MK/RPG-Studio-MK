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
            m.Layers = new List<Layer>() { new Layer("Layer 1"), new Layer("Layer 2"), new Layer("Layer 3"), new Layer("Layer 4"), new Layer("Layer 5") };
            m.Layers[0].Tiles = new List<TileData>();
            // Fills layer 1 with grass (tile id 0)
            for (int i = 0; i < m.Width * m.Height; i++)
            {
                m.Layers[0].Tiles.Add(new TileData { TilesetIndex = 0, TileID = 0 });
            }
            // Draws a diagonal line of tall grass on layer 2 (tile id 9)
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
            for (int i = 0; i < m.Width * m.Height; i++) m.Layers[2].Tiles.Add(null);
            for (int i = 0; i < m.Width * m.Height; i++) m.Layers[3].Tiles.Add(null);
            for (int i = 0; i < m.Width * m.Height; i++) m.Layers[4].Tiles.Add(null);
            m.Events = new List<Event>();
            return m;
        }
    }

    public class Layer
    {
        public string Name = "Unnamed Layer";
        public List<TileData> Tiles = new List<TileData>();

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
        All = Down | Left | Right | Up
    }
}
