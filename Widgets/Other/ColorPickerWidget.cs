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
	int LastSliderY;

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
		this.Color = this.PrimaryColor;
	}

	public void SetColor(Color Color)
	{
		if (!this.Color.Equals(Color))
		{
			this.Color = Color;
			// TODO: Select right position in slider and gradient boxes
			// Likely have to solve a system of linear equations
			// to find the value of the primary color and the crosshair location
			// If no such system exists, could also simulate the primary color and get as close as possible,
			// then compare that simulation with various others, with cost functions and random steps in an attempt
			// to get closer (Machine Learning). But a solveable linear solution probably exist.
			Redraw();
		}
	}

	public void SetPrimaryColor(Color PrimaryColor)
	{
		if (!this.PrimaryColor.Equals(PrimaryColor))
		{
			this.PrimaryColor = PrimaryColor;
			this.Redraw();
			DrawCrosshair(LastCrosshairX, LastCrosshairY);
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
			Sprites["slider"].Bitmap.FillGradientRect(
				new Rect(0, (int) y, 15, (int) LengthPerSection),
				OldColor, OldColor, c, c
			);
			SectionData.Add(((int) y, (int) LengthPerSection));
			y += LengthPerSection;
			OldColor = c;
		}

        Sprites["slider"].Bitmap.Lock();

		Sprites["color"].Bitmap?.Dispose();
		Sprites["color"].Bitmap = new Bitmap(Size.Width, 15);
		Sprites["color"].Bitmap.Unlock();
		Sprites["color"].Bitmap.FillRect(Size.Width, 15, Color);
		Sprites["color"].Bitmap.Lock();
    }

	private void DrawCrosshair(int CX, int CY)
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
		DrawColor(FC);
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
