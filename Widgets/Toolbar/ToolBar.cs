using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class ToolBar : Widget
    {
        IconButton Cut;
        IconButton Copy;
        IconButton Paste;
        IconButton Delete;
        IconButton Undo;
        IconButton Redo;

        public ModeButton MappingMode;
        public ModeButton EventingMode;
        public ModeButton ScriptingMode;
        public ModeButton DatabaseMode;

        Container ActionContainer;
        Container CopyContainer;

        PlayButton PlayButton;
        SaveButton SaveButton;

        public ToolBar(IContainer Parent) : base(Parent)
        {
            MappingMode = new ModeButton(this, "Maps", 23);
            MappingMode.SetPosition(4, 0);
            MappingMode.SetSelected(true);
            MappingMode.OnSelection += delegate (object sender, EventArgs e)
            {
                Editor.SetMode("MAPPING");
            };

            EventingMode = new ModeButton(this, "Events", 24);
            EventingMode.SetPosition(MappingMode.Position.X + MappingMode.Size.Width + 12, 0);
            EventingMode.OnSelection += delegate (object sender, EventArgs e)
            {
                Editor.SetMode("EVENTING");
            };

            ScriptingMode = new ModeButton(this, "Scripts", 25);
            ScriptingMode.SetPosition(EventingMode.Position.X + EventingMode.Size.Width + 12, 0);
            ScriptingMode.OnSelection += delegate (object sender, EventArgs e)
            {
                Editor.SetMode("SCRIPTING");
            };

            DatabaseMode = new ModeButton(this, "Database", 26);
            DatabaseMode.SetPosition(ScriptingMode.Position.X + ScriptingMode.Size.Width + 12, 0);
            DatabaseMode.OnSelection += delegate (object sender, EventArgs e)
            {
                Editor.SetMode("DATABASE");
            };

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

            PlayButton = new PlayButton(this);
            SaveButton = new SaveButton(this);
        }

        public void Refresh()
        {

        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            PlayButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width, 2);
            SaveButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width - 7 - SaveButton.Size.Width, 2);
            if (Size.Width < 800)
            {
                
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
