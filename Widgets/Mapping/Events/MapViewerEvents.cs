using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class MapViewer
{
    public EventsPanel EventsPanel;
    List<EventBox> EventBoxes = new List<EventBox>();

    private partial void ConstructorEvents()
    {
        EventsPanel = new EventsPanel(GridLayout);
        EventsPanel.SetGrid(0, 1, 2, 2);
        SidebarWidgetEvents = EventsPanel;
        MainContainer.OnDoubleLeftMouseDownInside += _ =>
        {
            if (!MovedSinceLastClick) OpenEventCursorIsOver();
        };
    }

    private void DrawEvents()
    {
        for (int i = 0; i < EventBoxes.Count; i++) EventBoxes[i].Dispose();
        EventBoxes.Clear();
        foreach (KeyValuePair<int, Event> kvp in Map.Events)
        {
            EventBox box = new EventBox(MainContainer);
            box.SetSize(DummyWidget.Size);
            box.ConsiderInAutoScrollCalculation = false;
            box.SetZIndex(3);
            box.SetEvent(kvp.Value);
            box.RepositionSprites(MapWidget);
            EventBoxes.Add(box);
        }
    }

    public void SelectEventOnMap(Event Event)
    {
        EventBox box = EventBoxes.Find(eb => eb.Event == Event);
        MapTileX = box.Event.X;
        MapTileY = box.Event.Y;
        CursorWidth = box.Event.Width - 1;
        CursorHeight = box.Event.Height - 1;
        CursorOrigin = Location.BottomLeft;
        UpdateCursorPosition();
        Cursor.SetVisible(true);
    }

    private void RepositionEvents()
    {
        EventBoxes.ForEach(eb =>
        {
            eb.RepositionSprites(MapWidget);
            eb.SetSize(DummyWidget.Size);
        });
        UpdateCursorPosition();
    }

    private partial void MouseDownEvents(MouseEventArgs e)
    {
        if (Mode == MapMode.Events && Mouse.LeftMouseTriggered && MainContainer.Mouse.Inside)
        {
            int rx = e.X - MapWidget.Viewport.X;
            int ry = e.Y - MapWidget.Viewport.Y;
            int movedx = MapWidget.Position.X - MapWidget.ScrolledPosition.X;
            int movedy = MapWidget.Position.Y - MapWidget.ScrolledPosition.Y;
            if (movedx >= MapWidget.Position.X)
            {
                movedx -= MapWidget.Position.X;
                rx += movedx;
            }
            if (movedy >= MapWidget.Position.Y)
            {
                movedy -= MapWidget.Position.Y;
                ry += movedy;
            }
            int OldX = MapTileX;
            int OldY = MapTileY;
            MapTileX = (int) Math.Floor(rx / (32d * ZoomFactor));
            MapTileY = (int) Math.Floor(ry / (32d * ZoomFactor));
            SelectEventBoxCursorIsOver();
            MovedSinceLastClick = OldX != MapTileX || OldY != MapTileY;
        }
    }

    bool MovedSinceLastClick = true;

    private void SelectEventBoxCursorIsOver()
    {
        EventsPanel.SelectEventInList(null);
        CursorWidth = 0;
        CursorHeight = 0;
        foreach (EventBox eb in EventBoxes)
        {
            Event ev = eb.Event;
            if (MapTileX >= ev.X && MapTileX < ev.X + ev.Width &&
                MapTileY >= ev.Y - ev.Height + 1 && MapTileY <= ev.Y)
            {
                EventsPanel.SelectEventInList(ev);
                MapTileX = ev.X;
                MapTileY = ev.Y;
                CursorWidth = ev.Width - 1;
                CursorHeight = ev.Height - 1;
                CursorOrigin = Location.BottomLeft;
                EventsPanel.SelectEventInList(ev);
                break;
            }
        }
        UpdateCursorPosition();
        Cursor.SetVisible(true);
    }

    public void OpenEventCursorIsOver()
    {
        foreach (EventBox eb in EventBoxes)
        {
            if (eb.Event.X == MapTileX && eb.Event.Y == MapTileY)
            {
                OpenEvent(eb.Event);
                break;
            }
        }
    }

    public void OpenEvent(Event Event)
    {
        Console.WriteLine($"Open event {Event.ID}: {Event.Name}");
        EditEventWindow win = new EditEventWindow(Event);
    }

    public void MoveCursorLeft()
    {
        MapTileX -= 1;
        SelectEventBoxCursorIsOver();
    }

    public void MoveCursorRight()
    {
        bool found = false;
        foreach (EventBox eb in EventBoxes)
        {
            if (eb.Event.X == MapTileX && eb.Event.Y == MapTileY)
            {
                MapTileX += eb.Event.Width;
                found = true;
                break;
            }
        }
        if (!found) MapTileX += 1;
        SelectEventBoxCursorIsOver();
    }

    public void MoveCursorUp()
    {
        bool found = false;
        foreach (EventBox eb in EventBoxes)
        {
            if (eb.Event.X == MapTileX && eb.Event.Y == MapTileY)
            {
                MapTileY -= eb.Event.Height;
                found = true;
                break;
            }
        }
        if (!found) MapTileY -= 1;
        SelectEventBoxCursorIsOver();
    }

    public void MoveCursorDown()
    {
        MapTileY += 1;
        SelectEventBoxCursorIsOver();
    }
}
