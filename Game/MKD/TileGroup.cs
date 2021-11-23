using System;
using System.Collections.Generic;

namespace RPGStudioMK.Game
{
    [Serializable]
    public class TileGroup
    {
        public List<TileData> Tiles = new List<TileData>();
        public int Width;
        public int Height;

        public TileGroup()
        {

        }

        public TileGroup(List<TileData> Tiles, int Width, int Height)
        {
            this.Tiles = Tiles;
            this.Width = Width;
            this.Height = Height;
        }

        public void AddTile(TileData Tile)
        {
            this.Tiles.Add(Tile);
        }

        public TileData GetTile(int ListIndex)
        {
            return Tiles[ListIndex];
        }
    }
}
