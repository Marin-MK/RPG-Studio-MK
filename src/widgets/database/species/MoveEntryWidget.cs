using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class MoveEntryWidget : Widget
{
    int Level;
    MoveResolver Move;

    Button RemoveButton;

    public BaseEvent OnButtonClicked;
    bool IsInside = false;
    int MoveNameWidth = 0;

    public MoveEntryWidget(IContainer parent, int parentWidgetIndex = -1) : base(parent, parentWidgetIndex)
    {
        SetSize(900, 26);
        Sprites["bg"] = new Sprite(this.Viewport, new SolidBitmap(870, 26, new Color(10, 23, 37)));
        Sprites["txt"] = new Sprite(this.Viewport);
        RemoveButton = new Button(this);
        RemoveButton.SetSize(26, 26);
        RemoveButton.SetText("X");
        RemoveButton.SetRightDocked(true);
        RemoveButton.OnClicked += _ => OnButtonClicked?.Invoke(new BaseEventArgs());
    }

    public void SetMove(int Level, MoveResolver Move)
    {
        if (this.Level != Level || this.Move != Move)
        {
            this.Level = Level;
            this.Move = Move;
            RedrawMove();
        }
    }

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
        RedrawMove();
	}

    public void RedrawMove()
    {
        if (this.Move == null) return;
        string moveName = this.Move.Valid ? this.Move.Move.Name : this.Move.ID;
        MoveNameWidth = Fonts.Paragraph.TextSize(moveName).Width;

		Sprites["txt"].Bitmap?.Dispose();
        Sprites["txt"].Bitmap = new Bitmap(Size);
        Sprites["txt"].Bitmap.Font = Fonts.Paragraph;
        Sprites["txt"].Bitmap.Unlock();
        Sprites["txt"].Bitmap.DrawText(this.Level == 0 ? "---" : this.Level.ToString(), 16, 4, Color.WHITE);
        Sprites["txt"].Bitmap.DrawText(moveName, 100, 4, Color.WHITE);
		Sprites["txt"].Bitmap.DrawText(this.Move.Valid ? this.Move.Move.Type.Type.Name : "???", 250, 4, Color.WHITE);
		Sprites["txt"].Bitmap.DrawText(this.Move.Valid ? this.Move.Move.Category : "???", 400, 4, Color.WHITE);
        string acc = this.Move.Valid ? (this.Move.Move.Accuracy switch
        {
            0 => "---",
            _ => this.Move.Move.Accuracy.ToString() + "%"
        }) : "???";
		Sprites["txt"].Bitmap.DrawText(acc, 550, 4, Color.WHITE);
		Sprites["txt"].Bitmap.DrawText(this.Move.Valid ? (this.Move.Move.BaseDamage == 0 ? "---" : this.Move.Move.BaseDamage.ToString()) : "???", 690, 4, Color.WHITE);
		Sprites["txt"].Bitmap.DrawText(this.Move.Valid ? this.Move.Move.Priority.ToString() : "???", 800, 4, Color.WHITE);
		Sprites["txt"].Bitmap.Lock();
    }

	public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
        int rx = e.X - Viewport.X + LeftCutOff;
        int ry = e.Y - Viewport.Y + TopCutOff;
        if (new Rect(90, 2, MoveNameWidth + 20, 22).Contains(rx, ry))
        {
            if (!IsInside)
            {
                IsInside = true;
                Input.SetCursor(CursorType.Hand);
            }
        }
        else if (IsInside)
        {
            IsInside = false;
            Input.SetCursor(CursorType.Arrow);
        }
	}

	public override void LeftMouseDownInside(MouseEventArgs e)
	{
		base.LeftMouseDownInside(e);
        if (IsInside)
        {
			Editor.ProjectSettings.LastMoveID = this.Move.ID;
			Editor.ProjectSettings.LastMoveScroll = 0;
			Editor.MainWindow.DatabaseWidget.SetMode(DatabaseMode.Moves);
            return;
		}
	}
}
