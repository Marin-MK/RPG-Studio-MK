using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace RPGStudioMK.Game;

public class Tileset : ICloneable
{
    public int ID;
    public string Name;
    public string GraphicName;
    public List<Passability> Passabilities = new List<Passability>();
    public List<int> Priorities = new List<int>();
    public List<int> Tags = new List<int>();
    public List<bool> BushFlags = new List<bool>();
    public List<bool> CounterFlags = new List<bool>();

    // RMXP properties
    public int PanoramaHue;
    public string PanoramaName;
    public string FogName;
    public int FogSX;
    public int FogSY;
    public byte FogOpacity;
    public int FogHue;
    public int FogZoom;
    public int FogBlendType;
    public string BattlebackName;
    public List<Autotile> Autotiles = new List<Autotile>();

    /// <summary>
    /// If an autotile's top center tile is equal to another autotile's top left tile, then this autotile
    /// is allowed to overlap the other autotile without updating its borders when drawn over it on the same layer.
    /// </summary>
    public List<List<int>> AutotileOverlapPermissions = new List<List<int>>();

    [JsonIgnore]
    private Bitmap _tb;
    [JsonIgnore]
    public Bitmap TilesetBitmap
    {
        get
        {
            if (_tb != null) return _tb;
            string filename = $"{Data.ProjectPath}/Graphics/Tilesets/{this.GraphicName}.png";
            _tb = null;
            if (Bitmap.FindRealFilename(filename) != null)
            {
                _tb = new Bitmap(filename);
            }
            return _tb;
        }
        set
        {
            _tb = value;
        }
    }
    [JsonIgnore]
    private Bitmap _tlb;
    [JsonIgnore]
    public Bitmap TilesetListBitmap
    {
        get
        {
            if (TilesetBitmap == null && _tlb == null) return null;
            if (_tlb != null) return _tlb;
            int tileycount = (int)Math.Floor(TilesetBitmap.Height / 32d);

            _tlb = new Bitmap(TilesetBitmap.Width + 7, TilesetBitmap.Height + tileycount - 1, Graphics.MaxTextureSize);
            _tlb.Unlock();

            for (int tiley = 0; tiley < tileycount; tiley++)
            {
                for (int tilex = 0; tilex < 8; tilex++)
                {
                    _tlb.Build(tilex * 32 + tilex, tiley * 32 + tiley, TilesetBitmap, tilex * 32, tiley * 32, 32, 32, BlendMode.None);
                }
            }
            _tlb.Lock();
            return _tlb;
        }
        set
        {
            _tlb = value;
        }
    }

    public Tileset()
    {
        MakeEmpty();
    }

    public Tileset(IntPtr data)
    {
        this.ID = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@id"));
        this.Name = Ruby.String.FromPtr(Ruby.GetIVar(data, "@name"));
        this.GraphicName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@tileset_name"));
        IntPtr passability = Ruby.GetIVar(data, "@passages");
        for (int i = 0; i < Compatibility.RMXP.Table.Size(passability); i++)
        {
            int code = (int)Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(passability, i));
            if ((code & 64) != 0)
            {
                BushFlags.Add(true);
                code ^= 64;
            }
            else BushFlags.Add(false);
            if ((code & 128) != 0)
            {
                CounterFlags.Add(true);
                code ^= 128;
            }
            else CounterFlags.Add(false);
            Passabilities.Add((Passability)15 - code);
        }
        IntPtr priorities = Ruby.GetIVar(data, "@priorities");
        for (int i = 0; i < Compatibility.RMXP.Table.Size(priorities); i++)
        {
            int code = (int)Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(priorities, i));
            this.Priorities.Add(code);
        }
        IntPtr tags = Ruby.GetIVar(data, "@terrain_tags");
        for (int i = 0; i < Compatibility.RMXP.Table.Size(tags); i++)
        {
            int code = (int)Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(tags, i));
            this.Tags.Add(code);
        }
        this.PanoramaHue = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@panorama_hue"));
        this.PanoramaName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@panorama_name"));
        this.FogName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@fog_name"));
        this.FogSX = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_sx"));
        this.FogSY = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_sy"));
        this.FogOpacity = (byte) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_opacity"));
        this.FogHue = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_hue"));
        this.FogZoom = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_zoom"));
        this.FogBlendType = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_blend_type"));
        this.BattlebackName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@battleback_name"));
        IntPtr autotiles = Ruby.GetIVar(data, "@autotile_names");
        for (int i = 0; i < Ruby.Array.Length(autotiles); i++)
        {
            string name = Ruby.String.FromPtr(Ruby.Array.Get(autotiles, i));
            if (string.IsNullOrEmpty(name)) this.Autotiles.Add(null);
            else
            {
                Autotile autotile = new Autotile();
                autotile.ID = this.ID * 7 + i;
                autotile.Name = name;
                autotile.Passability = this.Passabilities[(i + 1) * 48];
                autotile.Priority = (int)this.Priorities[(i + 1) * 48];
                autotile.Tag = (int)this.Tags[(i + 1) * 48];
                autotile.SetGraphic(name);
                this.Autotiles.Add(autotile);
                Data.Autotiles[autotile.ID] = autotile;
            }
        }

        UpdateAutotileOverlapPermissions();

        // Make sure the three arrays are just as big; trailing nulls may be left out if the data is edited externally
        int maxcount = Math.Max(Math.Max(Passabilities.Count, Priorities.Count), Tags.Count);
        this.Passabilities.AddRange(new Passability[maxcount - Passabilities.Count]);
        this.Priorities.AddRange(new int[maxcount - Priorities.Count]);
        this.Tags.AddRange(new int[maxcount - Tags.Count]);
        if (!string.IsNullOrEmpty(this.GraphicName)) this.CreateBitmap();
    }

    public IntPtr Save()
    {
        IntPtr obj = Ruby.Funcall(Compatibility.RMXP.Tileset.Class, "new");
        Ruby.Pin(obj);
        Ruby.SetIVar(obj, "@id", Ruby.Integer.ToPtr(this.ID));
        Ruby.SetIVar(obj, "@name", Ruby.String.ToPtr(this.Name));
        Ruby.SetIVar(obj, "@tileset_name", Ruby.String.ToPtr(this.GraphicName));
        Ruby.SetIVar(obj, "@panorama_name", Ruby.String.ToPtr(this.PanoramaName));
        Ruby.SetIVar(obj, "@panorama_hue", Ruby.Integer.ToPtr(this.PanoramaHue));
        Ruby.SetIVar(obj, "@fog_name", Ruby.String.ToPtr(this.FogName));
        Ruby.SetIVar(obj, "@fog_sx", Ruby.Integer.ToPtr(this.FogSX));
        Ruby.SetIVar(obj, "@fog_sy", Ruby.Integer.ToPtr(this.FogSY));
        Ruby.SetIVar(obj, "@fog_hue", Ruby.Integer.ToPtr(this.FogHue));
        Ruby.SetIVar(obj, "@fog_opacity", Ruby.Integer.ToPtr(this.FogOpacity));
        Ruby.SetIVar(obj, "@fog_zoom", Ruby.Integer.ToPtr(this.FogZoom));
        Ruby.SetIVar(obj, "@fog_blend_type", Ruby.Integer.ToPtr(this.FogBlendType));
        Ruby.SetIVar(obj, "@battleback_name", Ruby.String.ToPtr(this.BattlebackName));
        IntPtr autotile_names = Ruby.Array.Create(7, Ruby.String.ToPtr(""));
        Ruby.SetIVar(obj, "@autotile_names", autotile_names);
        foreach (Autotile autotile in this.Autotiles)
        {
            if (autotile == null) continue;
            int idx = autotile.ID - this.ID * 7;
            Ruby.Array.Set(autotile_names, idx, Ruby.String.ToPtr(autotile.GraphicName));
        }
        IntPtr passages = Ruby.Funcall(Compatibility.RMXP.Table.Class, "new", Ruby.Integer.ToPtr(this.Passabilities.Count));
        Ruby.SetIVar(obj, "@passages", passages);
        for (int i = 0; i < this.Passabilities.Count; i++)
        {
            int code = 15 - (int)this.Passabilities[i];
            if (BushFlags[i]) code += 64;
            if (CounterFlags[i]) code += 128;
            Compatibility.RMXP.Table.Set(passages, i, Ruby.Integer.ToPtr(code));
        }
        IntPtr priorities = Ruby.Funcall(Compatibility.RMXP.Table.Class, "new", Ruby.Integer.ToPtr(this.Priorities.Count));
        Ruby.SetIVar(obj, "@priorities", priorities);
        for (int i = 0; i < this.Priorities.Count; i++)
        {
            Compatibility.RMXP.Table.Set(priorities, i, Ruby.Integer.ToPtr(this.Priorities[i]));
        }
        IntPtr tags = Ruby.Funcall(Compatibility.RMXP.Table.Class, "new", Ruby.Integer.ToPtr(this.Tags.Count));
        Ruby.SetIVar(obj, "@terrain_tags", tags);
        for (int i = 0; i < this.Tags.Count; i++)
        {
            Compatibility.RMXP.Table.Set(tags, i, Ruby.Integer.ToPtr(this.Tags[i]));
        }
        Ruby.Unpin(obj);
        return obj;
    }

    public void SetGraphic(string GraphicName)
    {
        if (this.GraphicName != GraphicName)
        {
            this.GraphicName = GraphicName;
            this.CreateBitmap(true);
            int tileycount = (int)Math.Floor(TilesetBitmap.Height / 32d);
            int size = tileycount * 8;
            this.Passabilities.AddRange(new Passability[size - Passabilities.Count]);
            this.Priorities.AddRange(new int[size - Priorities.Count]);
            this.Tags.AddRange(new int[size - Tags.Count]);
        }
    }

    public void CreateBitmap(bool Redraw = false)
    {
        if (_tb == null || Redraw)
        {
            _tb?.Dispose();
            _tb = null;
            _tlb?.Dispose();
            _tlb = null;
        }
    }

    public void UpdateDataLists()
    {
        int newsize = 384;
        if (TilesetBitmap != null) newsize += 8 * (int) Math.Ceiling(TilesetBitmap.Height / 32d);

        // Resize the data lists
        if (newsize < Passabilities.Count)
        {
            Passabilities.RemoveRange(newsize, Passabilities.Count - newsize);
            Priorities.RemoveRange(newsize, Priorities.Count - newsize);
            Tags.RemoveRange(newsize, Tags.Count - newsize);
            BushFlags.RemoveRange(newsize, BushFlags.Count - newsize);
            CounterFlags.RemoveRange(newsize, CounterFlags.Count - newsize);
        }
        else if (newsize > Passabilities.Count)
        {
            Passabilities.AddRange(new Passability[newsize - Passabilities.Count]);
            Priorities.AddRange(new int[newsize - Priorities.Count]);
            Tags.AddRange(new int[newsize - Tags.Count]);
            BushFlags.AddRange(new bool[newsize - BushFlags.Count]);
            CounterFlags.AddRange(new bool[newsize - CounterFlags.Count]);
        }
    }

    /// <summary>
    /// If an autotile's top center tile is equal to another autotile's top left tile, then this autotile
    /// is allowed to overlap the other autotile without updating its borders when drawn over it on the same layer.
    /// </summary>
    public void UpdateAutotileOverlapPermissions()
    {
        for (int i1 = 0; i1 < this.Autotiles.Count; i1++)
        {
            Autotile a1 = this.Autotiles[i1];
            if (a1 is null || a1.AutotileBitmap is null) continue;
            for (int i2 = 0; i2 < this.Autotiles.Count; i2++)
            {
                Autotile a2 = this.Autotiles[i2];
                if (a2 is null || a2.AutotileBitmap is null) continue;
                if (a1 == a2 || a1 == null || a2 == null || a1.Format != a2.Format || a1.Format != AutotileFormat.RMXP) continue;
                bool Equal = true;
                for (int y = 0; y < 32; y++)
                {
                    if (!Equal) break;
                    for (int x = 0; x < 32; x++)
                    {
                        if (!Equal) break;
                        Color c1 = a1.AutotileBitmap.GetPixel(x, y);
                        Color c2 = a2.AutotileBitmap.GetPixel(x + 32, y);
                        if (!c1.Equals(c2))
                        {
                            Equal = false;
                            break;
                        }
                    }
                }
                if (Equal)
                {
                    a1.OverlappableBy.Add(a2.ID);
                }
            }
        }
    }

    public void MakeEmpty()
    {
        this.Name = "";
        this.GraphicName = "";
        this.Passabilities = new List<Passability>(new Passability[384]);
        this.Priorities = new List<int>(new int[384]);
        this.Tags = new List<int>(new int[384]);
        this.BushFlags = new List<bool>(new bool[384]);
        this.CounterFlags = new List<bool>(new bool[384]);
        for (int i = 0; i < 384; i++)
        {
            this.Passabilities[i] = Passability.All;
            if (i < 48) this.Priorities[i] = 5;
        }
        this.PanoramaHue = 0;
        this.PanoramaName = "";
        this.FogName = "";
        this.FogSX = 0;
        this.FogSY = 0;
        this.FogOpacity = 64;
        this.FogHue = 0;
        this.FogZoom = 200;
        this.FogBlendType = 0;
        this.BattlebackName = "";
        this.Autotiles = new List<Autotile>(new Autotile[7]);
        for (int i = 0; i < 7; i++)
        {
            if (Data.Autotiles[this.ID * 7 + i] != null)
            {
                Data.Autotiles[this.ID * 7 + i].AutotileBitmap?.Dispose();
                Data.Autotiles[this.ID * 7 + i].AutotileBitmap = null;
            }
            Data.Autotiles[this.ID * 7 + i] = null;
        }
        this.TilesetBitmap?.Dispose();
        this.TilesetBitmap = null;
        this.TilesetListBitmap?.Dispose();
        this.TilesetListBitmap = null;
    }

    public override string ToString()
    {
        return this.Name;
    }

    public object Clone()
    {
        Tileset t = new Tileset();
        t.ID = this.ID;
        t.Name = this.Name;
        t.GraphicName = this.GraphicName;
        t.Passabilities = new List<Passability>(this.Passabilities);
        t.Priorities = new List<int>(this.Priorities);
        t.Tags = new List<int>(this.Tags);
        t.BushFlags = new List<bool>(this.BushFlags);
        t.CounterFlags = new List<bool>(this.CounterFlags);
        t.PanoramaHue = this.PanoramaHue;
        t.PanoramaName = this.PanoramaName;
        t.FogName = this.FogName;
        t.FogSX = this.FogSX;
        t.FogSY = this.FogSY;
        t.FogOpacity = this.FogOpacity;
        t.FogHue = this.FogHue;
        t.FogZoom = this.FogZoom;
        t.FogBlendType = this.FogBlendType;
        t.BattlebackName = this.BattlebackName;
        t.Autotiles = this.Autotiles.ConvertAll(a => (Autotile)a?.Clone());
        t.TilesetBitmap = this.TilesetBitmap;
        t.TilesetListBitmap = this.TilesetListBitmap;
        return t;
    }
}
