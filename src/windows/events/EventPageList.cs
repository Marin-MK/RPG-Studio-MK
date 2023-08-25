using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventPageList : Widget
{
    public int HoveringPage { get; protected set; } = -1;
    public int SelectedPage { get; protected set; } = -1;

    public BaseEvent OnSelectedPageChanged;

    public Event Event { get; protected set; }

    public EventPageList(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 23, new Color(32, 170, 221)));
        Sprites["hover"].Visible = false;
    }

    public void SetSelectedPage(int Page, bool HardRefresh = false)
    {
        if (this.SelectedPage != Page || HardRefresh)
        {
            Sprites["bg"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Unlock();
            int OldPage = this.SelectedPage;
            this.SelectedPage = Page;
            if (OldPage >= 0 && OldPage < Event.Pages.Count) RedrawPage(OldPage);
            RedrawPage(this.SelectedPage);
            Sprites["bg"].Bitmap.Lock();
            Sprites["text"].Bitmap.Lock();
            OnSelectedPageChanged?.Invoke(new BaseEventArgs());
        }
    }

    public void SetEvent(Event Event)
    {
        this.Event = Event;
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["text"].Bitmap?.Dispose();
        SetSize(Size.Width, Event.Pages.Count * 26 - 3);
        Sprites["bg"].Bitmap = new Bitmap(Size);
        Sprites["bg"].Bitmap.Unlock();
        Sprites["text"].Bitmap = new Bitmap(Size);
        Sprites["text"].Bitmap.Font = Fonts.Paragraph;
        Sprites["text"].Bitmap.Unlock();
        for (int i = 0; i < Event.Pages.Count; i++)
        {
            RedrawPage(i);
        }
        Sprites["bg"].Bitmap.Lock();
        Sprites["text"].Bitmap.Lock();
    }

    void RedrawPage(int Page)
    {
        Color BGColor = this.SelectedPage == Page ? new Color(40, 62, 84) : new Color(31, 49, 68);
        Sprites["bg"].Bitmap.FillRect(0, Page * 26, Size.Width, 23, BGColor);
        Color TextColor = this.SelectedPage == Page ? new Color(32, 170, 221) : new Color(147, 158, 169);
        string text = $"Page {Page + 1}";
        Sprites["text"].Bitmap.DrawText(text, Size.Width / 2, 1 + Page * 26, TextColor, DrawOptions.CenterAlign);
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y + TopCutOff;
        int idx = (int) Math.Floor(ry / 26d);
        if (!Mouse.Inside || ry % 26 >= 23 || idx >= Event.Pages.Count)
        {
            Sprites["hover"].Visible = false;
            HoveringPage = -1;
        }
        else
        {
            Sprites["hover"].Visible = true;
            Sprites["hover"].Y = idx * 26;
            HoveringPage = idx;
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (HoveringPage != -1)
        {
            SetSelectedPage(HoveringPage);
        }
    }
}
