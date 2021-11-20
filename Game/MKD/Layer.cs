using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Game
{
    public class Layer : ICloneable
    {
        public string Name = "Unnamed Layer";
        public List<TileData> Tiles = new List<TileData>();
        public bool Visible = true;

        public Layer(string Name)
        {
            this.Name = Name;
        }

        public Layer(string Name, int Width, int Height)
        {
            this.Name = Name;
            this.Tiles = new TileData[Width * Height].ToList();
        }

        public object Clone()
        {
            Layer l = new Layer(this.Name);
            l.Tiles = new List<TileData>();
            this.Tiles.ForEach(t => {
                if (t == null) l.Tiles.Add(null);
                else l.Tiles.Add((TileData) t.Clone());
            });
            l.Visible = this.Visible;
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
}
