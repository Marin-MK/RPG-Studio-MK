using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class GraphPlotter : Widget
{
	public double StartXValue { get; protected set; }
	public double EndXValue { get; protected set; }
	public double StepSize { get; protected set; }
    public Func<double, double> GraphFunction { get; protected set; }
	public double StartYValue { get; protected set; }
	public double EndYValue { get; protected set; }
	public bool ScaleYAxis { get; protected set; } = true;
	public bool Interpolation { get; protected set; }

	bool StepSizeIsSet = false;
    double Range => EndXValue - StartXValue;
	double PointCount => Range / StepSize;
    double MinValue => Values.Min(e => e.Output);
    double MaxValue => Values.Max(e => e.Output);
    double YRange => MaxValue - MinValue;

    bool RecalculateGraph = true;

	List<(double Input, double Output)> Values = new List<(double Input, double Output)>();

	public GraphPlotter(IContainer Parent) : base(Parent)
	{
		Sprites["bg"] = new Sprite(this.Viewport);
		Sprites["bg"].Bitmap = new SolidBitmap(Size, Color.WHITE);
		Sprites["plot"] = new Sprite(this.Viewport);
	}

	public void SetStartXValue(double StartXValue)
	{
		if (this.StartXValue != StartXValue)
		{
			this.StartXValue = StartXValue;
			Recalculate();
		}
	}

	public void SetEndXValue(double EndXValue)
	{
		if (this.EndXValue != EndXValue)
		{
			this.EndXValue = EndXValue;
			Recalculate();
		}
	}

	public void SetRange(double StartXValue, double EndXValue)
	{
		SetStartXValue(StartXValue);
		SetEndXValue(EndXValue);
	}

    public void SetStartYValue(double StartYValue)
    {
        if (this.StartYValue != StartYValue)
        {
            this.StartYValue = StartYValue;
			ScaleYAxis = false;
            Recalculate();
        }
    }

    public void SetEndYValue(double EndYValue)
    {
        if (this.EndYValue != EndYValue)
        {
            this.EndYValue = EndYValue;
			ScaleYAxis = false;
            Recalculate();
        }
    }

    public void SetDomain(double StartYValue, double EndYValue)
    {
        SetStartYValue(StartYValue);
        SetEndYValue(EndYValue);
    }

    public void SetStepSize(double StepSize)
	{
		if (this.StepSize != StepSize)
		{
			this.StepSize = StepSize;
			StepSizeIsSet = true;
			Recalculate();
		}
	}

	public void SetGraphFunction(Func<double, double> GraphFunction)
	{
		this.GraphFunction = GraphFunction;
		Recalculate();
	}

	public void SetInterpolation(bool Interpolation)
	{
		if (this.Interpolation != Interpolation)
		{
			this.Interpolation = Interpolation;
			Redraw();
		}
	}

	public override void Update()
	{
		if (RecalculateGraph)
		{
			Recalculate(true);
		}
		base.Update();
	}

	public override void SizeChanged(BaseEventArgs e)
	{
		base.SizeChanged(e);
		((SolidBitmap) Sprites["bg"].Bitmap).SetSize(Size);
		Redraw();
	}

	protected void Recalculate(bool Now = false)
	{
		if (!Now)
		{
			RecalculateGraph = true;
            Redraw();
            return;
		}
		Values.Clear();
		if (!StepSizeIsSet) StepSize = (double) Range / Size.Width;
        for (int i = 0; i < PointCount + 1; i++)
        {
            double Argument = StartXValue + StepSize * i;
			double Y = GraphFunction.Invoke(Argument);
            Values.Add((Argument, Y));
        }
		RecalculateGraph = false;
    }

	protected override void Draw()
	{
		base.Draw();
		if (PointCount > Size.Width) throw new Exception("Graph too small for all data points");
		if (PointCount + 1 != Values.Count) throw new Exception("Data count mismatch");
		int StartIdx = 0;
		List<Point> Points = new List<Point>();
		for (int x = 0; x < Size.Width; x++)
		{
			double Fraction = (double) x / Size.Width;
			double ValueX = StartXValue + Fraction * Range;
			int idx = Values.FindIndex(e => e.Input == ValueX);
			if (idx != -1)
			{
				double Value = Values[idx].Output;
				Points.Add(new Point(x, ValueToYPosition(Value)));
				StartIdx = idx;
			}
			else
			{
				// Interpolate
				int idxA = 0;
				int idxB = 0;
				for (int i = StartIdx; i < Values.Count; i++)
				{
					if (Values[i].Input > ValueX)
					{
						idxA = i - 1;
						idxB = i;
						break;
					}
				}
				if (idxA == idxB) continue;
				double Range = Values[idxB].Input - Values[idxA].Input;
				double Domain = Values[idxB].Output - Values[idxA].Output;
				double Factor = (ValueX - Values[idxA].Input) / Range;
				double Value = Values[idxA].Output + Factor * Domain;
				Points.Add(new Point(x, ValueToYPosition(Value)));
                StartIdx = idxA;
            }
        }
        Sprites["plot"].Bitmap?.Dispose();
        Sprites["plot"].Bitmap = new Bitmap(Size);
        Sprites["plot"].Bitmap.Unlock();
		// Draw all points and interpolate between them
		for (int i = 0; i < Points.Count; i++)
		{
			Sprites["plot"].Bitmap.SetPixel(Points[i].X, Points[i].Y, Color.BLACK);
			if (Interpolation && i < Points.Count - 1)
			{
				if (Math.Abs(Points[i].Y - Points[i + 1].Y) > 1) // More than 1 pixel apart
				{
					Sprites["plot"].Bitmap.DrawLine(Points[i].X, Points[i].Y, Points[i + 1].X, Points[i + 1].Y, Color.BLACK);
				}
			}
		}
        Sprites["plot"].Bitmap.Lock();
	}

	private int ValueToYPosition(double Value)
	{
		if (ScaleYAxis)
		{
            double HeightFraction = (Value - MinValue) / YRange;
            double YPos = (Size.Height - 1) - HeightFraction * (Size.Height - 1);
			return (int) Math.Round(YPos);
        }
		double Domain = EndYValue - StartYValue - 1;
		double Fraction = (Value - StartYValue) / Domain;
		double Pos = (Size.Height - 1) - (Fraction * Domain);
		return (int) Math.Round(Pos);
	}
}