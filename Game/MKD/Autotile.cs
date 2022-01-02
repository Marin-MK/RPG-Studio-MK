using System;
using System.Collections.Generic;

namespace RPGStudioMK.Game;

[Serializable]
public class Autotile : ICloneable
{
    public int ID;
    public string Name;
    public AutotileFormat Format
    {
        get
        {
            if (AutotileBitmap?.Height == 32) return AutotileFormat.Single;
            else if (AutotileBitmap?.Height == 96) return AutotileFormat.RMVX;
            else return AutotileFormat.RMXP;
        }
    }
    public string GraphicName;
    public Passability Passability;
    public int Priority;
    public int Tag;
    public int AnimateSpeed = 16;
    public List<int?> QuickIDs = new List<int?>() { null, null, null, null, null, null };

    public List<int> OverlappableBy = new List<int>();

    [NonSerialized]
    private Bitmap _bmp;
    public Bitmap AutotileBitmap
    {
        get
        {
            if (_bmp != null) return _bmp;
            string filename = $"{Data.ProjectPath}/Graphics/Autotiles/{this.GraphicName}.png";
            _bmp = null;
            if (Bitmap.FindRealFilename(filename) != null)
            {
                _bmp = new Bitmap(filename);
            }
            return _bmp;
        }
        set
        {
            _bmp = value;
        }
    }

    public static Dictionary<AutotileFormat, List<List<int>>> AutotileCombinations = new Dictionary<AutotileFormat, List<List<int>>>()
        {
            {
                AutotileFormat.RMXP, new List<List<int>>()
                {
                    new List<int>() { 26, 27, 32, 33 }, new List<int>() {  4, 27, 32, 33 }, new List<int>() { 26,  5, 32, 33 }, new List<int>() {  4,  5, 32, 33 },
                    new List<int>() { 26, 27, 32, 11 }, new List<int>() {  4, 27, 32, 11 }, new List<int>() { 26,  5, 32, 11 }, new List<int>() {  4,  5, 32, 11 },
                    new List<int>() { 26, 27, 10, 33 }, new List<int>() {  4, 27, 10, 33 }, new List<int>() { 26,  5, 10, 33 }, new List<int>() {  4,  5, 10, 33 },
                    new List<int>() { 26, 27, 10, 11 }, new List<int>() {  4, 27, 10, 11 }, new List<int>() { 26,  5, 10, 11 }, new List<int>() {  4,  5, 10, 11 },
                    new List<int>() { 24, 25, 30, 31 }, new List<int>() { 24,  5, 30, 31 }, new List<int>() { 24, 25, 30, 11 }, new List<int>() { 24,  5, 30, 11 },
                    new List<int>() { 14, 15, 20, 21 }, new List<int>() { 14, 15, 20, 11 }, new List<int>() { 14, 15, 10, 21 }, new List<int>() { 14, 15, 10, 11 },
                    new List<int>() { 28, 29, 34, 35 }, new List<int>() { 28, 29, 10, 35 }, new List<int>() {  4, 29, 34, 35 }, new List<int>() {  4, 29, 10, 35 },
                    new List<int>() { 38, 39, 44, 45 }, new List<int>() {  4, 39, 44, 45 }, new List<int>() { 38,  5, 44, 45 }, new List<int>() {  4,  5, 44, 45 },
                    new List<int>() { 24, 29, 30, 35 }, new List<int>() { 14, 15, 44, 45 }, new List<int>() { 12, 13, 18, 19 }, new List<int>() { 12, 13, 18, 11 },
                    new List<int>() { 16, 17, 22, 23 }, new List<int>() { 16, 17, 10, 23 }, new List<int>() { 40, 41, 46, 47 }, new List<int>() {  4, 41, 46, 47 },
                    new List<int>() { 36, 37, 42, 43 }, new List<int>() { 36,  5, 42, 43 }, new List<int>() { 12, 17, 18, 23 }, new List<int>() { 12, 13, 42, 43 },
                    new List<int>() { 36, 41, 42, 47 }, new List<int>() { 16, 17, 46, 47 }, new List<int>() { 12, 17, 42, 47 }, new List<int>() {  0,  1,  6,  7 }
                }
            },
            {
                AutotileFormat.FullCorners, new List<List<int>>()
                {
                    new List<int>() { 26, 27, 32, 33 }, new List<int>() { 48, 49, 54, 55 }, new List<int>() { 50, 51, 56, 57 }, new List<int>() {  4,  5, 32, 33 },
                    new List<int>() { 62, 63, 68, 69 }, new List<int>() {  4, 27, 32, 11 }, new List<int>() { 26,  5, 32, 11 }, new List<int>() {  4,  5, 32, 11 },
                    new List<int>() { 60, 61,  6, 67 }, new List<int>() {  4, 27, 10, 33 }, new List<int>() { 26,  5, 10, 33 }, new List<int>() {  4,  5, 10, 33 },
                    new List<int>() { 26, 27, 10, 11 }, new List<int>() {  4, 27, 10, 11 }, new List<int>() { 26,  5, 10, 11 }, new List<int>() {  4,  5, 10, 11 },
                    new List<int>() { 24, 25, 30, 31 }, new List<int>() { 24,  5, 30, 31 }, new List<int>() { 24, 25, 30, 11 }, new List<int>() { 24,  5, 30, 11 },
                    new List<int>() { 14, 15, 20, 21 }, new List<int>() { 14, 15, 20, 11 }, new List<int>() { 14, 15, 10, 21 }, new List<int>() { 14, 15, 10, 11 },
                    new List<int>() { 28, 29, 34, 35 }, new List<int>() { 28, 29, 10, 35 }, new List<int>() {  4, 29, 34, 35 }, new List<int>() {  4, 29, 10, 35 },
                    new List<int>() { 38, 39, 44, 45 }, new List<int>() {  4, 39, 44, 45 }, new List<int>() { 38,  5, 44, 45 }, new List<int>() {  4,  5, 44, 45 },
                    new List<int>() { 24, 29, 30, 35 }, new List<int>() { 14, 15, 44, 45 }, new List<int>() { 12, 13, 18, 19 }, new List<int>() { 12, 13, 18, 11 },
                    new List<int>() { 16, 17, 22, 23 }, new List<int>() { 16, 17, 10, 23 }, new List<int>() { 40, 41, 46, 47 }, new List<int>() {  4, 41, 46, 47 },
                    new List<int>() { 36, 37, 42, 43 }, new List<int>() { 36,  5, 42, 43 }, new List<int>() { 12, 17, 18, 23 }, new List<int>() { 12, 13, 42, 43 },
                    new List<int>() { 36, 41, 42, 47 }, new List<int>() { 16, 17, 46, 47 }, new List<int>() { 12, 17, 42, 47 }, new List<int>() {  0,  1,  6,  7 }
                }
            },
            {
                AutotileFormat.RMVX, new List<List<int>>()
                {
                    new List<int>() { 13, 14, 17, 18 }, new List<int>() {  2, 14, 17, 18 }, new List<int>() { 13,  3, 17, 18 }, new List<int>() {  2,  3, 17, 18 },
                    new List<int>() { 13, 14, 17,  7 }, new List<int>() {  2, 14, 17,  7 }, new List<int>() { 13,  3, 17,  7 }, new List<int>() {  2,  3, 17,  7 },
                    new List<int>() { 13, 14,  6, 18 }, new List<int>() {  2, 14,  6, 18 }, new List<int>() { 13,  3,  6, 18 }, new List<int>() {  2,  3,  6, 18 },
                    new List<int>() { 13, 14,  6,  7 }, new List<int>() {  2, 14,  6,  7 }, new List<int>() { 13,  3,  6,  7 }, new List<int>() {  2,  3,  6,  7 },
                    new List<int>() { 12, 13, 16, 17 }, new List<int>() { 12,  3, 16, 17 }, new List<int>() { 12, 13, 16,  7 }, new List<int>() { 12,  3, 16,  7 },
                    new List<int>() {  9, 10, 13, 14 }, new List<int>() {  9, 10, 13,  7 }, new List<int>() {  9, 10,  6, 14 }, new List<int>() {  9, 10,  6,  7 },
                    new List<int>() { 14, 15, 18, 19 }, new List<int>() { 14, 15,  6, 19 }, new List<int>() {  2, 15, 18, 19 }, new List<int>() {  2, 15,  6, 19 },
                    new List<int>() { 17, 18, 21, 22 }, new List<int>() {  2, 18, 21, 22 }, new List<int>() { 17,  3, 21, 22 }, new List<int>() {  2,  3, 21, 22 },
                    new List<int>() { 12, 15, 16, 19 }, new List<int>() {  9, 10, 21, 22 }, new List<int>() {  8,  9, 12, 13 }, new List<int>() {  8,  9, 12,  7 },
                    new List<int>() { 10, 11, 14, 15 }, new List<int>() { 10, 11,  6, 15 }, new List<int>() { 18, 19, 22, 23 }, new List<int>() {  2, 19, 22, 23 },
                    new List<int>() { 16, 17, 20, 21 }, new List<int>() { 16,  3, 20, 21 }, new List<int>() {  8, 11, 12, 15 }, new List<int>() {  8,  9, 20, 21 },
                    new List<int>() { 16, 19, 20, 23 }, new List<int>() { 10, 11, 22, 23 }, new List<int>() {  8, 11, 20, 23 }, new List<int>() {  0,  1,  4,  5 }
                }
            }
        };

    public void SetGraphic(string GraphicName)
    {
        if (this.GraphicName != GraphicName)
        {
            this.Name = GraphicName;
            this.GraphicName = GraphicName;
            this.CreateBitmap(true);
        }
    }

    public void CreateBitmap(bool Redraw = false)
    {
        if (_bmp == null || Redraw)
        {
            _bmp?.Dispose();
            _bmp = null;
            this.OverlappableBy.Clear();
        }
    }

    public override string ToString()
    {
        return this.Name;
    }

    public object Clone()
    {
        Autotile a = new Autotile();
        a.ID = this.ID;
        a.Name = this.Name;
        a.GraphicName = this.GraphicName;
        a.Passability = this.Passability;
        a.Priority = this.Priority;
        a.Tag = this.Tag;
        a.AnimateSpeed = this.AnimateSpeed;
        a.QuickIDs = new List<int?>(this.QuickIDs);
        a.OverlappableBy = new List<int>(this.OverlappableBy);
        a.AutotileBitmap = this.AutotileBitmap;
        return a;
    }
}

public enum AutotileFormat
{
    RMXP,
    FullCorners,
    RMVX,
    Single
}
