using System;
using System.Collections.Generic;
using odl;
using RPGStudioMK.Game;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class ToolBar : Widget
    {
        IconButton Undo;
        IconButton Redo;

        public ModeButton MappingMode;
        public ModeButton EventingMode;
        public ModeButton ScriptingMode;
        public ModeButton DatabaseMode;

        Container ActionContainer;

        PlayButton PlayButton;
        SaveButton SaveButton;

        public ToolBar(IContainer Parent) : base(Parent)
        {
            MappingMode = new ModeButton(this, "Maps", 23);
            MappingMode.SetPosition(4, 0);
            MappingMode.SetSelected(true);
            MappingMode.OnSelection += delegate (BaseEventArgs e)
            {
                Editor.SetMode("MAPPING");
            };

            EventingMode = new ModeButton(this, "Events", 24);
            EventingMode.SetPosition(MappingMode.Position.X + MappingMode.Size.Width + 12, 0);
            EventingMode.OnSelection += delegate (BaseEventArgs e)
            {
                Editor.SetMode("EVENTING");
            };

            ScriptingMode = new ModeButton(this, "Scripts", 25);
            ScriptingMode.SetPosition(EventingMode.Position.X + EventingMode.Size.Width + 12, 0);
            ScriptingMode.OnSelection += delegate (BaseEventArgs e)
            {
                Editor.SetMode("SCRIPTING");
            };

            DatabaseMode = new ModeButton(this, "Database", 26);
            DatabaseMode.SetPosition(ScriptingMode.Position.X + ScriptingMode.Size.Width + 12, 0);
            DatabaseMode.OnSelection += delegate (BaseEventArgs e)
            {
                Editor.SetMode("DATABASE");
            };

            ActionContainer = new Container(this);
            ActionContainer.SetPosition(DatabaseMode.Position.X + DatabaseMode.Size.Width + 12, 3);
            ActionContainer.SetSize(83, 28);
            ActionContainer.Sprites["line"] = new Sprite(ActionContainer.Viewport, new SolidBitmap(1, 26, new Color(28, 50, 73)));
            Undo = new IconButton(ActionContainer);
            Undo.SetIcon(4, 0);
            Undo.Selectable = false;
            Undo.OnClicked += delegate (BaseEventArgs e) { Editor.Undo(); };
            Redo = new IconButton(ActionContainer);
            Redo.SetPosition(24, 0);
            Redo.SetIcon(5, 0);
            Redo.Selectable = false;
            Redo.OnClicked += delegate (BaseEventArgs e) { Editor.Redo(); };

            PlayButton = new PlayButton(this);
            SaveButton = new SaveButton(this);
        }

        public void Refresh()
        {

        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            PlayButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width, 2);
            SaveButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width - 7 - SaveButton.Size.Width, 2);
            if (Size.Width < 800)
            {
                
            }
            else if (Size.Width < 895)
            {
                ActionContainer.SetVisible(false);
            }
            else
            {
                ActionContainer.SetVisible(true);
            }
        }
    }
}
