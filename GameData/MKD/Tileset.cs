using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Data
{
    public class Tileset : Serializable
    {
        public int ID;
        public string Name;
        public string GraphicName;
        public List<Passability> Passabilities;
        public List<int?> Priorities;
        public List<int?> Tags;

        public Bitmap ResultBitmap;

        public Tileset()
        {

        }

        public Tileset(string path)
            : base(path)
        {
            this.ID = GetVar<int>("id");
            this.Name = GetVar<string>("name");
            this.GraphicName = GetVar<string>("graphic_name");
            List<int> _Passabilities = GetList<int>("passabilities");
            this.Passabilities = new List<Passability>();
            for (int i = 0; i < _Passabilities.Count; i++)
            {
                this.Passabilities.Add((Passability) _Passabilities[i]);
            }
            List<object> _priorities = GetList<object>("priorities");
            this.Priorities = new List<int?>();
            for (int i = 0; i < _priorities.Count; i++)
            {
                if (_priorities[i] is int) this.Priorities.Add((int)_priorities[i]);
                else this.Priorities.Add(null);
            }
            List<object> _tags = GetList<object>("tags");
            this.Tags = new List<int?>();
            for (int i = 0; i < _tags.Count; i++)
            {
                if (_tags[i] is int) this.Tags.Add((int) _tags[i]);
                else this.Tags.Add(null);
            }
            // Make sure the three arrays are just as big
            int maxcount = Math.Max(Math.Max(Passabilities.Count, Priorities.Count), Tags.Count);
            this.Passabilities.AddRange(new Passability[maxcount - Passabilities.Count]);
            this.Priorities.AddRange(new int?[maxcount - Priorities.Count]);
            this.Tags.AddRange(new int?[maxcount - Tags.Count]);
        }

        public static Tileset GetTileset()
        {
            Tileset t = new Tileset();
            t.Name = "Common";
            t.GraphicName = "common.png";
            t.Passabilities = new List<Passability>()
            {
                Passability.All, Passability.All, Passability.All, Passability.All, Passability.All, Passability.All, Passability.All, Passability.All,
                Passability.All, Passability.All, Passability.All, Passability.All, Passability.All, Passability.None, Passability.None, Passability.None,
                Passability.None, Passability.None, Passability.None, Passability.None, Passability.All, Passability.All, Passability.None, Passability.None,
                Passability.None, Passability.None, Passability.None, Passability.None, Passability.None, Passability.None, Passability.All, Passability.All,
                Passability.None, Passability.None, Passability.None, Passability.None, Passability.None, Passability.None, Passability.All, Passability.None,
                Passability.None, Passability.None, Passability.None, Passability.None, Passability.All, Passability.All, Passability.All, Passability.None,
                Passability.None, Passability.All, Passability.All, Passability.None, Passability.All, Passability.None, Passability.None, Passability.None,
                Passability.None, Passability.All, Passability.All, Passability.None, Passability.All, Passability.All, Passability.None, Passability.None
            };
            t.Priorities = new List<int?>();
            t.Tags = new List<int?>();

            Bitmap bmp = new Bitmap(t.GraphicName);
            int tileycount = (int) Math.Floor(bmp.Height / 32d);

            t.ResultBitmap = new Bitmap(bmp.Width + 7, bmp.Height + tileycount - 1);
            t.ResultBitmap.Unlock();

            for (int tiley = 0; tiley < tileycount; tiley++)
            {
                for (int tilex = 0; tilex < 8; tilex++)
                {
                    t.ResultBitmap.Build(tilex * 32 + tilex, tiley * 32 + tiley, bmp, tilex * 32, tiley * 32, 32, 32);
                }
            }
            t.ResultBitmap.Lock();

            return t;
        }
    }
}
