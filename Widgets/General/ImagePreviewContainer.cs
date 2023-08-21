using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Widgets;

public class ImagePreviewContainer : Widget
{
	ImageBox ImageBox;
	ImageBox GridBox;
	public Bitmap Bitmap => ImageBox.Bitmap;
	public int X => ImageBox.X;
	public int Y => ImageBox.Y;
	public int Z => ImageBox.Z;
	public new byte Opacity => ImageBox.Opacity;
	public int Angle => ImageBox.Angle;
	public bool MirrorX => ImageBox.MirrorX;
	public bool MirrorY => ImageBox.MirrorY;
	public int OX => ImageBox.OX;
	public int OY => ImageBox.OY;
	public bool DestroyBitmap => ImageBox.DestroyBitmap;
	public new double ZoomX => ImageBox.ZoomX;
	public new double ZoomY => ImageBox.ZoomY;
	public Color Color => ImageBox.Color;
	public Rect SrcRect => ImageBox.SrcRect;
	public FillMode FillMode => ImageBox.FillMode;

	public ImagePreviewContainer(IContainer parent) : base(parent)
    {
		Sprites["bg"] = new Sprite(this.Viewport);
		Container OffsetContainer = new Container(this);
		OffsetContainer.SetDocked(true);
		OffsetContainer.SetPadding(5);
		GridBox = new ImageBox(OffsetContainer);
		GridBox.SetFillMode(FillMode.TileAndCenter);
		Bitmap gridBitmap = new Bitmap(16, 16);
		gridBitmap.Unlock();
		gridBitmap.FillRect(0, 0, 16, 16, new Color(15, 30, 44));
		gridBitmap.FillRect(8, 0, 8, 8, new Color(51, 74, 97));
		gridBitmap.FillRect(0, 8, 8, 8, new Color(51, 74, 97));
		gridBitmap.Lock();
		GridBox.SetBitmap(gridBitmap);
		ImageBox = new ImageBox(OffsetContainer);
    }

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		Sprites["bg"].Bitmap?.Dispose();
		Sprites["bg"].Bitmap = new Bitmap(Size);
		Sprites["bg"].Bitmap.Unlock();

		Color darkOutline = new Color(15, 30, 44);
		Color grayOutline = new Color(115, 117, 118);
		Sprites["bg"].Bitmap.DrawRect(2, 2, Size.Width - 4, Size.Height - 4, grayOutline);
		Sprites["bg"].Bitmap.DrawRect(3, 3, Size.Width - 6, Size.Height - 6, grayOutline);
		Sprites["bg"].Bitmap.DrawRect(4, 4, Size.Width - 8, Size.Height - 8, 51, 74, 97);
		Sprites["bg"].Bitmap.SetPixel(4, 4, grayOutline);
		Sprites["bg"].Bitmap.SetPixel(Size.Width - 5, 4, grayOutline);
		Sprites["bg"].Bitmap.SetPixel(Size.Width - 5, Size.Height - 5, grayOutline);
		Sprites["bg"].Bitmap.SetPixel(4, Size.Height - 5, grayOutline);
		Sprites["bg"].Bitmap.DrawLine(1, 3, 3, 1, darkOutline);
		Sprites["bg"].Bitmap.DrawLine(2, 3, 3, 2, darkOutline);
		Sprites["bg"].Bitmap.FillRect(4, 0, Size.Width - 8, 2, darkOutline);
		Sprites["bg"].Bitmap.DrawLine(Size.Width - 4, 1, Size.Width - 2, 3, darkOutline);
		Sprites["bg"].Bitmap.DrawLine(Size.Width - 4, 2, Size.Width - 3, 3, darkOutline);
		Sprites["bg"].Bitmap.FillRect(Size.Width - 2, 4, 2, Size.Height - 8, darkOutline);
		Sprites["bg"].Bitmap.DrawLine(Size.Width - 2, Size.Height - 4, Size.Width - 4, Size.Height - 2, darkOutline);
		Sprites["bg"].Bitmap.DrawLine(Size.Width - 3, Size.Height - 4, Size.Width - 4, Size.Height - 3, darkOutline);
		Sprites["bg"].Bitmap.FillRect(4, Size.Height - 2, Size.Width - 8, 2, darkOutline);
		Sprites["bg"].Bitmap.DrawLine(1, Size.Height - 4, 3, Size.Height - 2, darkOutline);
		Sprites["bg"].Bitmap.DrawLine(2, Size.Height - 4, 3, Size.Height - 3, darkOutline);
		Sprites["bg"].Bitmap.FillRect(0, 4, 2, Size.Height - 8, darkOutline);

		Sprites["bg"].Bitmap.Lock();
	}

	public void SetBitmap(int Width, int Height)
	{
		ImageBox.SetBitmap(Width, Height);
	}

	public void SetBitmap(string Filename)
	{
		if (Filename == null)
		{
			ImageBox.DisposeBitmap();
			return;
		}
		ImageBox.SetBitmap(Filename);
	}

	public void SetBitmap(Bitmap Bitmap)
	{
		ImageBox.SetBitmap(Bitmap);
	}

	public void DisposeBitmap()
	{
		ImageBox.DisposeBitmap();
	}

	public void ClearBitmap()
	{
		ImageBox.ClearBitmap();
	}

	public void SetX(int X)
	{
		ImageBox.SetX(X);
	}

	public void SetY(int Y)
	{
		ImageBox.SetY(Y);
	}

	public void SetZ(int Z)
	{
		ImageBox.SetZ(Z);
	}

	public new void SetOpacity(byte Opacity)
	{
		ImageBox.SetOpacity(Opacity);
	}

	public void SetAngle(int Angle)
	{
		ImageBox.SetAngle(Angle);
	}

	public void SetMirrorX(bool MirrorX)
	{
		ImageBox.SetMirrorX(MirrorX);
	}

	public void SetMirrorY(bool MirrorY)
	{
		ImageBox.SetMirrorY(MirrorY);
	}

	public void SetOX(int OX)
	{
		ImageBox.SetOX(OX);
	}

	public void SetOY(int OY)
	{
		ImageBox.SetOY(OY);
	}

	public void SetDestroyBitmap(bool DestroyBitmap)
	{
		ImageBox.SetDestroyBitmap(DestroyBitmap);
	}

	public void SetZoomX(double ZoomX)
	{
		ImageBox.SetZoomX(ZoomX);
	}

	public void SetZoomY(double ZoomY)
	{
		ImageBox.SetZoomY(ZoomY);
	}

	public void SetColor(Color Color)
	{
		ImageBox.SetColor(Color);
	}

	public void SetSrcRect(Rect SrcRect)
	{
		ImageBox.SetSrcRect(SrcRect);
	}

	public void SetFillMode(FillMode FillMode)
	{
		ImageBox.SetFillMode(FillMode);
	}
}
