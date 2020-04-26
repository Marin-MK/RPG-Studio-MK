using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class MapViewerEvents : MapViewerBase
    {
        public CursorWidget Cursor;
        public int MapTileWidth = 1;
        public int MapTileHeight = 1;
        public bool Dragging = false;

        public MapViewerEvents(IContainer Parent) : base(Parent)
        {
            Cursor = new CursorWidget(MainContainer);
            Cursor.ConsiderInAutoScrollCalculation = false;
            Cursor.SetZIndex(6);
        }

        public override void PositionMap()
        {
            base.PositionMap();
            ((EventMapImageWidget) MapWidget).PositionEvents();
        }

        public void UpdateCursorPosition()
        {
            Cursor.SetPosition(MapWidget.Position.X + (int) Math.Round(MapTileX * 32 * ZoomFactor) - 7, MapWidget.Position.Y + (int) Math.Round(MapTileY * 32 * ZoomFactor) - 7);
            Cursor.SetSize((int) Math.Round(14 + MapTileWidth * 32 * ZoomFactor), (int) Math.Round(14 + MapTileHeight * 32 * ZoomFactor));
        }

        int DoubleClickEventID = -1;
        int DoubleClickX = -1;
        int DoubleClickY = -1;

        int DraggingEventID = -1;
        int DraggingAnchorX = -1;
        int DraggingAnchorY = -1;

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (e.LeftButton != e.OldLeftButton)
            {
                int X = -1,
                    Y = -1;
                GetHoveringTile(e, out X, out Y);
                if (X != -1 && Y != -1)
                {
                    if (X >= MapTileX && X < MapTileX + MapTileWidth && Y >= MapTileY && Y < MapTileY + MapTileHeight)
                    {
                        if (DoubleClickX == MapTileX && DoubleClickY == MapTileY)
                        {
                            if (TimerExists("double") && !TimerPassed("double"))
                            {
                                DestroyTimer("double");
                                if (DoubleClickEventID != -1)
                                {
                                    OpenEvent(DoubleClickEventID);
                                }
                                else
                                {
                                    NewEvent(MapTileX, MapTileY);
                                }
                            }
                            else
                            {
                                if (TimerExists("double")) DestroyTimer("double");
                                SetTimer("double", 300);
                            }
                        }
                        else
                        {
                            if (TimerExists("double")) DestroyTimer("double");
                            SetTimer("double", 300);
                            DoubleClickX = MapTileX;
                            DoubleClickY = MapTileY;
                        }
                    }
                    else
                    {
                        MapTileX = Math.Min(Math.Max(X, 0), Map.Width - 1);
                        MapTileY = Math.Min(Math.Max(Y, 0), Map.Height - 1);
                        MapTileWidth = 1;
                        MapTileHeight = 1;
                        DoubleClickEventID = -1;
                        foreach (Event ev in Map.Events.Values)
                        {
                            if (MapTileX >= ev.X && MapTileY >= ev.Y && MapTileX < ev.X + ev.Width && MapTileY < ev.Y + ev.Height)
                            {
                                MapTileX = ev.X;
                                MapTileY = ev.Y;
                                MapTileWidth = ev.Width;
                                MapTileHeight = ev.Height;
                                DraggingEventID = ev.ID;
                                DoubleClickEventID = ev.ID;
                                DraggingAnchorX = X;
                                DraggingAnchorY = Y;
                                Editor.MainWindow.EventingWidget.EventListPanel.SelectEvent(ev);
                                break;
                            }
                        }
                        if (DoubleClickEventID == -1) Editor.MainWindow.EventingWidget.EventListPanel.SelectEvent(null);
                        Dragging = true;
                        UpdateCursorPosition();
                        if (TimerExists("double")) DestroyTimer("double");
                        SetTimer("double", 300);
                        DoubleClickX = MapTileX;
                        DoubleClickY = MapTileY;
                    }
                }
            }
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            if (DraggingEventID != -1)
            {
                int X = -1,
                    Y = -1;
                GetHoveringTile(e, out X, out Y);
                Map.Events[DraggingEventID].X += X - DraggingAnchorX;
                Map.Events[DraggingEventID].Y += Y - DraggingAnchorY;
                MapTileX = Map.Events[DraggingEventID].X;
                MapTileY = Map.Events[DraggingEventID].Y;
                UpdateCursorPosition();
                DraggingAnchorX = X;
                DraggingAnchorY = Y;
                ((EventMapImageWidget) MapWidget).PositionEvents(Map.Events[DraggingEventID]);
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);
            if (DraggingEventID != -1 && e.LeftButton != e.OldLeftButton)
            {
                DraggingEventID = -1;
                DraggingAnchorX = -1;
                DraggingAnchorY = -1;
            }
        }

        public void GetHoveringTile(MouseEventArgs e, out int X, out int Y)
        {
            X = -1;
            Y = -1;
            if (MainContainer.WidgetIM.Hovering)
            {
                LastMouseX = e.X;
                LastMouseY = e.Y;
                int oldmousex = RelativeMouseX;
                int oldmousey = RelativeMouseY;
                // Cursor placement
                if (MainContainer.HScrollBar != null && (MainContainer.HScrollBar.Dragging || MainContainer.HScrollBar.Hovering)) return;
                if (MainContainer.VScrollBar != null && (MainContainer.VScrollBar.Dragging || MainContainer.VScrollBar.Hovering)) return;
                int rx = e.X - MapWidget.Viewport.X;
                int ry = e.Y - MapWidget.Viewport.Y;
                if (e.X < MainContainer.Viewport.X || e.Y < MainContainer.Viewport.Y ||
                    e.X >= MainContainer.Viewport.X + MainContainer.Viewport.Width ||
                    e.Y >= MainContainer.Viewport.Y + MainContainer.Viewport.Height) // Off the Map Viewer area
                {
                    if (Cursor.Visible) Cursor.SetVisible(false);
                    X = -1;
                    Y = -1;
                    return;
                }
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
                int tilex = (int) Math.Floor(rx / (32d * ZoomFactor));
                int tiley = (int) Math.Floor(ry / (32d * ZoomFactor));
                int cx = tilex * 32;
                int cy = tiley * 32;
                X = tilex;
                Y = tiley;
            }
        }

        public void OpenEvent(int EventID, bool NewEvent = false)
        {
            EditEvent ee = new EditEvent(Map.Events[EventID], NewEvent);
            ee.OnClosed += delegate (BaseEventArgs args)
            {

            };
        }

        public void NewEvent(int X, int Y)
        {
            Console.WriteLine($"New Event ({X}, {Y})");
            Event e = new Event(Editor.GetFreeEventID(Map));
            e.X = X;
            e.Y = Y;
            Map.Events[e.ID] = e;
            ((EventMapImageWidget) MapWidget).DrawEvent(e);
            ((EventMapImageWidget) MapWidget).PositionEvents(e);
            Editor.MainWindow.EventingWidget.EventListPanel.SetMap(Map);
            Editor.MainWindow.EventingWidget.EventListPanel.SelectEvent(e);
            DoubleClickX = X;
            DoubleClickY = Y;
            DoubleClickEventID = e.ID;
            OpenEvent(e.ID, true);
        }

        public void SelectEvent(Event e)
        {
            MapTileX = e.X;
            MapTileY = e.Y;
            MapTileWidth = e.Width;
            MapTileHeight = e.Height;
            UpdateCursorPosition();
        }
    }

    public class EventMapImageWidget : MapImageWidget
    {
        public List<EventWidget> EventWidgets = new List<EventWidget>();

        public EventMapImageWidget(IContainer Parent) : base(Parent)
        {

        }

        public override void LoadLayers(Map MapData, int RelativeX = 0, int RelativeY = 0)
        {
            base.LoadLayers(MapData, RelativeX, RelativeY);
            DrawEvents();
        }

        public void DrawEvent(Event e)
        {
            EventWidget ew = new EventWidget(this);
            ew.SetEvent(e);
            EventWidgets.Add(ew);
        }

        public void DrawEvents()
        {
            foreach (EventWidget e in EventWidgets) e.Dispose();
            EventWidgets.Clear();
            foreach (Event e in this.MapData.Events.Values)
            {
                DrawEvent(e);
            }
            PositionEvents();
        }

        public void PositionEvents(Event ev = null)
        {
            foreach (EventWidget e in EventWidgets)
            {
                if (ev != null && e.EventData != ev) continue;
                e.SetSize(this.Size);
                e.SetZoomFactor(this.ZoomFactor);
                e.SetBoxPosition((int) Math.Round(e.EventData.X * 32 * ZoomFactor), (int) Math.Round(e.EventData.Y * 32 * ZoomFactor));
                e.SetBoxSize((int) Math.Round(e.EventData.Width * 32d), (int) Math.Round(e.EventData.Height * 32d));
                e.Reposition();
            }
        }
    }
}
