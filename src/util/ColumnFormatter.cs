using System;
using System.Linq;

namespace RPGStudioMK.Utility;

public class ColumnFormatter
{
    public int N { get; }
    public int Columns { get; }
	public int X { get; private set; }
	public int Y { get; private set; }
	public int XDistance { get; private set; }
	public int YDistance { get; private set; }

	private int[] counts;

	/// <summary>
	/// Algorithm to split <paramref name="n"/> items up into <paramref name="columns"/> vertically.
	/// </summary>
	/// <param name="n">The number of items to split up.</param>
	/// <param name="columns">The number of columns to split the items up in.</param>
	public ColumnFormatter(int n, int columns)
	{
		this.N = n;
        this.Columns = columns;
		counts = new int[columns];

		int remn = n;
		int remc = columns;
		for (int i = 0; i < columns; i++)
		{
			float count = remn / (float) remc;
			if (Utilities.FloatIsInt(count))
			{
				Array.Fill(counts, (int) count, i, remc);
				break;
			}
			else
			{
				count = (float) Math.Ceiling(count);
				counts[i] = (int) count;
				remn -= (int) count;
				remc--;
			}
		}
	}

	public void SetX(int x)
	{
		this.X = x;
	}

	public void SetY(int y)
	{
		this.Y = y;
	}

	public void SetXDistance(int xDistance)
	{
		this.XDistance = xDistance;
	}

	public void SetYDistance(int yDistance)
	{
		this.YDistance = yDistance;
	}

	public (int column, int row) GetColumnAndRow(int index)
	{
		int cIdx = 0;
		int column = 0;
		int row = 0;
		while (index >= 0)
		{
			if (index >= counts[cIdx])
			{
				index -= counts[cIdx];
				cIdx++;
			}
			else
			{
				column = cIdx;
				row = index;
				break;
			}
		}
		return (column, row);
	}

	public Point GetPosition(int index)
	{
		(int column, int row) = GetColumnAndRow(index);
		return new Point(this.X + column * this.XDistance, this.Y + row * this.YDistance);
	}

	public int GetMaxY()
	{
		return this.Y + counts.Max() * this.YDistance;
	}
}
