using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class TilesetTab : Widget
    {
        public Data.Tileset  Tileset        { get; protected set; }
        public int          TilesetIndex    { get; protected set; } = 0;
        public int          TileX           { get; protected set; } = 0;
        public int          TileY           { get; protected set; } = 0;

        public LayersTab LayersTab;
        public MapViewer MapViewer;

        Container AllTilesetContainer;
        VStackPanel TilesetStackPanel;

        CursorWidget Cursor;

        MouseInputManager CursorIM;

        List<CollapsibleContainer> TilesetContainers = new List<CollapsibleContainer>();
        List<PictureBox> TilesetImages = new List<PictureBox>();

        public TilesetTab(object Parent, string Name = "tilesetTab")
            : base(Parent, Name)
        {
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].X = 6;
            this.Sprites["text"].Y = 14;
            Font f = Font.Get("Fonts/Quicksand Bold", 16);
            Size s = f.TextSize("Tilesets");
            this.Sprites["text"].Bitmap = new Bitmap(s);
            this.Sprites["text"].Bitmap.Unlock();
            this.Sprites["text"].Bitmap.Font = f;
            this.Sprites["text"].Bitmap.DrawText("Tilesets", 0, 0, Color.WHITE);
            this.Sprites["text"].Bitmap.Lock();

            this.SetBackgroundColor(27, 28, 32);

            this.OnWidgetSelect += WidgetSelect;

            CursorIM = new MouseInputManager(this);
            CursorIM.OnMouseDown += MouseDown;

            AllTilesetContainer = new Container(this);
            AllTilesetContainer.SetPosition(10, 47);
            AllTilesetContainer.SetWidth(299);
            AllTilesetContainer.AutoScroll = true;

            Cursor = new CursorWidget(AllTilesetContainer);
            Cursor.SetPosition(20, 33);
            Cursor.SetZIndex(1);

            TilesetStackPanel = new VStackPanel(AllTilesetContainer);
            TilesetStackPanel.SetWidth(283);
        }

        public void SetTilesets(List<int> TilesetIDs)
        {
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
                c.SetSize(AllTilesetContainer.Size.Width - 13, tileset.TilesetListBitmap.Height + 33);
                TilesetContainers.Add(c);
                PictureBox image = new PictureBox(c);
                image.SetPosition(20, 33);
                image.Sprite.Bitmap = tileset.TilesetListBitmap;
                image.SetSize(tileset.TilesetListBitmap.Width, tileset.TilesetListBitmap.Height);
                TilesetImages.Add(image);
            }
        }

        public void AddTileset(Data.Tileset Tileset, int Index)
        {
            this.Tileset = Tileset;
            if (Index >= this.TilesetContainers.Count)
            {
                Index = this.TilesetContainers.Count;
                CollapsibleContainer c = new CollapsibleContainer(TilesetStackPanel);
                c.SetText(this.Tileset.Name);
                c.SetMargin(0, 0, 0, 8);
                c.OnCollapsedChanged += delegate (object sender, EventArgs e) { UpdateCursorPosition(); };
                TilesetContainers.Add(c);
                PictureBox t = new PictureBox(c);
                t.SetPosition(20, 33);
                TilesetImages.Add(t);
            }
            TilesetImages[Index].Sprite.Bitmap = this.Tileset.TilesetBitmap;
            TilesetImages[Index].SetSize(TilesetImages[Index].Sprite.Bitmap.Width, TilesetImages[Index].Sprite.Bitmap.Height);
            TilesetContainers[Index].SetSize(AllTilesetContainer.Size.Width - 13, TilesetImages[Index].Size.Height + TilesetImages[Index].Position.Y);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            AllTilesetContainer.SetHeight(this.Size.Height - 57);
        }

        public override void Update()
        {
            CursorIM.Update(AllTilesetContainer.Viewport.Rect);
            base.Update();
        }

        public void UpdateCursorPosition()
        {
            LayoutContainer lc = TilesetStackPanel.Widgets[TilesetIndex] as LayoutContainer;
            CollapsibleContainer cc = lc.Widget as CollapsibleContainer;
            if (cc.Collapsed)
            {
                Cursor.SetPosition(0, 0);
                Cursor.SetVisible(false);
                AllTilesetContainer.UpdateAutoScroll();
            }
            else
            {
                Cursor.SetPosition(20 + TileX * 33, 33 + lc.Position.Y + TileY * 33);
                Cursor.SetVisible(true);
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (e.LeftButton == e.OldLeftButton) return; // A button other than the left mouse button was pressed
            if (Parent.ScrollBarY != null && (Parent.ScrollBarY.Dragging || Parent.ScrollBarY.Hovering)) return;
            int rx = e.X - this.AllTilesetContainer.Viewport.X;
            int ry = e.Y - this.AllTilesetContainer.Viewport.Y;
            if (rx < 0 || ry < 0 || rx >= this.AllTilesetContainer.Viewport.Width || ry >= this.AllTilesetContainer.Viewport.Height) return; // Off the widget
            rx += this.AllTilesetContainer.ScrolledX;
            ry += this.AllTilesetContainer.ScrolledY;
            if (rx < 20 || ry < 33 || rx >= 283) return; // Not over a tileset
            int crx = rx - 20; // container (c) relative (r) x (x)
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
                if (cry < 33) continue; // In the name part of the container
                cry -= 33;
                int tilex = (int) Math.Floor(crx / 33d);
                int tiley = (int) Math.Floor(cry / 33d);
                TilesetIndex = i;
                TileX = tilex;
                TileY = tiley;
                Cursor.SetPosition(20 + tilex * 33, 33 + height + tiley * 33);
                Cursor.SetVisible(true);
                break;
            }
        }
    }
}
