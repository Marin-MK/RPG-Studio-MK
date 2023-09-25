using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public partial class MapViewer
{
    public EventsPanel EventsPanel;
    List<EventBox> EventBoxes = new List<EventBox>();

    bool MovedSinceLastClick = true;
    EventBox DraggingEvent = null;
    EventBox StartEventBox;

    private partial void ConstructorEvents()
    {
        EventsPanel = new EventsPanel(GridLayout);
        EventsPanel.SetGrid(0, 1, 2, 2);
        SidebarWidgetEvents = EventsPanel;
        MainContainer.OnDoubleLeftMouseDownInside += _ =>
        {
            if (!MovedSinceLastClick && Mode == MapMode.Events) OpenOrCreateEventCursorIsOver();
        };
    }

	private partial void RegisterMenuEvents()
	{
        if (Map == null || Mode != MapMode.Events) return;
        MapWidget.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New")
            {
                IsClickable = e => e.Value = !EventBoxes.Any(eb => eb.Event.X == MapTileX && eb.Event.Y == MapTileY) && (Map.ID != Data.System.StartMapID || MapTileX != Data.System.StartX && MapTileY != Data.System.StartY),
                OnClicked = _ => OpenOrCreateEventCursorIsOver()
            },
            new MenuItem("Edit")
            {
                IsClickable = e => e.Value = EventBoxes.Any(eb => eb.Event.X == MapTileX && eb.Event.Y == MapTileY),
                OnClicked = _ => OpenOrCreateEventCursorIsOver()
			},
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                IsClickable = e => e.Value = EventBoxes.Any(eb => eb.Event.X == MapTileX && eb.Event.Y == MapTileY),
                OnClicked = _ => CutEvents()
			},
            new MenuItem("Copy")
            {
                IsClickable = e => e.Value = EventBoxes.Any(eb => eb.Event.X == MapTileX && eb.Event.Y == MapTileY),
                OnClicked = _ => CopyEvents(false)
			},
            new MenuItem("Paste")
            {
                IsClickable = e => e.Value = Clipboard.IsValid(BinaryData.EVENT) &&
                                                !EventBoxes.Any(eb => eb.Event.X == MapTileX && eb.Event.Y == MapTileY) && (Map.ID != Data.System.StartMapID || MapTileX != Data.System.StartX && MapTileY != Data.System.StartY),
                OnClicked = _ => PasteEvents()
			},
            new MenuSeparator(),
            new MenuItem("Game Starting Position")
            {
                IsClickable = e => e.Value = !EventBoxes.Any(eb => eb.Event.X == MapTileX && eb.Event.Y == MapTileY) && (Map.ID != Data.System.StartMapID || MapTileX != Data.System.StartX && MapTileY != Data.System.StartY),
                OnClicked = _ => SetStartPosition()
			},
            new MenuSeparator(),
            new MenuItem("Delete")
            {
                IsClickable = e => e.Value = EventBoxes.Any(eb => eb.Event.X == MapTileX && eb.Event.Y == MapTileY),
                OnClicked = _ => DeleteEventCursorIsOver()
            },
        });
	}

	private void DrawEvents()
    {
        for (int i = 0; i < EventBoxes.Count; i++) EventBoxes[i].Dispose();
        EventBoxes.Clear();
        StartEventBox?.Dispose();
        StartEventBox = null;
        if (Map == null) return;
        foreach (KeyValuePair<int, Event> kvp in Map.Events)
        {
            CreateEventBox(kvp.Value);
        }
        CreateStartEventBox();
        RepositionEvents();
    }

    public void HideEventBoxes()
    {
        EventBoxes.ForEach(eb => eb.SetVisible(false));
        StartEventBox?.SetVisible(false);
    }

    public void ShowEventBoxes()
    {
        EventBoxes.ForEach(eb => eb.SetVisible(true));
        StartEventBox?.SetVisible(true);
        RepositionEvents();
    }

    public void UpdateEventBoxesViewMode()
    {
        EventBoxes.ForEach(eb => eb.RepositionSprites(MapWidget, eb.Event.X, eb.Event.Y));
        StartEventBox?.RepositionSprites(MapWidget, Data.System.StartX, Data.System.StartY);
    }

    private void CreateStartEventBox()
    {
        if (this.Map.ID != Data.System.StartMapID) return;
        StartEventBox?.Dispose();
        StartEventBox = new EventBox(MainContainer);
        StartEventBox.SetSize(DummyWidget.Size);
        StartEventBox.ConsiderInAutoScrollCalculation = false;
        StartEventBox.SetZIndex(3);
        StartEventBox.SetEvent(Map, null);
        StartEventBox.RepositionSprites(MapWidget, Data.System.StartX, Data.System.StartY);
    }

    private void CreateEventBox(Event Event)
    {
        EventBox box = new EventBox(MainContainer);
        box.SetSize(DummyWidget.Size);
        box.ConsiderInAutoScrollCalculation = false;
        box.SetZIndex(3);
        box.SetEvent(Map, Event);
        box.RepositionSprites(MapWidget, Event.X, Event.Y);
        EventBoxes.Add(box);
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
            eb.RepositionSprites(MapWidget, eb.Event.X, eb.Event.Y);
            eb.SetSize(DummyWidget.Size);
        });
        StartEventBox?.RepositionSprites(MapWidget, Data.System.StartX, Data.System.StartY);
        StartEventBox?.SetSize(DummyWidget.Size);
        UpdateCursorPosition();
    }

    private EventBox SelectEventBoxCursorIsOver()
    {
        EventsPanel.SelectEventInList(null);
        CursorWidth = 0;
        CursorHeight = 0;
        EventBox box = null;
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
                box = eb;
                Cursor.SetVisible(true);
                break;
            }
        }
        if (StartEventBox != null && box == null)
        {
            if (MapTileX == Data.System.StartX && MapTileY == Data.System.StartY) box = StartEventBox;
        }
        UpdateCursorPosition();
        return box;
    }

    public void OpenOrCreateEventCursorIsOver()
    {
        if (Map == null) return;
        bool found = false;
        foreach (EventBox eb in EventBoxes)
        {
            if (eb.Event.X == MapTileX && eb.Event.Y == MapTileY)
            {
                OpenEvent(eb.Event);
                found = true;
                break;
            }
        }
        if (!found && StartEventBox is not null && MapTileX == Data.System.StartX && MapTileY == Data.System.StartY) return;
        if (!found && MapTileX >= 0 && MapTileX < Map.Width && MapTileY >= 0 && MapTileY < Map.Height)
        {
            Event ev = new Event(Editor.GetFreeEventID(Map));
            ev.X = MapTileX;
            ev.Y = MapTileY;
            Map.Events[ev.ID] = ev;
            CreateEventBox(ev);
            EventsPanel.RedrawEvents();
            EventsPanel.SelectEventInList(ev);
            OpenEvent(ev, true);
        }
    }

    public void DeleteEventCursorIsOver()
    {
        if (Map == null) return;
        EventBox box = null;
        foreach (EventBox eb in EventBoxes)
        {
            if (eb.Event.X == MapTileX && eb.Event.Y == MapTileY)
            {
                box = eb;
                break;
            }
        }
        if (box != null)
        {
            Map.Events.Remove(box.Event.ID);
            box.Dispose();
            EventBoxes.Remove(box);
            EventsPanel.RedrawEvents();
            SelectEventBoxCursorIsOver();
        }
    }

    public void OpenEvent(Event Event, bool DeleteIfCancelled = false, Action OnAppliedAction = null)
    {
        string OldName = Event.Name;
        int OldWidth = Event.Width;
        int OldHeight = Event.Height;
        EventBox box = null;
        foreach (EventBox eb in EventBoxes)
        {
            if (eb.Event.X == MapTileX && eb.Event.Y == MapTileY)
            {
                box = eb;
                break;
            }
        }
        if (box is null) throw new Exception("Could not find EventBox");
        EditEventWindow win = new EditEventWindow(Map, Event);
        win.OnClosed += _ =>
        {
            if (!win.Apply)
            {
                if (DeleteIfCancelled) DeleteEventCursorIsOver();
                return;
            }
            box.SetEvent(Map, win.Event);
            box.RepositionSprites(MapWidget, win.Event.X, win.Event.Y);
            EventsPanel.RedrawEvents();
            EventsPanel.SelectEventInList(win.Event);
            if (win.Event.Width != OldWidth || win.Event.Height != OldHeight || win.UpdateGraphic)
            {
                SelectEventBoxCursorIsOver();
            }
            MainContainer.UpdateAutoScroll();
            OnAppliedAction?.Invoke();
            Window.UI.SetSelectedWidget(this);
        };
    }

    private void SetStartPosition()
    {
        Data.System.StartMapID = Map.ID;
        Data.System.StartX = MapTileX;
        Data.System.StartY = MapTileY;
        CreateStartEventBox();
    }

    public void MoveCursorLeft()
    {
        if (Map == null) return;
        MapTileX -= 1;
        SelectEventBoxCursorIsOver();
    }

    public void MoveCursorRight()
    {
        if (Map == null) return;
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
        if (Map == null) return;
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
        if (Map == null) return;
        MapTileY += 1;
        SelectEventBoxCursorIsOver();
    }

    private partial void MouseDownEvents(MouseEventArgs e)
    {
        if (Map == null) return;
        if (Mode == MapMode.Events && (Mouse.LeftMouseTriggered || Mouse.RightMouseTriggered) && MainContainer.Mouse.Inside)
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
            int PreX = MapTileX;
            int PreY = MapTileY;
            EventBox box = SelectEventBoxCursorIsOver();
            MovedSinceLastClick = OldX != MapTileX || OldY != MapTileY;
            if (box != null && Mouse.LeftMouseTriggered)
            {
                DraggingEvent = box;
                OriginPoint = new Point(PreX - (DraggingEvent.Event?.X ?? Data.System.StartX), PreY - (DraggingEvent.Event?.Y ?? Data.System.StartY));
                Editor.CanUndo = false;
            }
            Cursor.SetVisible(true);
        }
    }

    private partial void MouseMovingEvents(MouseEventArgs e)
    {
        if (Map == null) return;
        if (Mode == MapMode.Events && DraggingEvent != null && Mouse.LeftMousePressed)
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
            int MapX = (int) Math.Floor(rx / (32d * ZoomFactor));
            int MapY = (int) Math.Floor(ry / (32d * ZoomFactor));
            MapTileX = MapX - OriginPoint.X;
            MapTileY = MapY - OriginPoint.Y;
            UpdateCursorPosition();
            DraggingEvent.RepositionSprites(MapWidget, MapTileX, MapTileY);
        }
    }

    private partial void MouseUpEvents(MouseEventArgs e)
    {
        if (Mouse.LeftMouseReleased && DraggingEvent != null)
        {
            Editor.CanUndo = true;
            if (DraggingEvent.Event == null && DraggingEvent == StartEventBox)
            {
                Data.System.StartX = MapTileX;
                Data.System.StartY = MapTileY;
            }
            else if (DraggingEvent.Event.X != MapTileX || DraggingEvent.Event.Y != MapTileY)
            {
                // It actually moved from its original location
                DraggingEvent.Event.X = MapTileX;
                DraggingEvent.Event.Y = MapTileY;
            }
            DraggingEvent = null;
        }
    }

    private partial void CopyEvents(bool Cut)
    {
        EventBox box = null;
        foreach (EventBox eb in EventBoxes)
        {
            if (eb.Event.X == MapTileX && eb.Event.Y == MapTileY)
            {
                box = eb;
                break;
            }
        }
        if (box != null)
        {
            Event ev = box.Event;
            Clipboard.SetObject(ev, BinaryData.EVENT);
            if (Cut) DeleteEventCursorIsOver();
        }
    }

    private partial void CutEvents()
    {
        CopyEvents(true);
    }

    private partial void PasteEvents()
    {
        if (!Clipboard.IsValid(BinaryData.EVENT)) return;
        EventBox box = null;
        foreach (EventBox eb in EventBoxes)
        {
            if (eb.Event.X == MapTileX && eb.Event.Y == MapTileY)
            {
                box = eb;
                break;
            }
        }
        if (box == null)
        {
            Event ev = Clipboard.GetObject<Event>();
            ev.X = MapTileX;
            ev.Y = MapTileY;
            ev.ID = Editor.GetFreeEventID(Map);
            Map.Events[ev.ID] = ev;
            CreateEventBox(ev);
            EventsPanel.RedrawEvents();
            SelectEventBoxCursorIsOver();
            MainContainer.UpdateAutoScroll();
        }
    }
}
