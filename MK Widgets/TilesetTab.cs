using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class TilesetTab : Widget
    {
        public int           TilesetIndex    { get; protected set; } = 0;
        public int           TileX           { get; protected set; } = 0;
        public int           TileY           { get; protected set; } = 0;

        public LayersTab LayersTab;
        public MapViewer MapViewer;

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
            this.Sprites["header"] = new Sprite(this.Viewport, new Bitmap(314, 22));
            this.Sprites["header"].Bitmap.Unlock();
            this.Sprites["header"].Bitmap.FillRect(0, 0, 314, 22, new Color(135, 135, 135));
            this.Sprites["header"].Bitmap.Font = Font.Get("Fonts/Ubuntu-R", 16);
            this.Sprites["header"].Bitmap.DrawText("Tiles", 6, 0, Color.WHITE);
            this.Sprites["header"].Bitmap.Lock();

            this.SetBackgroundColor(47, 49, 54);

            this.OnWidgetSelect += WidgetSelect;

            Container DrawContainer = new Container(this);
            DrawContainer.SetPosition(75, 29);
            DrawContainer.SetSize(140, 26);

            IconButton pencil = new IconButton(DrawContainer);
            pencil.SetIcon(1, 0);
            pencil.SetSelected(true);

            IconButton bucket = new IconButton(DrawContainer);
            bucket.SetIcon(2, 0);
            bucket.SetPosition(28, 0);

            IconButton ellipse = new IconButton(DrawContainer);
            ellipse.SetIcon(3, 0);
            ellipse.SetPosition(56, 0);

            IconButton rect = new IconButton(DrawContainer);
            rect.SetIcon(4, 0);
            rect.SetPosition(84, 0);

            IconButton select = new IconButton(DrawContainer);
            select.SetIcon(5, 0);
            select.SetPosition(112, 0);

            IconButton erase = new IconButton(this);
            erase.SetIcon(6, 0);
            erase.SetPosition(215, 29);
            erase.Toggleable = true;

            TabControl = new TabView(this);
            TabControl.SetPosition(0, 62);
            TabControl.SetSize(this.Size.Width, this.Size.Height - 62);
            TabControl.CreateTab("Tilesets");
            TabControl.CreateTab("Autotiles");
            TabControl.OnSelectionChanged += delegate (object sender, EventArgs e)
            {
                if (TabControl.SelectedIndex == 0) UpdateCursorPosition();
            };

            CursorIM = new MouseInputManager(this);
            CursorIM.OnMouseDown += MouseDown;

            TilesetContainer = new Container(TabControl.GetTab(0));
            TilesetContainer.SetPosition(0, 4);
            TilesetContainer.SetSize(this.Size.Width, TabControl.GetTab(0).Size.Height - 8);
            TilesetContainer.AutoScroll = true;

            Cursor = new CursorWidget(TilesetContainer);
            Cursor.SetPosition(20, 33);
            Cursor.SetZIndex(1);

            TilesetStackPanel = new VStackPanel(TilesetContainer);
            TilesetStackPanel.SetWidth(283);
            TilesetStackPanel.SetPosition(8, 13);
        }

        public void SetTilesets(List<int> TilesetIDs)
        {
            Cursor.SetPosition(28, 46);
            TilesetIndex = 0;
            TileX = 0;
            TileY = 0;
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
                image.SetPosition(20, 33);
                image.Sprite.Bitmap = tileset.TilesetListBitmap;
                image.SetSize(tileset.TilesetListBitmap.Width, tileset.TilesetListBitmap.Height);
                TilesetImages.Add(image);
            }
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TabControl.SetSize(this.Size.Width, this.Size.Height - 62);
            TilesetContainer.SetSize(this.Size.Width, TabControl.GetTab(0).Size.Height - 8);
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
            if (cc.Collapsed)
            {
                Cursor.SetPosition(0, 0);
                Cursor.SetVisible(false);
                TilesetContainer.UpdateAutoScroll();
            }
            else
            {
                Cursor.SetPosition(28 + TileX * 33, 46 + lc.Position.Y + TileY * 33);
                Cursor.SetVisible(true);
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            if (TabControl.SelectedIndex != 0) return;
            base.MouseMoving(sender, e);
            if (e.LeftButton == e.OldLeftButton) return; // A button other than the left mouse button was pressed
            if (Parent.ScrollBarY != null && (Parent.ScrollBarY.Dragging || Parent.ScrollBarY.Hovering)) return;
            Container cont = TilesetContainer;
            int rx = e.X - cont.Viewport.X;
            int ry = e.Y - cont.Viewport.Y;
            if (rx < 0 || ry < 0 || rx >= cont.Viewport.Width || ry >= cont.Viewport.Height) return; // Off the widget
            rx += cont.ScrolledX;
            ry += cont.ScrolledY;
            if (rx < 28 || ry < 46 || rx >= 291) return; // Not over a tileset
            int crx = rx - 28; // container (c) relative (r) x (x)
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
                if (cry < 46) continue; // In the name part of the container
                cry -= 46;
                int tilex = (int) Math.Floor(crx / 33d);
                int tiley = (int) Math.Floor(cry / 33d);
                TilesetIndex = i;
                TileX = tilex;
                TileY = tiley;
                Cursor.SetPosition(28 + tilex * 33, 46 + height + tiley * 33);
                Cursor.SetVisible(true);
                break;
            }
        }
    }
}
