using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class DatabaseModeList : Widget
    {
        public DatabaseWidget DBWidget;

        public List<List<string>> Tabs = new List<List<string>>();

        public int SelectedIndex = -1;

        public DatabaseModeList(IContainer Parent) : base(Parent)
        {
            SetBackgroundColor(10, 23, 37);

            Sprites["bg"] = new Sprite(this.Viewport);
            Sprites["text"] = new Sprite(this.Viewport);

            Bitmap c = new Bitmap(20, 20);
            #region Shadow Corner
            c.Unlock();
            c.DrawLine(0, 19, 8, 19, 0, 13, 27, 26);
            c.DrawLine(0, 18, 10, 18, 0, 13, 27, 26);
            c.DrawLine(0, 17, 12, 17, 0, 13, 27, 26);
            c.DrawLine(1, 16, 14, 16, 0, 13, 27, 26);
            c.DrawLine(8, 15, 15, 15, 0, 13, 27, 26);
            c.DrawLine(10, 14, 16, 14, 0, 13, 27, 26);
            c.DrawLine(11, 13, 16, 13, 0, 13, 27, 26);
            c.DrawLine(12, 12, 17, 12, 0, 13, 27, 26);
            c.DrawLine(13, 11, 17, 11, 0, 13, 27, 26);
            c.DrawLine(14, 10, 18, 10, 0, 13, 27, 26);
            c.SetPixel(15, 9, 0, 13, 27, 26);
            c.SetPixel(15, 8, 0, 13, 27, 26);
            c.DrawLine(16, 9, 16, 1, 0, 13, 27, 26);
            c.DrawLine(17, 9, 17, 0, 0, 13, 27, 26);
            c.DrawLine(18, 10, 18, 0, 0, 13, 27, 26);
            c.DrawLine(19, 8, 19, 0, 0, 13, 27, 26);
            c.SetPixel(0, 16, 0, 13, 17, 26);
            c.DrawLine(0, 15, 7, 15, 0, 13, 17, 26);
            c.DrawLine(6, 14, 9, 14, 0, 13, 17, 26);
            c.SetPixel(9, 13, 0, 13, 17, 26);
            c.SetPixel(10, 13, 0, 13, 17, 26);
            c.SetPixel(11, 12, 0, 13, 17, 26);
            c.SetPixel(16, 0, 0, 13, 17, 26);
            c.DrawLine(15, 0, 15, 7, 0, 13, 17, 26);
            c.DrawLine(14, 6, 14, 9, 0, 13, 17, 26);
            c.SetPixel(13, 9, 0, 13, 17, 26);
            c.SetPixel(13, 10, 0, 13, 17, 26);
            c.SetPixel(12, 10, 0, 13, 17, 26);
            c.SetPixel(12, 11, 0, 13, 17, 26);
            c.DrawLine(0, 14, 4, 14, 0, 3, 7, 26);
            c.SetPixel(11, 10, 0, 3, 7, 26);
            c.SetPixel(11, 9, 0, 3, 7, 26);
            c.SetPixel(12, 9, 0, 3, 7, 26);
            c.SetPixel(12, 8, 0, 3, 7, 26);
            c.DrawLine(5, 13, 7, 13, 0, 3, 7, 26);
            c.SetPixel(8, 12, 0, 3, 7, 26);
            c.SetPixel(9, 12, 0, 3, 7, 26);
            c.SetPixel(9, 11, 0, 3, 7, 26);
            c.SetPixel(10, 11, 0, 3, 7, 26);
            c.DrawLine(13, 7, 13, 5, 0, 3, 7, 26);
            c.DrawLine(14, 4, 14, 0, 0, 3, 7, 26);
            c.SetPixel(14, 5, 0, 3, 17, 26);
            c.SetPixel(13, 8, 0, 3, 17, 26);
            c.SetPixel(11, 11, 0, 3, 17, 26);
            c.SetPixel(10, 12, 0, 3, 17, 26);
            c.SetPixel(8, 13, 0, 3, 17, 26);
            c.SetPixel(5, 14, 0, 3, 17, 26);
            c.SetPixel(13, 4, 1, 4, 0, 28);
            c.SetPixel(12, 7, 1, 4, 0, 28);
            c.SetPixel(10, 10, 1, 4, 0, 28);
            c.SetPixel(13, 4, 1, 4, 0, 28);
            c.SetPixel(7, 12, 1, 4, 0, 28);
            c.SetPixel(4, 13, 1, 4, 0, 28);
            c.SetPixel(2, 13, 2, 0, 6, 33);
            c.SetPixel(3, 13, 2, 0, 6, 33);
            c.SetPixel(6, 12, 2, 0, 6, 33);
            c.SetPixel(8, 11, 2, 0, 6, 33);
            c.SetPixel(13, 2, 2, 0, 6, 33);
            c.SetPixel(13, 3, 2, 0, 6, 33);
            c.SetPixel(12, 6, 2, 0, 6, 33);
            c.SetPixel(11, 8, 2, 0, 6, 33);
            c.SetPixel(0, 13, 0, 8, 17, 51);
            c.SetPixel(1, 13, 0, 8, 17, 51);
            c.SetPixel(10, 9, 0, 8, 17, 51);
            c.SetPixel(9, 10, 0, 8, 17, 51);
            c.SetPixel(13, 0, 0, 8, 17, 51);
            c.SetPixel(13, 1, 0, 8, 17, 51);
            c.SetPixel(4, 12, 0, 8, 12, 51);
            c.SetPixel(5, 12, 0, 8, 12, 51);
            c.SetPixel(7, 11, 0, 8, 12, 51);
            c.SetPixel(8, 10, 0, 8, 12, 51);
            c.SetPixel(12, 4, 0, 8, 12, 51);
            c.SetPixel(12, 5, 0, 8, 12, 51);
            c.SetPixel(11, 7, 0, 8, 12, 51);
            c.SetPixel(10, 8, 0, 8, 12, 51);
            c.SetPixel(3, 12, 0, 3, 12, 51);
            c.SetPixel(12, 3, 0, 3, 12, 51);
            c.SetPixel(0, 12, 0, 3, 7, 51);
            c.SetPixel(1, 12, 0, 3, 7, 51);
            c.SetPixel(2, 12, 0, 3, 7, 51);
            c.SetPixel(5, 11, 0, 3, 7, 51);
            c.SetPixel(6, 11, 0, 3, 7, 51);
            c.SetPixel(7, 10, 0, 3, 7, 51);
            c.SetPixel(9, 9, 0, 3, 7, 51);
            c.SetPixel(12, 0, 0, 3, 7, 51);
            c.SetPixel(12, 1, 0, 3, 7, 51);
            c.SetPixel(12, 2, 0, 3, 7, 51);
            c.SetPixel(11, 5, 0, 3, 7, 51);
            c.SetPixel(11, 6, 0, 3, 7, 51);
            c.SetPixel(10, 7, 0, 3, 7, 51);
            c.SetPixel(4, 11, 0, 3, 2, 51);
            c.SetPixel(8, 9, 0, 3, 2, 51);
            c.SetPixel(11, 4, 0, 3, 2, 51);
            c.SetPixel(9, 8, 0, 3, 2, 51);
            c.SetPixel(3, 11, 1, 0, 5, 55);
            c.SetPixel(6, 10, 1, 0, 5, 55);
            c.SetPixel(11, 3, 1, 0, 5, 55);
            c.SetPixel(10, 6, 1, 0, 5, 55);
            c.SetPixel(1, 11, 1, 0, 0, 55);
            c.SetPixel(2, 11, 1, 0, 0, 55);
            c.SetPixel(7, 9, 1, 0, 0, 55);
            c.SetPixel(11, 1, 1, 0, 0, 55);
            c.SetPixel(11, 2, 1, 0, 0, 55);
            c.SetPixel(9, 7, 1, 0, 0, 55);
            c.SetPixel(0, 11, 0, 6, 10, 76);
            c.SetPixel(5, 10, 0, 6, 10, 76);
            c.SetPixel(8, 8, 0, 6, 10, 76);
            c.SetPixel(11, 0, 0, 6, 10, 76);
            c.SetPixel(10, 5, 0, 6, 10, 76);
            c.SetPixel(3, 10, 0, 3, 7, 76);
            c.SetPixel(4, 10, 0, 3, 7, 76);
            c.SetPixel(6, 9, 0, 3, 7, 76);
            c.SetPixel(10, 3, 0, 3, 7, 76);
            c.SetPixel(10, 4, 0, 3, 7, 76);
            c.SetPixel(9, 6, 0, 3, 7, 76);
            c.SetPixel(2, 10, 0, 3, 4, 76);
            c.SetPixel(7, 8, 0, 3, 4, 76);
            c.SetPixel(10, 2, 0, 3, 4, 76);
            c.SetPixel(8, 7, 0, 3, 4, 76);
            c.SetPixel(1, 10, 0, 0, 4, 78);
            c.SetPixel(5, 9, 0, 0, 4, 78);
            c.SetPixel(10, 1, 0, 0, 4, 78);
            c.SetPixel(9, 5, 0, 0, 4, 78);
            c.SetPixel(0, 10, 0, 0, 1, 78);
            c.SetPixel(4, 9, 0, 0, 1, 78);
            c.SetPixel(6, 8, 0, 0, 1, 78);
            c.SetPixel(10, 0, 0, 0, 1, 78);
            c.SetPixel(9, 4, 0, 0, 1, 78);
            c.SetPixel(8, 6, 0, 0, 1, 78);
            c.SetPixel(3, 9, 0, 3, 7, 102);
            c.SetPixel(9, 3, 0, 3, 7, 102);
            c.SetPixel(1, 9, 0, 3, 4, 102);
            c.SetPixel(2, 9, 0, 3, 4, 102);
            c.SetPixel(5, 8, 0, 3, 4, 102);
            c.SetPixel(9, 1, 0, 3, 4, 102);
            c.SetPixel(9, 2, 0, 3, 4, 102);
            c.SetPixel(8, 5, 0, 3, 4, 102);
            c.SetPixel(0, 9, 0, 0, 2, 102);
            c.SetPixel(4, 8, 0, 0, 2, 102);
            c.SetPixel(9, 0, 0, 0, 2, 102);
            c.SetPixel(8, 4, 0, 0, 2, 102);
            c.SetPixel(3, 8, 0, 1, 0, 103);
            c.SetPixel(8, 3, 0, 1, 0, 103);
            c.SetPixel(2, 8, 0, 3, 7, 128);
            c.SetPixel(8, 2, 0, 3, 7, 128);
            c.SetPixel(1, 8, 0, 3, 5, 128);
            c.SetPixel(8, 1, 0, 3, 5, 128);
            c.SetPixel(0, 8, 0, 1, 3, 128);
            c.SetPixel(8, 0, 0, 1, 3, 128);
            c.Lock();
            #endregion

            Bitmap v = new Bitmap(1, 13);
            #region Shadow Vertical
            v.Unlock();
            v.SetPixel(0, 12, 0, 13, 27, 26);
            v.SetPixel(0, 11, 0, 13, 27, 26);
            v.SetPixel(0, 10, 0, 13, 27, 26);
            v.SetPixel(0, 9, 0, 13, 27, 26);
            v.SetPixel(0, 8, 0, 13, 17, 26);
            v.SetPixel(0, 7, 0, 3, 17, 26);
            v.SetPixel(0, 6, 0, 3, 7, 26);
            v.SetPixel(0, 5, 0, 8, 12, 51);
            v.SetPixel(0, 4, 0, 3, 7, 51);
            v.SetPixel(0, 3, 0, 6, 10, 77);
            v.SetPixel(0, 2, 0, 0, 1, 78);
            v.SetPixel(0, 1, 0, 0, 2, 102);
            v.SetPixel(0, 0, 0, 1, 3, 128);
            v.Lock();
            #endregion

            Bitmap h = new Bitmap(13, 1);
            #region Shadow Vertical
            h.Unlock();
            h.SetPixel(12, 0, 0, 13, 27, 26);
            h.SetPixel(11, 0, 0, 13, 27, 26);
            h.SetPixel(10, 0, 0, 13, 27, 26);
            h.SetPixel(9, 0, 0, 13, 27, 26);
            h.SetPixel(8, 0, 0, 13, 17, 26);
            h.SetPixel(7, 0, 0, 3, 17, 26);
            h.SetPixel(6, 0, 0, 3, 7, 26);
            h.SetPixel(5, 0, 0, 8, 12, 51);
            h.SetPixel(4, 0, 0, 3, 7, 51);
            h.SetPixel(3, 0, 0, 6, 10, 77);
            h.SetPixel(2, 0, 0, 0, 1, 78);
            h.SetPixel(1, 0, 0, 0, 2, 102);
            h.SetPixel(0, 0, 0, 1, 3, 128);
            h.Lock();
            #endregion

            Sprites["topright"] = new Sprite(this.Viewport, c);
            Sprites["topright"].X = 146;
            Sprites["bottomright"] = new Sprite(this.Viewport, c);
            Sprites["bottomright"].X = 146;
            Sprites["bottomright"].MirrorY = true;

            Sprites["top"] = new Sprite(this.Viewport, v);
            Sprites["top"].ZoomX = 146;
            Sprites["bottom"] = new Sprite(this.Viewport, v);
            Sprites["bottom"].MirrorY = true;
            Sprites["bottom"].ZoomX = 146;

            Sprites["side1"] = new Sprite(this.Viewport, h);
            Sprites["side1"].X = 154;
            Sprites["side2"] = new Sprite(this.Viewport, h);
            Sprites["side2"].X = Sprites["side1"].X;

            Tabs = new List<List<string>>()
            {
                new List<string>() { "Species", "database_species" },
                new List<string>() { "Moves", "database_moves" },
                new List<string>() { "Abilities", "database_abilities" },
                new List<string>() { "Items", "database_items" },
                new List<string>() { "TM/HM", "database_tms" },
                new List<string>() { "Tilesets", "database_tilesets" },
                new List<string>() { "Autotiles", "database_autotiles" },
                new List<string>() { "Types", "database_types" },
                new List<string>() { "Trainers", "database_trainers" },
                new List<string>() { "Animations", "database_animations" },
                new List<string>() { "System", "database_system" }
            };
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            if (Size.Height < 41 * Tabs.Count) SetSize(355, 41 * Tabs.Count);
        }

        public void SetSelectedIndex(int Index)
        {
            if (this.SelectedIndex != Index)
            {
                this.SelectedIndex = Index;
                DBWidget.DBDataList.RedrawHeader();
                DBWidget.DBDataList.SetSelectedIndex(0);
                this.Redraw();
            }
        }

        protected override void Draw()
        {
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(355, Math.Max(Size.Height, 41 * Tabs.Count));
            Sprites["text"].Bitmap = new Bitmap(355, 41 * Tabs.Count);
            Sprites["text"].Bitmap.Font = Font.Get("Fonts/Ubuntu-B", 15);
            Sprites["text"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.Unlock();
            int y = 1;
            for (int i = 0; i < Tabs.Count; i++)
            {
                if (i == SelectedIndex)
                {
                    Sprites["topright"].Y = y - 10;
                    Sprites["bottomright"].Y = y + 29;
                    Sprites["top"].Y = y - 2;
                    Sprites["bottom"].Y = y + 28;
                    Sprites["side1"].ZoomY = y - 10;
                    Sprites["side2"].Y = y + 49;
                    Sprites["side2"].ZoomY = Size.Height - y - 49;
                }
                else
                {
                    Sprites["bg"].Bitmap.FillRect(0, y, 154, 39, new Color(61, 87, 139));
                    if (i != 0) Sprites["bg"].Bitmap.SetPixel(153, y, Color.ALPHA);
                    Sprites["bg"].Bitmap.SetPixel(153, y + 38, Color.ALPHA);
                }
                Bitmap icon = new Bitmap(Tabs[i][1]);
                Sprites["bg"].Bitmap.Build(33 - icon.Width / 2, y + 20 - icon.Height / 2, icon);
                Sprites["text"].Bitmap.DrawText(Tabs[i][0], 66, y + 10, Color.WHITE);
                y += 41;
            }
            if (Sprites["bg"].Bitmap.Height - y > 0)
            {
                Sprites["bg"].Bitmap.FillRect(0, y, 154, Sprites["bg"].Bitmap.Height - y, new Color(61, 87, 139));
                Sprites["bg"].Bitmap.SetPixel(153, y, Color.ALPHA);
            }
            Sprites["bg"].Bitmap.Lock();
            Sprites["text"].Bitmap.Lock();
            base.Draw();
        }
    }
}
