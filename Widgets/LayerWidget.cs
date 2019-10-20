using System;
using System.Collections.Generic;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class LayerWidget : Widget
    {
        public MapViewer MapViewer { get { return (Parent.Parent.Parent.Parent as LayersTab).MapViewer; } }
        public List<Layer> Layers { get; private set; }
        public int SelectedLayer { get; private set; }

        public LayerWidget(object Parent, string Name = "layerWidget")
            : base(Parent, Name)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(2, 24, new Color(59, 227, 255)));
            Sprites["selector"].Visible = false;
            WidgetIM.OnHoverChanged += HoverChanged;
            WidgetIM.OnMouseMoving += MouseMoving;
            WidgetIM.OnMouseDown += MouseDown;
        }

        public void SetLayers(List<Layer> Layers)
        {
            if (this.Layers != Layers)
            {
                this.Layers = Layers;
                SetSize(278, Layers.Count * 24);
                SelectedLayer = Layers.Count - 1;
                Redraw();
            }
        }

        public void UpdateLayers()
        {
            if (Layers.Count * 24 > Size.Height)
            {
                SetHeight(Layers.Count * 24);
            }
            Redraw();
        }

        public void SetSelectedLayer(int layerindex)
        {
            SelectedLayer = layerindex;
            Redraw();
        }

        public void SetLayerVisible(int layerindex, bool visible)
        {
            MapViewer.SetLayerVisible(layerindex, visible);
            Redraw();
        }

        protected override void Draw()
        {
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size.Width, 24 * Layers.Count);
            Sprites["text"].Bitmap = new Bitmap(Size.Width, 24 * Layers.Count);
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = f;
            for (int i = 0; i < Layers.Count; i++)
            {
                int y = 24 * (Layers.Count - i - 1);
                Color c = Color.WHITE;
                bool visible = Layers[i].Visible;
                if (i == SelectedLayer)
                {
                    c = new Color(55, 187, 255);
                    Sprites["bg"].Bitmap.FillRect(0, y, 39, 24, new Color(28, 50, 73));
                    Sprites["bg"].Bitmap.FillRect(44, y, Size.Width - 44, 24, new Color(28, 50, 73));
                    if (visible) Sprites["bg"].Bitmap.Build(8, y - 1, Utilities.IconSheet, new Rect(14 * 24, 24, 24, 24));
                }
                else if (visible) Sprites["bg"].Bitmap.Build(8, y - 1, Utilities.IconSheet, new Rect(14 * 24, 0, 24, 24));
                Sprites["text"].Bitmap.DrawText(Layers[i].Name, 53, y + 3, c);
            }
            Sprites["bg"].Bitmap.Lock();
            Sprites["text"].Bitmap.Lock();
            base.Draw();
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            if (!WidgetIM.Hovering) Sprites["selector"].Visible = false;
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!WidgetIM.Hovering)
            {
                Sprites["selector"].Visible = false;
                return;
            }
            int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
            Sprites["selector"].Y = 24 * (int) Math.Floor(ry / 24d);
            Sprites["selector"].Visible = true;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            MouseMoving(sender, e);
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y + Position.Y - ScrolledPosition.Y;
            if (!WidgetIM.Hovering) return;
            int layerindex = Layers.Count - 1 - (int) Math.Floor(ry / 24d);
            if (layerindex < 0 || layerindex >= Layers.Count) return;
            if (rx < 39)
            {
                if (e.LeftButton != e.OldLeftButton && e.LeftButton)
                {
                    SetLayerVisible(layerindex, !Layers[layerindex].Visible);
                }
                else
                {
                    SetSelectedLayer(layerindex);
                }
            }
            else
            {
                SetSelectedLayer(layerindex);
            }
        }
    }
}
