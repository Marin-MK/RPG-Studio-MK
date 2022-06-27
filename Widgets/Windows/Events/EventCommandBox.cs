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
        GroupBoxWithScrollBars gb = new GroupBoxWithScrollBars(this);
        gb.SetDocked(true);
        gb.SetFillerColor(new Color(40, 62, 84));

        ScrollContainer = new Container(gb);
        ScrollContainer.SetDocked(true);
        ScrollContainer.SetPadding(3, 3, 16, 16);

        VScrollBar vs = new VScrollBar(this);
        vs.SetRightDocked(true);
        vs.SetVDocked(true);
        vs.SetPadding(0, 3, 1, 15);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;
        vs.OnValueChanged += _ =>
        {
            // Ensure we set the selected widget back to being active after dragging the slider
            MainCommandWidget.GetSelectedWidget()?.WidgetSelected(new BaseEventArgs());
        };

        HScrollBar hs = new HScrollBar(this);
        hs.SetBottomDocked(true);
        hs.SetHDocked(true);
        hs.SetPadding(3, 0, 15, 1);
        ScrollContainer.SetHScrollBar(hs);
        ScrollContainer.HAutoScroll = true;
        hs.OnValueChanged += _ =>
        {
            // Ensure we get the selected widget back to being active after dragging the slider
            MainCommandWidget.GetSelectedWidget()?.WidgetSelected(new BaseEventArgs());
        };

        MainCommandWidget = new BaseCommandWidget(ScrollContainer, -1);
        MainCommandWidget.MainCommandWidget = MainCommandWidget;
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

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (!Mouse.Inside)
        {
            MainCommandWidget.DeselectAll();
        }
    }
}
