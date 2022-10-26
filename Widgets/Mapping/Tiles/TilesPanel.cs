using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class TilesPanel : Widget
{
    public int AutotileIndex { get; protected set; } = -1;
    public int AutotileCombination { get; protected set; } = -1;

    public int TilesetIndex { get; protected set; } = 0;
    public int TileStartX { get; protected set; } = 0;
    public int TileStartY { get; protected set; } = 0;
    public int TileEndX { get; protected set; } = 0;
    public int TileEndY { get; protected set; } = 0;

    public DrawTools DrawTool
    {
        get
        {
            if (PencilButton.Selected) return DrawTools.Pencil;
            else if (FillButton.Selected) return DrawTools.Bucket;
            else if (EllipseButton.Selected) return Editor.GeneralSettings.PreferEllipseFill ? DrawTools.EllipseFilled : DrawTools.EllipseOutline;
            else if (RectButton.Selected) return Editor.GeneralSettings.PreferRectangleFill ? DrawTools.RectangleFilled : DrawTools.RectangleOutline;
            else if (SelectButton.Selected) return Editor.GeneralSettings.PreferSelectionAll ? DrawTools.SelectionAllLayers : DrawTools.SelectionActiveLayer;
            else throw new Exception("Unknown draw tool.");
        }
        set
        {
            PencilButton.SetSelected(value == DrawTools.Pencil);
            FillButton.SetSelected(value == DrawTools.Bucket);
            EllipseButton.SetSelected(value == DrawTools.EllipseFilled || value == DrawTools.EllipseOutline);
            RectButton.SetSelected(value == DrawTools.RectangleFilled || value == DrawTools.RectangleOutline);
            bool oldsel = SelectButton.Selected;
            SelectButton.SetSelected(value == DrawTools.SelectionActiveLayer || value == DrawTools.SelectionAllLayers);
            if (SelectButton.Selected != oldsel)
            {
                if (SelectButton.Selected)
                {
                    MapViewer.Cursor.SetVisible(false);
                    Cursor.SetPosition(0, 0);
                    Cursor.SetVisible(false);
                }
                else
                {
                    UpdateCursor();
                }
            }
        }
    }
    public bool Erase
    {
        get
        {
            return EraserButton.Selected;
        }
        set
        {
            EraserButton.SetSelected(value);
        }
    }

    public bool UsingLeft = false;
    public bool UsingRight = false;

    private Container DrawToolsContainer;
    private IconButton PencilButton;
    private IconButton FillButton;
    private IconButton EllipseButton;
    private IconButton RectButton;
    private IconButton SelectButton;
    private IconButton EraserButton;

    public LayerPanel LayerPanel { get { return MapViewer.LayerPanel; } }
    public MapViewer MapViewer;

    bool DraggingTileset = false;

    public Game.Map MapData;

    // Tilesets tab
    Container MainContainer;
    VStackPanel MainStackPanel;
    public CursorWidget Cursor;

    CollapsibleContainer SingleAutotileContainer;
    int SingleAutotileCount = 0;
    List<object> AutotileContainers = new List<object>();
    List<CollapsibleContainer> TilesetContainers = new List<CollapsibleContainer>();

    public TilesPanel(IContainer Parent) : base(Parent)
    {
        Label Header = new Label(this);
        Header.SetText("Tiles");
        Header.SetFont(Fonts.Header);
        Header.SetPosition(5, 5);

        Sprites["sep"] = new Sprite(this.Viewport, new SolidBitmap(288, 2, new Color(10, 23, 37)));
        Sprites["sep"].Y = 50;

        Sprites["slider"] = new Sprite(this.Viewport, new SolidBitmap(10, Size.Height - 34, new Color(10, 23, 37)));
        Sprites["slider"].Y = 53;

        this.OnWidgetSelected += WidgetSelected;

        MainContainer = new Container(this);
        MainContainer.VAutoScroll = true;
        MainContainer.SetDocked(true);
        MainContainer.SetPadding(0, 53, 11, 1);

        VScrollBar vs = new VScrollBar(this);
        vs.SetVDocked(true);
        vs.SetRightDocked(true);
        vs.SetPadding(0, 54, 0, 2);
        MainContainer.SetVScrollBar(vs);

        Cursor = new CursorWidget(MainContainer);
        Cursor.SetPosition(3, 8);
        Cursor.SetZIndex(1);

        MainStackPanel = new VStackPanel(MainContainer);
        MainStackPanel.SetWidth(264);
        MainStackPanel.SetPosition(8, 3);

        DrawToolsContainer = new Container(this);
        DrawToolsContainer.SetPosition(46, 22);
        DrawToolsContainer.SetSize(194, 28);
        DrawToolsContainer.Sprites["line"] = new Sprite(DrawToolsContainer.Viewport, new SolidBitmap(1, 22, new Color(79, 108, 159)));
        DrawToolsContainer.Sprites["line"].X = 160;
        DrawToolsContainer.Sprites["line"].Y = 2;

        PencilButton = new IconButton(DrawToolsContainer);
        PencilButton.SetIcon(Icon.Pencil);
        PencilButton.SetSelected(true);
        PencilButton.OnSelection += delegate (BaseEventArgs e)
        {
            Window.UI.SetSelectedWidget(Editor.MainWindow.MapWidget.MapViewer);
        };

        RectButton = new IconButton(DrawToolsContainer);
        RectButton.SetIcon(Editor.GeneralSettings.PreferRectangleFill ? Icon.RectangleFilled : Icon.RectangleOutline);
        RectButton.SetPosition(32, 0);
        RectButton.OnSelection += delegate (BaseEventArgs e)
        {
            Window.UI.SetSelectedWidget(Editor.MainWindow.MapWidget.MapViewer);
        };
        RectButton.OnRightMouseDownInside += e =>
        {
            e.Handled = true;
            ShowRectOptionsMenu();
        };

        EllipseButton = new IconButton(DrawToolsContainer);
        EllipseButton.SetIcon(Editor.GeneralSettings.PreferEllipseFill ? Icon.CircleFilled : Icon.CircleOutline);
        EllipseButton.SetPosition(64, 0);
        EllipseButton.OnSelection += delegate (BaseEventArgs e)
        {
            Window.UI.SetSelectedWidget(Editor.MainWindow.MapWidget.MapViewer);
        };
        EllipseButton.OnRightMouseDownInside += e =>
        {
            e.Handled = true;
            ShowEllipseOptionsMenu();
        };

        FillButton = new IconButton(DrawToolsContainer);
        FillButton.SetIcon(Icon.Bucket);
        FillButton.SetPosition(96, 0);
        FillButton.OnSelection += delegate (BaseEventArgs e)
        {
            Window.UI.SetSelectedWidget(Editor.MainWindow.MapWidget.MapViewer);
        };

        EraserButton = new IconButton(DrawToolsContainer);
        EraserButton.SetIcon(Icon.Eraser);
        EraserButton.SetPosition(128, 0);
        EraserButton.Toggleable = true;
        EraserButton.OnSelection += delegate (BaseEventArgs e)
        {
            if (AutotileIndex != -1 || TilesetIndex != -1 || TileStartX != -1 || TileEndX != -1 || TileStartY != -1 || TileEndY != -1)
                SelectTile(null);
            Window.UI.SetSelectedWidget(Editor.MainWindow.MapWidget.MapViewer);
        };
        EraserButton.OnDeselection += delegate (BaseEventArgs e)
        {
            if (AutotileIndex == -1 && TilesetIndex == -1 && TileStartX == -1 && TileEndX == -1 && TileStartY == -1 && TileEndY == -1 &&
                !MapViewer.SelectionOnMap)
                SelectTile(0, 0, 0);
            else UpdateCursor();
        };

        SelectButton = new IconButton(DrawToolsContainer);
        SelectButton.SetIcon(Editor.GeneralSettings.PreferSelectionAll ? Icon.SelectionMultiple : Icon.Selection);
        SelectButton.SetPosition(168, 0);
        SelectButton.OnSelection += delegate (BaseEventArgs e)
        {
            Window.UI.SetSelectedWidget(Editor.MainWindow.MapWidget.MapViewer);
        };
        SelectButton.OnRightMouseDownInside += e =>
        {
            e.Handled = true;
            ShowSelectOptionsMenu();
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
        SingleAutotileCount = MapData.AutotileIDs.FindAll(id => id != -1 && Data.Autotiles[id] != null/*Data.Autotiles[id].Format == AutotileFormat.Single*/).Count;
        if (SingleAutotileCount > 0)
        {
            SingleAutotileContainer = new CollapsibleContainer(MainStackPanel);
            SingleAutotileContainer.SetText("Autotiles");// SetText("Single Autotiles");
            SingleAutotileContainer.SetPadding(0, 0, 0, 8);
            SingleAutotileContainer.OnCollapsedChanged += delegate (BaseEventArgs e) { UpdateCursor(); };
            singles = new Bitmap(263, 33 + 33 * (int)Math.Floor(SingleAutotileCount / 8d));
            singles.Unlock();
            SingleAutotileContainer.SetSize(MainContainer.Size.Width - 13, singles.Height + 23);
            ImageBox image = new ImageBox(SingleAutotileContainer);
            image.SetPosition(0, 23);
            image.SetBitmap(singles);
        }
        int SingleIndex = 0;
        for (int i = 0; i < MapData.AutotileIDs.Count; i++)
        {
            int autotileid = MapData.AutotileIDs[i];
            if (autotileid == -1) continue;
            Autotile autotile = Data.Autotiles[autotileid];
            if (autotile == null) continue;
            //if (autotile.Format == AutotileFormat.Single)
            //{
            int x = 33 * (SingleIndex % 8);
            int y = 33 * (int)Math.Floor(SingleIndex / 8d);
            singles.Build(x, y, autotile.AutotileBitmap, new Rect(0, 0, 32, 32));
            AutotileContainers.Add(i);
            SingleIndex++;
            //    continue;
            //}
            /*CollapsibleContainer c = new CollapsibleContainer(MainStackPanel);
            c.SetText(autotile.Name);
            c.SetMargin(0, 0, 0, 8);
            c.OnCollapsedChanged += delegate (BaseEventArgs e) { UpdateCursor(); };
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
                // Constructs a bitmap with the four corner autotile pieces.
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
            bmp.Lock();*/
        }
        if (singles != null) singles.Lock();
        for (int i = 0; i < this.TilesetContainers.Count; i++)
        {
            this.TilesetContainers[i].Dispose();
        }
        this.TilesetContainers.Clear();
        for (int i = 0; i < MapData.TilesetIDs.Count; i++)
        {
            int tilesetid = MapData.TilesetIDs[i];
            Tileset tileset = Data.Tilesets[tilesetid];
            CollapsibleContainer c = new CollapsibleContainer(MainStackPanel);
            c.SetText(tileset.Name);
            c.SetPadding(0, 0, 0, 8);
            c.OnCollapsedChanged += delegate (BaseEventArgs e) { UpdateCursor(); };
            TilesetContainers.Add(c);
            if (tileset.TilesetListBitmap == null)
            {
                c.SetSize(MainContainer.Size.Width - 13, 22);
            }
            else
            {
                c.SetSize(MainContainer.Size.Width - 13, tileset.TilesetListBitmap.Height + 23);
                if (!tileset.TilesetListBitmap.IsChunky)
                {
                    ImageBox image = new ImageBox(c);
                    image.SetPosition(0, 23);
                    image.SetBitmap(tileset.TilesetListBitmap);
                    image.SetDestroyBitmap(false);
                }
                else
                {
                    int y = 23;
                    foreach (Bitmap b in tileset.TilesetListBitmap.InternalBitmaps)
                    {
                        ImageBox img = new ImageBox(c);
                        img.SetPosition(0, y);
                        img.SetBitmap(b);
                        if (!b.Locked) b.Lock();
                        img.SetDestroyBitmap(false);
                        img.SetSize(b.Width, b.Height);
                        y += b.Height;
                    }
                }
            }
        }
    }

    public void DrawQuickAutotiles(int AutotileIndex)
    {
        int autotileid = MapData.AutotileIDs[AutotileIndex];
        Autotile autotile = Data.Autotiles[autotileid];
        bool locked = ((AutotileContainers[AutotileIndex] as CollapsibleContainer).Widgets[0] as ImageBox).Bitmap.Locked;
        if (locked) ((AutotileContainers[AutotileIndex] as CollapsibleContainer).Widgets[0] as ImageBox).Bitmap.Unlock();
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
                List<int> Tiles = Autotile.AutotileCombinations[autotile.Format][(int)autotile.QuickIDs[i]];
                for (int j = 0; j < 4; j++)
                {
                    bmp.Build(new Rect(16 * (j % 2), 16 * (int)Math.Floor(j / 2d), 16, 16), autotile.AutotileBitmap,
                        new Rect(16 * (Tiles[j] % 6), 16 * (int)Math.Floor(Tiles[j] / 6d), 16, 16));
                }
            }
            bmp.Lock();
            ((AutotileContainers[AutotileIndex] as CollapsibleContainer).Widgets[0] as ImageBox).Bitmap.Build(x, y, bmp);
            bmp.Dispose();
        }
        if (locked) ((AutotileContainers[AutotileIndex] as CollapsibleContainer).Widgets[0] as ImageBox).Bitmap.Lock();
    }

    public void SelectTile(int ContainerIndex, int TileX, int TileY)
    {
        int autotileboxes = AutotileContainers.Count - AutotileContainers.FindAll(e => e is int).Count + 1;
        if (AutotileContainers.Count == 0) autotileboxes = 0;
        if (ContainerIndex < autotileboxes) // Autotile
        {
            if (ContainerIndex == 0 && SingleAutotileCount > 0)
            {
                if (TileX + TileY * 8 >= SingleAutotileCount) return;
                this.AutotileIndex = (int)AutotileContainers[TileX + TileY * 8];
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
                            idx = (int)(AutotileContainers[i] as CollapsibleContainer).ObjectData;
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
                            AutotilePickerMap atp = new AutotilePickerMap();
                            atp.SetAutotile(autotile);
                            atp.OnClosed += delegate (BaseEventArgs e)
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
                                AutotilePickerMap atp = new AutotilePickerMap();
                                atp.SetAutotile(autotile);
                                atp.OnClosed += delegate (BaseEventArgs e)
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
            this.TilesetIndex = ContainerIndex - autotileboxes;
            this.TileStartX = this.TileEndX = TileX;
            this.TileStartY = this.TileEndY = TileY;
        }
        MapViewer.SelectionOnMap = false;
        if (this.Erase) this.Erase = false;
        if (this.DrawTool == DrawTools.SelectionActiveLayer || this.DrawTool == DrawTools.SelectionAllLayers)
        {
            this.DrawTool = DrawTools.Pencil;
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
            this.Erase = true;
            MapViewer.TileDataList.Clear();
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
                this.Erase = false;
            }
            else if (tile.TileType == TileType.Tileset)
            {
                AutotileIndex = -1;
                AutotileCombination = -1;
                TilesetIndex = tile.Index;
                TileStartX = TileEndX = tile.ID % 8;
                TileStartY = TileEndY = (int)Math.Floor(tile.ID / 8d);
                MapViewer.SelectionOnMap = false;
                this.Erase = false;
            }
            else
            {
                throw new Exception("Invalid autotile format.");
            }
        }
        UpdateCursor();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["slider"].X = Size.Width - 11;
        ((SolidBitmap) Sprites["slider"].Bitmap).SetSize(10, Size.Height - 54);
    }

    public void UpdateCursor()
    {
        if (MapViewer.SelectionOnMap)
        {
            AutotileIndex = AutotileCombination = TilesetIndex = TileStartX = TileEndX = TileStartY = TileEndY = -1;
        }
        if (AutotileIndex == -1 && (TilesetIndex == -1 || TileStartX == -1 || TileEndX == -1 || TileStartY == -1 || TileEndY == -1) ||
            this.DrawTool == DrawTools.SelectionActiveLayer || this.DrawTool == DrawTools.SelectionAllLayers)
        {
            Cursor.SetPosition(0, 0);
            Cursor.SetVisible(false);
            MainContainer.UpdateAutoScroll();
            return;
        }
        if (this.DrawTool == DrawTools.SelectionActiveLayer || this.DrawTool == DrawTools.SelectionAllLayers)
        {
            this.DrawTool = DrawTools.Pencil;
        }
        if (AutotileIndex != -1) // Autotile selected
        {
            int image = AutotileContainers.IndexOf(AutotileIndex);
            //if (image is int) // Single autotile
            //{
            CollapsibleContainer cc = SingleAutotileContainer;
            if (cc.Collapsed || this.Erase || MapViewer.SelectionOnMap)
            {
                Cursor.SetVisible(false);
                Cursor.SetPosition(0, 0);
                MainContainer.UpdateAutoScroll();
            }
            else
            {
                Cursor.SetVisible(true);
                Cursor.SetPosition(1 + 33 * (((int)image) % 8), cc.Position.Y + 19 + 33 * (int)Math.Floor(((int)image) / 8d));
                Cursor.SetSize(32 + 14, 32 + 14);
            }
            //}
            //else if (image is CollapsibleContainer) // Other autotile format
            //{
            //    CollapsibleContainer cc = image as CollapsibleContainer;
            //    if (cc.Collapsed || this.Erase || MapViewer.SelectionOnMap)
            //    {
            //        Cursor.SetVisible(false);
            //        Cursor.SetPosition(0, 0);
            //        MainContainer.UpdateAutoScroll();
            //    }
            //    else
            //    {
            //        Cursor.SetVisible(true);
            //        if (AutotileCombination != -1)
            //        {
            //            Cursor.SetPosition(67 + 33 * AutotileCombination, cc.Position.Y + 19 + 16);
            //            Cursor.SetSize(32 + 14, 32 + 14);
            //        }
            //        else
            //        {
            //            Cursor.SetPosition(1, cc.Position.Y + 19);
            //            Cursor.SetSize(64 + 14, 64 + 14);
            //        }
            //    }
            //}
            MapViewer.CursorOrigin = Location.TopLeft;
            MapViewer.TileDataList.Clear();
            MapViewer.CursorWidth = 0;
            MapViewer.CursorHeight = 0;
            MapViewer.SelectionOnMap = false;
            MapViewer.UpdateCursorPosition();
            int tileid = -1;
            if (AutotileCombination != -1) tileid = (int)Data.Autotiles[MapData.AutotileIDs[AutotileIndex]].QuickIDs[AutotileCombination];
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
            if (cc.Collapsed || this.Erase || MapViewer.SelectionOnMap || Data.Tilesets[MapData.TilesetIDs[TilesetIndex]].TilesetBitmap == null)
            {
                Cursor.SetPosition(0, 0);
                Cursor.SetVisible(false);
                MainContainer.UpdateAutoScroll();
            }
            else
            {
                Cursor.SetVisible(true);
                Cursor.SetPosition(1 + TileStartX * 33 - PosDiffX, 19 + cc.Position.Y + TileStartY * 33 - PosDiffY);
                Cursor.SetSize(32 * (DiffX + 1) + DiffX + 14, 32 * (DiffY + 1) + DiffY + 14);
            }
        }
    }

    public override void MouseMoving(MouseEventArgs e)
    {
        base.MouseMoving(e);
        if (!DraggingTileset || MapViewer.UsingLeft || MapViewer.UsingRight || LayerPanel.UsingLeft || LayerPanel.UsingRight) return;
        int idx = -1,
            x = -1,
            y = -1;
        GetTilePosition(e, out idx, out x, out y);
        if (idx != -1 && x != -1 && y != -1 && (UsingLeft || UsingRight))
        {
            // Makes sure you can only have a selection within the same tileset
            if (AutotileIndex != -1)
            {
                if (idx != AutotileIndex) return;
            }
            else
            {
                int autotileboxes = AutotileContainers.Count - AutotileContainers.FindAll(e => e is int).Count + 1;
                if (AutotileContainers.Count == 0) autotileboxes = 0;
                if (idx != TilesetIndex + autotileboxes) return;
            }
            TileEndX = x;
            TileEndY = y;
            UpdateCursor();
        }
    }

    public override void MouseDown(MouseEventArgs e)
    {
        base.MouseDown(e);
        if (e.MiddleButton != e.OldMiddleButton) return; // A button other than the middle mouse button was pressed (left or right)
        if (MapViewer.UsingLeft || MapViewer.UsingRight || LayerPanel.UsingLeft || LayerPanel.UsingRight) return;
        if (Mouse.Inside)
        {
            if (Mouse.LeftMouseTriggered) UsingLeft = true;
            if (Mouse.RightMouseTriggered) UsingRight = true;
        }
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

    public override void MouseUp(MouseEventArgs e)
    {
        base.MouseUp(e);
        if (Mouse.LeftMouseReleased) UsingLeft = false;
        if (Mouse.RightMouseReleased) UsingRight = false;
        DraggingTileset = e.LeftButton || e.RightButton;
    }

    public void GetTilePosition(MouseEventArgs e, out int ContainerIndex, out int X, out int Y)
    {
        ContainerIndex = -1;
        X = -1;
        Y = -1;
        if (!e.LeftButton && !e.RightButton) return;
        if (Parent.VScrollBar != null && (Parent.VScrollBar.SliderDragging || Parent.VScrollBar.SliderHovering)) return;
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
            CollapsibleContainer cc = MainStackPanel.Widgets[i] as CollapsibleContainer;
            int height = cc.Position.Y;
            if (ry < height) break; // Somehow already gone past the container it's in
            if (ry > height + cc.Size.Height) continue;
            // By now we know ry is inside this CollapsibleContainer.
            // So we now need to determine the y coordinate relative to this container
            // To determine which tile we're over.
            int cry = ry - cc.Position.Y; // container (c) relative (r) y (y)
            if (cry < 26) continue; // In the name part of the container
            cry -= 26;
            int tilex = (int)Math.Floor(crx / 33d);
            int tiley = (int)Math.Floor(cry / 33d);
            ContainerIndex = i;
            X = tilex;
            Y = tiley;
            break;
        }
    }

    void ShowRectOptionsMenu()
    {
        ToolDropdown d = new ToolDropdown(this);
        d.SetPosition(DrawToolsContainer.Position.X + RectButton.Position.X - 6, DrawToolsContainer.Position.Y + RectButton.Position.Y - 6);
        d.SetZIndex(1);
        d.SetIcon1(Icon.RectangleOutline);
        d.SetIcon2(Icon.RectangleFilled);
        d.MouseMoving(Graphics.LastMouseEvent);
        d.OnMouseDown += delegate (MouseEventArgs e)
        {
            if (d.HoveringIndex == -1) d.Dispose();
            else
            {
                Editor.GeneralSettings.PreferRectangleFill = d.HoveringIndex == 1;
                RectButton.SetIcon(d.HoveringIndex == 1 ? Icon.RectangleFilled : Icon.RectangleOutline, RectButton.Selected);
                this.DrawTool = d.HoveringIndex == 1 ? DrawTools.RectangleFilled : DrawTools.RectangleOutline;
                d.Dispose();
            }
        };
    }

    void ShowEllipseOptionsMenu()
    {
        ToolDropdown d = new ToolDropdown(this);
        d.SetPosition(DrawToolsContainer.Position.X + EllipseButton.Position.X - 6, DrawToolsContainer.Position.Y + EllipseButton.Position.Y - 6);
        d.SetZIndex(1);
        d.SetIcon1(Icon.CircleOutline);
        d.SetIcon2(Icon.CircleFilled);
        d.MouseMoving(Graphics.LastMouseEvent);
        d.OnMouseDown += delegate (MouseEventArgs e)
        {
            if (d.HoveringIndex == -1) d.Dispose();
            else
            {
                Editor.GeneralSettings.PreferEllipseFill = d.HoveringIndex == 1;
                EllipseButton.SetIcon(d.HoveringIndex == 1 ? Icon.CircleFilled : Icon.CircleOutline, EllipseButton.Selected);
                this.DrawTool = d.HoveringIndex == 1 ? DrawTools.EllipseFilled : DrawTools.EllipseOutline;
                d.Dispose();
            }
        };
    }

    void ShowSelectOptionsMenu()
    {
        ToolDropdown d = new ToolDropdown(this);
        d.SetPosition(DrawToolsContainer.Position.X + SelectButton.Position.X - 6, DrawToolsContainer.Position.Y + SelectButton.Position.Y - 6);
        d.SetZIndex(1);
        d.SetIcon1(Icon.Selection);
        d.SetIcon2(Icon.SelectionMultiple);
        d.MouseMoving(Graphics.LastMouseEvent);
        d.OnMouseDown += delegate (MouseEventArgs e)
        {
            if (d.HoveringIndex == -1) d.Dispose();
            else
            {
                Editor.GeneralSettings.PreferSelectionAll = d.HoveringIndex == 1;
                SelectButton.SetIcon(d.HoveringIndex == 1 ? Icon.SelectionMultiple : Icon.Selection, SelectButton.Selected);
                this.DrawTool = d.HoveringIndex == 1 ? DrawTools.SelectionAllLayers : DrawTools.SelectionActiveLayer;
                d.Dispose();
            }
        };
    }
}

public enum DrawTools
{
    Pencil,
    Bucket,
    EllipseOutline,
    EllipseFilled,
    RectangleOutline,
    RectangleFilled,
    SelectionActiveLayer,
    SelectionAllLayers
}
