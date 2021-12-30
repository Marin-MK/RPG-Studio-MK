using System;

namespace RPGStudioMK.Widgets;

public class PickerBox : amethyst.DropdownBox
{
    public PickerBox(IContainer Parent) : base(Parent)
    {
        TextArea.SetPosition(6, 4);
        TextArea.SetFont(Fonts.ProductSansMedium.Use(14));
        TextArea.SetCaretColor(Color.WHITE);
        TextArea.SetReadOnly(true);
        this.DropdownWidth = 34;
    }

    protected override void Draw()
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Color lightgrey = new Color(121, 121, 122);
        Color darkgrey = new Color(96, 100, 100);
        Color filler = new Color(10, 23, 37);
        Color dropdownfiller = WidgetIM.Hovering ? new Color(6, 53, 108) : new Color(28, 50, 73);
        int linex = Size.Width - DropdownWidth + 8;
        Sprites["bg"].Bitmap.DrawRect(Size, lightgrey);
        Sprites["bg"].Bitmap.DrawRect(1, 1, Size.Width - 2, Size.Height - 2, lightgrey);
        Sprites["bg"].Bitmap.FillRect(2, 2, Size.Width - 4, Size.Height - 4, filler);
        Sprites["bg"].Bitmap.FillRect(linex, 2, Size.Width - linex - 2, Size.Height - 4, dropdownfiller);
        Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, 1, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(1, 0, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 1, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 0, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 2, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 1, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 2, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 1, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(2, 2, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 3, 2, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(2, Size.Height - 3, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 3, Size.Height - 3, darkgrey);

        Sprites["bg"].Bitmap.DrawLine(linex, 2, linex, Size.Height - 3, lightgrey);
        Sprites["bg"].Bitmap.DrawLine(linex - 1, 2, linex - 1, Size.Height - 3, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(linex - 1, 2, lightgrey);
        Sprites["bg"].Bitmap.SetPixel(linex - 2, 2, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(linex - 1, 3, lightgrey);
        Sprites["bg"].Bitmap.SetPixel(linex - 2, 3, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(linex - 1, Size.Height - 3, lightgrey);
        Sprites["bg"].Bitmap.SetPixel(linex - 2, Size.Height - 3, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(linex - 1, Size.Height - 4, lightgrey);
        Sprites["bg"].Bitmap.SetPixel(linex - 2, Size.Height - 4, darkgrey);
        Sprites["bg"].Bitmap.DrawLine(linex + 1, 2, linex + 1, 4, darkgrey);
        Sprites["bg"].Bitmap.DrawLine(linex + 1, Size.Height - 5, linex + 1, Size.Height - 3, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(linex + 2, 2, darkgrey);
        Sprites["bg"].Bitmap.SetPixel(linex + 2, Size.Height - 3, darkgrey);

        Bitmap arrow = new Bitmap("assets/img/pickerbox_arrow");
        int x = Size.Width - DropdownWidth / 2 - arrow.Width / 2 + 4;
        int y = (int) Math.Floor(Size.Height / 2d) - arrow.Height / 2;
        Sprites["bg"].Bitmap.Build(x, y, arrow);
        Sprites["bg"].Bitmap.Lock();
        base.Draw();
    }

    public override void HoverChanged(MouseEventArgs e)
    {
        base.HoverChanged(e);
        this.Redraw();
    }
}
