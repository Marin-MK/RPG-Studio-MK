using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class ExpandArrow : Widget
{
    public bool Expanded { get; protected set; } = false;

    public BaseEvent OnStateChanged;
    public BaseEvent OnCollapsed;
    public BaseEvent OnExpanded;

    public ExpandArrow(IContainer Parent) : base(Parent)
    {
        Sprites["dropdown"] = new Sprite(this.Viewport);
        RedrawDropdown();
        SetSize(15, 15);
    }

    public void SetExpanded(bool Expanded)
    {
        if (this.Expanded != Expanded)
        {
            this.Expanded = Expanded;
            Sprites["dropdown"].MirrorY = Expanded;
            Sprites["dropdown"].Y = Expanded ? -5 : 0;
            OnStateChanged?.Invoke(new BaseEventArgs());
            if (Expanded) OnExpanded?.Invoke(new BaseEventArgs());
            else OnCollapsed?.Invoke(new BaseEventArgs());
        }
    }

    private void RedrawDropdown()
    {
        Color outline = new Color(10, 23, 37);
        Color filler = new Color(179, 180, 181);
        Sprites["dropdown"].Bitmap?.Dispose();
        Sprites["dropdown"].Bitmap = new Bitmap(15, 15);
        Sprites["dropdown"].Bitmap.Unlock();
        Sprites["dropdown"].Bitmap.DrawLine(1, 0, 13, 0, outline);
        Sprites["dropdown"].Bitmap.DrawLine(0, 1, 7, 8, outline);
        Sprites["dropdown"].Bitmap.DrawLine(1, 3, 7, 9, outline);
        Sprites["dropdown"].Bitmap.DrawLine(8, 7, 14, 1, outline);
        Sprites["dropdown"].Bitmap.DrawLine(8, 8, 13, 3, outline);
        Sprites["dropdown"].Bitmap.DrawLine(1, 1, 13, 1, filler);
        Sprites["dropdown"].Bitmap.DrawLine(2, 2, 12, 2, filler);
        Sprites["dropdown"].Bitmap.DrawLine(3, 3, 11, 3, filler);
        Sprites["dropdown"].Bitmap.DrawLine(4, 4, 10, 4, filler);
        Sprites["dropdown"].Bitmap.DrawLine(5, 5, 9, 5, filler);
        Sprites["dropdown"].Bitmap.DrawLine(6, 6, 8, 6, filler);
        Sprites["dropdown"].Bitmap.SetPixel(7, 7, filler);
        Sprites["dropdown"].Bitmap.Lock();
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Mouse.Inside || !Visible) return;
        SetExpanded(!Expanded);
    }
}
