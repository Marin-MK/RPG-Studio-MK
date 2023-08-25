using System;
using System.IO;

namespace RPGStudioMK.Game;

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

    public TileData()
    {
        this.TileType = TileType.Tileset;
        this.Index = 0;
        this.ID = 0;
    }

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
            TileData data = (TileData)obj;
            return this.TileType == data.TileType &&
                   this.Index == data.Index &&
                   this.ID == data.ID;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
