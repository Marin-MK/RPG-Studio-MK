using amethyst.Animations;
using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class PopupWindow : Widget, IPopupWindow
{
    public bool Blocked = false;
    public string Title { get; protected set; }

    public BaseEvent OnClosed;
    public List<Button> Buttons = new List<Button>();

    public int WindowEdges = 7;

    bool HasAnimated = false;
    bool ShowAnimation;

    public PopupWindow(bool ShowAnimation = true) : base(((UIWindow) Graphics.Windows[0]).UI)
    {
        Window.SetOverlayOpacity(96);
        Sprites["shadow"] = new Sprite(this.Viewport);
        Sprites["window"] = new Sprite(this.Viewport);
        Sprites["window"].X = WindowEdges;
        Sprites["window"].Y = WindowEdges;
        Sprites["title"] = new Sprite(this.Viewport);
        Sprites["title"].X = 5 + WindowEdges;
        Sprites["title"].Y = 3 + WindowEdges;
        this.WindowLayer = Window.ActiveWidget.WindowLayer + 1;
        this.Window.SetActiveWidget(this);
        Window.SetOverlayZIndex(WindowLayer * 10 - 1);
        this.SetZIndex(WindowLayer * 10);
        Editor.CanUndo = false;
        this.ShowAnimation = ShowAnimation;
        if (this.ShowAnimation)
        {
            SetVisible(false);
        }
        else
        {
            this.HasAnimated = true;
        }
    }

    public override void Update()
    {
        if (!HasAnimated)
        {
            // Do this in the update method rather than the constructor, because some window constructors
            StartAnimation(new SigmoidAnimation("zoom_in", 400, x =>
            {
                SetGlobalZoom((float) x);
                Center();
                SetVisible(true);
            }));
            HasAnimated = true;
        }
        base.Update();
    }

    public Button CreateButton(string Text, BaseEvent? OnClicked = null)
    {
        Button b = new Button(this);
        int x = Buttons.Count > 0 ? Buttons[Buttons.Count - 1].Position.X - b.Size.Width : Size.Width - b.Size.Width - 4;
        int y = Size.Height - b.Size.Height - 4;
        b.SetPosition(x - WindowEdges, y - WindowEdges);
        b.SetText(Text);
        if (OnClicked != null) b.OnClicked = OnClicked;
        Buttons.Add(b);
        return b;
    }

    public void MakePriorityWindow()
    {
        if (Window.ActiveWidget == this) return;
        this.WindowLayer = Window.ActiveWidget.WindowLayer + 1;
        this.Window.SetActiveWidget(this);
        Window.SetOverlayZIndex(WindowLayer * 10 - 1);
        this.SetZIndex(WindowLayer * 10);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["window"].Bitmap?.Dispose();
        Sprites["window"].Bitmap = new Bitmap(Size.Width - WindowEdges * 2, Size.Height - WindowEdges * 2);
        Sprites["window"].Bitmap.Unlock();
        Sprites["window"].Bitmap.DrawRect(0, 0, Sprites["window"].Bitmap.Width, Sprites["window"].Bitmap.Height, new Color(59, 227, 255));
        Sprites["window"].Bitmap.FillRect(1, 1, Sprites["window"].Bitmap.Width - 2, Sprites["window"].Bitmap.Height - 2, new Color(40, 62, 84));
        Sprites["window"].Bitmap.Lock();
        
        Sprites["shadow"].Bitmap?.Dispose();
        Sprites["shadow"].Bitmap = new Bitmap(Size);
        Sprites["shadow"].Bitmap.Unlock();
        Sprites["shadow"].Bitmap.FillGradientRectOutside(
            new Rect(Size),
            new Rect(WindowEdges, WindowEdges, Size.Width - WindowEdges * 2, Size.Height - WindowEdges * 2),
            new Color(0, 0, 0, 64),
            Color.ALPHA,
            false
        );
        Sprites["shadow"].Bitmap.Lock();
        for (int i = 0; i < Buttons.Count; i++)
        {
            Button b = Buttons[i];
            int x = i > 0 ? Buttons[i - 1].Position.X - b.Size.Width : Size.Width - b.Size.Width - 4;
            int y = Size.Height - b.Size.Height - 4;
            b.SetPosition(x - WindowEdges, y - WindowEdges);
        }
    }

    public override void ParentSizeChanged(BaseEventArgs e)
    {
        base.ParentSizeChanged(e);
        Center();
    }

    public void Center()
    {
        int width = Window.Width;
        int height = Window.Height;
        this.SetPosition(
            (int) Math.Round(width / 2 - (this.Size.Width * Viewport.ZoomX) / 2),
            (int) Math.Round(height / 2 - (this.Size.Height * Viewport.ZoomY) / 2)
        );
    }

    public void SetTitle(string Title)
    {
        this.Title = Title;
        Font f = Fonts.Header;
        Size s = f.TextSize(Title);
        if (Sprites["title"].Bitmap != null) Sprites["title"].Bitmap.Dispose();
        Sprites["title"].Bitmap = new Bitmap(s);
        Sprites["title"].Bitmap.Unlock();
        Sprites["title"].Bitmap.Font = f;
        Sprites["title"].Bitmap.DrawText(Title, Color.WHITE);
        Sprites["title"].Bitmap.Lock();
    }

    public virtual void Close()
    {
        if (Disposed)
        {
            Editor.CanUndo = true;
            this.OnClosed?.Invoke(new BaseEventArgs());
            return;
        }
        StartAnimation(new SigmoidAnimation("zoom_in", 400, x =>
        {
            if (Disposed) return;
            SetGlobalZoom(1 - (float) x);
            Center();
            if (x == 1)
            {
                Editor.CanUndo = true;
                Dispose();
                this.OnClosed?.Invoke(new BaseEventArgs());
            }
        }));
    }

    public override void Dispose()
    {
        if (!Editor.CanUndo)
        {
            // I've accidentally Disposed instead of Closing, so close first anyway
            Editor.CanUndo = true;
            this.OnClosed?.Invoke(new BaseEventArgs());
        }
        base.Dispose();
        if (this.Window.ActiveWidget == this)
        {
            // Remove current widget/window
            this.Window.Widgets.Remove(this);
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
    }
}
