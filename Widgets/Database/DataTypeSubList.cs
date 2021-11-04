using System;
using System.Collections.Generic;
using System.Text;
using amethyst;
using odl;

namespace RPGStudioMK.Widgets
{
    public class DataTypeSubList : Widget
    {
        public BaseEvent OnSelectionChanged;

        Container ScrollContainer;
        Widget TextWidget;

        public int SelectedIndex { get; protected set; } = -1;
        public int HoveringIndex { get; protected set; } = -1;

        public List<ListItem> Items = new List<ListItem>();

        public DataTypeSubList(IContainer Parent) : base(Parent)
        {
            Sprites["line1"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, Color.BLACK));
            Sprites["line1"].X = 168;
            Sprites["line2"] = new Sprite(this.Viewport, new SolidBitmap(1, 1, Color.BLACK));
            Sprites["line2"].X = 180;
            ScrollContainer = new Container(this);
            VScrollBar vs = new VScrollBar(this);
            vs.SetPosition(170, 1);
            ScrollContainer.SetVScrollBar(vs);
            ScrollContainer.VAutoScroll = true;
            TextWidget = new Widget(ScrollContainer);
            TextWidget.Sprites["text"] = new Sprite(this.Viewport);
            TextWidget.Sprites["text"].X = 11;
            TextWidget.Sprites["text"].Y = 9;
            TextWidget.Sprites["selection"] = new Sprite(this.Viewport, new SolidBitmap(2, 20, new Color(37, 192, 250)));
            TextWidget.Sprites["selection"].Visible = false;
            SetWidth(181);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            ScrollContainer.SetSize(Size);
            ScrollContainer.VScrollBar.SetHeight(Size.Height - 2);
            ScrollContainer.UpdateAutoScroll();
            ((SolidBitmap) Sprites["line1"].Bitmap).SetSize(1, Size.Height);
            ((SolidBitmap) Sprites["line2"].Bitmap).SetSize(1, Size.Height);
            Sprites["line1"].Visible = ScrollContainer.VScrollBar.Visible;
        }

        public void SetItems(List<ListItem> Items)
        {
            this.Items = Items;
            RedrawText();
        }

        public void SetSelectedIndex(int SelectedIndex)
        {
            if (this.SelectedIndex != SelectedIndex)
            {
                this.SelectedIndex = SelectedIndex;
                this.OnSelectionChanged?.Invoke(new BaseEventArgs());
                RedrawText();
            }
        }

        public void RedrawText()
        {
            TextWidget.Sprites["text"].Bitmap?.Dispose();
            Font f = Font.Get("Fonts/Ubuntu-B", 14);
            TextWidget.Sprites["text"].Bitmap = new Bitmap(Size.Width, 20 * Items.Count);
            TextWidget.SetSize(Size.Width, 18 + 20 * Items.Count);
            TextWidget.Sprites["text"].Bitmap.Unlock();
            TextWidget.Sprites["text"].Bitmap.Font = f;
            for (int i = 0; i < Items.Count; i++)
            {
                string Text = Items[i].ToString();
                Size s = f.TextSize(Text);
                TextWidget.Sprites["text"].Bitmap.DrawText(Text, 0, i * 20 + 2, SelectedIndex == i ? new Color(37, 192, 250) : Color.WHITE);
            }
            TextWidget.Sprites["text"].Bitmap.Lock();
            Sprites["line1"].Visible = 20 * Items.Count > ScrollContainer.Size.Height;
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            if (!WidgetIM.Hovering)
            {
                TextWidget.Sprites["selection"].Visible = false;
                HoveringIndex = -1;
                return;
            }
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y + TextWidget.Position.Y - TextWidget.ScrolledPosition.Y;
            if (ry < 9 || ry >= Items.Count * 20 + 9 ||
                rx >= 170 && ScrollContainer.VScrollBar.Visible ||
                ScrollContainer.VScrollBar.SliderDragging)
            {
                TextWidget.Sprites["selection"].Visible = false;
                HoveringIndex = -1;
                return;
            }
            HoveringIndex = (int) Math.Floor((ry - 9) / 20d);
            if (HoveringIndex >= Items.Count)
            {
                TextWidget.Sprites["selection"].Visible = false;
                HoveringIndex = -1;
                return;
            }
            TextWidget.Sprites["selection"].Y = 9 + HoveringIndex * 20;
            TextWidget.Sprites["selection"].Visible = true;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (!WidgetIM.Hovering || HoveringIndex == -1) return;
            SetSelectedIndex(HoveringIndex);
        }
    }
}
