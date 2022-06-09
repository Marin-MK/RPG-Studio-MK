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

    Dictionary<EventPage, (List<BaseCommandWidget.CommandUndoAction>, List<BaseCommandWidget.CommandUndoAction>)> UndoRedoLists =
        new Dictionary<EventPage, (List<BaseCommandWidget.CommandUndoAction>, List<BaseCommandWidget.CommandUndoAction>)>();

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
        vs.OnValueChanged += _ =>
        {
            // Ensure we set the selected widget back to being active after dragging the slider
            MainCommandWidget.GetSelectedWidget()?.WidgetSelected(new BaseEventArgs());
        };

        MainCommandWidget = new BaseCommandWidget(ScrollContainer, -1);
        MainCommandWidget.SetHDocked(true);
    }

    public void SetCommands(Map Map, Event Event, EventPage Page, List<EventCommand> Commands)
    {
        EventPage OldPage = this.Page;
        this.Map = Map;
        this.Event = Event;
        this.Page = Page;
        this.Commands = Commands;
        if (OldPage != null)
        {
            (List<BaseCommandWidget.CommandUndoAction>, List<BaseCommandWidget.CommandUndoAction>) lists = MainCommandWidget.GetUndoRedoLists();
            if (UndoRedoLists.ContainsKey(OldPage)) UndoRedoLists.Remove(OldPage);
            UndoRedoLists.Add(OldPage, lists);
        }
        MainCommandWidget.SetCommand(Map, Event, Page, null, Commands, -1, -1);
        if (UndoRedoLists.ContainsKey(Page))
        {
            MainCommandWidget.SetUndoRedoLists(UndoRedoLists[Page].Item1, UndoRedoLists[Page].Item2);
        }
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

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Mouse.Inside)
        {
            MainCommandWidget.DeselectAll();
        }
    }
}
