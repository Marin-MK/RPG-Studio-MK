using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class TilesetTab : Widget
    {
        public int           TilesetIndex    { get; protected set; } = 0;
        public int           TileStartX      { get; protected set; } = 0;
        public int           TileStartY      { get; protected set; } = 0;
        public int           TileEndX        { get; protected set; } = 0;
        public int           TileEndY        { get; protected set; } = 0;

        public IconButton PencilButton;
        public IconButton FillButton;
        public IconButton EllipseButton;
        public IconButton RectButton;
        public IconButton SelectButton;
        public IconButton EraserButton;

        public LayersTab LayersTab;
        public MapViewer MapViewer;

        bool DraggingTileset = false;

        TabView TabControl;

        // Tilesets tab
        Container TilesetContainer;
        VStackPanel TilesetStackPanel;
        CursorWidget Cursor;
        MouseInputManager CursorIM;

        List<CollapsibleContainer> TilesetContainers = new List<CollapsibleContainer>();
        List<PictureBox> TilesetImages = new List<PictureBox>();

        public TilesetTab(object Parent, string Name = "tilesetTab")
            : base(Parent, Name)
        {
            SetBackgroundColor(10, 23, 37);

            Sprites["bg"] = new Sprite(this.Viewport);
            Sprites["bg"].Y = 25;

            this.OnWidgetSelect += WidgetSelect;

            CursorIM = new MouseInputManager(this);
            CursorIM.OnMouseDown += MouseDown;
            CursorIM.OnMouseUp += MouseUp;
            CursorIM.OnMouseMoving += MouseMoving;

            TabControl = new TabView(this);
            TabControl.CreateTab("Tilesets");
            TabControl.CreateTab("Autotiles");
            TabControl.OnSelectionChanged += delegate (object sender, EventArgs e)
            {
                if (TabControl.SelectedIndex == 0) UpdateCursorPosition();
            };

            Container DrawContainer = new Container(TabControl.GetTab(0));
            DrawContainer.SetPosition(15, 4);
            DrawContainer.SetSize(140, 26);

            PencilButton = new IconButton(DrawContainer);
            PencilButton.SetIcon(1, 0);
            PencilButton.SetSelected(true);

            FillButton = new IconButton(DrawContainer);
            FillButton.SetIcon(2, 0);
            FillButton.SetPosition(28, 0);

            EllipseButton = new IconButton(DrawContainer);
            EllipseButton.SetIcon(3, 0);
            EllipseButton.SetPosition(56, 0);

            RectButton = new IconButton(DrawContainer);
            RectButton.SetIcon(4, 0);
            RectButton.SetPosition(84, 0);

            SelectButton = new IconButton(DrawContainer);
            SelectButton.SetIcon(5, 0);
            SelectButton.SetPosition(112, 0);

            EraserButton = new IconButton(TabControl.GetTab(0));
            EraserButton.SetIcon(6, 0);
            EraserButton.SetPosition(155, 4);
            EraserButton.Toggleable = true;
            EraserButton.OnSelection += delegate (object sender, EventArgs e)
            {
                Cursor.SetPosition(0, 0);
                Cursor.SetVisible(false);
                MapViewer.TileDataList = new List<Data.TileData>() { null };
                MapViewer.CursorWidth = 0;
                MapViewer.CursorHeight = 0;
                UpdateCursorPosition();
                MapViewer.UpdateCursorPosition();
            };
            EraserButton.OnDeselection += delegate (object sender, EventArgs e)
            {
                UpdateCursorPosition();
            };

            TilesetContainer = new Container(TabControl.GetTab(0));
            TilesetContainer.SetPosition(1, 38);
            TilesetContainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(TabControl.GetTab(0));
            TilesetContainer.SetVScrollBar(vs);

            Cursor = new CursorWidget(TilesetContainer);
            Cursor.SetPosition(7, 8);
            Cursor.SetZIndex(1);

            TilesetStackPanel = new VStackPanel(TilesetContainer);
            TilesetStackPanel.SetWidth(264);
            TilesetStackPanel.SetPosition(8, 7);

            SetSize(293, 200); // Dummy size so the sprites can be drawn properly
        }

        public void SetTilesets(List<int> TilesetIDs)
        {
            Cursor.SetPosition(28-7, 46-7);
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
                Data.Tileset tileset = Data.GameData.Tilesets[tilesetid];
                tileset.EnsureBitmap();
                CollapsibleContainer c = new CollapsibleContainer(TilesetStackPanel);
                c.SetText(tileset.Name);
                c.SetMargin(0, 0, 0, 8);
                c.OnCollapsedChanged += delegate (object sender, EventArgs e) { UpdateCursorPosition(); };
                c.SetSize(TilesetContainer.Size.Width - 13, tileset.TilesetListBitmap.Height + 33);
                TilesetContainers.Add(c);
                PictureBox image = new PictureBox(c);
                image.SetPosition(0, 33);
                image.Sprite.Bitmap = tileset.TilesetListBitmap;
                image.SetSize(tileset.TilesetListBitmap.Width, tileset.TilesetListBitmap.Height);
                TilesetImages.Add(image);
            }
        }

        public void SelectTile(Data.TileData tile)
        {
            TilesetIndex = tile.TilesetIndex;
            TileStartX = TileEndX = tile.TileID % 8;
            TileStartY = TileEndY = (int) Math.Floor(tile.TileID / 8d);
            MapViewer.SelectionOnMap = false;
            UpdateCursorPosition();
            EraserButton.SetSelected(false);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TabControl.SetSize(Size);
            TilesetContainer.SetSize(Size.Width - 13, TabControl.GetTab(0).Size.Height - 39);
            TilesetContainer.VScrollBar.SetPosition(Size.Width - 10, 38);
            TilesetContainer.VScrollBar.SetSize(8, Size.Height - 66);
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size.Width, Size.Height - 25);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.FillRect(0, 0, Size.Width, 37, new Color(28, 50, 73));
            Sprites["bg"].Bitmap.DrawRect(0, 37, Size.Width, Size.Height - 25 - 37, new Color(28, 50, 73));
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 38, Size.Width - 12, Size.Height - 27, new Color(28, 50, 73));
            Sprites["bg"].Bitmap.Lock();
        }

        public override void Update()
        {
            CursorIM.Update(TilesetContainer.Viewport.Rect);
            base.Update();
        }

        public void UpdateCursorPosition()
        {
            if (TabControl.SelectedIndex != 0) return;
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
                Cursor.SetPosition(8 + TileStartX * 33 - PosDiffX-7, 40 + lc.Position.Y + TileStartY * 33 - PosDiffY-7);
                Cursor.SetSize(32 * (DiffX + 1) + DiffX+14, 32 * (DiffY + 1) + DiffY+14);
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
                        MapViewer.TileDataList.Add(new Data.TileData() { TileID = tileid, TilesetIndex = TilesetIndex });
                    }
                }
                Cursor.SetVisible(true);
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
                UpdateCursorPosition();
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
                TilesetIndex = idx;
                TileStartX = TileEndX = x;
                TileStartY = TileEndY = y;
                MapViewer.SelectionOnMap = false;
                UpdateCursorPosition();
                if (EraserButton.Selected)
                {
                    EraserButton.SetSelected(false);
                }
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
            if (TabControl.SelectedIndex != 0) return;
            if (!e.LeftButton && !e.RightButton) return;
            if (Parent.VScrollBar != null && (Parent.VScrollBar.Dragging || Parent.VScrollBar.Hovering)) return;
            Container cont = TilesetContainer;
            int rx = e.X - cont.Viewport.X;
            int ry = e.Y - cont.Viewport.Y;
            if (rx < 0 || ry < 0 || rx >= cont.Viewport.Width || ry >= cont.Viewport.Height) return; // Off the widget
            rx += cont.ScrolledX;
            ry += cont.ScrolledY;
            if (rx < 8 || ry < 7 || rx >= 264) return; // Not over a tileset
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
                if (cry < 40) continue; // In the name part of the container
                cry -= 40;
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
