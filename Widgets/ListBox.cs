using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class ListBox : Widget
    {
        public List<ListItem> Items = new List<ListItem>();
        public int SelectedIndex { get { return ListDrawer.SelectedIndex; } }
        public string ButtonText;
        public bool HasButton { get { return ButtonText != null; } }

        public ListItem SelectedItem { get { return SelectedIndex == -1 ? null : Items[SelectedIndex]; } }

        public Container MainContainer;
        public ListDrawer ListDrawer;

        public EventHandler<EventArgs> OnSelectionChanged;

        public ListBox(object Parent, string Name = "listBox")
            : base(Parent, Name)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            MainContainer = new Container(this);
            MainContainer.SetPosition(1, 2);
            MainContainer.VAutoScroll = true;
            ListDrawer = new ListDrawer(MainContainer);
            VScrollBar vs = new VScrollBar(this);
            MainContainer.SetVScrollBar(vs);
            SetSize(132, 174);
        }

        public void SetButtonText(string Text)
        {
            if (this.ButtonText != Text)
            {
                this.ButtonText = Text;
                ListDrawer.RedrawButton();
            }
        }

        public void SetItems(List<ListItem> Items)
        {
            if (this.Items != Items)
            {
                this.Items = Items;
                ListDrawer.SetSize(ListDrawer.Size.Width, 20 * (Items.Count + 1));
                ListDrawer.Redraw();
            }
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.DrawRect(Size, 86, 108, 134);
            Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, 10, 23, 37);
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(1, 1, 40, 62, 84);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 1, 40, 62, 84);
            Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 2, 40, 62, 84);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, 40, 62, 84);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 1, Size.Width - 12, Size.Height - 2, 40, 62, 84);
            Sprites["bg"].Bitmap.Lock();
            MainContainer.SetSize(Size.Width - 13, Size.Height - 4);
            MainContainer.VScrollBar.SetPosition(Size.Width - 10, 2);
            ListDrawer.SetWidth(MainContainer.Size.Width);
            MainContainer.VScrollBar.SetSize(8, Size.Height - 4);
        }

        public void SetSelectedIndex(int idx)
        {
            ListDrawer.SelectedIndex = idx;
            if (idx == -1)
            {
                ListDrawer.Sprites["selection"].Visible = false;
            }
            else
            {
                ListDrawer.Sprites["selection"].Visible = true;
                ListDrawer.Sprites["selection"].Y = 20 * idx;
            }
            if (OnSelectionChanged != null) OnSelectionChanged.Invoke(null, new EventArgs());
        }

        protected override void Draw()
        {
            ListDrawer.SetSize(ListDrawer.Size.Width, 20 * (Items.Count + 1));
            ListDrawer.ForceRedraw();
            base.Draw();
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
            return Name ?? Object.ToString();
        }
    }

    public class ListDrawer : Widget
    {
        public EventHandler<EventArgs> OnButtonClicked;
        bool HasButton { get { return (Parent.Parent as ListBox).HasButton; } }
        private bool HoveringButton = false;
        public int SelectedIndex = -1;

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

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            (Sprites["selection"].Bitmap as SolidBitmap).SetSize(Size.Width, 20);
        }

        public void ForceRedraw()
        {
            this.Draw();
            MouseMoving(null, Graphics.LastMouseEvent);
        }

        public void RedrawButton()
        {
            if (Sprites["btn"].Bitmap != null) Sprites["btn"].Bitmap.Dispose();
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
            Sprites["btn"].Bitmap.DrawText((Parent.Parent as ListBox).ButtonText, 15, 0, HoveringButton ? Color.BLACK : Color.WHITE);
            Sprites["btn"].Bitmap.Lock();
        }

        protected override void Draw()
        {
            if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
            Sprites["text"].Bitmap = new Bitmap(Size);
            Font f = Font.Get("Fonts/ProductSans-M", 13);
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.Unlock();
            for (int i = 0; i < (Parent.Parent as ListBox).Items.Count; i++)
            {
                ListItem item = (Parent.Parent as ListBox).Items[i];
                Sprites["text"].Bitmap.DrawText(item.ToString(), 10, 20 * i, Color.WHITE);
            }
            Sprites["text"].Bitmap.Lock();
            Sprites["btn"].Y = 20 * (Parent.Parent as ListBox).Items.Count;
            base.Draw();
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
            int index = (int) Math.Floor(ry / 20d);
            if (!HasButton && index == (Parent.Parent as ListBox).Items.Count) return;
            Sprites["hover"].Visible = false;
            bool OldHover = HoveringButton;
            HoveringButton = false;
            if (!WidgetIM.Hovering) { }
            else if (index == (Parent.Parent as ListBox).Items.Count) // Hovering over button
            {
                if (rx >= 7 && rx < 7 + Sprites["btn"].Bitmap.Width) HoveringButton = true;
            }
            else // Hovering over item
            {
                Sprites["hover"].Visible = true;
                Sprites["hover"].Y = index * 20;
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
                SelectedIndex = -1;
                if (OnButtonClicked != null) OnButtonClicked.Invoke(sender, e);
                if ((Parent.Parent as ListBox).OnSelectionChanged != null) (Parent.Parent as ListBox).OnSelectionChanged.Invoke(null, new EventArgs());
            }
            else if (Sprites["hover"].Visible)
            {
                Sprites["selection"].Y = Sprites["hover"].Y;
                Sprites["selection"].Visible = true;
                SelectedIndex = Sprites["hover"].Y / 20;
                if ((Parent.Parent as ListBox).OnSelectionChanged != null) (Parent.Parent as ListBox).OnSelectionChanged.Invoke(null, new EventArgs());
            }
        }
    }
}
