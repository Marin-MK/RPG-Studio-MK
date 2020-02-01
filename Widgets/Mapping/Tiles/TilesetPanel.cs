using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class TilesetPanel : Widget
    {
        public int           TilesetIndex    { get; protected set; } = 0;
        public int           TileStartX      { get; protected set; } = 0;
        public int           TileStartY      { get; protected set; } = 0;
        public int           TileEndX        { get; protected set; } = 0;
        public int           TileEndY        { get; protected set; } = 0;

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

        // Tilesets tab
        Container TilesetContainer;
        VStackPanel TilesetStackPanel;
        public CursorWidget Cursor;
        MouseInputManager CursorIM;

        List<CollapsibleContainer> TilesetContainers = new List<CollapsibleContainer>();
        List<PictureBox> TilesetImages = new List<PictureBox>();

        public TilesetPanel(object Parent, string Name = "tilesetTab")
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

            TilesetContainer = new Container(this);
            TilesetContainer.SetPosition(0, 53);
            TilesetContainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(this);
            TilesetContainer.SetVScrollBar(vs);

            Cursor = new CursorWidget(TilesetContainer);
            Cursor.SetPosition(3, 8);
            Cursor.SetZIndex(1);

            TilesetStackPanel = new VStackPanel(TilesetContainer);
            TilesetStackPanel.SetWidth(264);
            TilesetStackPanel.SetPosition(8, 3);

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
                if (TilesetIndex != -1 || TileStartX != -1 || TileEndX != -1 || TileStartY != -1 || TileEndY != -1)
                    SelectTile(null);
            };
            EraserButton.OnDeselection += delegate (object sender, EventArgs e)
            {
                if (TilesetIndex == -1 && TileStartX == -1 && TileEndX == -1 && TileStartY == -1 && TileEndY == -1 &&
                    !MapViewer.SelectionOnMap)
                    SelectTile(0, 0, 0);
                else UpdateCursor();
            };

            SetSize(288, 200); // Dummy size so the sprites can be drawn properly
        }

        public void SetTilesets(List<int> TilesetIDs)
        {
            Cursor.SetPosition(21, 39);
            TilesetIndex = 0;
            TileStartX = 0;
            TileStartY = 0;
            TileEndX = 0;
            TileEndY = 0;
            for (int i = 0; i < this.TilesetContainers.Count; i++)
            {
                this.TilesetContainers[i].Dispose();
            }
            this.TilesetContainers.Clear();
            this.TilesetImages.Clear();
            for (int i = 0; i < TilesetIDs.Count; i++)
            {
                int tilesetid = TilesetIDs[i];
                Tileset tileset = Data.Tilesets[tilesetid];
                CollapsibleContainer c = new CollapsibleContainer(TilesetStackPanel);
                c.SetText(tileset.Name);
                c.SetMargin(0, 0, 0, 8);
                c.OnCollapsedChanged += delegate (object sender, EventArgs e) { UpdateCursor(); };
                c.SetSize(TilesetContainer.Size.Width - 13, tileset.TilesetListBitmap.Height + 23);
                TilesetContainers.Add(c);
                PictureBox image = new PictureBox(c);
                image.SetPosition(0, 23);
                image.Sprite.Bitmap = tileset.TilesetListBitmap;
                image.SetSize(tileset.TilesetListBitmap.Width, tileset.TilesetListBitmap.Height);
                TilesetImages.Add(image);
            }
        }

        public void SelectTile(int TilesetIndex, int TileX, int TileY)
        {
            this.TilesetIndex = TilesetIndex;
            this.TileStartX = this.TileEndX = TileX;
            this.TileStartY = this.TileEndY = TileY;
            MapViewer.SelectionOnMap = false;
            if (EraserButton.Selected) EraserButton.SetSelected(false);
            if (SelectButton.Selected)
            {
                SelectButton.SetSelected(false);
                PencilButton.SetSelected(true);
            }
            UpdateCursor();
        }

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
                TilesetIndex = tile.TilesetIndex;
                TileStartX = TileEndX = tile.TileID % 8;
                TileStartY = TileEndY = (int) Math.Floor(tile.TileID / 8d);
                MapViewer.SelectionOnMap = false;
                EraserButton.SetSelected(false);
            }
            UpdateCursor();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TilesetContainer.SetSize(Size.Width - 11, Size.Height - TilesetContainer.Position.Y - 1);
            TilesetContainer.VScrollBar.SetPosition(Size.Width - 10, 54);
            TilesetContainer.VScrollBar.SetSize(8, Size.Height - 56);
            Sprites["slider"].X = Size.Width - 11;
            (Sprites["slider"].Bitmap as SolidBitmap).SetSize(10, Size.Height - 54);
        }

        public override void Update()
        {
            CursorIM.Update(TilesetContainer.Viewport.Rect);
            base.Update();
        }

        public void UpdateCursor()
        {
            if (MapViewer.SelectionOnMap)
            {
                TilesetIndex = TileStartX = TileEndX = TileStartY = TileEndY = -1;
            }
            if (TilesetIndex == -1 || TileStartX == -1 || TileEndX == -1 || TileStartY == -1 || TileEndY == -1 ||
                SelectButton.Selected)
            {
                Cursor.SetPosition(0, 0);
                Cursor.SetVisible(false);
                TilesetContainer.UpdateAutoScroll();
                return;
            }
            if (SelectButton.Selected)
            {
                SelectButton.SetSelected(false);
                PencilButton.SetSelected(true);
            }
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
                    MapViewer.TileDataList.Add(new TileData() { TileID = tileid, TilesetIndex = TilesetIndex });
                }
            }
            LayoutContainer lc = TilesetStackPanel.Widgets[TilesetIndex] as LayoutContainer;
            CollapsibleContainer cc = lc.Widget as CollapsibleContainer;
            if (cc.Collapsed || EraserButton.Selected || MapViewer.SelectionOnMap)
            {
                Cursor.SetPosition(0, 0);
                Cursor.SetVisible(false);
                TilesetContainer.UpdateAutoScroll();
            }
            else
            {
                Cursor.SetVisible(true);
                Cursor.SetPosition(1 + TileStartX * 33 - PosDiffX, 19 + lc.Position.Y + TileStartY * 33 - PosDiffY);
                Cursor.SetSize(32 * (DiffX + 1) + DiffX + 14, 32 * (DiffY + 1) + DiffY + 14);
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
                if (idx != TilesetIndex) return;
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

        public void GetTilePosition(MouseEventArgs e, out int TilesetIndex, out int X, out int Y)
        {
            TilesetIndex = -1;
            X = -1;
            Y = -1;
            if (!e.LeftButton && !e.RightButton) return;
            if (Parent.VScrollBar != null && (Parent.VScrollBar.Dragging || Parent.VScrollBar.Hovering)) return;
            Container cont = TilesetContainer;
            int rx = e.X - cont.Viewport.X;
            int ry = e.Y - cont.Viewport.Y;
            if (rx < 0 || ry < 0 || rx >= cont.Viewport.Width || ry >= cont.Viewport.Height) return; // Off the widget
            rx += cont.ScrolledX;
            ry += cont.ScrolledY;
            if (rx < 8 || ry < 3 || rx >= 264) return; // Not over a tileset
            int crx = rx - 8; // container (c) relative (r) x (x)
            // crx will always be between 0 and 256 because any other value has been caught with the if-statements already
            for (int i = 0; i < TilesetStackPanel.Widgets.Count; i++)
            {
                LayoutContainer lc = TilesetStackPanel.Widgets[i] as LayoutContainer;
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
                TilesetIndex = i;
                X = tilex;
                Y = tiley;
                break;
            }
        }
    }
}
