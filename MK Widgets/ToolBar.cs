using System;
using ODL;

namespace MKEditor.Widgets
{
    public class ToolBar : Widget
    {
        public MapViewer MapViewer;
        IconButton Cut;
        IconButton Copy;
        IconButton Paste;
        IconButton Delete;
        IconButton Undo;
        IconButton Redo;
        Container ZoomContainer;
        IconButton Zoom1;
        IconButton Zoom2;
        IconButton Zoom3;

        ModeButton MappingMode;
        ModeButton ScriptingMode;
        ModeButton DatabaseMode;

        PlayButton PlayButton;
        SaveButton SaveButton;

        int DrawnX = 0;
        int DrawnY = 0;

        public ToolBar(object Parent, string Name = "toolBar")
            : base(Parent, Name)
        {
            Sprites["status"] = new Sprite(this.Viewport);

            MappingMode = new ModeButton(this);
            MappingMode.SetPosition(8, 11);
            MappingMode.SetText("Mapping");

            ScriptingMode = new ModeButton(this);
            ScriptingMode.SetPosition(120, 11);
            ScriptingMode.SetText("Scripting");

            DatabaseMode = new ModeButton(this);
            DatabaseMode.SetPosition(232, 11);
            DatabaseMode.SetText("Database");

            PlayButton = new PlayButton(this);
            SaveButton = new SaveButton(this);

            SetBackgroundColor(10, 23, 37);
            Cut = new IconButton(this);
            Cut.SetPosition(355, 35);
            Cut.SetIcon(0, 0);
            Copy = new IconButton(this);
            Copy.SetPosition(387, 35);
            Copy.SetIcon(1, 0);
            Paste = new IconButton(this);
            Paste.SetPosition(419, 35);
            Paste.SetIcon(2, 0);
            Delete = new IconButton(this);
            Delete.SetPosition(451, 35);
            Delete.SetIcon(3, 0);
            Undo = new IconButton(this);
            Undo.SetPosition(483, 35);
            Undo.SetIcon(4, 0);
            Redo = new IconButton(this);
            Redo.SetPosition(515, 35);
            Redo.SetIcon(5, 0);

            ZoomContainer = new Container(this);
            ZoomContainer.SetPosition(564, 35);
            ZoomContainer.SetSize(86, 27);
            Zoom1 = new IconButton(ZoomContainer);
            Zoom1.SetIcon(7, 0);
            Zoom1.SetSelected(true);
            Zoom1.OnSelection = delegate (object sender, EventArgs e)
            {
                MapViewer.SetZoomFactor(1);
            };
            Zoom2 = new IconButton(ZoomContainer);
            Zoom2.SetPosition(32, 0);
            Zoom2.SetIcon(8, 0);
            Zoom2.OnSelection = delegate (object sender, EventArgs e)
            {
                MapViewer.SetZoomFactor(0.5);
            };
            Zoom3 = new IconButton(ZoomContainer);
            Zoom3.SetPosition(64, 0);
            Zoom3.SetIcon(9, 0);
            Zoom3.OnSelection = delegate (object sender, EventArgs e)
            {
                MapViewer.SetZoomFactor(0.25);
            };
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            PlayButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width, 11);
            SaveButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width - 7 - SaveButton.Size.Width, 11);
        }

        protected override void Draw()
        {
            if (Sprites["status"].Bitmap != null) Sprites["status"].Bitmap.Dispose();
            Sprites["status"].Bitmap = new Bitmap(this.Size);
            Sprites["status"].Bitmap.Unlock();
            Sprites["status"].Bitmap.Font = Font.Get("Fonts/ProductSans-M", 14);
            if (MapViewer.MapTileX >= 0 && MapViewer.MapTileY >= 0 && MapViewer.MapTileX < MapViewer.Map.Width && MapViewer.MapTileY < MapViewer.Map.Height)
            {
                string c1 = Utilities.Digits(MapViewer.MapTileX, 3);
                string c2 = Utilities.Digits(MapViewer.MapTileY, 3);
                Sprites["status"].Bitmap.DrawText(c1, Size.Width - 350, 40, new Color(80, 146, 218), DrawOptions.RightAlign);
                Sprites["status"].Bitmap.DrawText("x", Size.Width - 347, 40, new Color(80, 146, 218));
                Sprites["status"].Bitmap.DrawText(c2, Size.Width - 337, 40, new Color(80, 146, 218));
            }
            Sprites["status"].Bitmap.Lock();
            DrawnX = MapViewer.MapTileX;
            DrawnY = MapViewer.MapTileY;
            base.Draw();
        }

        public override void Update()
        {
            if (DrawnX != MapViewer.MapTileX || DrawnY != MapViewer.MapTileY) Redraw();
            base.Update();
        }
    }
}
