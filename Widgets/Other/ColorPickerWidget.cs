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
	public Color Color { get; protected set; }
	public float Hue { get; protected set; } = 0f;
	public float Saturation { get; protected set; } = 1f;
	public float Lightness { get; protected set; } = 0.5f;

	public BaseEvent OnColorChanged;

	bool InsideGradient;
	bool InsideSlider;

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
			new Shortcut(this, new Key(Keycode.G), _ => SetColor(Utilities.RandomColor()))
		});
		this.Color = Color.FromHSL(0, 1, 0.5f);
	}

	public void SetColor(Color Color)
	{
		if (!this.Color.Equals(Color))
		{
			this.Color = Color;
			(float hue, float sat, float light) = Color.GetHSL();
			SetHue(hue);
			SetSaturation(sat);
			SetLightness(light);
			RedrawColor();
			this.OnColorChanged?.Invoke(new BaseEventArgs());
		}
	}

	public void SetHue(float Hue)
	{
		if (this.Hue != Hue)
		{
			this.Hue = Hue;
			Sprites["slide"].Y = (int) Math.Round(Hue / 360 * (Size.Height - 23));
            SetColor(Color.FromHSL(this.Hue, this.Saturation, this.Lightness));
			RedrawGradientSquare();
        }
	}

	public void SetSaturation(float Saturation, bool RedrawCrosshair = true)
	{
		if (this.Saturation != Saturation)
		{
			this.Saturation = Saturation;
			if (RedrawCrosshair) DrawCrosshair((int) Math.Round(this.Saturation * (Size.Width - 21)), LightnessToY(this.Lightness, this.Saturation));
            SetColor(Color.FromHSL(this.Hue, this.Saturation, this.Lightness));
        }
	}

	public void SetLightness(float Lightness, bool RedrawCrosshair = true)
	{
		if (this.Lightness != Lightness)
		{
			this.Lightness = Lightness;
			if (RedrawCrosshair) DrawCrosshair((int) Math.Round(this.Saturation * (Size.Width - 21)), LightnessToY(this.Lightness, this.Saturation));
            SetColor(Color.FromHSL(this.Hue, this.Saturation, this.Lightness));
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
		DrawCrosshair((int) Math.Round(this.Saturation * (Size.Width - 21)), LightnessToY(this.Lightness, this.Saturation));
		RedrawGradientSquare();
		RedrawGradientSlider();
	}

	private float XYToLightness(float x, float f)
	{
        float lightness = (1 - x) * (1f - f) + x * (0.5f - 0.5f * f);
		return lightness;
    }

    private int LightnessToY(float lightness, float x)
    {
		float y = (float) (lightness + 0.5f * x - 1) / (0.5f * x - 1);
        return Math.Clamp((int) Math.Round(y * (Size.Height - 21)), 0, Size.Height - 21);
    }

    private void RedrawGradientSquare()
	{
        Sprites["box"].Bitmap?.Dispose();
        Sprites["box"].Bitmap = new Bitmap(Size.Width - 20, Size.Height - 20);
        Sprites["box"].Bitmap.Unlock();
        for (int y = 0; y < Size.Height - 20; y++)
        {
            for (int x = 0; x < Size.Width - 20; x++)
            {
                float saturation = (float) x / (Size.Width - 21);
                float lightness = XYToLightness(saturation, (float) y / (Size.Height - 21));
                Color pixColor = Color.FromHSL(this.Hue, saturation, lightness);
                Sprites["box"].Bitmap.SetPixel(x, y, pixColor);
            }
        }

        Sprites["box"].Bitmap.Lock();
    }

	private void RedrawGradientSlider()
	{
        Sprites["slider"].Bitmap?.Dispose();
        Sprites["slider"].Bitmap = new Bitmap(15, Size.Height - 20);
        Sprites["slider"].Bitmap.Unlock();

        for (int y = 0; y < Size.Height - 20; y++)
        {
            float hue = 360 * (float)y / (Size.Height - 21);
            Color lineColor = Color.FromHSL(hue, 1, 0.5f);
            Sprites["slider"].Bitmap.DrawLine(0, y, 14, y, lineColor);
        }

        Sprites["slider"].Bitmap.Lock();
    }

	private void RedrawColor()
	{
        Sprites["color"].Bitmap?.Dispose();
        Sprites["color"].Bitmap = new Bitmap(Size.Width, 15);
        Sprites["color"].Bitmap.Unlock();
        Sprites["color"].Bitmap.FillRect(Size.Width, 15, Color);
        Sprites["color"].Bitmap.Lock();
    }

	private void DrawCrosshair(int cx, int cy)
	{
		Sprites["crosshair"].Bitmap.Unlock();
		Sprites["crosshair"].Bitmap.Clear();
		if (cy > 0) Sprites["crosshair"].Bitmap.DrawLine(0, cy - 1, Size.Width - 21, cy - 1, Color.WHITE);
        if (cy < Size.Height - 21) Sprites["crosshair"].Bitmap.DrawLine(0, cy + 1, Size.Width - 21, cy + 1, Color.WHITE);
        if (cx > 0) Sprites["crosshair"].Bitmap.DrawLine(cx - 1, 0, cx - 1, Size.Height - 21, Color.WHITE);
        if (cx < Size.Width - 21) Sprites["crosshair"].Bitmap.DrawLine(cx + 1, 0, cx + 1, Size.Height - 21, Color.WHITE);
		Sprites["crosshair"].Bitmap.DrawLine(0, cy, Size.Width - 21, cy, Color.BLACK);
        Sprites["crosshair"].Bitmap.DrawLine(cx, 0, cx, Size.Height - 21, Color.BLACK);
		Sprites["crosshair"].Bitmap.Lock();
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
			SetSaturation((float) rx / (Size.Width - 21), false);
			SetLightness(XYToLightness(this.Saturation, (float) ry / (Size.Height - 21)), false);
			DrawCrosshair(rx, ry);
		}
		else if (rx >= Size.Width - 15 && rx < Size.Width)
		{
			InsideSlider = true;
			SetHue(360 * (float) ry / (Size.Height - 21));
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
            SetSaturation((float) rx / (Size.Width - 21), false);
            SetLightness(XYToLightness(this.Saturation, (float)ry / (Size.Height - 21)), false);
            DrawCrosshair(rx, ry);
        }
		else if (InsideSlider)
		{
			SetHue(360 * (float) ry / (Size.Height - 21));
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
