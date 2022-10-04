using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class SwitchPickerBox : BrowserBox
{
    public int SwitchID { get; protected set; }

    public BaseEvent OnSwitchChanged;

    bool HoveringArrow;
    bool PressingArrow;

    public SwitchPickerBox(IContainer Parent) : base(Parent)
    {
        OnDropDownClicked += _ =>
        {
            SwitchPicker picker = new SwitchPicker(this.SwitchID);
            picker.OnClosed += _ =>
            {
                if (!picker.Apply)
                {
                    // Ensure the name is updated
                    this.SetSwitchID(this.SwitchID);
                    return;
                }
                this.SetSwitchID(picker.SwitchID);
                this.OnSwitchChanged?.Invoke(new BaseEventArgs());
            };
        };
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        TextArea.SetSize(this.Size.Width - 29, this.Size.Height - 3);
    }

    public void SetSwitchID(int SwitchID)
    {
        this.SwitchID = SwitchID;
        this.SetText(GetSwitchText(SwitchID));
    }

    private string GetSwitchText(int SwitchID)
    {
        return $"[{Utilities.Digits(SwitchID, 3)}]: {Data.System.Switches[SwitchID - 1]}";
    }

    protected override void Draw()
    {
        if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, 86, 108, 134);
        Color FillerColor = this.Enabled ? new Color(10, 23, 37) : new Color(24, 38, 53);
        Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, FillerColor);
        Color OutlineColor = new Color(86, 108, 134);
        Sprites["bg"].Bitmap.DrawLine(Size.Width - 25, 1, Size.Width - 25, Size.Height - 2, OutlineColor);
        Color ArrowColor = this.Enabled ? (PressingArrow ? new Color(32, 170, 221) : HoveringArrow ? Color.WHITE : OutlineColor) : OutlineColor;
        if (PressingArrow && !HoveringArrow) ArrowColor = Color.WHITE;

        if (this.Enabled && (HoveringArrow || PressingArrow))
        {
            Sprites["bg"].Bitmap.DrawRect(Size.Width - 25, 0, 24, Size.Height, ArrowColor);
        }

        Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
        Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
        
        int x = Size.Width - 18;
        int y = 7;
        Sprites["bg"].Bitmap.FillRect(x + 2, y, 2, 11, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 4, y + 1, x + 4, y + 9, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 5, y + 2, x + 5, y + 8, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 6, y + 3, x + 6, y + 7, ArrowColor);
        Sprites["bg"].Bitmap.DrawLine(x + 7, y + 4, x + 7, y + 6, ArrowColor);
        Sprites["bg"].Bitmap.SetPixel(x + 8, y + 5, ArrowColor);

        Sprites["bg"].Bitmap.Lock();
        Color TextColor = this.Enabled ? Color.WHITE : new Color(147, 158, 169);
        TextArea.SetTextColor(TextColor);

        this.Drawn = true;
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        bool OldHoveringArrow = HoveringArrow;
        if (rx >= Size.Width - 25 && rx < Size.Width && ry >= 0 && ry < Size.Height)
        {
            HoveringArrow = true;
        }
        else
        {
            HoveringArrow = false;
        }
        if (OldHoveringArrow != HoveringArrow) Redraw();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (Mouse.LeftMousePressed)
        {
            PressingArrow = HoveringArrow;
            Redraw();
        }
    }

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (Mouse.LeftMouseReleased)
        {
            PressingArrow = false;
            Redraw();
        }
    }
}
