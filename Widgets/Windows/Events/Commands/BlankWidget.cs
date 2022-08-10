using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class BlankWidget : BaseCommandWidget
{
    public BlankWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex)
    {
        //Widget PlusWidget = new Widget(this);
        //PlusWidget.SetSize(15, 15);
        //PlusWidget.SetPosition(12, 8);
        //Color pluscolor = new Color(149, 158, 181);
        //PlusWidget.Sprites["plus"] = new Sprite(PlusWidget.Viewport);
        //PlusWidget.Sprites["plus"].Bitmap = new Bitmap(15, 15);
        //PlusWidget.Sprites["plus"].Bitmap.Unlock();
        //PlusWidget.Sprites["plus"].Bitmap.FillRect(6, 1, 3, 13, pluscolor);
        //PlusWidget.Sprites["plus"].Bitmap.FillRect(1, 6, 13, 3, pluscolor);
        //PlusWidget.Sprites["plus"].Bitmap.SetPixel(7, 0, pluscolor);
        //PlusWidget.Sprites["plus"].Bitmap.SetPixel(0, 7, pluscolor);
        //PlusWidget.Sprites["plus"].Bitmap.SetPixel(7, 14, pluscolor);
        //PlusWidget.Sprites["plus"].Bitmap.SetPixel(14, 7, pluscolor);
        //PlusWidget.Sprites["plus"].Bitmap.Lock();
    }

    public override void LoadCommand()
    {
        base.LoadCommand();
        HeaderLabel.SetVisible(false);
        ScaleGradientWithSize = true;
        SetWidth(40);
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (e.Handled || this.Indentation == -1 || InsideChild())
        {
            CancelDoubleClick();
            return;
        }
        SetSelected(true);
    }

    protected override bool IsEditable()
    {
        return false;
    }
}
