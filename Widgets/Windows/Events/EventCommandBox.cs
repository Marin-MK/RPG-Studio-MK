using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventCommandBox : Widget
{
    Map Map;
    Event Event;
    EventPage Page;
    List<EventCommand> Commands;

    Container ScrollContainer;
    public BaseCommandWidget MainCommandWidget;

    public EventCommandBox(IContainer Parent) : base(Parent)
    {
        Sprites["bg"] = new Sprite(this.Viewport);
        ScrollContainer = new Container(this);
        ScrollContainer.SetDocked(true);
        ScrollContainer.SetPadding(3, 3, 13, 3);

        VScrollBar vs = new VScrollBar(this);
        vs.SetRightDocked(true);
        vs.SetVDocked(true);
        vs.SetPadding(0, 3);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;

        MainCommandWidget = new BaseCommandWidget(ScrollContainer, -1);
        MainCommandWidget.SetHDocked(true);
    }

    public void SetCommands(Map Map, Event Event, EventPage Page, List<EventCommand> Commands)
    {
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;
        this.Commands = Commands;
        MainCommandWidget.SetReady(false);
        MainCommandWidget.SetCommand(Map, Event, Page, null, Commands, -1, -1);
        MainCommandWidget.SetReady(true);
    }

    protected override void Draw()
    {
        base.Draw();
        Sprites["bg"].Bitmap?.Dispose();
        Sprites["bg"].Bitmap = new Bitmap(Size);
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
        Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 1, Size.Width - 12, Size.Height - 2, DarkOutline);
        Sprites["bg"].Bitmap.Lock();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Redraw();
    }
}
