using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.MKD
{
    public class Tileset
    {
        public int ID;
        public string Name;
        public string GraphicName;
        public List<Passability> Passabilities;
        public List<int> Priorities;
        public List<int> Tags;

        public List<Bitmap> TileBitmaps = new List<Bitmap>();
        public Bitmap ResultBitmap;

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
            t.Priorities = new List<int>();
            t.Tags = new List<int>();

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
