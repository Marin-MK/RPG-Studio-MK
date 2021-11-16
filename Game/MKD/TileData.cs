using System;

namespace RPGStudioMK.Game
{
    public class TileData : ICloneable
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


        public object Clone()
        {
            TileData o = new TileData();
            o.TileType = this.TileType;
            o.Index = this.Index;
            o.ID = this.ID;
            return o;
        }
    }
}
