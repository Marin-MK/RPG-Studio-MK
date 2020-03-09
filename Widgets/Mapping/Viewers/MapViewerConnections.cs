using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MapViewerConnections : MapViewerBase
    {
        public MapViewerConnections(object Parent, string Name = "mapViewerConnections")
            : base(Parent, Name)
        {
            
        }
    }

    public class MapConnectionWidget : MapImageWidget
    {
        public int MapID;
        public string Side;
        public int Offset;
        public int PixelOffset => (int) Math.Round(this.Offset * 32 * ZoomFactor);
        public int Depth;

        public MapConnectionWidget(object Parent, string Name = "mapConnectionWidget")
            : base(Parent, Name)
        {

        }

        public override void UpdateSize()
        {
            int Width = (int) Math.Round(MapData.Width * 32 * ZoomFactor);
            int Height = (int) Math.Round(MapData.Height * 32 * ZoomFactor);
            if (Side == ":north" || Side == ":south") Height = (int) Math.Round(Math.Min(Depth, MapData.Height) * 32 * ZoomFactor);
            else if (Side == ":east" || Side == ":west") Width = (int) Math.Round(Math.Min(Depth, MapData.Width) * 32 * ZoomFactor);
            this.SetSize(Width, Height);
        }

        public override void LoadLayers(Map MapData, string Side = "", int Offset = 0)
        {
            this.MapID = MapData.ID;
            this.MapData = MapData;
            this.Side = Side;
            this.Offset = Offset;
            UpdateSize();
            RedrawLayers();
        }

        public override void RedrawLayers()
        {
            foreach (string s in this.Sprites.Keys)
            {
                if (s != "_bg" && s != "dark") this.Sprites[s].Dispose();
            }
            // Create layers
            for (int i = 0; i < MapData.Layers.Count; i++)
            {
                this.Sprites[i.ToString()] = new Sprite(this.Viewport);
                this.Sprites[i.ToString()].Z = i * 2;
                this.Sprites[i.ToString()].Visible = MapData.Layers[i].Visible;
            }
            int SX = 0;
            int SY = 0;
            int Width = MapData.Width;
            int Height = MapData.Height;
            if (this.Side == ":north")
            {
                SY = MapData.Height - Depth;
                Height = Depth;
            }
            else if (this.Side == ":east")
            {
                Width = Depth;
            }
            else if (this.Side == ":south")
            {
                Height = Depth;
            }
            else if (this.Side == ":west")
            {
                SX = MapData.Width - Depth;
                Width = Depth;
            }
            List<Bitmap> bmps = GetBitmaps(MapData.ID, SX, SY, Width, Height);
            for (int i = 0; i < bmps.Count; i++) Sprites[i.ToString()].Bitmap = bmps[i];
            // Zoom layers
            SetZoomFactor(ZoomFactor);
        }
    }
}
