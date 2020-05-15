using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class DropdownBox : Widget
    {
        public string Text { get { return TextArea.Text; } }
        public Color TextColor { get { return TextArea.TextColor; } }
        public bool ReadOnly { get { return TextArea.ReadOnly; } }
        public int SelectedIndex { get; protected set; } = 0;
        public List<ListItem> Items { get; protected set; } = new List<ListItem>();
        public bool Enabled { get; protected set; } = true;

        public TextArea TextArea;

        public BaseEvent OnTextChanged { get { return TextArea.OnTextChanged; } set { TextArea.OnTextChanged = value; } }
        public BaseEvent OnDropDownClicked;
        public BaseEvent OnSelectionChanged;

        public DropdownBox(IContainer Parent) : base(Parent)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            TextArea = new TextArea(this);
            TextArea.SetPosition(3, 3);
            TextArea.SetCaretHeight(13);
            TextArea.SetZIndex(1);
            TextArea.SetReadOnly(true);
            SetSize(100, 21);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            TextArea.SetSize(this.Size.Width - 21, this.Size.Height - 3);
        }

        public void SetEnabled(bool Enabled)
        {
            if (this.Enabled != Enabled)
            {
                this.Enabled = Enabled;
                Redraw();
                TextArea.SetEnabled(this.Enabled);
            }
        }

        public void SetInitialText(string Text)
        {
            this.TextArea.SetInitialText(Text);
        }

        public void SetFont(Font f)
        {
            this.TextArea.SetFont(f);
        }

        public void SetTextColor(Color c)
        {
            this.TextArea.SetTextColor(c);
        }

        public void SetReadOnly(bool ReadOnly)
        {
            this.TextArea.SetReadOnly(ReadOnly);
        }

        public void SetSelectedIndex(int Index)
        {
            if (this.SelectedIndex != Index)
            {
                this.TextArea.SetInitialText(Items[Index].Name);
                this.SelectedIndex = Index;
                this.OnSelectionChanged?.Invoke(new BaseEventArgs());
            }
        }

        public void SetItems(List<ListItem> Items)
        {
            this.Items = Items;
            this.TextArea.SetInitialText(Items[SelectedIndex].Name);
        }

        protected override void Draw()
        {
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, 10, 23, 37);
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, Enabled ? new Color(54, 81, 108) : new Color(72, 72, 72));
            Sprites["bg"].Bitmap.FillRect(Size.Width - 18, 1, 17, Size.Height - 2, Enabled ? new Color(28, 50, 73) : new Color(72, 72, 72));
            int y = (int) Math.Floor(Size.Height / 2d) - 1;
            Color arrowcolor = Enabled ? Color.WHITE : new Color(28, 50, 73);
            Sprites["bg"].Bitmap.FillRect(Size.Width - 13, y, 7, 2, arrowcolor);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, y + 2, Size.Width - 8, y + 2, arrowcolor);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 11, y + 3, Size.Width - 9, y + 3, arrowcolor);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 10, y + 4, arrowcolor);
            Sprites["bg"].Bitmap.Lock();
            base.Draw();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (!TextArea.WidgetIM.Hovering && TextArea.SelectedWidget)
            {
                Window.UI.SetSelectedWidget(null);
            }
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx >= Size.Width - 18 && rx < Size.Width - 1 &&
                ry >= 1 && ry < Size.Height - 1 && this.Enabled)
            {
                this.OnDropDownClicked?.Invoke(new BaseEventArgs());
                if (this.Items.Count > 0)
                {
                    DropdownWidget dropdown = new DropdownWidget(Window.UI, this.Size.Width, this.Items);
                    dropdown.SetPosition(this.Viewport.X, this.Viewport.Y + this.Viewport.Height);
                    dropdown.SetSelected(SelectedIndex);
                    dropdown.OnDisposed += delegate (BaseEventArgs e)
                    {
                        if (dropdown.SelectedIndex != -1)
                        {
                            this.SetSelectedIndex(dropdown.SelectedIndex);
                        }
                    };
                }
            };
        }

        public override object GetValue(string Identifier)
        {
            if (Identifier == "bool") return this.SelectedIndex == 0;
            else return this.SelectedIndex;
        }

        public override void SetValue(string Identifier, object Value)
        {
            this.SetSelectedIndex(Convert.ToInt32(Value));
        }
    }

    public class DropdownWidget : Widget
    {
        public int SelectedIndex { get; protected set; }
        public int HoveringIndex { get; protected set; }

        public DropdownWidget(IContainer Parent, int Width, List<ListItem> Items) : base(Parent)
        {
            SetZIndex(Window.ActiveWidget is UIManager ? 9 : (Window.ActiveWidget as Widget).ZIndex + 9);
            SetSize(Width, Items.Count * 18 + 4);
            Sprites["box"] = new Sprite(this.Viewport);
            Sprites["box"].Bitmap = new Bitmap(this.Size);
            Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.DrawLine(1, 0, Size.Width - 2, 0, Color.BLACK);
            Sprites["box"].Bitmap.DrawLine(Size.Width - 1, 1, Size.Width - 1, Size.Height - 2, Color.BLACK);
            Sprites["box"].Bitmap.DrawLine(0, 1, 0, Size.Height - 2, Color.BLACK);
            Sprites["box"].Bitmap.DrawLine(1, Size.Height - 1, Size.Width - 2, Size.Height - 1, Color.BLACK);
            Sprites["box"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, new Color(45, 69, 107));
            Sprites["box"].Bitmap.Lock();
            Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(Width - 2, 18, new Color(86, 108, 134)));
            Sprites["selector"].X = 1;
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].Bitmap = new Bitmap(this.Size);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = Font.Get("Fonts/ProductSans-M", 12);
            for (int i = 0; i < Items.Count; i++)
            {
                Sprites["text"].Bitmap.DrawText(Items[i].Name, 6, i * 18 + 2, Color.WHITE);
            }
            Sprites["text"].Bitmap.Lock();
            Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 18, new Color(55, 187, 255)));
            Sprites["hover"].X = 1;
            Sprites["hover"].Visible = false;
            WindowLayer = Window.ActiveWidget.WindowLayer + 1;
            Window.SetActiveWidget(this);
        }

        public override void Dispose()
        {
            if (this.Window.ActiveWidget == this)
            {
                this.Window.Widgets.RemoveAt(Window.Widgets.Count - 1);
                this.Window.SetActiveWidget(Window.Widgets[Window.Widgets.Count - 1]);
            }
            base.Dispose();
        }

        public void SetSelected(int Index)
        {
            Sprites["selector"].Y = 2 + 18 * Index;
            Sprites["selector"].Visible = true;
            SelectedIndex = Index;
        }

        public void SetHovering(int Index)
        {
            if (Index == -1)
            {
                Sprites["hover"].Visible = false;
            }
            else
            {
                Sprites["hover"].Y = 2 + 18 * Index;
                Sprites["hover"].Visible = true;
            }
            HoveringIndex = Index;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (!WidgetIM.Hovering) SelectedIndex = -1;
            else SelectedIndex = HoveringIndex;
            Dispose();
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            if (WidgetIM.Hovering)
            {
                int ry = e.Y - Viewport.Y;
                if (ry < 2 || ry >= this.Size.Height - 2) SetHovering(-1);
                SetHovering((int) Math.Floor(ry / 18d));
            }
            else SetHovering(-1);
        }
    }
}
