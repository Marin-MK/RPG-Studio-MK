namespace RPGStudioMK.Widgets;

public class ToolBar : Widget
{
    public IconButton Undo;
    public IconButton Redo;

    public ModeButton MappingMode;
    public ModeButton ScriptingMode;
    public ModeButton DatabaseMode;

    Container ActionContainer;

    GradientButton PlayButton;
    GradientButton SaveButton;

    public ToolBar(IContainer Parent) : base(Parent)
    {
        MappingMode = new ModeButton(this, "Maps");
        MappingMode.SetPosition(8, 0);
        MappingMode.SetSelected(true);
        MappingMode.OnSelection += delegate (BaseEventArgs e)
        {
            Editor.SetMode(EditorMode.Mapping);
        };

        ScriptingMode = new ModeButton(this, "Scripts");
        ScriptingMode.SetPosition(MappingMode.Position.X + MappingMode.Size.Width + 8, 0);
        ScriptingMode.OnSelection += delegate (BaseEventArgs e)
        {
            Editor.SetMode(EditorMode.Scripting);
        };

        DatabaseMode = new ModeButton(this, "Database");
        DatabaseMode.SetPosition(ScriptingMode.Position.X + ScriptingMode.Size.Width + 8, 0);
        DatabaseMode.OnSelection += delegate (BaseEventArgs e)
        {
            Editor.SetMode(EditorMode.Database);
        };

        ActionContainer = new Container(this);
        ActionContainer.SetPosition(DatabaseMode.Position.X + DatabaseMode.Size.Width + 8, 3);
        ActionContainer.SetSize(83, 28);
        ActionContainer.Sprites["line"] = new Sprite(ActionContainer.Viewport, new SolidBitmap(1, 26, new Color(28, 50, 73)));
        Undo = new IconButton(ActionContainer);
        Undo.SetIcon(Icon.Undo);
        Undo.Selectable = false;
        Undo.OnClicked += delegate (BaseEventArgs e) { Editor.Undo(); };
        Undo.SetEnabled(false);
        Redo = new IconButton(ActionContainer);
        Redo.SetPosition(24, 0);
        Redo.SetIcon(Icon.Redo);
        Redo.Selectable = false;
        Redo.OnClicked += delegate (BaseEventArgs e) { Editor.Redo(); };
        Redo.SetEnabled(false);

        PlayButton = new GradientButton(this, "Save");
        PlayButton.SetGradient(new Color(184, 56, 98), new Color(143, 49, 167));
        SaveButton = new GradientButton(this, "Play");
        SaveButton.SetGradient(new Color(87, 168, 127), new Color(78, 102, 195));
    }

    public void Refresh()
    {

    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        SaveButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width, 2);
        PlayButton.SetPosition(Size.Width - 6 - PlayButton.Size.Width - 12 - SaveButton.Size.Width, 2);
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
