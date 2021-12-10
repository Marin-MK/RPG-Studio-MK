using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class PopupWindow : Widget, IPopupWindow
{
    public bool Blocked = false;
    public string Title { get; protected set; }

    public BaseEvent OnClosed;
    public List<Button> Buttons = new List<Button>();

    public int WindowEdges = 7;

    public PopupWindow() : base(((MainEditorWindow)Graphics.Windows[0]).UI)
    {
        //Window.SetOverlayOpacity(128);
        Sprites["shadow"] = new Sprite(this.Viewport);
        Sprites["window"] = new RectSprite(this.Viewport, new Size(this.Size.Width - WindowEdges * 2, this.Size.Height - WindowEdges * 2),
            new Color(59, 227, 255), new Color(40, 62, 84));
        Sprites["window"].X = WindowEdges;
        Sprites["window"].Y = WindowEdges;
        Sprites["title"] = new Sprite(this.Viewport);
        Sprites["title"].X = 5 + WindowEdges;
        Sprites["title"].Y = 3 + WindowEdges;
        this.WindowLayer = Window.ActiveWidget.WindowLayer + 1;
        this.Window.SetActiveWidget(this);
        Window.SetOverlayZIndex(WindowLayer * 10 - 1);
        this.SetZIndex(WindowLayer * 10);
    }

    public void CreateButton(string Text, BaseEvent OnClicked)
    {
        Button b = new Button(this);
        int x = Buttons.Count > 0 ? Buttons[Buttons.Count - 1].Position.X - b.Size.Width : Size.Width - b.Size.Width - 4;
        int y = Size.Height - b.Size.Height - 4;
        b.SetPosition(x - WindowEdges, y - WindowEdges);
        b.SetText(Text);
        b.OnClicked = OnClicked;
        Buttons.Add(b);
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
        (Sprites["window"] as RectSprite).SetSize(this.Size.Width - WindowEdges * 2, this.Size.Height - WindowEdges * 2);
        Sprites["shadow"].Bitmap?.Dispose();
        Sprites["shadow"].Bitmap = new Bitmap(Size);
        Sprites["shadow"].Bitmap.Unlock();
        Sprites["shadow"].Bitmap.FillGradientRectOutside(
            new Rect(Size),
            new Rect(WindowEdges, WindowEdges, Size.Width - WindowEdges * 2, Size.Height - WindowEdges * 2),
            new Color(0, 0, 0, 200),
            Color.ALPHA,
            false
        );
        Sprites["shadow"].Bitmap.Lock();
        for (int i = 0; i < Buttons.Count; i++)
        {
            Button b = Buttons[i];
            int x = Buttons.Count > 0 ? Buttons[Buttons.Count - 1].Position.X - b.Size.Width : Size.Width - b.Size.Width - 4;
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
        this.SetPosition(width / 2 - (this.Size.Width) / 2, height / 2 - (this.Size.Height) / 2);
    }

    public void SetTitle(string Title)
    {
        this.Title = Title;
        Font f = Fonts.UbuntuBold.Use(14);
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
        Dispose();
        this.OnClosed?.Invoke(new BaseEventArgs());
    }

    public override void Dispose()
    {
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
