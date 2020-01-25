using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MapViewer : Widget
    {
        public Map Map;
        public TilesetsPanel TilesetTab;
        public LayersPanel LayersTab;
        public ToolBar ToolBar;
        public StatusBar StatusBar;

        public int TopLeftX
        {
            get
            {
                int x = MapTileX;
                if (CursorOrigin == Location.TopRight || CursorOrigin == Location.BottomRight) x -= CursorWidth;
                return x;
            }
        }
        public int TopLeftY
        {
            get
            {
                int y = MapTileY;
                if (CursorOrigin == Location.BottomLeft || CursorOrigin == Location.BottomRight) y -= CursorHeight;
                return y;
            }
        }

        public int RelativeMouseX = 0;
        public int RelativeMouseY = 0;
        public int MapTileX = 0;
        public int MapTileY = 0;
        public Location CursorOrigin;
        public List<TileData> TileDataList = new List<TileData>();
        public int CursorWidth = 0;
        public int CursorHeight = 0;
        public Point OriginPoint;
        public bool SelectionOnMap = false;
        public int SelectionX = -1;
        public int SelectionY = -1;
        public int SelectionWidth = 0;
        public int SelectionHeight = 0;

        public bool MiddleMouseScrolling = false;
        public int MiddleScrolledX = 0;
        public int MiddleScrolledY = 0;
        public int LastMouseX = 0;
        public int LastMouseY = 0;

        public double ZoomFactor = 1.0;

        public Container MainContainer;
        public CursorWidget Cursor;
        public MapImageWidget MapWidget;
        public Widget DummyWidget;
        public GridBackground GridBackground;
        public SelectionBackground SelectionBackground;
        public VignetteFade Fade;

        public MapViewer(object Parent, string Name = "mapViewer")
            : base(Parent, Name)
        {
            this.SetBackgroundColor(28, 50, 73);
            this.WidgetIM.OnMouseMoving += MouseMoving;
            this.WidgetIM.OnMouseDown += MouseDown;
            this.WidgetIM.OnMouseUp += MouseUp;
            this.WidgetIM.OnMouseWheel += MouseWheel;
            this.OnWidgetSelected += WidgetSelected;
            MainContainer = new Container(this);
            MainContainer.HAutoScroll = MainContainer.VAutoScroll = true;
            Cursor = new CursorWidget(MainContainer);
            Cursor.ConsiderInAutoScroll = false;
            MapWidget = new MapImageWidget(MainContainer);
            DummyWidget = new Widget(MainContainer);
            Sprites["hslider"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width - 13, 11, new Color(10, 23, 37)));
            Sprites["vslider"] = new Sprite(this.Viewport, new SolidBitmap(11, Size.Height - 12, new Color(10, 23, 37)));
            HScrollBar = new HScrollBar(this);
            HScrollBar.SetPosition(2, Size.Height - 9);
            HScrollBar.SetSize(Size.Width - 15, 8);
            HScrollBar.OnValueChanged += delegate (object sender, EventArgs e)
            {
                if (Graphics.LastMouseEvent != null) MouseMoving(sender, Graphics.LastMouseEvent);
            };
            MainContainer.SetHScrollBar(HScrollBar);
            VScrollBar = new VScrollBar(this);
            VScrollBar.SetPosition(Size.Width - 9, 1);
            VScrollBar.SetSize(8, Size.Height - 14);
            VScrollBar.OnValueChanged += delegate (object sender, EventArgs e)
            {
                if (Graphics.LastMouseEvent != null) MouseMoving(sender, Graphics.LastMouseEvent);
            };
            MainContainer.SetVScrollBar(VScrollBar);
            GridBackground = new GridBackground(MainContainer);
            SelectionBackground = new SelectionBackground(MainContainer);

            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.ESCAPE), new EventHandler<EventArgs>(CancelSelection))
            });

            Fade = new VignetteFade(this);
        }

        public void SetZoomFactor(double factor, bool FromStatusBar = false)
        {
            this.ZoomFactor = factor;
            Editor.ProjectSettings.LastZoomFactor = factor;
            MapWidget.SetZoomFactor(factor);
            GridBackground.SetTileSize((int) Math.Round(32 * this.ZoomFactor));
            if (!FromStatusBar) StatusBar.ZoomControl.SetZoomFactor(factor, true);
            PositionMap();
            MouseMoving(null, Graphics.LastMouseEvent);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            MainContainer.SetSize(this.Size.Width - 11, this.Size.Height - 11);
            PositionMap();

            (Sprites["hslider"].Bitmap as SolidBitmap).SetSize(Size.Width - 12, 11);
            Sprites["hslider"].Y = Size.Height - 11;

            (Sprites["vslider"].Bitmap as SolidBitmap).SetSize(11, Size.Height - 12);
            Sprites["vslider"].X = Size.Width - 11;

            HScrollBar.SetPosition(1, Size.Height - 9);
            HScrollBar.SetSize(Size.Width - 15, 8);
            VScrollBar.SetPosition(Size.Width - 9, 1);
            VScrollBar.SetSize(8, Size.Height - 14);

            Fade.SetSize(this.Size.Width - 11, this.Size.Height - 11);
        }

        public void SetMap(Map Map)
        {
            this.Map = Map;
            StatusBar.SetMap(Map);
            TilesetTab.SetTilesets(Map.TilesetIDs);
            this.CreateLayerBitmaps();
            this.LayersTab.CreateLayers();
            TilesetTab.SelectTile(new TileData() { TilesetIndex = 0, TileID = 0 });
            if (MainContainer.HScrollBar != null) MainContainer.HScrollBar.SetValue(0.5);
            if (MainContainer.VScrollBar != null) MainContainer.VScrollBar.SetValue(0.5);
        }

        public void CreateNewLayer(int Index, Game.Layer LayerData)
        {
            MapWidget.CreateNewLayer(Index, LayerData);
        }

        public void DeleteLayer(int Index)
        {
            MapWidget.DeleteLayer(Index);
        }

        public void SwapLayers(int Index1, int Index2)
        {
            MapWidget.SwapLayers(Index1, Index2);
        }

        public void CreateLayerBitmaps()
        {
            MapWidget.LoadLayers(this.Map);
            PositionMap();
        }

        public void PositionMap()
        {
            int x = 0;
            if (Map.Width * 32 * ZoomFactor < Viewport.Width)
            {
                x = MainContainer.Viewport.Width / 2 - (int) Math.Round(Map.Width * 32 * ZoomFactor / 2d);
            }
            else
            {
                x = MainContainer.Viewport.Width / 4;
            }
            int y = 0;
            if (Map.Height * 32 * ZoomFactor < Viewport.Height)
            {
                y = MainContainer.Viewport.Height / 2 - (int) Math.Round(Map.Height * 32 * ZoomFactor / 2d);
            }
            else
            {
                y = MainContainer.Viewport.Height / 4;
            }
            MapWidget.SetPosition(x, y);
            MapWidget.SetSize((int) Math.Round(Map.Width * 32 * ZoomFactor), (int) Math.Round(Map.Height * 32 * ZoomFactor));
            GridBackground.SetPosition(MapWidget.Position);
            GridBackground.SetSize(MapWidget.Size);
            UpdateSelection();
            DummyWidget.SetSize(2 * x + MapWidget.Size.Width, 2 * y + MapWidget.Size.Height);
            if (Map.Width * 32 * ZoomFactor >= Viewport.Width || Map.Height * 32 * ZoomFactor >= Viewport.Height)
            {
                MainContainer.HScrollBar.SetValue(0.5);
                MainContainer.VScrollBar.SetValue(0.5);
            }
            MainContainer.UpdateAutoScroll();
        }

        public void CancelSelection(object sender, EventArgs e)
        {
            if (SelectionX != -1 || SelectionY != -1 || SelectionWidth != 0 || SelectionHeight != 0 || SelectionBackground.Visible)
            {
                SelectionX = -1;
                SelectionY = -1;
                SelectionWidth = 0;
                SelectionHeight = 0;
                SelectionBackground.SetVisible(false);
            }
        }

        public void UpdateSelection()
        {
            if (SelectionX >= 0 && SelectionY >= 0 && SelectionWidth > 0 && SelectionHeight > 0)
            {
                SelectionBackground.SetVisible(true);
                SelectionBackground.SetPosition(
                    (int) Math.Round(MapWidget.Position.X + SelectionX * 32 * this.ZoomFactor),
                    (int) Math.Round(MapWidget.Position.Y + SelectionY * 32 * this.ZoomFactor)
                );
                SelectionBackground.SetSize(
                    (int) Math.Round(SelectionWidth * 32 * this.ZoomFactor + 1),
                    (int) Math.Round(SelectionHeight * 32 * this.ZoomFactor + 1)
                );
            }
            else SelectionBackground.SetVisible(false);
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            if (MiddleMouseScrolling)
            {
                int dx = LastMouseX - e.X;
                int dy = LastMouseY - e.Y;
                MainContainer.ScrolledX += dx;
                MainContainer.ScrolledY += dy;

                MainContainer.ScrolledX = Math.Max(0, Math.Min(MainContainer.ScrolledX, MainContainer.MaxChildWidth - MainContainer.Viewport.Width));
                MainContainer.ScrolledY = Math.Max(0, Math.Min(MainContainer.ScrolledY, MainContainer.MaxChildHeight - MainContainer.Viewport.Height));

                LastMouseX = e.X;
                LastMouseY = e.Y;

                MainContainer.UpdateAutoScroll();
            }
            else
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
                    MapTileX = -1;
                    MapTileY = -1;
                    RelativeMouseX = -1;
                    RelativeMouseY = -1;
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
                if (!ToolBar.SelectButton.Selected) Cursor.SetVisible(true);
                int cx = tilex * 32;
                int cy = tiley * 32;
                RelativeMouseX = cx;
                RelativeMouseY = cy;
                cx += MapWidget.Position.X;
                cy += MapWidget.Position.Y;
                MapTileX = tilex;
                MapTileY = tiley;
                UpdateCursorPosition();
                if (oldmousex != RelativeMouseX || oldmousey != RelativeMouseY)
                {
                    UpdateTilePlacement(oldmousex, oldmousey, RelativeMouseX, RelativeMouseY);
                }
            }
        }

        public void UpdateCursorPosition()
        {
            int offsetx = 0;
            int offsety = 0;
            if (CursorOrigin == Location.TopRight || CursorOrigin == Location.BottomRight)
                offsetx = (int) Math.Round(32 * CursorWidth * ZoomFactor);
            if (CursorOrigin == Location.BottomLeft || CursorOrigin == Location.BottomRight)
                offsety = (int) Math.Round(32 * CursorHeight * ZoomFactor);
            Cursor.SetPosition(MapWidget.Position.X + (int) Math.Round(32 * MapTileX * ZoomFactor) - offsetx - 7, MapWidget.Position.Y + (int) Math.Round(32 * MapTileY * ZoomFactor) - offsety - 7);
            Cursor.SetSize((int) Math.Round(32 * ZoomFactor) * (CursorWidth + 1) + 14, (int) Math.Round(32 * ZoomFactor) * (CursorHeight + 1) + 14);
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            // Update position - to make sure you're drawing where the mouse is, not where the cursor is
            // (the cursor obviously follows the mouse with this call if they're not aligned (which they should be))
            MouseMoving(sender, e);
            if (e.LeftButton != e.OldLeftButton && e.LeftButton ||
                e.RightButton != e.OldRightButton && e.RightButton) 
                UpdateTilePlacement(RelativeMouseX, RelativeMouseY, RelativeMouseX, RelativeMouseY);
            else if (e.MiddleButton != e.OldMiddleButton && e.MiddleButton)
            {
                if (WidgetIM.Hovering)
                {
                    Input.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
                    this.MiddleMouseScrolling = true;
                    this.MiddleScrolledX = MainContainer.ScrolledX;
                    this.MiddleScrolledY = MainContainer.ScrolledY;
                }
            }
        }

        public override void MouseUp(object sender, MouseEventArgs e)
        {
            base.MouseUp(sender, e);
            if (!e.LeftButton && !e.RightButton) OriginPoint = null;
            if (e.MiddleButton != e.OldMiddleButton && !e.MiddleButton)
            {
                Input.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
                this.MiddleMouseScrolling = false;
                this.MiddleScrolledX = 0;
                this.MiddleScrolledY = 0;
            }
        }

        public override void MouseWheel(object sender, MouseEventArgs e)
        {
            base.MouseWheel(sender, e);
            if (!Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LCTRL) && !Input.Press(SDL2.SDL.SDL_Keycode.SDLK_RCTRL)) return;
            if (e.WheelY > 0) StatusBar.ZoomControl.IncreaseZoom();
            else StatusBar.ZoomControl.DecreaseZoom();
        }

        public void UpdateTilePlacement(int oldx = -1, int oldy = -1, int newx = -1, int newy = -1)
        {
            if (MainContainer.HScrollBar != null && (MainContainer.HScrollBar.Dragging || MainContainer.HScrollBar.Hovering)) return;
            if (MainContainer.VScrollBar != null && (MainContainer.VScrollBar.Dragging || MainContainer.VScrollBar.Hovering)) return;
            bool Left = WidgetIM.ClickedLeftInArea == true;
            bool Right = WidgetIM.ClickedRightInArea == true;

            if (Left || Right)
            {
                if (OriginPoint == null)
                    OriginPoint = new Point(
                        (int) Math.Floor(oldx / 32d),
                        (int) Math.Floor(oldy / 32d)
                    );
            }

            // Input handling
            if (Left)
            {
                if (ToolBar.SelectButton.Selected) // Selection tool
                {
                    int sx = OriginPoint.X < MapTileX ? OriginPoint.X : MapTileX;
                    int ex = OriginPoint.X < MapTileX ? MapTileX : OriginPoint.X;
                    int sy = OriginPoint.Y < MapTileY ? OriginPoint.Y : MapTileY;
                    int ey = OriginPoint.Y < MapTileY ? MapTileY : OriginPoint.Y;
                    if (sx < 0) sx = 0;
                    else if (sx >= Map.Width) sx = Map.Width - 1;
                    if (sy < 0) sy = 0;
                    else if (sy >= Map.Height) sy = Map.Height - 1;
                    if (ex < 0) ex = 0;
                    else if (ex >= Map.Width) ex = Map.Width - 1;
                    if (ey < 0) ey = 0;
                    else if (ey >= Map.Height) ey = Map.Height - 1;
                    SelectionX = sx;
                    SelectionY = sy;
                    SelectionWidth = ex - sx + 1;
                    SelectionHeight = ey - sy + 1;
                    UpdateSelection();
                }
                else // Pencil tool
                {
                    int Layer = this.LayersTab.SelectedLayer;
                    if (TileDataList.Count == 0)
                    {
                        if (ToolBar.EraserButton.Selected) TileDataList.Add(null);
                        else
                        {
                            ToolBar.EraserButton.SetSelected(true);
                            TileDataList.Add(null);
                            throw new Exception($"The tile data list is empty, but the eraser tool is not selected.\nCan't find tiles to draw with.");
                        }
                    }
                    MapWidget.DrawTiles(oldx, oldy, newx, newy, Layer);
                }
            }
            else if (Right)
            {
                if (ToolBar.SelectButton.Selected) // Selection tool
                {
                    if (SelectionX != -1 || SelectionY != -1 || SelectionWidth != 0 || SelectionHeight != 0)
                    {
                        SelectionX = -1;
                        SelectionY = -1;
                        SelectionWidth = 0;
                        SelectionHeight = 0;
                        UpdateSelection();
                    }
                }
                else // Pencil tool
                {
                    int Layer = this.LayersTab.SelectedLayer;
                    TileDataList.Clear();

                    int OriginDiffX = MapTileX - OriginPoint.X;
                    int OriginDiffY = MapTileY - OriginPoint.Y;
                    CursorOrigin = Location.BottomRight;
                    if (OriginDiffX < 0)
                    {
                        OriginDiffX = -OriginDiffX;
                        CursorOrigin = Location.BottomLeft;
                    }
                    if (OriginDiffY < 0)
                    {
                        OriginDiffY = -OriginDiffY;
                        if (CursorOrigin == Location.BottomLeft) CursorOrigin = Location.TopLeft;
                        else CursorOrigin = Location.TopRight;
                    }
                    CursorWidth = OriginDiffX;
                    CursorHeight = OriginDiffY;
                    UpdateCursorPosition();

                    if (CursorWidth == 0 && CursorHeight == 0)
                    {
                        int MapTileIndex = MapTileY * Map.Width + MapTileX;
                        if (MapTileX < 0 || MapTileX >= Map.Width || MapTileY < 0 || MapTileY >= Map.Height)
                            ToolBar.EraserButton.SetSelected(true);
                        else
                        {
                            TileData tile = Map.Layers[Layer].Tiles[MapTileIndex];
                            if (tile == null) ToolBar.EraserButton.SetSelected(true);
                            else TilesetTab.SelectTile(tile);
                        }
                    }
                    else
                    {
                        SelectionOnMap = true;
                        ToolBar.EraserButton.SetSelected(false);
                        int sx = OriginPoint.X < MapTileX ? OriginPoint.X : MapTileX;
                        int ex = OriginPoint.X < MapTileX ? MapTileX : OriginPoint.X;
                        int sy = OriginPoint.Y < MapTileY ? OriginPoint.Y : MapTileY;
                        int ey = OriginPoint.Y < MapTileY ? MapTileY : OriginPoint.Y;
                        for (int y = sy; y <= ey; y++)
                        {
                            for (int x = sx; x <= ex; x++)
                            {
                                int index = y * Map.Width + x;
                                if (x < 0 || x >= Map.Width || y < 0 || y >= Map.Height)
                                    TileDataList.Add(null);
                                else
                                {
                                    TileData tile = Map.Layers[Layer].Tiles[index];
                                    TileDataList.Add(tile);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetLayerVisible(int layerindex, bool Visible)
        {
            MapWidget.SetLayerVisible(layerindex, Visible);
        }
    }

    public class MapImageWidget : Widget
    {
        public MapViewer MapViewer;
        public Map MapData;

        public double ZoomFactor = 1.0;

        public MapImageWidget(object Parent, string Name = "mapImageWidget")
            : base(Parent, Name)
        {
            SetBackgroundColor(73, 89, 109);
            this.MapViewer = this.Parent.Parent as MapViewer;
        }

        public void SetZoomFactor(double factor)
        {
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                Sprites[i.ToString()].ZoomX = Sprites[i.ToString()].ZoomY = factor;
            }
            this.ZoomFactor = factor;
        }

        public void LoadLayers(Map MapData)
        {
            this.MapData = MapData;
            this.SetSize((int) Math.Round(MapData.Width * 32 * ZoomFactor), (int) Math.Round(MapData.Height * 32 * ZoomFactor));
            RedrawLayers();
        }

        public void SetLayerVisible(int layerindex, bool Visible)
        {
            MapData.Layers[layerindex].Visible = Visible;
            Sprites[layerindex.ToString()].Visible = Visible;
        }

        public void CreateNewLayer(int Index, Game.Layer LayerData)
        {
            for (int i = MapData.Layers.Count - 1; i >= Index; i--)
            {
                Sprite s = this.Sprites[i.ToString()] as Sprite;
                s.Z += 2;
                this.Sprites.Remove(i.ToString());
                this.Sprites.Add((i + 1).ToString(), s);
            }
            MapData.Layers.Insert(Index, LayerData);
            string key = Index.ToString();
            Sprites[key] = new Sprite(this.Viewport, MapData.Width * 32, MapData.Height * 32);
            Sprites[key].Z = Index;
            Sprites[key].ZoomX = Sprites[key].ZoomY = this.ZoomFactor;
        }

        public void DeleteLayer(int Index)
        {
            this.Sprites[Index.ToString()].Dispose();
            this.Sprites.Remove(Index.ToString());
            for (int i = Index + 1; i < MapData.Layers.Count; i++)
            {
                Sprite s = this.Sprites[i.ToString()] as Sprite;
                s.Z -= 2;
                this.Sprites.Remove(i.ToString());
                this.Sprites.Add((i - 1).ToString(), s);
            }
            MapData.Layers.RemoveAt(Index);
        }

        public void SwapLayers(int Index1, int Index2)
        {
            Sprite s1 = this.Sprites[Index1.ToString()] as Sprite;
            s1.Z = Index2 * 2;
            Sprite s2 = this.Sprites[Index2.ToString()] as Sprite;
            s2.Z = Index1 * 2;
            this.Sprites.Remove(Index1.ToString());
            this.Sprites.Remove(Index2.ToString());
            this.Sprites[Index1.ToString()] = s2;
            this.Sprites[Index2.ToString()] = s1;
            Layer l1 = MapData.Layers[Index1];
            MapData.Layers[Index1] = MapData.Layers[Index2];
            MapData.Layers[Index2] = l1;
        }

        public void RedrawLayers()
        {
            Console.WriteLine("(Re)drawing layers");
            foreach (string s in this.Sprites.Keys)
            {
                if (s != "_bg") this.Sprites[s].Dispose();
            }
            // Create layers
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                this.Sprites[i.ToString()] = new Sprite(this.Viewport, MapData.Width * 32, MapData.Height * 32);
                this.Sprites[i.ToString()].Z = i * 2;
                this.Sprites[i.ToString()].Bitmap.Unlock();
                this.Sprites[i.ToString()].Visible = MapData.Layers[i].Visible;
            }
            // Draw tiles
            for (int layer = 0; layer < MapData.Layers.Count; layer++)
            {
                Bitmap layerbmp = this.Sprites[layer.ToString()].Bitmap as Bitmap;
                // Iterate through all vertical tiles
                for (int y = 0; y < MapData.Height; y++)
                {
                    // Iterate through all horizontal tiles
                    for (int x = 0; x < MapData.Width; x++)
                    {
                        // Each individual tile
                        if (MapData.Layers[layer] == null || MapData.Layers[layer].Tiles == null ||
                            y * MapData.Width + x >= MapData.Layers[layer].Tiles.Count ||
                            MapData.Layers[layer].Tiles[y * MapData.Width + x] == null) continue;
                        int tileset_index = MapData.Layers[layer].Tiles[y * MapData.Width + x].TilesetIndex;
                        int tileset_id = MapData.TilesetIDs[tileset_index];
                        Bitmap tilesetimage = Data.Tilesets[tileset_id].TilesetBitmap;
                        int mapx = x * 32;
                        int mapy = y * 32;
                        int tile_id = MapData.Layers[layer].Tiles[y * MapData.Width + x].TileID;
                        int tilesetx = tile_id % 8;
                        int tilesety = (int) Math.Floor(tile_id / 8d);
                        layerbmp.Build(new Rect(mapx, mapy, 32, 32), tilesetimage, new Rect(tilesetx * 32, tilesety * 32, 32, 32));
                    }
                }
            }
            // Lock layers
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                this.Sprites[i.ToString()].Bitmap.Lock();
            }
            // Zoom layers
            SetZoomFactor(ZoomFactor);
        }

        public void DrawTiles(int oldx, int oldy, int newx, int newy, int layer)
        {
            // Avoid drawing a line from top left to current tile
            if (oldx == -1 || oldy == -1)
            {
                oldx = newx;
                oldy = newy;
            }
            Point Origin = MapViewer.OriginPoint;
            // This resets the two points tiles are drawn in between if the mouse has gone off the map (otherwise it'd draw a line between
            // the last point on the map and the current point on the map)
            bool blanktile = MapViewer.ToolBar.EraserButton.Selected;
            bool line = !(oldx == newx && oldy == newy);
            List<Point> TempCoords = new List<Point>();
            if (MapViewer.ToolBar.FillButton.Selected)
            {
                int sx, sy, ex, ey;
                if (MapViewer.SelectionX != -1 && MapViewer.SelectionY != -1 && MapViewer.SelectionWidth != 0 && MapViewer.SelectionHeight != 0 && MapViewer.SelectionBackground.Visible)
                {
                    int mx = (int) Math.Floor(newx / 32d);
                    int my = (int) Math.Floor(newy / 32d);
                    sx = MapViewer.SelectionX;
                    ex = MapViewer.SelectionX + MapViewer.SelectionWidth;
                    sy = MapViewer.SelectionY;
                    ey = MapViewer.SelectionY + MapViewer.SelectionHeight;
                    if (!(mx >= sx && mx < ex && my >= sy && my < ey)) return; // Outside selection
                }
                else
                {
                    sx = 0;
                    sy = 0;
                    ex = MapData.Width;
                    ey = MapData.Height;
                }
                for (int y = sy; y < ey; y++)
                {
                    for (int x = sx; x < ex; x++)
                    {
                        TempCoords.Add(new Point(x, y));
                    }
                }
            }
            else if (line) // Draw tiles between several tiles - use simple line drawing algorithm to determine the tiles to draw on
            {
                int x1 = oldx;
                int y1 = oldy;
                int x2 = newx;
                int y2 = newy;
                for (int x = x1 > x2 ? x2 : x1; (x1 > x2) ? (x <= x1) : (x <= x2); x++)
                {
                    double fact = ((double) x - x1) / (x2 - x1);
                    int y = (int) Math.Round(y1 + ((y2 - y1) * fact));
                    if (y >= 0)
                    {
                        int tilex = (int) Math.Floor(x / 32d);
                        int tiley = (int) Math.Floor(y / 32d);
                        if (!TempCoords.Exists(c => c.X == tilex && c.Y == tiley)) TempCoords.Add(new Point(tilex, tiley));
                    }
                }
                int sy = y1 > y2 ? y2 : y1;
                for (int y = y1 > y2 ? y2 : y1; (y1 > y2) ? (y <= y1) : (y <= y2); y++)
                {
                    double fact = ((double) y - y1) / (y2 - y1);
                    int x = (int) Math.Round(x1 + ((x2 - x1) * fact));
                    if (x >= 0)
                    {
                        int tilex = (int) Math.Floor(x / 32d);
                        int tiley = (int) Math.Floor(y / 32d);
                        if (!TempCoords.Exists(c => c.X == tilex && c.Y == tiley)) TempCoords.Add(new Point(tilex, tiley));
                    }
                }
            }
            else // Just one singular tile
            {
                TempCoords.Add(new Point((int) Math.Floor(newx / 32d), (int) Math.Floor(newy / 32d)));
            }

            this.Sprites[layer.ToString()].Bitmap.Unlock();

            for (int i = 0; i < TempCoords.Count; i++)
            {
                // Both of these depend on the origin, but we need them both at the top left as we begin drawing there
                // and to be able to compare them to use in the modulus, they both have to be adjusted
                int MapTileX = TempCoords[i].X;
                int MapTileY = TempCoords[i].Y;
                int OriginX = MapViewer.OriginPoint.X;
                int OriginY = MapViewer.OriginPoint.Y;
                if (MapViewer.CursorOrigin == Location.TopRight || MapViewer.CursorOrigin == Location.BottomRight)
                {
                    MapTileX -= MapViewer.CursorWidth;
                    OriginX -= MapViewer.CursorWidth;
                }
                if (MapViewer.CursorOrigin == Location.BottomLeft || MapViewer.CursorOrigin == Location.BottomRight)
                {
                    MapTileY -= MapViewer.CursorHeight;
                    OriginY -= MapViewer.CursorHeight;
                }
                // MapTileX and MapTileY are now the top left no matter the origin point
                int SelArea = MapViewer.TileDataList.Count;

                int OriginDiffX = (OriginX - MapTileX) % (MapViewer.CursorWidth + 1);
                int OriginDiffY = (OriginY - MapTileY) % (MapViewer.CursorHeight + 1);

                for (int j = 0; j < SelArea; j++)
                {
                    bool Blank = blanktile;

                    int actualx = MapTileX + (j % (MapViewer.CursorWidth + 1));
                    int actualy = MapTileY + (int) Math.Floor((double) j / (MapViewer.CursorWidth + 1));
                    int MapPosition = actualy * MapData.Width + actualx;
                    if (actualx < 0 || actualx >= MapData.Width || actualy < 0 || actualy >= MapData.Height) continue;
                    if (MapViewer.SelectionX != -1 && MapViewer.SelectionY != -1 && MapViewer.SelectionWidth != 0 && MapViewer.SelectionHeight != 0 && MapViewer.SelectionBackground.Visible)
                    {
                        // NOT within the selection
                        if (!(actualx >= MapViewer.SelectionX && actualx < MapViewer.SelectionX + MapViewer.SelectionWidth &&
                              actualy >= MapViewer.SelectionY && actualy < MapViewer.SelectionY + MapViewer.SelectionHeight))
                            continue;
                    }

                    int selx = j % (MapViewer.CursorWidth + 1);
                    if (OriginDiffX < 0) selx -= OriginDiffX;
                    if (OriginDiffX > 0) selx -= OriginDiffX;
                    if (selx < 0) selx += MapViewer.CursorWidth + 1;
                    selx %= MapViewer.CursorWidth + 1;
                    int sely = (int) Math.Floor((double) j / (MapViewer.CursorWidth + 1));
                    if (OriginDiffY < 0) sely -= OriginDiffY;
                    if (OriginDiffY > 0) sely -= OriginDiffY;
                    if (sely < 0) sely += MapViewer.CursorHeight + 1;
                    sely %= MapViewer.CursorHeight + 1;

                    TileData tiledata = MapViewer.TileDataList[sely * (MapViewer.CursorWidth + 1) + selx];
                    int tileid = -1;
                    int tilesetindex = -1;
                    int tilesetx = -1;
                    int tilesety = -1;
                    if (tiledata != null)
                    {
                        tileid = tiledata.TileID;
                        tilesetindex = tiledata.TilesetIndex;
                        tilesetx = tileid % 8;
                        tilesety = (int) Math.Floor(tileid / 8d);
                    }
                    else Blank = true;

                    TileData olddata = MapData.Layers[layer].Tiles[MapPosition];
                    if (Blank)
                    {
                        MapData.Layers[layer].Tiles[MapPosition] = null;
                    }
                    else
                    {
                        MapData.Layers[layer].Tiles[MapPosition] = new TileData
                        {
                            TilesetIndex = tilesetindex,
                            TileID = tileid
                        };
                    }
                    
                    if (olddata != MapData.Layers[layer].Tiles[MapPosition])
                    {
                        Editor.UnsavedChanges = true;
                        this.Sprites[layer.ToString()].Bitmap.FillRect(actualx * 32, actualy * 32, 32, 32, Color.ALPHA);
                        if (!Blank)
                        {
                            this.Sprites[layer.ToString()].Bitmap.Build(
                                actualx * 32, actualy * 32,
                                Data.Tilesets[MapData.TilesetIDs[tilesetindex]].TilesetBitmap,
                                new Rect(tilesetx * 32, tilesety * 32, 32, 32)
                            );
                        }
                    }
                }
            }

            this.Sprites[layer.ToString()].Bitmap.Lock();
        }
    }
}
