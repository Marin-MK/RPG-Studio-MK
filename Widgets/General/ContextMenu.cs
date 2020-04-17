using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class ContextMenu : Widget
    {
        public List<IMenuItem> Items = new List<IMenuItem>();
        public IMenuItem SelectedItem;

        public BaseEvent OnItemInvoked;

        public ContextMenu(IContainer Parent) : base(Parent)
        {
            this.SetZIndex(Window.ActiveWidget is UIManager ? 9 : (Window.ActiveWidget as Widget).ZIndex + 9);
            this.SetWidth(192);
            this.Sprites["bg"] = new RectSprite(this.Viewport);
            (this.Sprites["bg"] as RectSprite).SetColor(Color.BLACK, new Color(45, 69, 107));
            this.Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(2, 18, new Color(55, 187, 255)));
            this.Sprites["selector"].X = 4;
            this.Sprites["selector"].Visible = false;
            this.Sprites["items"] = new Sprite(this.Viewport);
            this.OnHelpTextWidgetCreated += HelpTextWidgetCreated;
            this.OnFetchHelpText += FetchHelpText;
            
            this.WindowLayer = Window.ActiveWidget.WindowLayer + 1;
            this.Window.SetActiveWidget(this);
        }

        public void SetInnerColor(byte R, byte G, byte B, byte A = 255)
        {
            SetInnerColor(new Color(R, G, B, A));
        }
        public void SetInnerColor(Color c)
        {
            (Sprites["bg"] as RectSprite).SetInnerColor(c);
        }

        public void SetOuterColor(byte R, byte G, byte B, byte A = 255)
        {
            SetOuterColor(new Color(R, G, B, A));
        }
        public void SetOuterColor(Color c)
        {
            (Sprites["bg"] as RectSprite).SetOuterColor(c);
        }

        public void SetItems(List<IMenuItem> Items)
        {
            this.Items = new List<IMenuItem>(Items);
            this.SetSize(192, CalcHeight() + 10);
            (this.Sprites["bg"] as RectSprite).SetSize(this.Size);
        }

        protected override void Draw()
        {
            if (this.Sprites["items"].Bitmap != null) this.Sprites["items"].Bitmap.Dispose();
            this.Sprites["items"].Bitmap = new Bitmap(192, CalcHeight() + 10);
            Font f = Font.Get("Fonts/ProductSans-M", 12);
            this.Sprites["items"].Bitmap.Font = f;
            this.Sprites["items"].Bitmap.Unlock();

            int y = 5;
            for (int i = 0; i < this.Items.Count; i++)
            {
                IMenuItem item = this.Items[i];
                if (item is MenuItem)
                {
                    MenuItem menuitem = item as MenuItem;
                    Color c = Color.WHITE;
                    if (menuitem.IsClickable != null)
                    {
                        BoolEventArgs e = new BoolEventArgs();
                        menuitem.IsClickable(e);
                        if (!e.Value) c = new Color(155, 164, 178);
                        menuitem.LastClickable = e.Value;
                    }
                    this.Sprites["items"].Bitmap.DrawText(menuitem.Text, 10, y + 4, c);
                    if (!string.IsNullOrEmpty(menuitem.Shortcut))
                        this.Sprites["items"].Bitmap.DrawText(menuitem.Shortcut, Size.Width - 9, y + 4, c, DrawOptions.RightAlign);
                    y += 23;
                }
                else if (item is MenuSeparator)
                {
                    this.Sprites["items"].Bitmap.DrawLine(6, y + 2, Size.Width - 12, y + 2, 38, 56, 82);
                    y += 5;
                }
            }
            this.Sprites["items"].Bitmap.Lock();
            base.Draw();
        }

        public override void Dispose()
        {
            if (this.Window.ActiveWidget == this)
            {
                this.Window.Widgets.RemoveAt(Window.Widgets.Count - 1);
                this.Window.SetActiveWidget(Window.Widgets[Window.Widgets.Count - 1]);
            }
            if (HelpTextWidget != null) HelpTextWidget.Dispose();
            base.Dispose();
        }

        private int CalcHeight(int upto = -1)
        {
            int h = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                if (upto == -1 || i <= upto)
                {
                    if (Items[i] is MenuSeparator) h += 5;
                    else h += 23;
                }
            }
            return h;
        }

        public override void HoverChanged(MouseEventArgs e)
        {
            base.HoverChanged(e);
            this.MouseMoving(e);
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            int rx = e.X - this.Viewport.X;
            int ry = e.Y - this.Viewport.Y;
            if (!WidgetIM.Hovering || rx < 0 || rx > this.Size.Width)
            {
                this.Sprites["selector"].Visible = false;
                this.SelectedItem = null;
                return;
            }
            int y = 4;
            if (ry < y)
            {
                Sprites["selector"].Visible = false;
                this.SelectedItem = null;
                return;
            }
            IMenuItem OldSelected = SelectedItem;
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (Items[i] is MenuItem)
                {
                    if (y <= ry && y + 23 > ry)
                    {
                        Sprites["selector"].Y = y + 2;
                        Sprites["selector"].Visible = true;
                        this.SelectedItem = Items[i];
                        break;
                    }
                    y += 23;
                }
                else
                {
                    if (y <= ry && y + 5 > ry)
                    {
                        Sprites["selector"].Visible = false;
                        this.SelectedItem = Items[i];
                        break;
                    }
                    y += 5;
                }
            }
            if (OldSelected != SelectedItem)
            {
                if (HelpTextWidget != null) HelpTextWidget.Dispose();
                HelpTextWidget = null;
            }
        }

        public void TryClick(MouseEventArgs e)
        {
            if ((this.SelectedItem as MenuItem).LastClickable)
            {
                (SelectedItem as MenuItem).OnLeftClick?.Invoke(e);
                OnItemInvoked?.Invoke(new BaseEventArgs());
                this.Dispose();
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering && this.SelectedItem != null && this.SelectedItem is MenuItem)
            {
                TryClick(e);
            }
        }

        public void HelpTextWidgetCreated(BaseEventArgs e)
        {
            HelpTextWidget.SetZIndex(this.ZIndex);
        }

        public override void FetchHelpText(StringEventArgs e)
        {
            base.FetchHelpText(e);
            e.String = null;
            if (SelectedItem != null && SelectedItem is MenuItem)
            {
                e.String = (SelectedItem as MenuItem).HelpText;
                if (!(SelectedItem as MenuItem).LastClickable) e.String += "\nUnavailable.";
            }
        }
    }
}
