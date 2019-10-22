using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using ODL;

namespace MKEditor.Game
{
    public class Tileset
    {
        public int ID;
        public string Name;
        public string GraphicName;
        public List<Passability> Passabilities;
        public List<int?> Priorities;
        public List<int?> Tags;

        public Bitmap TilesetBitmap;
        public Bitmap TilesetListBitmap;

        public Tileset()
        {

        }

        public Tileset(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string) Data["^c"] != "MKD::Tileset") throw new Exception("Invalid class - Expected class of type MKD::Tileset but got " + (string) Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            this.ID = Convert.ToInt32(Data["@id"]);
            this.Name = (string) Data["@name"];
            this.GraphicName = (string) Data["@graphic_name"];
            Priorities = new List<int?>();
            foreach (object o in ((JArray) Data["@priorities"]).ToObject<List<object>>())
            {
                if (o is null) Priorities.Add(null);
                else Priorities.Add(Convert.ToInt32(o));
            }
            Passabilities = new List<Passability>();
            foreach (object o in ((JArray) Data["@passabilities"]).ToObject<List<object>>())
            {
                Passabilities.Add((Passability) Convert.ToInt32(o));
            }
            Tags = new List<int?>();
            foreach (object o in ((JArray) Data["@tags"]).ToObject<List<object>>())
            {
                if (o is null) Tags.Add(null);
                else Tags.Add(Convert.ToInt32(o));
            }
            // Make sure the three arrays are just as big; trailing nulls may be left out if the data is edited externally
            int maxcount = Math.Max(Math.Max(Passabilities.Count, Priorities.Count), Tags.Count);
            this.Passabilities.AddRange(new Passability[maxcount - Passabilities.Count]);
            this.Priorities.AddRange(new int?[maxcount - Priorities.Count]);
            this.Tags.AddRange(new int?[maxcount - Tags.Count]);
            this.CreateBitmap();
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

        public void CreateBitmap(bool Redraw = false)
        {
            if (this.TilesetBitmap == null || Redraw)
            {
                if (this.TilesetBitmap != null) this.TilesetBitmap.Dispose();
                
                string GfxFolder = Directory.GetParent(Data.DataPath).FullName + "\\gfx";
                Bitmap bmp = new Bitmap($"{GfxFolder}\\tilesets\\{this.GraphicName}.png");
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
