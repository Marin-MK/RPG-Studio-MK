using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class TilesPanel : Widget
    {
        public int AutotileIndex { get; protected set; } = -1;
        public int AutotileCombination { get; protected set; } = -1;

        public int TilesetIndex { get; protected set; } = 0;
        public int TileStartX   { get; protected set; } = 0;
        public int TileStartY   { get; protected set; } = 0;
        public int TileEndX     { get; protected set; } = 0;
        public int TileEndY     { get; protected set; } = 0;

        Container DrawToolsContainer;
        public IconButton PencilButton;
        public IconButton FillButton;
        public IconButton EllipseButton;
        public IconButton RectButton;
        public IconButton SelectButton;
        public IconButton EraserButton;

        public LayerPanel LayerPanel;
        public MapViewerTiles MapViewer;

        bool DraggingTileset = false;

        public Game.Map MapData;

        // Tilesets tab
        Container MainContainer;
        VStackPanel MainStackPanel;
        public CursorWidget Cursor;
        MouseInputManager CursorIM;

        CollapsibleContainer SingleAutotileContainer;
        int SingleAutotileCount = 0;
        List<object> AutotileContainers = new List<object>();
        List<CollapsibleContainer> TilesetContainers = new List<CollapsibleContainer>();
        List<PictureBox> TilesetImages = new List<PictureBox>();

        public TilesPanel(object Parent, string Name = "tilesetTab")
            : base(Parent, Name)
        {
            Label Header = new Label(this);
            Header.SetText("Tiles");
            Header.SetFont(Font.Get("Fonts/Ubuntu-B", 16));
            Header.SetPosition(5, 5);

            Sprites["sep"] = new Sprite(this.Viewport, new SolidBitmap(288, 2, new Color(10, 23, 37)));
            Sprites["sep"].Y = 50;

            Sprites["slider"] = new Sprite(this.Viewport, new SolidBitmap(10, Size.Height - 34, new Color(10, 23, 37)));
            Sprites["slider"].Y = 53;

            this.OnWidgetSelected += WidgetSelected;

            CursorIM = new MouseInputManager(this);
            CursorIM.OnMouseDown += MouseDown;
            CursorIM.OnMouseUp += MouseUp;
            CursorIM.OnMouseMoving += MouseMoving;

            MainContainer = new Container(this);
            MainContainer.SetPosition(0, 53);
            MainContainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(this);
            MainContainer.SetVScrollBar(vs);

            Cursor = new CursorWidget(MainContainer);
            Cursor.SetPosition(3, 8);
            Cursor.SetZIndex(1);

            MainStackPanel = new VStackPanel(MainContainer);
            MainStackPanel.SetWidth(264);
            MainStackPanel.SetPosition(8, 3);

            DrawToolsContainer = new Container(this);
            DrawToolsContainer.SetPosition(46, 22);
            DrawToolsContainer.SetSize(186, 28);
            DrawToolsContainer.Sprites["line1"] = new Sprite(DrawToolsContainer.Viewport, new SolidBitmap(1, 26, new Color(28, 50, 73)));
            DrawToolsContainer.Sprites["line1"].X = 144;
            DrawToolsContainer.Sprites["line2"] = new Sprite(DrawToolsContainer.Viewport, new SolidBitmap(1, 26, new Color(28, 50, 73)));
            DrawToolsContainer.Sprites["line2"].X = 185;

            PencilButton = new IconButton(DrawToolsContainer);
            PencilButton.SetIcon(15, 0);
            PencilButton.SetSelected(true);

            FillButton = new IconButton(DrawToolsContainer);
            FillButton.SetIcon(16, 0);
            FillButton.SetPosition(32, 0);

            EllipseButton = new IconButton(DrawToolsContainer);
            EllipseButton.SetIcon(17, 0);
            EllipseButton.SetPosition(64, 0);

            RectButton = new IconButton(DrawToolsContainer);
            RectButton.SetIcon(18, 0);
            RectButton.SetPosition(96, 0);

            SelectButton = new IconButton(DrawToolsContainer);
            SelectButton.SetIcon(19, 0);
            SelectButton.SetPosition(128, 0);
            SelectButton.OnSelection += delegate (object sender, EventArgs e)
            {
                MapViewer.Cursor.SetVisible(false);
                Cursor.SetPosition(0, 0);
                Cursor.SetVisible(false);
            };
            SelectButton.OnDeselection += delegate (object sender, EventArgs e)
            {
                UpdateCursor();
            };

            EraserButton = new IconButton(DrawToolsContainer);
            EraserButton.SetIcon(20, 0);
            EraserButton.SetPosition(160, 0);
            EraserButton.Toggleable = true;
            EraserButton.OnSelection += delegate (object sender, EventArgs e)
            {
                if (AutotileIndex != -1 || TilesetIndex != -1 || TileStartX != -1 || TileEndX != -1 || TileStartY != -1 || TileEndY != -1)
                    SelectTile(null);
            };
            EraserButton.OnDeselection += delegate (object sender, EventArgs e)
            {
                if (AutotileIndex == -1 && TilesetIndex == -1 && TileStartX == -1 && TileEndX == -1 && TileStartY == -1 && TileEndY == -1 &&
                    !MapViewer.SelectionOnMap)
                    SelectTile(0, 0, 0);
                else UpdateCursor();
            };

            SetSize(288, 200); // Dummy size so the sprites can be drawn properly
        }

        public void SetMap(Map Map)
        {
            this.MapData = Map;
            Cursor.SetPosition(21, 39);
            AutotileIndex = -1;
            AutotileCombination = -1;
            TilesetIndex = 0;
            TileStartX = 0;
            TileStartY = 0;
            TileEndX = 0;
            TileEndY = 0;
            for (int i = 0; i < this.AutotileContainers.Count; i++)
            {
                if (this.AutotileContainers[i] is CollapsibleContainer)
                    (this.AutotileContainers[i] as CollapsibleContainer).Dispose();
            }
            this.AutotileContainers.Clear();
            if (SingleAutotileContainer is CollapsibleContainer) SingleAutotileContainer.Dispose();
            SingleAutotileContainer = null;
            Bitmap singles = null;
            SingleAutotileCount = MapData.AutotileIDs.FindAll(id => Data.Autotiles[id].Format == AutotileFormat.Single).Count;
            if (SingleAutotileCount > 0)
            {
                SingleAutotileContainer = new CollapsibleContainer(MainStackPanel);
                SingleAutotileContainer.SetText("Single Autotiles");
                SingleAutotileContainer.SetMargin(0, 0, 0, 8);
                SingleAutotileContainer.OnCollapsedChanged += delegate (object sender, EventArgs e) { UpdateCursor(); };
                singles = new Bitmap(263, 33 + 33 * (int) Math.Floor(SingleAutotileCount / 8d));
                singles.Unlock();
                SingleAutotileContainer.SetSize(MainContainer.Size.Width - 13, singles.Height + 23);
                PictureBox image = new PictureBox(SingleAutotileContainer);
                image.SetPosition(0, 23);
                image.Sprite.Bitmap = singles;
                image.SetSize(image.Sprite.Bitmap.Width, image.Sprite.Bitmap.Height);
            }
            int SingleIndex = 0;
            for (int i = 0; i < MapData.AutotileIDs.Count; i++)
            {
                int autotileid = MapData.AutotileIDs[i];
                Autotile autotile = Data.Autotiles[autotileid];
                if (autotile.Format == AutotileFormat.Single)
                {
                    int x = 33 * (SingleIndex % 8);
                    int y = 33 * (int) Math.Floor(SingleIndex / 8d);
                    singles.Build(x, y, autotile.AutotileBitmap, new Rect(0, 0, 32, 32));
                    AutotileContainers.Add(SingleIndex);
                    SingleIndex++;
                    continue;
                }
                CollapsibleContainer c = new CollapsibleContainer(MainStackPanel);
                c.SetText(autotile.Name);
                c.SetMargin(0, 0, 0, 8);
                c.OnCollapsedChanged += delegate (object sender, EventArgs e) { UpdateCursor(); };
                c.SetSize(MainContainer.Size.Width - 13, 87);
                c.ObjectData = i;
                AutotileContainers.Add(c);
                PictureBox image = new PictureBox(c);
                image.SetPosition(0, 23);
                AutotileFormat f = autotile.Format;
                Bitmap bmp = new Bitmap(263, 64);
                bmp.Unlock();
                for (int j = 0; j < 4; j++)
                {
                    int x = 0,
                        y = 0;
                    if (j == 0) { y = 32; }
                    else if (j == 1) { x = (f == AutotileFormat.RMVX ? 32 : 64); y = 32; }
                    else if (j == 2) { y = (f == AutotileFormat.RMVX ? 64 : 96); }
                    else if (j == 3) { x = (f == AutotileFormat.RMVX ? 32 : 64); y = (f == AutotileFormat.RMVX ? 64 : 96); }
                    bmp.Build(32 * (j % 2), 32 * (int) Math.Floor(j / 2d), autotile.AutotileBitmap, new Rect(x, y, 32, 32));
                }
                image.Sprite.Bitmap = bmp;
                image.SetSize(image.Sprite.Bitmap.Width, image.Sprite.Bitmap.Height);
                DrawQuickAutotiles(i);
                bmp.Lock();
            }
            if (singles != null) singles.Lock();
            for (int i = 0; i < this.TilesetContainers.Count; i++)
            {
                this.TilesetContainers[i].Dispose();
            }
            this.TilesetContainers.Clear();
            this.TilesetImages.Clear();
            for (int i = 0; i < MapData.TilesetIDs.Count; i++)
            {
                int tilesetid = MapData.TilesetIDs[i];
                Tileset tileset = Data.Tilesets[tilesetid];
                CollapsibleContainer c = new CollapsibleContainer(MainStackPanel);
                c.SetText(tileset.Name);
                c.SetMargin(0, 0, 0, 8);
                c.OnCollapsedChanged += delegate (object sender, EventArgs e) { UpdateCursor(); };
                c.SetSize(MainContainer.Size.Width - 13, tileset.TilesetListBitmap.Height + 23);
                TilesetContainers.Add(c);
                PictureBox image = new PictureBox(c);
                image.SetPosition(0, 23);
                image.Sprite.Bitmap = tileset.TilesetListBitmap;
                image.Sprite.DestroyBitmap = false;
                image.SetSize(image.Sprite.Bitmap.Width, image.Sprite.Bitmap.Height);
                TilesetImages.Add(image);
            }
        }

        public void DrawQuickAutotiles(int AutotileIndex)
        {
            int autotileid = MapData.AutotileIDs[AutotileIndex];
            Autotile autotile = Data.Autotiles[autotileid];
            bool locked = ((AutotileContainers[AutotileIndex] as CollapsibleContainer).Widgets[0] as PictureBox).Sprite.Bitmap.Locked;
            if (locked) ((AutotileContainers[AutotileIndex] as CollapsibleContainer).Widgets[0] as PictureBox).Sprite.Bitmap.Unlock();
            if (autotile.Format == AutotileFormat.Single) throw new Exception("Can't draw quick autotiles for autotiles with a format of 'Single'");
            for (int i = 0; i < autotile.QuickIDs.Count; i++)
            {
                int x = 66 + 33 * i;
                int y = 16;
                Bitmap bmp = new Bitmap(32, 32);
                bmp.Unlock();
                if (autotile.QuickIDs[i] == null)
                {
                    bmp.DrawRect(0, 0, 32, 32, new Color(19, 36, 55));
                    bmp.DrawRect(1, 1, 30, 30, new Color(19, 36, 55));
                    bmp.FillRect(7, 21, 3, 3, new Color(19, 36, 55));
                    bmp.FillRect(14, 21, 3, 3, new Color(19, 36, 55));
                    bmp.FillRect(21, 21, 3, 3, new Color(19, 36, 55));
                }
                else
                {
                    List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][(int) autotile.QuickIDs[i]];
                    for (int j = 0; j < 4; j++)
                    {
                        bmp.Build(new Rect(16 * (j % 2), 16 * (int) Math.Floor(j / 2d), 16, 16), autotile.AutotileBitmap,
                            new Rect(16 * (Tiles[j] % 6), 16 * (int) Math.Floor(Tiles[j] / 6d), 16, 16));
                    }
                }
                bmp.Lock();
                ((AutotileContainers[AutotileIndex] as CollapsibleContainer).Widgets[0] as PictureBox).Sprite.Bitmap.Build(x, y, bmp);
                bmp.Dispose();
            }
            if (locked) ((AutotileContainers[AutotileIndex] as CollapsibleContainer).Widgets[0] as PictureBox).Sprite.Bitmap.Lock();
        }

        public void SelectTile(int ContainerIndex, int TileX, int TileY)
        {
            if (ContainerIndex < AutotileContainers.Count) // Autotile
            {
                if (ContainerIndex == 0 && SingleAutotileCount > 0)
                {
                    if (TileX + TileY * 8 >= SingleAutotileCount) return;
                    for (int i = 0; i < AutotileContainers.Count; i++)
                    {
                        if (AutotileContainers[i] is int && (int) AutotileContainers[i] == TileX + TileY * 8)
                        {
                            this.AutotileIndex = i;
                        }
                    }
                }
                else
                {
                    int container = SingleAutotileCount == 0 ? 0 : 1;
                    int idx = -1;
                    bool found = false;
                    for (int i = 0; i < AutotileContainers.Count; i++)
                    {
                        if (AutotileContainers[i] is CollapsibleContainer)
                        {
                            if (container == ContainerIndex)
                            {
                                found = true;
                                idx = (int) (AutotileContainers[i] as CollapsibleContainer).ObjectData;
                                break;
                            }
                            else
                            {
                                container++;
                            }
                        }
                    }
                    if (found)
                    {
                        if (TileX > 1)
                        {
                            Autotile autotile = Data.Autotiles[MapData.AutotileIDs[idx]];
                            if (autotile.QuickIDs[TileX - 2] is null)
                            {
                                AutotilePickerMap atp = new AutotilePickerMap(Window);
                                atp.SetAutotile(autotile);
                                atp.OnClosed += delegate (object sender, EventArgs e)
                                {
                                    if (atp.SelectedTileID != -1)
                                    {
                                        this.AutotileIndex = idx;
                                        this.AutotileCombination = TileX - 2;
                                        autotile.QuickIDs[TileX - 2] = atp.SelectedTileID;
                                        DrawQuickAutotiles(idx);
                                        UpdateCursor();
                                    }
                                };
                            }
                            else
                            {
                                if (TimerExists("double") && !TimerPassed("double") && DoubleClickIndex == idx && DoubleClickX == TileX)
                                {
                                    AutotilePickerMap atp = new AutotilePickerMap(Window);
                                    atp.SetAutotile(autotile);
                                    atp.OnClosed += delegate (object sender, EventArgs e)
                                    {
                                        if (atp.SelectedTileID != -1)
                                        {
                                            this.AutotileIndex = idx;
                                            this.AutotileCombination = TileX - 2;
                                            autotile.QuickIDs[TileX - 2] = atp.SelectedTileID;
                                            DrawQuickAutotiles(idx);
                                            UpdateCursor();
                                        }
                                    };
                                }
                                if (TimerExists("double")) DestroyTimer("double");
                                if (!TimerExists("double"))
                                {
                                    SetTimer("double", 300);
                                    DoubleClickIndex = idx;
                                    DoubleClickX = TileX;
                                    this.AutotileIndex = idx;
                                    this.AutotileCombination = TileX - 2;
                                }
                            }
                        }
                        else
                        {
                            this.AutotileIndex = idx;
                            this.AutotileCombination = -1;
                        }
                    }
                    else throw new Exception("Couldn't find autotile index of the selected container.");
                }
                this.TilesetIndex = this.TileStartX = this.TileEndX = this.TileStartY = this.TileEndY = -1;
            }
            else // Tileset
            {
                this.AutotileIndex = -1;
                this.AutotileCombination = -1;
                this.TilesetIndex = ContainerIndex - AutotileContainers.Count;
                this.TileStartX = this.TileEndX = TileX;
                this.TileStartY = this.TileEndY = TileY;
            }
            MapViewer.SelectionOnMap = false;
            if (EraserButton.Selected) EraserButton.SetSelected(false);
            if (SelectButton.Selected)
            {
                SelectButton.SetSelected(false);
                PencilButton.SetSelected(true);
            }
            UpdateCursor();
        }

        int DoubleClickIndex;
        int DoubleClickX;

        public void SelectTile(TileData tile)
        {
            if (tile == null)
            {
                TilesetIndex = -1;
                TileStartX = TileEndX = -1;
                TileStartY = TileEndY = -1;
                EraserButton.SetSelected(true);
                MapViewer.TileDataList = new List<TileData>() { };
                for (int i = 0; i < MapViewer.CursorWidth * MapViewer.CursorHeight; i++)
                    MapViewer.TileDataList.Add(null);
                MapViewer.UpdateCursorPosition();
            }
            else
            {
                if (tile.TileType == TileType.Autotile)
                {
                    AutotileIndex = tile.Index;
                    AutotileCombination = -1;
                    MapViewer.SelectionOnMap = false;
                    EraserButton.SetSelected(false);
                }
                else if (tile.TileType == TileType.Tileset)
                {
                    AutotileIndex = -1;
                    AutotileCombination = -1;
                    TilesetIndex = tile.Index;
                    TileStartX = TileEndX = tile.ID % 8;
                    TileStartY = TileEndY = (int) Math.Floor(tile.ID / 8d);
                    MapViewer.SelectionOnMap = false;
                    EraserButton.SetSelected(false);
                }
                else
                {
                    throw new Exception("Invalid autotile format.");
                }
            }
            UpdateCursor();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            MainContainer.SetSize(Size.Width - 11, Size.Height - MainContainer.Position.Y - 1);
            MainContainer.VScrollBar.SetPosition(Size.Width - 10, 54);
            MainContainer.VScrollBar.SetSize(8, Size.Height - 56);
            Sprites["slider"].X = Size.Width - 11;
            (Sprites["slider"].Bitmap as SolidBitmap).SetSize(10, Size.Height - 54);
        }

        public override void Update()
        {
            CursorIM.Update(MainContainer.Viewport.Rect);
            base.Update();
        }

        public void UpdateCursor()
        {
            if (MapViewer.SelectionOnMap)
            {
                AutotileIndex = AutotileCombination = TilesetIndex = TileStartX = TileEndX = TileStartY = TileEndY = -1;
            }
            if (AutotileIndex == -1 && (TilesetIndex == -1 || TileStartX == -1 || TileEndX == -1 || TileStartY == -1 || TileEndY == -1) ||
                SelectButton.Selected)
            {
                Cursor.SetPosition(0, 0);
                Cursor.SetVisible(false);
                MainContainer.UpdateAutoScroll();
                return;
            }
            if (SelectButton.Selected)
            {
                SelectButton.SetSelected(false);
                PencilButton.SetSelected(true);
            }
            if (AutotileIndex != -1) // Autotile selected
            {
                object image = AutotileContainers[AutotileIndex];
                if (image is int) // Single autotile
                {
                    CollapsibleContainer cc = SingleAutotileContainer;
                    LayoutContainer lc = cc.Parent as LayoutContainer;
                    if (cc.Collapsed || EraserButton.Selected || MapViewer.SelectionOnMap)
                    {
                        Cursor.SetVisible(false);
                        Cursor.SetPosition(0, 0);
                        MainContainer.UpdateAutoScroll();
                    }
                    else
                    {
                        Cursor.SetVisible(true);
                        Cursor.SetPosition(1 + 33 * (((int) image) % 8), lc.Position.Y + 19 + 33 * (int) Math.Floor(((int) image) / 8d));
                        Cursor.SetSize(32 + 14, 32 + 14);
                    }
                }
                else if (image is CollapsibleContainer) // Other autotile format
                {
                    CollapsibleContainer cc = image as CollapsibleContainer;
                    LayoutContainer lc = cc.Parent as LayoutContainer;
                    if (cc.Collapsed || EraserButton.Selected || MapViewer.SelectionOnMap)
                    {
                        Cursor.SetVisible(false);
                        Cursor.SetPosition(0, 0);
                        MainContainer.UpdateAutoScroll();
                    }
                    else
                    {
                        Cursor.SetVisible(true);
                        if (AutotileCombination != -1)
                        {
                            Cursor.SetPosition(67 + 33 * AutotileCombination, lc.Position.Y + 19 + 16);
                            Cursor.SetSize(32 + 14, 32 + 14);
                        }
                        else
                        {
                            Cursor.SetPosition(1, lc.Position.Y + 19);
                            Cursor.SetSize(64 + 14, 64 + 14);
                        }
                    }
                }
                MapViewer.CursorOrigin = Location.TopLeft;
                MapViewer.TileDataList.Clear();
                MapViewer.CursorWidth = 0;
                MapViewer.CursorHeight = 0;
                MapViewer.SelectionOnMap = false;
                MapViewer.UpdateCursorPosition();
                int tileid = -1;
                if (AutotileCombination != -1) tileid = (int) Data.Autotiles[MapData.AutotileIDs[AutotileIndex]].QuickIDs[AutotileCombination];
                MapViewer.TileDataList = new List<TileData>() { new TileData() { TileType = TileType.Autotile, Index = AutotileIndex, ID = tileid } };
            }
            else // Tileset selected
            {
                int DiffX = TileEndX - TileStartX;
                int DiffY = TileEndY - TileStartY;
                int PosDiffX = 0;
                int PosDiffY = 0;
                Location origin = Location.BottomRight;
                // If the origin is bottom right instead of top left
                if (DiffX < 0)
                {
                    DiffX = -DiffX;
                    PosDiffX = 33 * DiffX;
                    origin = Location.BottomLeft;
                }
                if (DiffY < 0)
                {
                    DiffY = -DiffY;
                    PosDiffY = 33 * DiffY;
                    if (origin == Location.BottomLeft) origin = Location.TopLeft;
                    else origin = Location.TopRight;
                }
                MapViewer.CursorOrigin = origin;
                MapViewer.TileDataList.Clear();
                MapViewer.CursorWidth = DiffX;
                MapViewer.CursorHeight = DiffY;
                MapViewer.SelectionOnMap = false;
                MapViewer.UpdateCursorPosition();
                int sx = TileStartX < TileEndX ? TileStartX : TileEndX;
                int ex = TileStartX < TileEndX ? TileEndX : TileStartX;
                int sy = TileStartY < TileEndY ? TileStartY : TileEndY;
                int ey = TileStartY < TileEndY ? TileEndY : TileStartY;
                for (int y = sy; y <= ey; y++)
                {
                    for (int x = sx; x <= ex; x++)
                    {
                        int tileid = y * 8 + x;
                        MapViewer.TileDataList.Add(new TileData() { TileType = TileType.Tileset, Index = TilesetIndex, ID = tileid });
                    }
                }
                CollapsibleContainer cc = TilesetContainers[TilesetIndex];
                LayoutContainer lc = cc.Parent as LayoutContainer;
                if (cc.Collapsed || EraserButton.Selected || MapViewer.SelectionOnMap)
                {
                    Cursor.SetPosition(0, 0);
                    Cursor.SetVisible(false);
                    MainContainer.UpdateAutoScroll();
                }
                else
                {
                    Cursor.SetVisible(true);
                    Cursor.SetPosition(1 + TileStartX * 33 - PosDiffX, 19 + lc.Position.Y + TileStartY * 33 - PosDiffY);
                    Cursor.SetSize(32 * (DiffX + 1) + DiffX + 14, 32 * (DiffY + 1) + DiffY + 14);
                }
            }
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!DraggingTileset) return;
            int idx = -1,
                x = -1,
                y = -1;
            GetTilePosition(e, out idx, out x, out y);
            if (idx != -1 && x != -1 && y != -1)
            {
                // Makes sure you can only have a selection within the same tileset
                if (AutotileIndex != -1)
                {
                    if (idx != AutotileIndex) return;
                }
                else
                {
                    if (idx != TilesetIndex + AutotileContainers.Count) return;
                }
                TileEndX = x;
                TileEndY = y;
                UpdateCursor();
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (e.MiddleButton != e.OldMiddleButton) return; // A button other than the middle mouse button was pressed (left or right)
            int idx = -1,
                x = -1,
                y = -1;
            GetTilePosition(e, out idx, out x, out y);
            if (idx != -1 && x != -1 && y != -1)
            {
                DraggingTileset = true;
                SelectTile(idx, x, y);
            }
        }

        public override void MouseUp(object sender, MouseEventArgs e)
        {
            base.MouseUp(sender, e);
            DraggingTileset = e.LeftButton || e.RightButton;
        }

        public void GetTilePosition(MouseEventArgs e, out int ContainerIndex, out int X, out int Y)
        {
            ContainerIndex = -1;
            X = -1;
            Y = -1;
            if (!e.LeftButton && !e.RightButton) return;
            if (Parent.VScrollBar != null && (Parent.VScrollBar.Dragging || Parent.VScrollBar.Hovering)) return;
            Container cont = MainContainer;
            int rx = e.X - cont.Viewport.X;
            int ry = e.Y - cont.Viewport.Y;
            if (rx < 0 || ry < 0 || rx >= cont.Viewport.Width || ry >= cont.Viewport.Height) return; // Off the widget
            rx += cont.ScrolledX;
            ry += cont.ScrolledY;
            if (rx < 8 || ry < 3 || rx >= 264) return; // Not over an autotile/tileset
            int crx = rx - 8; // container (c) relative (r) x (x)
            for (int i = 0; i < MainStackPanel.Widgets.Count; i++)
            {
                LayoutContainer lc = MainStackPanel.Widgets[i] as LayoutContainer;
                CollapsibleContainer cc = lc.Widget as CollapsibleContainer;
                int height = lc.Position.Y;
                if (ry < height) break; // Somehow already gone past the container it's in
                if (ry > height + cc.Size.Height) continue;
                // By now we know ry is inside this CollapsibleContainer.
                // So we now need to determine the y coordinate relative to this container
                // To determine which tile we're over.
                int cry = ry - lc.Position.Y; // container (c) relative (r) y (y)
                if (cry < 26) continue; // In the name part of the container
                cry -= 26;
                int tilex = (int) Math.Floor(crx / 33d);
                int tiley = (int) Math.Floor(cry / 33d);
                ContainerIndex = i;
                X = tilex;
                Y = tiley;
                break;
            }
        }
    }
}
