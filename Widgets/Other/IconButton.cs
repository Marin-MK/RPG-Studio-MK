namespace RPGStudioMK.Widgets;

public class IconButton : Widget
{
    public Icon Icon;
    public bool Toggleable = false;
    public bool Selectable = true;

    public bool Selected { get; protected set; } = false;
    public bool Enabled { get; protected set; } = true;

    public BaseEvent OnSelection;
    public BaseEvent OnDeselection;
    public BaseEvent OnClicked;

    public IconButton(IContainer Parent) : base(Parent)
    {
        SetSize(24, 28);
        Sprites["icon"] = new Sprite(Viewport);
        Sprites["icon"].Bitmap = Utilities.IconSheet;
        Sprites["icon"].DestroyBitmap = false;
        Sprites["icon"].SrcRect.Width = 24;
        Sprites["icon"].SrcRect.Height = 24;
        Sprites["selector"] = new Sprite(Viewport, new SolidBitmap(24, 2, 59, 227, 255));
        Sprites["selector"].Y = 26;
        Sprites["selector"].Visible = false;
        OnWidgetSelected += WidgetSelected;
    }

    public void SetSelected(bool Selected)
    {
        if (this.Selected != Selected)
        {
            if (Selected && !Toggleable)
            {
                foreach (Widget w in Parent.Widgets)
                {
                    if (!(w is IconButton)) continue;
                    IconButton b = w as IconButton;
                    if (b.Selected && !b.Toggleable) b.SetSelected(false);
                }
            }
            this.Selected = Selected;
            if (Selected) this.OnSelection?.Invoke(new BaseEventArgs());
            if (!Selected) this.OnDeselection?.Invoke(new BaseEventArgs());
            SetIcon(Icon, Selected);
        }
    }

    public void SetIcon(Icon Icon, bool Selected = false)
    {
        Sprites["icon"].SrcRect.X = (int)Icon * 24;
        Sprites["icon"].SrcRect.Y = Selected ? 24 : 0;
        // Changing SrcRect doesn't update the viewport/renderer
        Sprites["icon"].Update();
        this.Icon = Icon;
    }

    public void SetSelectorOffset(int pixels)
    {
        SetSize(24, 28 + pixels);
        Sprites["selector"].Y = 26 + pixels;
    }

    public void SetEnabled(bool Enabled)
    {
        if (this.Enabled != Enabled)
        {
            this.Enabled = Enabled;
            Sprites["icon"].Opacity = (byte) (Enabled ? 255 : 128);
        }
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Sprites["selector"].Visible = WidgetIM.Hovering;
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        int ry = e.Y - Viewport.Y;
        if (WidgetIM.Hovering && ry < 29 && !TimerExists("reset") && e.OldLeftButton != e.LeftButton && e.LeftButton && Enabled)
        {
            if (Toggleable) SetSelected(!Selected);
            else if (Selectable) SetSelected(true);
            else // Normal pressable button
            {
                SetIcon(Icon, true);
                SetTimer("reset", 100);
                this.OnClicked?.Invoke(e);
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (TimerPassed("reset"))
        {
            SetIcon(Icon, false);
            DestroyTimer("reset");
        }
    }
}

public enum Icon
{
    Save = 2,
    Cut = 3,
    Copy = 4,
    Paste = 5,
    Cancel = 6,
    Undo = 7,
    Redo = 8,
    Pencil = 9,
    Bucket = 10,
    CircleOutline = 11,
    RectangleOutline = 12,
    Selection = 13,
    Eraser = 14,
    CircleFilled = 15,
    RectangleFilled = 16,
    SelectionMultiple = 17,
    ZoomOut = 18,
    ZoomIn = 19,

    Eyes = 28,
    EyeOpen = 29,
    EyeClosed = 30
}
