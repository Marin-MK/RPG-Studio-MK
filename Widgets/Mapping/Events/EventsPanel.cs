using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventsPanel : Widget
{
    public Map Map;

    VStackPanel StackPanel;

    public EventsPanel(IContainer Parent) : base(Parent)
    {
        Label Header = new Label(this);
        Header.SetText("Events");
        Header.SetFont(Fonts.UbuntuBold.Use(13));
        Header.SetPosition(7, 7);

        Sprites["sep"] = new Sprite(this.Viewport, new SolidBitmap(288, 2, new Color(10, 23, 37)));
        Sprites["sep"].Y = 30;

        Sprites["slider"] = new Sprite(this.Viewport, new SolidBitmap(10, Size.Height - 34, new Color(10, 23, 37)));
        Sprites["slider"].Y = 33;

        Container ScrollContainer = new Container(this);
        ScrollContainer.SetPadding(2, 33, 14, 1);
        ScrollContainer.SetDocked(true);
        VScrollBar vs = new VScrollBar(this);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        vs.SetPadding(0, 34, 0, 2);
        ScrollContainer.SetVScrollBar(vs);
        ScrollContainer.VAutoScroll = true;

        StackPanel = new VStackPanel(ScrollContainer);
        StackPanel.SetHDocked(true);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["slider"].X = Size.Width - 11;
        ((SolidBitmap) Sprites["slider"].Bitmap).SetSize(10, Size.Height - 34);
    }

    public void SetMap(Map Map)
    {
        this.Map = Map;
        RedrawEvents();
    }

    public void SelectEventOnMap(Event Event)
    {
        Editor.MainWindow.MapWidget.MapViewer.SelectEventOnMap(Event);
    }

    public void OpenEvent(Event Event)
    {
        Editor.MainWindow.MapWidget.MapViewer.OpenEvent(Event);
    }

    public void SelectEventInList(Event Event)
    {
        SelectLabel((EventLabel) StackPanel.Widgets.Find(w => ((EventLabel) w).Event == Event));
    }

    private void SelectLabel(EventLabel label)
    {
        StackPanel.Widgets.ForEach(w => ((EventLabel) w).SetSelected(false));
        label?.SetSelected(true);
    }

    public void RedrawEvents()
    {
        while (StackPanel.Widgets.Count > 0)
        {
            StackPanel.Widgets[0].Dispose();
        }
        List<int> keys = Map.Events.Keys.ToList();
        keys.Sort();
        for (int i = 0; i < keys.Count; i++)
        {
            Event Event = Map.Events[keys[i]];
            EventLabel label = new EventLabel(StackPanel);
            label.SetEvent(Event);
            label.OnLeftMouseDownInside += _ =>
            {
                SelectLabel(label);
                SelectEventOnMap(label.Event);
            };
            label.OnDoubleLeftMouseDownInside += _ =>
            {
                OpenEvent(label.Event);
            };
        }
    }
}
