using System;

namespace RPGStudioMK.Game
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class TileData : ICloneable
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
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

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj is TileData)
            {
                TileData data = (TileData) obj;
                return this.TileType == data.TileType &&
                       this.Index == data.Index &&
                       this.ID == data.ID;
            }
            return false;
        }
    }
}
