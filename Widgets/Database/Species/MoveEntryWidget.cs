using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class MoveEntryWidget : Widget
{
    int Level;
    Move Move;

    Button RemoveButton;

    public BaseEvent OnButtonClicked;

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

    public void SetMove(int Level, Move Move)
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
        Sprites["txt"].Bitmap?.Dispose();
        Sprites["txt"].Bitmap = new Bitmap(Size);
        Sprites["txt"].Bitmap.Font = Fonts.Paragraph;
        Sprites["txt"].Bitmap.Unlock();
        Sprites["txt"].Bitmap.DrawText(this.Level == 0 ? "---" : this.Level.ToString(), 16, 4, Color.WHITE);
        Sprites["txt"].Bitmap.DrawText(this.Move.Name, 100, 4, Color.WHITE);
		Sprites["txt"].Bitmap.DrawText(this.Move.Type.Type.Name, 250, 4, Color.WHITE);
		Sprites["txt"].Bitmap.DrawText(this.Move.Category, 400, 4, Color.WHITE);
        string acc = this.Move.Accuracy switch
        {
            0 => "---",
            _ => this.Move.Accuracy.ToString() + "%"
        };
		Sprites["txt"].Bitmap.DrawText(acc, 550, 4, Color.WHITE);
		Sprites["txt"].Bitmap.DrawText(this.Move.TotalPP.ToString(), 700, 4, Color.WHITE);
		Sprites["txt"].Bitmap.DrawText(this.Move.Priority.ToString(), 800, 4, Color.WHITE);
		Sprites["txt"].Bitmap.Lock();
    }
}
