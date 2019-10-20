using System;
using ODL;

namespace MKEditor.Widgets
{
    public class PopupWindow : Widget
    {
        public bool Blocked = false;
        public string DisplayName { get; protected set; }

        public EventHandler<EventArgs> OnClosed;

        public PopupWindow(object Parent, string Name = "popupWindow")
            : base(Parent, Name)
        {
            Window.SetOverlayOpacity(128);
            Sprites["window"] = new RectSprite(this.Viewport, new Size(this.Size.Width - 14, this.Size.Height - 14),
                new Color(59, 227, 255), new Color(40, 62, 84));
            Sprites["name"] = new Sprite(this.Viewport);
            Sprites["name"].X = 5;
            Sprites["name"].Y = 3;
            this.WindowLayer = Window.ActiveWidget.WindowLayer + 1;
            this.Window.SetActiveWidget(this);
            Window.SetOverlayZIndex(WindowLayer * 10 - 1);
            this.SetZIndex(WindowLayer * 10);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            (Sprites["window"] as RectSprite).SetSize(this.Size.Width, this.Size.Height);
        }

        public override void ParentSizeChanged(object sender, SizeEventArgs e)
        {
            base.ParentSizeChanged(sender, e);
            Center();
        }

        public void Center()
        {
            int width = Window.Width;
            int height = Window.Height;
            this.SetPosition(width / 2 - (this.Size.Width) / 2, height / 2 - (this.Size.Height) / 2);
        }

        public void SetName(string Name)
        {
            this.DisplayName = Name;
            Font f = Font.Get("Fonts/ProductSans-B", 14);
            Size s = f.TextSize(Name);
            if (Sprites["name"].Bitmap != null) Sprites["name"].Bitmap.Dispose();
            Sprites["name"].Bitmap = new Bitmap(s);
            Sprites["name"].Bitmap.Unlock();
            Sprites["name"].Bitmap.Font = f;
            Sprites["name"].Bitmap.DrawText(Name, Color.WHITE);
            Sprites["name"].Bitmap.Lock();
        }

        public void Close()
        {
            Dispose();
            if (OnClosed != null) OnClosed.Invoke(null, new EventArgs());
        }

        public override void Dispose()
        {
            if (this.Window.ActiveWidget == this)
            {
                // Remove current widget/window
                this.Window.Widgets.RemoveAt(Window.Widgets.Count - 1);
                // Set the last (undisposed) widget as active
                for (int i = 0; i < Window.Widgets.Count; i++)
                {
                    IContainer widget = Window.Widgets[Window.Widgets.Count - i - 1];
                    if (widget is Widget && (widget as Widget).Disposed)
                    {
                        Window.Widgets.RemoveAt(Window.Widgets.Count - i - 1);
                        i--;
                        continue;
                    }
                    Window.SetActiveWidget(widget);
                    break;
                }
                // Update overlay Z
                Window.SetOverlayZIndex(Window.ActiveWidget.WindowLayer * 10 - 1);
            }
            base.Dispose();
        }
    }
}
