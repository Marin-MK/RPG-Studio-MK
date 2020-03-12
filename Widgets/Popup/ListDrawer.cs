﻿using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class ListDrawer : Widget
    {
        public EventHandler<EventArgs> OnButtonClicked;
        public EventHandler<EventArgs> OnSelectionChanged;

        public bool Button { get; protected set; } = false;
        public string ButtonText { get; protected set; }

        public Font Font { get; protected set; } = Font.Get("Fonts/ProductSans-M", 13);
        public int LineHeight { get; protected set; } = 20;

        public List<ListItem> Items { get; protected set; } = new List<ListItem>();

        private bool HoveringButton = false;

        public int SelectedIndex { get; protected set; } = -1;
        public ListItem SelectedItem { get { return SelectedIndex == -1 ? null : Items[SelectedIndex]; } }

        public ListDrawer(object Parent, string Name = "listDrawer")
            : base(Parent, Name)
        {
            Sprites["selection"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width, 20, new Color(28, 50, 73)));
            Sprites["selection"].Visible = false;
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 20, new Color(59, 227, 255)));
            Sprites["hover"].Visible = false;
            WidgetIM.OnMouseMoving += MouseMoving;
            WidgetIM.OnHoverChanged += HoverChanged;
            WidgetIM.OnMouseDown += MouseDown;
            Sprites["btn"] = new Sprite(this.Viewport);
            Sprites["btn"].X = 7;
        }

        public void SetItems(List<ListItem> Items)
        {
            this.Items = Items;
            Redraw();
            if (SelectedIndex >= Items.Count) SetSelectedIndex(Items.Count - 1);
        }

        public void SetFont(Font f)
        {
            if (this.Font != f)
            {
                this.Font = f;
                Redraw();
            }
        }

        public void SetLineHeight(int Height)
        {
            if (this.LineHeight != Height)
            {
                this.LineHeight = Height;
                if (SelectedIndex != -1) Sprites["selection"].Y = LineHeight * SelectedIndex;
                Redraw();
            }
        }

        public void SetButton(bool Exist, string ButtonText = "")
        {
            if (!Exist)
            {
                this.Button = false;
                this.ButtonText = null;
            }
            else
            {
                this.Button = true;
                this.ButtonText = ButtonText;
            }
            RedrawButton();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            (Sprites["selection"].Bitmap as SolidBitmap).SetSize(Size.Width, LineHeight);
        }

        public void SetSelectedIndex(int Index)
        {
            if (this.SelectedIndex != Index)
            {
                this.SelectedIndex = Index;
                if (Index == -1)
                {
                    Sprites["selection"].Visible = false;
                }
                else
                {
                    Sprites["selection"].Visible = true;
                    Sprites["selection"].Y = LineHeight * Index;
                }
                if (this.OnSelectionChanged != null) this.OnSelectionChanged.Invoke(null, new EventArgs());
            }
        }

        public void ForceRedraw()
        {
            this.Draw();
            MouseMoving(null, Graphics.LastMouseEvent);
        }

        public void RedrawButton()
        {
            if (Sprites["btn"].Bitmap != null) Sprites["btn"].Bitmap.Dispose();
            if (!Button) return;
            Sprites["btn"].Bitmap = new Bitmap(92, 16);
            Sprites["btn"].Bitmap.Unlock();
            Sprites["btn"].Bitmap.FillRect(0, 0, 92, 16, HoveringButton ? new Color(59, 227, 255) : new Color(86, 108, 134));
            Sprites["btn"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["btn"].Bitmap.SetPixel(91, 0, Color.ALPHA);
            Sprites["btn"].Bitmap.SetPixel(0, 15, Color.ALPHA);
            Sprites["btn"].Bitmap.SetPixel(91, 15, Color.ALPHA);
            Sprites["btn"].Bitmap.DrawLine(5, 7, 9, 7, HoveringButton ? Color.BLACK : Color.WHITE);
            Sprites["btn"].Bitmap.DrawLine(7, 5, 7, 9, HoveringButton ? Color.BLACK : Color.WHITE);
            Font f = Font.Get("Fonts/Ubuntu-B", 12);
            Sprites["btn"].Bitmap.Font = f;
            Sprites["btn"].Bitmap.DrawText(this.ButtonText, 15, 0, HoveringButton ? Color.BLACK : Color.WHITE);
            Sprites["btn"].Bitmap.Lock();
        }

        protected override void Draw()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            SetSize(Size.Width, LineHeight * (Items.Count + (Button ? 1 : 0)));
            Sprites["text"].Bitmap = new Bitmap(Size);
            Sprites["text"].Bitmap.Font = this.Font;
            Sprites["text"].Bitmap.Unlock();
            for (int i = 0; i < this.Items.Count; i++)
            {
                Sprites["text"].Bitmap.DrawText(this.Items[i].ToString(), 10, LineHeight * i, Color.WHITE);
            }
            Sprites["text"].Bitmap.Lock();
            Sprites["btn"].Y = LineHeight * this.Items.Count;
            base.Draw();
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
            int index = (int) Math.Floor(ry / (double) LineHeight);
            if (!Button && index == this.Items.Count) return;
            Sprites["hover"].Visible = false;
            bool OldHover = HoveringButton;
            HoveringButton = false;
            if (!WidgetIM.Hovering) { }
            else if (index == this.Items.Count) // Hovering over button
            {
                if (rx >= 7 && rx < 7 + Sprites["btn"].Bitmap.Width) HoveringButton = true;
            }
            else // Hovering over item
            {
                Sprites["hover"].Visible = true;
                Sprites["hover"].Y = index * LineHeight;
            }
            if (OldHover != HoveringButton) RedrawButton();
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            if (!WidgetIM.Hovering) Sprites["hover"].Visible = false;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (!WidgetIM.Hovering) return;
            if (HoveringButton)
            {
                Sprites["selection"].Visible = false;
                this.SelectedIndex = -1;
                if (this.OnButtonClicked != null) this.OnButtonClicked.Invoke(sender, e);
                if (this.OnSelectionChanged != null) this.OnSelectionChanged.Invoke(null, new EventArgs());
            }
            else if (Sprites["hover"].Visible)
            {
                Sprites["selection"].Y = Sprites["hover"].Y;
                Sprites["selection"].Visible = true;
                this.SelectedIndex = Sprites["hover"].Y / this.LineHeight;
                if (this.OnSelectionChanged != null) this.OnSelectionChanged.Invoke(null, new EventArgs());
            }
        }
    }

    public class ListItem
    {
        public string Name;
        public object Object;

        public ListItem(string Name, object Object)
        {
            this.Name = Name;
            this.Object = Object;
        }

        public ListItem(object Object)
        {
            this.Object = Object;
        }

        public override string ToString()
        {
            return Name ?? (Object is null ? "" : Object.ToString());
        }
    }
}