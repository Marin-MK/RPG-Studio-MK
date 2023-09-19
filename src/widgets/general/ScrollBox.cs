using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class ScrollBox : Widget
{
    public ScrollBox(IContainer parent) : base(parent)
    {
		SetChildPadding(3, 3, 0, 0);

        Sprites["bg"] = new Sprite(this.Viewport);

		VScrollBar vs = new VScrollBar(this);
		vs.SetVDocked(true);
		vs.SetRightDocked(true);

		HScrollBar hs = new HScrollBar(this);
		hs.SetHDocked(true);
		hs.SetBottomDocked(true);

		this.SetVScrollBar(vs);
		this.VAutoScroll = true;
		this.SetHScrollBar(hs);
		this.HAutoScroll = true;

		vs.OnVisibilityChanged += _ => UpdatePadding();
		hs.OnVisibilityChanged += _ => UpdatePadding();

		OnSizeChanged += _ => RedrawBox(hs.Visible, vs.Visible);
		OnChildBoundsChanged += _ => UpdatePadding();
	}

	private void UpdatePadding()
	{
		if (VScrollBar.Visible && HScrollBar.Visible)
		{
			HScrollBar.SetPadding(3, 0, 13, 0);
			VScrollBar.SetPadding(0, 3, 0, 13);
		}
		else if (VScrollBar.Visible)
		{
			VScrollBar.SetPadding(0, 3, 0, 3);
		}
		else if (HScrollBar.Visible)
		{
			HScrollBar.SetPadding(3, 0, 3, 0);
		}
		SetChildPadding(3, 3, VScrollBar.Visible ? -13 : -3, HScrollBar.Visible ? -13 : -3);
		UpdateAutoScroll();
		RedrawBox(HScrollBar.Visible, VScrollBar.Visible);
	}

    public void RedrawBox(bool hScrollBarVisible, bool vScrollBarVisible)
    {
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(this.Size);
        Sprites["bg"].Bitmap.Unlock();
		Sprites["bg"].Bitmap.DrawRect(Size, new Color(86, 108, 134));
		Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, new Color(10, 23, 37));
		Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
		Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
		Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
		Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
		Color DarkOutline = new Color(40, 62, 84);
		Sprites["bg"].Bitmap.SetPixel(1, 1, DarkOutline);
		Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 1, DarkOutline);
		Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 2, DarkOutline);
		Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, DarkOutline);
		
		if (vScrollBarVisible) Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 1, Size.Width - 12, Size.Height - 2, DarkOutline);
		if (hScrollBarVisible) Sprites["bg"].Bitmap.DrawLine(1, Size.Height - 12, Size.Width - 2, Size.Height - 12, DarkOutline);
		if (hScrollBarVisible && vScrollBarVisible) Sprites["bg"].Bitmap.FillRect(Size.Width - 12, Size.Height - 12, 11, 11, new Color(64, 104, 146));

		Sprites["bg"].Bitmap.Lock();
    }
}
