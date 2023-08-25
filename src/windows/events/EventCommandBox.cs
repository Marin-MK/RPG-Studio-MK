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
        Container gb = new Container(this);
        gb.SetBackgroundColor(40, 62, 84);
        gb.SetDocked(true);
        gb.Sprites["line1"] = new Sprite(gb.Viewport, new SolidBitmap(1, gb.Size.Height, new Color(24, 38, 53)));
        gb.Sprites["line2"] = new Sprite(gb.Viewport, new SolidBitmap(gb.Size.Width, 1, new Color(24, 38, 53)));
        gb.Sprites["box"] = new Sprite(gb.Viewport, new SolidBitmap(10, 10, new Color(24, 38, 53)));
        gb.OnSizeChanged += _ =>
        {
            ((SolidBitmap) gb.Sprites["line1"].Bitmap).SetSize(1, gb.Size.Height);
            gb.Sprites["line1"].X = gb.Size.Width - 11;
            ((SolidBitmap) gb.Sprites["line2"].Bitmap).SetSize(gb.Size.Width, 1);
            gb.Sprites["line2"].Y = gb.Size.Height - 11;
            gb.Sprites["box"].X = gb.Sprites["line1"].X + 1;
            gb.Sprites["box"].Y = gb.Sprites["line2"].Y + 1;
        };

        ScrollContainer = new Container(gb);
        ScrollContainer.SetDocked(true);
        ScrollContainer.SetPadding(3, 3, 16, 16);

        VScrollBar vs = new VScrollBar(this);
        vs.SetRightDocked(true);
        vs.SetVDocked(true);
        vs.SetPadding(0, 2, -1, 12);
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
        hs.SetPadding(2, 0, 12, -1);
        ScrollContainer.SetHScrollBar(hs);
        ScrollContainer.HAutoScroll = true;
        hs.OnValueChanged += _ =>
        {
            // Ensure we get the selected widget back to being active after dragging the slider
            MainCommandWidget.GetSelectedWidget()?.WidgetSelected(new BaseEventArgs());
        };

        Container HeaderContainer = new Container(ScrollContainer);
        HeaderContainer.SetHDocked(true);
        HeaderContainer.SetHeight(24);
        HeaderContainer.SetPosition(0, 8);
        Label HeaderLabel = new Label(HeaderContainer);
        HeaderLabel.SetFont(Fonts.Header);
        HeaderLabel.SetText("Commands");
        HeaderContainer.OnSizeChanged += _ =>
        {
            HeaderLabel.SetPosition(HeaderContainer.Size.Width / 2 - HeaderLabel.Size.Width / 2, 0);
        };

        MainCommandWidget = new BaseCommandWidget(ScrollContainer, -1);
        MainCommandWidget.SetPosition(0, 34);
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
