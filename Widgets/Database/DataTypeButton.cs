namespace RPGStudioMK.Widgets;

public class DataTypeButton : Widget
{
    public bool Selected { get; protected set; } = false;
    public string Icon { get; protected set; }
    public string Text { get; protected set; }

    public DataTypeButton(IContainer Parent) : base(Parent)
    {
        Sprites["icon"] = new Sprite(this.Viewport);
        Sprites["icon"].X = 18;
        Sprites["icon"].Y = 6;
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].X = 65;
        Sprites["text"].Y = 12;
        Sprites["selection"] = new Sprite(this.Viewport, new SolidBitmap(2, 44, new Color(59, 227, 255)));
        Sprites["selection"].Visible = false;
        SetSize(181, 44);
    }

    public void SetType(string Icon, string Text)
    {
        this.Icon = Icon;
        this.Text = Text;
        Sprites["icon"].Bitmap?.Dispose();
        Sprites["icon"].Bitmap = new Bitmap($"assets/img/data_type_{Icon}");
        Sprites["icon"].SrcRect.Width = Sprites["icon"].Bitmap.Width / 2;
        Sprites["text"].Bitmap?.Dispose();
        Font f = Fonts.UbuntuBold.Use(16);
        Size s = f.TextSize(Text);
        Sprites["text"].Bitmap = new Bitmap(s);
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.Font = f;
        Sprites["text"].Bitmap.DrawText(Text, Color.WHITE);
        Sprites["text"].Bitmap.Lock();
    }

    public void SetSelected(bool Selected)
    {
        if (this.Selected != Selected)
        {
            if (Selected)
            {
                foreach (Widget w in Parent.Widgets)
                {
                    if (w is DataTypeButton && w != this && ((DataTypeButton)w).Selected) ((DataTypeButton)w).SetSelected(false);
                }
            }
            Sprites["icon"].SrcRect.X = Selected ? Sprites["icon"].SrcRect.Width : 0;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Clear();
            Sprites["text"].Bitmap.DrawText(this.Text, Selected ? new Color(37, 192, 250) : Color.WHITE);
            Sprites["text"].Bitmap.Lock();
            this.Selected = Selected;
            Editor.MainWindow.DatabaseWidget.SetMode(this.Icon);
        }
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        Sprites["selection"].Visible = WidgetIM.Hovering;
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (WidgetIM.Hovering)
        {
            this.SetSelected(true);
        }
    }
}
