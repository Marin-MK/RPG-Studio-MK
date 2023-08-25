namespace RPGStudioMK.Widgets;

public class EventCategoryButton : Widget
{
	public CommandCategory Category { get; protected set; }
	public bool Selected { get; protected set; } = false;

	public BaseEvent OnSelected;

	public EventCategoryButton(CommandCategory Category, IContainer Parent) : base(Parent)
	{
		this.Category = Category;

		Sprites["box"] = new Sprite(this.Viewport);
        Sprites["shadow"] = new Sprite(this.Viewport);
        Sprites["icon"] = new Sprite(this.Viewport);
        Sprites["text"] = new Sprite(this.Viewport);

		Redraw();
	}

	public void SetSelected(bool Selected)
	{
		if (this.Selected != Selected)
		{
			this.Selected = Selected;
			if (this.Selected)
			{
				foreach (Widget w in Parent.Widgets)
				{
					if (w is EventCategoryButton && w != this)
					{
						((EventCategoryButton) w).SetSelected(false);
					}
				}
				OnSelected?.Invoke(new BaseEventArgs());
			}
			Redraw();
		}
	}

	protected override void Draw()
	{
		Sprites["box"].Bitmap?.Dispose();
		Sprites["box"].Bitmap = new Bitmap(Size);
		Sprites["box"].Bitmap.Unlock();
		Color c1 = BaseCommandWidget.CategoryInfo[this.Category].TopBarColor;
		Color c2 = BaseCommandWidget.CategoryInfo[this.Category].BottomBarColor;
		if (Selected) Sprites["box"].Bitmap.FillRect(0, 0, Size.Width, Size.Height, new Color(39, 81, 104));
		else Sprites["box"].Bitmap.FillGradientRect(0, 0, Size.Width, Size.Height, c1, c1, c2, c2);
		Sprites["box"].Bitmap.Lock();
		Sprites["shadow"].Bitmap?.Dispose();
		Sprites["shadow"].Bitmap = new Bitmap("assets/img/command_categories");
		Sprites["shadow"].SrcRect.Width = Sprites["shadow"].SrcRect.Height = 48;
		Sprites["shadow"].SrcRect.X = BaseCommandWidget.CategoryInfo[this.Category].Index * Sprites["shadow"].SrcRect.Width;
		Sprites["shadow"].SrcRect.Y = 2 * Sprites["shadow"].SrcRect.Height;
		Sprites["shadow"].X = Size.Width / 2 - Sprites["shadow"].SrcRect.Width / 2;
		Sprites["shadow"].Y = Size.Height / 2 - Sprites["shadow"].SrcRect.Height / 2 - 4;
		Sprites["icon"].Bitmap = Sprites["shadow"].Bitmap;
		Sprites["icon"].DestroyBitmap = false;
        Sprites["icon"].SrcRect.Width = Sprites["icon"].SrcRect.Height = Sprites["shadow"].SrcRect.Width;
		Sprites["icon"].SrcRect.X = Sprites["shadow"].SrcRect.X;
        Sprites["icon"].SrcRect.Y = (Selected ? 0 : 1) * Sprites["icon"].SrcRect.Height;
        Sprites["icon"].X = Sprites["shadow"].X;
		Sprites["icon"].Y = Sprites["shadow"].Y;
		if (Selected) Sprites["icon"].Color = new Color(212, 212, 75);
		else Sprites["icon"].Color = new Color(255, 255, 255, 0);
		Sprites["text"].Bitmap?.Dispose();
		string text = BaseCommandWidget.CategoryInfo[this.Category].Name;
		Size s = Fonts.ParagraphBold.TextSize(text);
		Sprites["text"].Bitmap = new Bitmap(s);
		Sprites["text"].Bitmap.Unlock();
		Sprites["text"].Bitmap.Font = Fonts.ParagraphBold;
		Sprites["text"].Bitmap.DrawText(text, Color.WHITE);
        Sprites["text"].Bitmap.Lock();
		Sprites["text"].X = Size.Width / 2 - s.Width / 2;
		Sprites["text"].Y = Size.Height - 20;
        base.Draw();
	}

	public override void LeftMouseDownInside(MouseEventArgs e)
	{
		base.LeftMouseDownInside(e);
		SetSelected(true);
	}
}
