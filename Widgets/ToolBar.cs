using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class ToolBar : Widget
    {
        public MapViewer MapViewer;
        public TilesetTab TilesetTab;
        public StatusBar StatusBar;

        IconButton Cut;
        IconButton Copy;
        IconButton Paste;
        IconButton Delete;
        IconButton Undo;
        IconButton Redo;

        ModeButton MappingMode;
        ModeButton EventingMode;
        ModeButton ScriptingMode;
        ModeButton DatabaseMode;

        Container ActionContainer;
        Container CopyContainer;
        Container DrawToolsContainer;

        public IconButton PencilButton;
        public IconButton FillButton;
        public IconButton EllipseButton;
        public IconButton RectButton;
        public IconButton SelectButton;
        public IconButton EraserButton;

        PlayButton PlayButton;
        SaveButton SaveButton;

        public ToolBar(object Parent, string Name = "toolBar")
            : base(Parent, Name)
        {
            SetBackgroundColor(10, 23, 37);

            MappingMode = new ModeButton("Maps", 23, this);
            MappingMode.SetPosition(4, 0);
            MappingMode.SetSelected(true);

            EventingMode = new ModeButton("Events", 24, this);
            EventingMode.SetPosition(MappingMode.Position.X + MappingMode.Size.Width + 12, 0);

            ScriptingMode = new ModeButton("Scripts", 25, this);
            ScriptingMode.SetPosition(EventingMode.Position.X + EventingMode.Size.Width + 12, 0);

            DatabaseMode = new ModeButton("Database", 26, this);
            DatabaseMode.SetPosition(ScriptingMode.Position.X + ScriptingMode.Size.Width + 12, 0);

            ActionContainer = new Container(this);
            ActionContainer.SetPosition(DatabaseMode.Position.X + DatabaseMode.Size.Width + 12, 3);
            ActionContainer.SetSize(83, 28);
            ActionContainer.Sprites["line"] = new Sprite(ActionContainer.Viewport, new SolidBitmap(1, 26, new Color(28, 50, 73)));
            Delete = new IconButton(ActionContainer);
            Delete.SetPosition(6, 0);
            Delete.SetIcon(3, 0);
            Delete.Selectable = false;
            Undo = new IconButton(ActionContainer);
            Undo.SetPosition(30, 0);
            Undo.SetIcon(4, 0);
            Undo.Selectable = false;
            Redo = new IconButton(ActionContainer);
            Redo.SetPosition(54, 0);
            Redo.SetIcon(5, 0);
            Redo.Selectable = false;

            CopyContainer = new Container(this);
            CopyContainer.SetPosition(ActionContainer.Position.X + ActionContainer.Size.Width, 3);
            CopyContainer.SetSize(83, 28);
            CopyContainer.Sprites["line"] = new Sprite(CopyContainer.Viewport, new SolidBitmap(1, 26, new Color(28, 50, 73)));
            Cut = new IconButton(CopyContainer);
            Cut.SetPosition(6, 0);
            Cut.SetIcon(0, 0);
            Cut.Selectable = false;
            Copy = new IconButton(CopyContainer);
            Copy.SetPosition(30, 0);
            Copy.SetIcon(1, 0);
            Copy.Selectable = false;
            Paste = new IconButton(CopyContainer);
            Paste.SetPosition(54, 0);
            Paste.SetIcon(2, 0);
            Paste.Selectable = false;

            DrawToolsContainer = new Container(this);
            DrawToolsContainer.SetPosition(Size.Width - 283 - 186, 3);
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
            FillButton.SetPosition(28, 0);

            EllipseButton = new IconButton(DrawToolsContainer);
            EllipseButton.SetIcon(17, 0);
            EllipseButton.SetPosition(56, 0);

            RectButton = new IconButton(DrawToolsContainer);
            RectButton.SetIcon(18, 0);
            RectButton.SetPosition(84, 0);

            SelectButton = new IconButton(DrawToolsContainer);
            SelectButton.SetIcon(19, 0);
            SelectButton.SetPosition(112, 0);

            EraserButton = new IconButton(DrawToolsContainer);
            EraserButton.SetIcon(20, 0);
            EraserButton.SetPosition(153, 0);
            EraserButton.Toggleable = true;
            EraserButton.OnSelection += delegate (object sender, EventArgs e)
            {
                TilesetTab.Cursor.SetPosition(0, 0);
                TilesetTab.Cursor.SetVisible(false);
                MapViewer.TileDataList = new List<Data.TileData>() { null };
                MapViewer.CursorWidth = 0;
                MapViewer.CursorHeight = 0;
                TilesetTab.UpdateCursorPosition();
                MapViewer.UpdateCursorPosition();
            };
            EraserButton.OnDeselection += delegate (object sender, EventArgs e)
            {
                TilesetTab.UpdateCursorPosition();
            };

            PlayButton = new PlayButton(this);
            SaveButton = new SaveButton(this);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            DrawToolsContainer.SetPosition(Size.Width - 283 - 186, 3);
            PlayButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width, 2);
            SaveButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width - 7 - SaveButton.Size.Width, 2);
            if (Size.Width < 800)
            {
                DrawToolsContainer.SetPosition(SaveButton.Position.X - 185, 3);
            }
            else if (Size.Width < 895)
            {
                ActionContainer.SetVisible(false);
                CopyContainer.SetVisible(false);
            }
            else if (Size.Width < 978)
            {
                ActionContainer.SetVisible(true);
                CopyContainer.SetVisible(false);
            }
            else
            {
                ActionContainer.SetVisible(true);
                CopyContainer.SetVisible(true);
            }
        }
    }
}
