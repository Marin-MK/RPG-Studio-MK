using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class ColorPickerWidget : Widget
{
	public Color PrimaryColor { get; protected set; } = new Color(255, 0, 0);
	public Color Color { get; protected set; }

	public BaseEvent OnColorChanged;

	List<Color> SliderColors = new List<Color>()
	{
		new Color(255, 0, 0),
		new Color(255, 0, 255),
		new Color(0, 0, 255),
		new Color(0, 255, 255),
		new Color(0, 255, 0),
		new Color(255, 255, 0),
		new Color(255, 0, 0),
	};

	List<(int Position, int Size)> SectionData = new List<(int, int)>();

	bool InsideGradient;
	bool InsideSlider;
	int LastCrosshairX;
	int LastCrosshairY;

	public ColorPickerWidget(IContainer Parent) : base(Parent)
	{
		Sprites["box"] = new Sprite(this.Viewport);
		Sprites["crosshair"] = new Sprite(this.Viewport);
		Sprites["slider"] = new Sprite(this.Viewport);
		Sprites["slide"] = new Sprite(this.Viewport);
		Sprites["slide"].Bitmap = new Bitmap(15, 3);
		Sprites["slide"].Bitmap.Unlock();
		Sprites["slide"].Bitmap.DrawLine(0, 0, 14, 0, Color.WHITE);
		Sprites["slide"].Bitmap.DrawLine(0, 1, 14, 1, Color.BLACK);
		Sprites["slide"].Bitmap.DrawLine(0, 2, 14, 2, Color.WHITE);
		Sprites["slide"].Bitmap.Lock();
		Sprites["slide"].Y = -1;
		Sprites["color"] = new Sprite(this.Viewport);

		RegisterShortcuts(new List<Shortcut>()
		{
			new Shortcut(this, new Key(Keycode.G), _ => SetColor(Utilities.RandomColor()), true)
		});
		this.Color = this.PrimaryColor;
	}

	public void SetColor(Color Color)
	{
		if (!this.Color.Equals(Color))
		{
			this.Color = Color;
			(double Position, double Alpha, double Beta) = FindPrimaryColorAndCoordinates(Color);
			Sprites["slide"].Y = (int) Math.Round((Position / 6d) * (Size.Height - 20)) - 1;
			Color PC = CalculatePrimaryColor(Position);
			LastCrosshairX = (int) Math.Round(Alpha * (Size.Width - 21));
			LastCrosshairY = (int) Math.Round(Beta * (Size.Height - 21));
			if (PC.Equals(this.PrimaryColor)) DrawCrosshair(LastCrosshairX, LastCrosshairY, false);
			else SetPrimaryColor(PC, false);
			this.OnColorChanged?.Invoke(new BaseEventArgs());
		}
	}

	private (double Position, double Alpha, double Beta) FindPrimaryColorAndCoordinates(Color Color)
	{
		double LowestAlpha = 0;
		double LowestBeta = 0;
		double LowestPosition = 0;
		double LowestCost = double.MaxValue;
		double Position = 0;
		double Alpha = 0;
		double Beta = 0;
		double PositionStepSize = 0.02d;
		double CoordStepSize = 0.05d;
		// If one the three colors is 0, we can get an exact result from just the slider.
		bool TestTopRightOnly = Color.Red == 0 || Color.Green == 0 || Color.Blue == 0;
		bool Quit = false;
		while (Position <= 6)
        {
            if (Quit) break;
			if (TestTopRightOnly)
			{
				Alpha = 1;
				Beta = 0;
				double Cost = CalculateCost(Position, 1, 0, Color);
				if (Cost < LowestCost)
				{
					LowestPosition = Position;
					LowestAlpha = 1;
					LowestBeta = 0;
					if (Cost == 0) Quit = true;
				}
				Position += PositionStepSize;
				continue;
			}
            Alpha = 0;
			while (Alpha <= 1)
            {
                if (Quit) break;
                Beta = 0;
				while (Beta <= 1)
				{
					if (Quit) break;
					double Cost = CalculateCost(Position, Alpha, Beta, Color);
					if (Cost < LowestCost)
					{
						LowestPosition = Position;
						LowestAlpha = Alpha;
						LowestBeta = Beta;
						LowestCost = Cost;
						if (Cost == 0) Quit = true;
					}
					Beta += CoordStepSize;
				}
				Alpha += CoordStepSize;
			}
			Position += PositionStepSize;
		}
		Color PC = CalculatePrimaryColor(LowestPosition);
		Color FC = CalculateColor(LowestPosition, LowestAlpha, LowestBeta);
		if (LowestCost > 0)
		{
			(double FinalPos, double FinalAlpha, double FinalBeta, double FinalCost) = Utilities.GradientDescent3D(
				LowestPosition, LowestAlpha, LowestBeta, PositionStepSize / 2, CoordStepSize / 2, CoordStepSize / 2, 20, (Pos, Alpha, Beta) =>
			{
				return CalculateCost(Pos, Alpha, Beta, Color);
			});
            PC = CalculatePrimaryColor(LowestPosition);
            FC = CalculateColor(LowestPosition, LowestAlpha, LowestBeta);
            //if (FinalCost > 0)
			//{
			//	Console.WriteLine($"{FinalCost} - {Color}");
			//}
			return (FinalPos, FinalAlpha, FinalBeta);
		}
		return (LowestPosition, LowestAlpha, LowestBeta);
	}

	private double CalculateCost(double Position, double Alpha, double Beta, Color Color)
	{
		if (Position < 0 || Alpha < 0 || Beta < 0) return double.MaxValue;
		if (Position > 6 || Alpha > 1 || Beta > 1) return double.MaxValue;
		Color PC = CalculatePrimaryColor(Position);
		return CalculateCrosshairCost(Alpha, Beta, Color, PC);
	}

	private double CalculateCrosshairCost(double Alpha, double Beta, Color Color, Color PrimaryColor)
	{
		Color Top = Bitmap.Interpolate2D(Color.WHITE, PrimaryColor, 1 - Alpha);
		Color Bottom = Color.BLACK;
		Color Int = Bitmap.Interpolate2D(Top, Bottom, 1 - Beta);
		return CalculateColorCost(Int, Color);
	}

	private Color CalculateColor(double Position, double Alpha, double Beta)
	{
		Color PC = CalculatePrimaryColor(Position);
        Color Top = Bitmap.Interpolate2D(Color.WHITE, PC, 1 - Alpha);
        Color Bottom = Color.BLACK;
        Color Int = Bitmap.Interpolate2D(Top, Bottom, 1 - Beta);
		return Int;
    }

	private double CalculateColorCost(Color Color1, Color Color2)
	{
		return Math.Abs(Color2.Red - Color1.Red) + Math.Abs(Color2.Green - Color1.Green) + Math.Abs(Color2.Blue - Color1.Blue);
	}

	private Color CalculatePrimaryColor(double Position)
	{
        if (Position < 0 || Position > SliderColors.Count) throw new Exception("Invalid slider position");
        int idx = (int) Math.Floor(Position);
        double factor = 1 - (Position - idx);
        Color c1 = SliderColors[idx];
        Color c2 = c1;
        if (idx < SliderColors.Count - 1) c2 = SliderColors[idx + 1];
        return Bitmap.Interpolate2D(c1, c2, factor);
    }

	public void SetPrimaryColor(Color PrimaryColor, bool RedrawColor = true)
	{
		if (!this.PrimaryColor.Equals(PrimaryColor))
		{
			this.PrimaryColor = PrimaryColor;
			this.Redraw();
			DrawCrosshair(LastCrosshairX, LastCrosshairY, RedrawColor);
		}
	}

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		Sprites["slider"].X = Size.Width - 15;
		Sprites["slide"].X = Size.Width - 15;
		Sprites["color"].Y = Size.Height - 15;
		Sprites["crosshair"].Bitmap?.Dispose();
		Sprites["crosshair"].Bitmap = new Bitmap(Size.Width - 20, Size.Height - 20);
		DrawCrosshair(LastCrosshairX, LastCrosshairY);
	}

	protected override void Draw()
	{
		Sprites["box"].Bitmap?.Dispose();
		Sprites["box"].Bitmap = new Bitmap(Size.Width - 20, Size.Height - 20);
		Sprites["box"].Bitmap.Unlock();
		Sprites["box"].Bitmap.FillGradientRect(
			new Rect(0, 0, Size.Width - 20, Size.Height - 20),
			Color.WHITE, PrimaryColor, Color.BLACK, Color.BLACK
		);
        Sprites["box"].Bitmap.Lock();
		base.Draw();

		Sprites["slider"].Bitmap?.Dispose();
		Sprites["slider"].Bitmap = new Bitmap(15, Size.Height - 20);
		Sprites["slider"].Bitmap.Unlock();

		int Sections = SliderColors.Count - 1;
		int Length = Size.Height - 20;
		double LengthPerSection = (double) Length / Sections;
		Color OldColor = SliderColors[0];
		double y = 0;
		SectionData.Clear();
		for (int i = 1; i < SliderColors.Count; i++)
		{
			Color c = SliderColors[i];
			int leng = (int) Math.Round(LengthPerSection);
			if ((int) y + leng >= Sprites["slider"].Bitmap.Height) leng -= ((int) y + leng) - Sprites["slider"].Bitmap.Height;
			Sprites["slider"].Bitmap.FillGradientRect(
				new Rect(0, (int) y, 15, leng),
				OldColor, OldColor, c, c
			);
			if (i == SliderColors.Count - 1) leng--;
			SectionData.Add(((int) y, leng));
			y += leng;
			OldColor = c;
		}

        Sprites["slider"].Bitmap.Lock();

		Sprites["color"].Bitmap?.Dispose();
		Sprites["color"].Bitmap = new Bitmap(Size.Width, 15);
		Sprites["color"].Bitmap.Unlock();
		Sprites["color"].Bitmap.FillRect(Size.Width, 15, Color);
		Sprites["color"].Bitmap.Lock();
    }

	private void DrawCrosshair(int CX, int CY, bool RedrawColor = true)
	{
		LastCrosshairX = CX;
		LastCrosshairY = CY;
		Sprites["crosshair"].Bitmap.Unlock();
		Sprites["crosshair"].Bitmap.Clear();
		if (CY > 0) Sprites["crosshair"].Bitmap.DrawLine(0, CY - 1, Size.Width - 21, CY - 1, Color.WHITE);
        if (CY < Size.Height - 21) Sprites["crosshair"].Bitmap.DrawLine(0, CY + 1, Size.Width - 21, CY + 1, Color.WHITE);
        if (CX > 0) Sprites["crosshair"].Bitmap.DrawLine(CX - 1, 0, CX - 1, Size.Height - 21, Color.WHITE);
        if (CX < Size.Width - 21) Sprites["crosshair"].Bitmap.DrawLine(CX + 1, 0, CX + 1, Size.Height - 21, Color.WHITE);
		Sprites["crosshair"].Bitmap.DrawLine(0, CY, Size.Width - 21, CY, Color.BLACK);
        Sprites["crosshair"].Bitmap.DrawLine(CX, 0, CX, Size.Height - 21, Color.BLACK);
		Sprites["crosshair"].Bitmap.Lock();
		double lft = 1 - (double) CX / (Size.Width - 21);
		double tft = 1 - (double) CY / (Size.Height - 21);
		Color HC = Bitmap.Interpolate2D(Color.WHITE, this.PrimaryColor, lft);
		Color FC = Bitmap.Interpolate2D(HC, Color.BLACK, tft);
		if (!this.Color.Equals(FC))
		{
			this.Color = FC;
			this.OnColorChanged?.Invoke(new BaseEventArgs());
		}
		if (RedrawColor) DrawColor(FC);
    }

	private void DrawSlider(int SY)
	{
		Sprites["slide"].Y = SY - 1;
		for (int i = SectionData.Count - 1; i >= 0; i--)
		{
			(int Pos, int Size) = SectionData[i];
			if (SY >= Pos)
			{
				double factor = 1 - (double) (SY - Pos) / Size;
				Color c1 = SliderColors[i];
				Color c2 = SliderColors[i + 1];
				Color FC = Bitmap.Interpolate2D(c1, c2, factor);
				SetPrimaryColor(FC);
				break;
			}
		}
	}

	private void DrawColor(Color Color)
	{
		if (Sprites["color"].Bitmap == null) return;
		Sprites["color"].Bitmap.Unlock();
		Sprites["color"].Bitmap.FillRect(Size.Width, 15, Color);
		Sprites["color"].Bitmap.Lock();
		this.Color = Color;
	}

	public override void MouseDown(MouseEventArgs e)
	{
		base.MouseDown(e);
		if (!Mouse.Inside || !Mouse.LeftMouseTriggered) return;
		int rx = e.X - Viewport.X;
		int ry = e.Y - Viewport.Y;
		if (ry >= Size.Width - 20) return;
		if (rx < Size.Width - 20)
		{
			InsideGradient = true;
			DrawCrosshair(rx, ry);
		}
		else if (rx >= Size.Width - 15 && rx < Size.Width)
		{
			InsideSlider = true;
			DrawSlider(ry);
		}
	}

	public override void MouseMoving(MouseEventArgs e)
	{
		base.MouseMoving(e);
        int rx = e.X - Viewport.X;
        int ry = e.Y - Viewport.Y;
        rx = Math.Clamp(rx, 0, Size.Width - 21);
        ry = Math.Clamp(ry, 0, Size.Height - 21);
        if (InsideGradient)
		{
			DrawCrosshair(rx, ry);
		}
		else if (InsideSlider)
		{
			DrawSlider(ry);
		}
	}

	public override void MouseUp(MouseEventArgs e)
	{
		base.MouseUp(e);
		if (Mouse.LeftMouseReleased)
		{
			InsideGradient = false;
			InsideSlider = false;
		}
	}
}
