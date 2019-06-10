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
        public int       LayerIndex    { get; protected set; }
        public MapViewer MapViewer     { get { return (Parent.Parent.Parent.Parent as LayersTab).MapViewer; } }

        private bool RedrawText = true;
        private bool RedrawVisible = true;
        private Rect VisibleRect;

        MouseInputManager VisibleIM;

        public LayerWidget(object Parent, string Name = "layerWidget")
            : base(Parent, Name)
        {
            this.SetSize(263, 32);
            this.LayerIndex = this.Parent.Parent.Widgets.FindAll(w => (w as LayoutContainer).Widget is LayerWidget).Count;
            this.SetBackgroundColor(this.LayerIndex % 2 == 0 ? new Color(36, 38, 41) : new Color(48, 50, 53));
            this.Text = $"Layer {6 - LayerIndex}";
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].X = 44;
            this.Sprites["text"].Y = 9;
            this.Sprites["visible"] = new Sprite(this.Viewport);
            this.Sprites["visible"].X = 10;
            this.Sprites["visible"].Y = 9;
            this.VisibleIM = new MouseInputManager(this);
            this.VisibleIM.OnLeftClick += LayerVisibleClicked;
            this.WidgetIM.OnLeftClick += LayerClicked;
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
                if (Visible) this.Sprites["visible"].X = 10;
                else this.Sprites["visible"].X = 9;
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
                if (this.LayerSelected)
                {
                    this.SetBackgroundColor(new Color(0, 120, 215));
                }
                else
                {
                    this.SetBackgroundColor(this.LayerIndex % 2 == 0 ? new Color(36, 38, 41) : new Color(48, 50, 53));
                }
            }
        }

        public void SetLayerIndex(int Index)
        {
            if (this.LayerIndex != Index)
            {
                this.LayerIndex = Index;
                this.SetBackgroundColor(this.LayerIndex % 2 == 0 ? new Color(36, 38, 41) : new Color(48, 50, 53));
            }
        }

        public override void Update()
        {
            int DiffX = this.Parent.AdjustedPosition.X;
            int DiffY = this.Parent.AdjustedPosition.Y;
            int x = this.Viewport.X + 6 - DiffX;
            int y = this.Viewport.Y + 9 - DiffY;
            int w = 26 - Parent.AdjustedSize.Width;
            int h = 24 - Parent.AdjustedSize.Height;
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
                Font f = Font.Get("Fonts/Quicksand Bold", 16);
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
