using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using ODL;

namespace MKEditor.Game
{
    public class Autotile
    {
        public int ID;
        public string Name;
        public AutotileFormat Format;
        public string GraphicName;
        public List<Passability> Passabilities = new List<Passability>();
        public List<int?> Priorities = new List<int?>();
        public List<int?> Tags = new List<int?>();

        public Bitmap AutotileBitmap;

        public Autotile()
        {

        }

        public Autotile(Dictionary<string, object> Data)
        {
            if (Data.ContainsKey("^c"))
            {
                if ((string)Data["^c"] != "MKD::Autotile") throw new Exception("Invalid class - Expected class of type MKD::Autotile but got " + (string)Data["^c"] + ".");
            }
            else
            {
                throw new Exception("Could not find a ^c key to identify this class.");
            }
            this.ID = Convert.ToInt32(Data["@id"]);
            this.Name = (string) Data["@name"];
            this.GraphicName = (string) Data["@graphic_name"];
            string format = (string) Data["@format"];
            if (format == ":legacy") this.Format = AutotileFormat.Legacy;
            else if (format == ":full_corners") this.Format = AutotileFormat.FullCorners;
            else if (format == ":manual") this.Format = AutotileFormat.Manual;
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
            foreach (object o in ((JArray)Data["@tags"]).ToObject<List<object>>())
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
            Data["@format"] = Format;
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
                int tileycount = (int) Math.Floor(AutotileBitmap.Height / 32d);
                int size = tileycount * 8;
                this.Passabilities.AddRange(new Passability[size - Passabilities.Count]);
                this.Priorities.AddRange(new int?[size - Priorities.Count]);
                this.Tags.AddRange(new int?[size - Tags.Count]);
            }
        }

        public void CreateBitmap(bool Redraw = false)
        {
            if (this.AutotileBitmap == null || Redraw)
            {
                if (this.AutotileBitmap != null) this.AutotileBitmap.Dispose();
                Bitmap bmp = new Bitmap($"{Game.Data.ProjectPath}\\gfx\\autotiles\\{this.GraphicName}.png");
                this.AutotileBitmap = bmp;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public enum AutotileFormat
    {
        Legacy,
        FullCorners,
        Manual
    }
}
