using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class MenuStrip : Widget
    {
        private List<IMenuItem> Items = new List<IMenuItem>();
        private List<int> Widths = new List<int>();
        private int HoverIndex = -1;
        public int SelectedIndex = -1;
        private List<Sprite> Boxes = new List<Sprite>();

        public MenuStrip(object Parent, string Name = "menuStrip")
            : base(Parent, Name)
        {
            this.WidgetIM.OnMouseMoving += this.MouseMoving;
            this.WidgetIM.OnMouseDown += this.MouseDown;
            this.Size = new Size(this.Window.Width - this.Viewport.X, this.Window.Height - this.Viewport.Y);
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Items = new List<IMenuItem>()
            {
                new MenuItem("menuItem1")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Option 1")
                        {
                            Items = new List<IMenuItem>()
                            {
                                new MenuItem("Sub Option 1"),
                                new MenuItem("Sub Option 2")
                            }
                        },
                        new MenuItem("Option 2"),
                        new MenuItem("Option 3"),
                        new MenuSeparator(),
                        new MenuItem("Option 4")
                    }
                },
                new MenuItem("menuItem2")
                {
                    Items = new List<IMenuItem>()
                    {
                        new MenuItem("Option 5"),
                        new MenuItem("Option 6")
                    }
                }
            };
        }

        protected override void Draw()
        {
            if (SelectedIndex == -1)
            {
                this.Boxes.ForEach(b => b.Dispose());
                this.Boxes.Clear();
            }
            if (this.Sprites["text"].Bitmap != null) this.Sprites["text"].Bitmap.Dispose();
            this.Sprites["text"].Bitmap = new Bitmap(this.Size.Width, 24);
            this.Sprites["text"].Bitmap.FillRect(this.Size, Color.WHITE);
            Font f = Font.Get("Fonts/Segoe UI", 12);
            this.Sprites["text"].Bitmap.Font = f;
            int x = 15;
            this.Widths.Clear();
            for (int i = 0; i < this.Items.Count; i++) this.Widths.Add(0);
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (this.Items[i] is MenuSeparator) throw new Exception("Can't have a MenuSeparator in the MenuStrip root");

                string Text = (this.Items[i] as MenuItem).Text;
                Widths[i] = f.TextSize(Text).Width;

                if (i == SelectedIndex)
                {
                    Color BorderColor = new Color(128, 128, 128);
                    this.Sprites["text"].Bitmap.DrawLine(x - 9, 2, x - 9, 21, BorderColor);
                    this.Sprites["text"].Bitmap.DrawLine(x - 8, 2, x + Widths[i] + 12, 2, BorderColor);
                    this.Sprites["text"].Bitmap.DrawLine(x + Widths[i] + 12, 2, x + Widths[i] + 12, 21, BorderColor);
                    this.Sprites["text"].Bitmap.FillRect(x - 8, 3, Widths[i] + 19, 18, new Color(251, 251, 251));
                    CreateWindow(this.Items[SelectedIndex] as MenuItem, x - 9, 21, Widths[i] + 22);
                }
                else if (i == HoverIndex)
                {
                    this.Sprites["text"].Bitmap.DrawRect(x - 9, 2, Widths[i] + 21, 20, new Color(0, 120, 215));
                    this.Sprites["text"].Bitmap.FillRect(x - 8, 3, Widths[i] + 19, 18, new Color(179, 215, 243));
                }

                this.Sprites["text"].Bitmap.DrawText(Text, x, 3, Color.BLACK);
                x += Widths[i] + 21;
            }
            base.Draw();
        }

        private void CreateWindow(MenuItem item, int ox, int oy, int topleftwidth)
        {
            int innerheight = 1;
            for (int i = 0; i < item.Items.Count; i++)
            {
                if (item.Items[i] is MenuItem) innerheight += 22;
                else if (item.Items[i] is MenuSeparator) innerheight += 6;
            }
            Sprite box = new Sprite(this.Viewport, 121, innerheight + 2);
            Boxes.Add(box);
            box.Bitmap.Font = Font.Get("Fonts/Segoe UI", 12);
            box.X = ox;
            box.Y = oy;
            Color BorderColor = new Color(128, 128, 128);
            box.Bitmap.DrawLines(BorderColor,
                new Point(0, 0),
                new Point(0, innerheight + 1),
                new Point(120, innerheight + 1),
                new Point(120, 0),
                new Point(topleftwidth, 0)
            );
            box.Bitmap.FillRect(1, 0, topleftwidth - 2, 2, new Color(253, 253, 253));
            box.Bitmap.FillRect(25, 1, 95, innerheight, new Color(253, 253, 253));
            box.Bitmap.FillRect(1, 2, 24, innerheight - 1, new Color(238, 238, 238));
            int y = 0;
            for (int i = 0; i < item.Items.Count; i++)
            {
                if (item.Items[i] is MenuItem)
                {
                    box.Bitmap.DrawText((item.Items[i] as MenuItem).Text, 36, y + 3, Color.BLACK);
                    if ((item.Items[i] as MenuItem).Items.Count > 0)
                    {
                        box.Bitmap.DrawLine(105, y + 10, 105, y + 16, Color.BLACK);
                        box.Bitmap.DrawLine(106, y + 11, 106, y + 15, Color.BLACK);
                        box.Bitmap.DrawLine(107, y + 12, 107, y + 14, Color.BLACK);
                        box.Bitmap.SetPixel(108, y + 13, Color.BLACK);
                    }
                    y += 22;
                }
                else if (item.Items[i] is MenuSeparator)
                {
                    box.Bitmap.DrawLine(33, y + 3, 33 + 85, y + 3, new Color(189, 189, 189));
                    y += 6;
                }
            }
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            UpdateMenu(e);
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            UpdateMenu(e);
        }

        private void UpdateMenu(MouseEventArgs e)
        {
            int fullwidth = 0;
            this.Widths.ForEach(w => fullwidth += w + 21);
            fullwidth = Math.Min(fullwidth, this.Size.Width - 7);
            int rx = e.X - this.Viewport.X;
            int ry = e.Y - this.Viewport.Y;
            int oldidx = HoverIndex;
            if (rx >= 6 && ry >= 2 && ry < 22 && rx < 6 + fullwidth)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    rx -= this.Widths[i] + 26;
                    if (rx <= 0)
                    {
                        HoverIndex = i;
                        break;
                    }
                }
            }
            else
            {
                HoverIndex = -1;
            }
            if (oldidx != HoverIndex)
            {
                this.Redraw();
            }
            if (e.LeftButton && !e.OldLeftButton)
            {
                if (HoverIndex != -1 && (this.Items[HoverIndex] as MenuItem).Items.Count > 0)
                {
                    if (SelectedIndex == HoverIndex) SelectedIndex = -1;
                    else SelectedIndex = HoverIndex;
                }
                else
                {
                    SelectedIndex = -1;
                }
                this.Redraw();
            }
        }
    }
}
