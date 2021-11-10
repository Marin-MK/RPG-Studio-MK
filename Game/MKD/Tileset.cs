using System;
using System.Collections.Generic;
using odl;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class Tileset
    {
        public int ID;
        public string Name;
        public string GraphicName;
        public List<Passability> Passabilities = new List<Passability>();
        public List<int?> Priorities = new List<int?>();
        public List<int?> Tags = new List<int?>();

        // RMXP properties
        public int PanoramaHue;
        public string PanoramaName;
        public string FogName;
        public int FogSX;
        public int FogSY;
        public int FogOpacity;
        public int FogHue;
        public int FogZoom;
        public int FogBlendType;
        public string BattlebackName;
        public List<string> Autotiles = new List<string>();
        /// <summary>
        /// List of tile IDs that have the bush flag set.
        /// </summary>
        public List<int> BushFlags = new List<int>();

        public Bitmap TilesetBitmap;
        public Bitmap TilesetListBitmap;

        public Tileset()
        {

        }

        public Tileset(IntPtr data)
        {
            this.ID = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@id"));
            this.Name = Ruby.String.FromPtr(Ruby.GetIVar(data, "@name"));
            this.GraphicName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@tileset_name"));
            IntPtr passability = Ruby.GetIVar(data, "@passages");
            for (int i = 0; i < Compatibility.RMXP.Table.Size(passability); i++)
            {
                int code = (int) Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(passability, i));
                if ((code & 64) != 0)
                {
                    BushFlags.Add(i);
                    code -= 64;
                }
                Passabilities.Add((Passability) code);
            }
            IntPtr priorities = Ruby.GetIVar(data, "@priorities");
            for (int i = 0; i < Compatibility.RMXP.Table.Size(priorities); i++)
            {
                int code = (int) Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(priorities, i));
                this.Priorities.Add(code);
            }
            IntPtr tags = Ruby.GetIVar(data, "@terrain_tags");
            for (int i = 0; i < Compatibility.RMXP.Table.Size(tags); i++)
            {
                int code = (int) Ruby.Integer.FromPtr(Compatibility.RMXP.Table.Get(tags, i));
                this.Tags.Add(code);
            }
            this.PanoramaHue = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@panorama_hue"));
            this.PanoramaName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@panorama_name"));
            this.FogName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@fog_name"));
            this.FogSX = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_sx"));
            this.FogSY = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_sy"));
            this.FogOpacity = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_opacity"));
            this.FogHue = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_hue"));
            this.FogZoom = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_zoom"));
            this.FogBlendType = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@fog_blend_type"));
            this.BattlebackName = Ruby.String.FromPtr(Ruby.GetIVar(data, "@battleback_name"));
            IntPtr autotiles = Ruby.GetIVar(data, "@autotile_names");
            for (int i = 0; i < Ruby.Array.Length(autotiles); i++)
            {
                string autotile = Ruby.String.FromPtr(Ruby.Array.Get(autotiles, i));
                this.Autotiles.Add(autotile);
            }

            // Make sure the three arrays are just as big; trailing nulls may be left out if the data is edited externally
            int maxcount = Math.Max(Math.Max(Passabilities.Count, Priorities.Count), Tags.Count);
            this.Passabilities.AddRange(new Passability[maxcount - Passabilities.Count]);
            this.Priorities.AddRange(new int?[maxcount - Priorities.Count]);
            this.Tags.AddRange(new int?[maxcount - Tags.Count]);
            if (!string.IsNullOrEmpty(this.GraphicName)) this.CreateBitmap();
        }

        public Dictionary<string, object> ToJSON()
        {
            Dictionary<string, object> Data = new Dictionary<string, object>();
            Data["^c"] = "MKD::Tileset";
            Data["@id"] = ID;
            Data["@name"] = Name;
            Data["@graphic_name"] = GraphicName;
            Data["@priorities"] = Priorities;
            Data["@passabilities"] = Passabilities;
            Data["@tags"] = Tags;
            return Data;
        }

        public void SetGraphic(string GraphicName)
        {
            if (this.GraphicName != GraphicName)
            {
                this.GraphicName = GraphicName;
                this.CreateBitmap(true);
                int tileycount = (int) Math.Floor(TilesetBitmap.Height / 32d);
                int size = tileycount * 8;
                this.Passabilities.AddRange(new Passability[size - Passabilities.Count]);
                this.Priorities.AddRange(new int?[size - Priorities.Count]);
                this.Tags.AddRange(new int?[size - Tags.Count]);
            }
        }

        public void CreateBitmap(bool Redraw = false)
        {
            if (this.TilesetBitmap == null || Redraw)
            {
                this.TilesetBitmap?.Dispose();
                this.TilesetListBitmap?.Dispose();
                
                Bitmap bmp = new Bitmap($"{Data.ProjectPath}/Graphics/Tilesets/{this.GraphicName}.png");
                this.TilesetBitmap = bmp;
                int tileycount = (int) Math.Floor(bmp.Height / 32d);

                this.TilesetListBitmap = new Bitmap(bmp.Width + 7, bmp.Height + tileycount - 1);
                this.TilesetListBitmap.Unlock();

                for (int tiley = 0; tiley < tileycount; tiley++)
                {
                    for (int tilex = 0; tilex < 8; tilex++)
                    {
                        this.TilesetListBitmap.Build(tilex * 32 + tilex, tiley * 32 + tiley, bmp, tilex * 32, tiley * 32, 32, 32);
                    }
                }
                this.TilesetListBitmap.Lock();
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
