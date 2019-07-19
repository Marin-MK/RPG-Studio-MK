using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class LayerWidget : Widget
    {
        public string    Text          { get; protected set; }
        public bool      LayerVisible  { get; protected set; } = true;
        public bool      LayerSelected { get; protected set; } = false;
        public int       LayerIndex    { get; set; }
        public MapViewer MapViewer     { get { return (Parent.Parent.Parent.Parent as LayersTab).MapViewer; } }

        private bool RedrawText = true;
        private bool RedrawVisible = true;
        private Rect VisibleRect;

        MouseInputManager VisibleIM;

        public LayerWidget(object Parent, string Name = "layerWidget", int Index = -1)
            : base(Parent, Name, Index)
        {
            this.SetSize(293, 32);
            this.Sprites["bar"] = new Sprite(this.Viewport, new SolidBitmap(this.Size.Width - 30, 32));
            this.Sprites["bar"].X = 30;
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].X = 40;
            this.Sprites["text"].Y = 8;
            this.Sprites["visible"] = new Sprite(this.Viewport);
            this.Sprites["visible"].X = 5;
            this.Sprites["visible"].Y = 9;
            this.VisibleIM = new MouseInputManager(this);
            this.VisibleIM.OnLeftClick += LayerVisibleClicked;
            this.WidgetIM.OnLeftClick += LayerClicked;
            this.WidgetIM.OnRightClick += LayerClicked;
        }

        public Widget SetText(string Text)
        {
            if (this.Text != Text)
            {
                this.Text = Text;
                this.RedrawText = true;
                this.Redraw();
            }
            return this;
        }

        public void SetLayerVisible(bool Visible)
        {
            if (this.LayerVisible != Visible)
            {
                if (Visible) this.Sprites["visible"].X = 5;
                else this.Sprites["visible"].X = 4;
                this.LayerVisible = Visible;
                this.RedrawVisible = true;
                this.MapViewer.Sprites[(this.MapViewer.Map.Layers.Count - LayerIndex).ToString()].Visible = this.LayerVisible;
                this.Redraw();
            }
        }

        public void SetLayerSelected(bool LayerSelected)
        {
            if (this.LayerSelected != LayerSelected)
            {
                this.LayerSelected = LayerSelected;
                if (this.LayerSelected)
                {
                    foreach (LayoutContainer lc in this.Parent.Parent.Widgets.FindAll(wdgt => wdgt is LayoutContainer))
                    {
                        LayerWidget lw = lc.Widget as LayerWidget;
                        if (lw != this) lw.SetLayerSelected(false);
                    }
                }
                this.Sprites["bar"].Bitmap.Unlock();
                if (this.LayerSelected)
                {
                    (this.Sprites["bar"].Bitmap as SolidBitmap).SetColor(new Color(255, 168, 54));
                    this.Sprites["text"].Color = Color.BLACK;
                }
                else
                {
                    (this.Sprites["bar"].Bitmap as SolidBitmap).SetColor(Color.ALPHA);
                    this.Sprites["text"].Color = Color.WHITE;
                }
                this.Sprites["bar"].Bitmap.Lock();
            }
        }

        public void SetLayerIndex(int Index)
        {
            if (this.LayerIndex != Index)
            {
                this.LayerIndex = Index;
            }
        }

        public override void Update()
        {
            int DiffX = this.Parent.AdjustedPosition.X;
            int DiffY = this.Parent.AdjustedPosition.Y;
            int x = this.Viewport.X - DiffX;
            int y = this.Viewport.Y - DiffY + 4;
            int w = 30 - Parent.AdjustedSize.Width;
            int h = 28 - Parent.AdjustedSize.Height;
            if (x < this.Viewport.X) { w -= this.Viewport.X - x; x = this.Viewport.X; }
            if (y < this.Viewport.Y) { h -= this.Viewport.Y - y; y = this.Viewport.Y; }
            if (this.Viewport.Width < 0) w = 0;
            if (this.Viewport.Height < 0) h = 0;
            VisibleRect = new Rect(x, y, w, h);
            this.VisibleIM.Update(VisibleRect);
            base.Update();
        }

        protected override void Draw()
        {
            if (this.RedrawText)
            {
                Font f = Font.Get("Fonts/Ubuntu-R", 16);
                Size s = f.TextSize(this.Text);
                if (this.Sprites["text"].Bitmap != null) this.Sprites["text"].Bitmap.Dispose();
                this.Sprites["text"].Bitmap = new Bitmap(s);
                this.Sprites["text"].Bitmap.Unlock();
                this.Sprites["text"].Bitmap.Font = f;
                this.Sprites["text"].Bitmap.DrawText(this.Text, Color.WHITE);
                this.Sprites["text"].Bitmap.Lock();
            }

            if (this.RedrawVisible)
            {
                Font f = Font.Get("Fonts/FontAwesome Solid", 16);
                char c = this.LayerVisible ? '\uf06e' : '\uf070';
                Size s = f.TextSize(c);
                if (this.Sprites["visible"].Bitmap != null) this.Sprites["visible"].Bitmap.Dispose();
                this.Sprites["visible"].Bitmap = new Bitmap(s);
                this.Sprites["visible"].Bitmap.Unlock();
                this.Sprites["visible"].Bitmap.Font = f;
                this.Sprites["visible"].Bitmap.DrawGlyph(c, Color.WHITE);
                this.Sprites["visible"].Bitmap.Lock();
            }

            this.RedrawText = false;
            this.RedrawVisible = false;
            base.Draw();
        }

        private void LayerVisibleClicked(object sender, MouseEventArgs e)
        {
            this.SetLayerVisible(!this.LayerVisible);
        }
        
        private void LayerClicked(object sender, MouseEventArgs e)
        {
            // Can't select a layer when inside the Visible button
            if (VisibleRect.Contains(e.X, e.Y)) return;
            this.SetLayerSelected(true);
        }
    }
}
