using System;
using ODL;

namespace MKEditor.Widgets
{
    public class LayerWidget : Widget
    {
        public string Text { get; protected set; }
        public bool LayerVisible { get; protected set; } = true;

        private bool RedrawText = true;
        private bool RedrawVisible = true;

        MouseInputManager VisibleIM;

        public LayerWidget(object Parent, string Name = "layerWidget")
            : base(Parent, Name)
        {
            this.SetSize(280, 40);
            int LayerIndex = this.Parent.Parent.Widgets.FindAll(w => (w as LayoutContainer).Widget is LayerWidget).Count;
            this.SetBackgroundColor(LayerIndex % 2 == 0 ? new Color(36, 38, 41) : new Color(48, 50, 53));
            this.Text = $"Layer {6 - LayerIndex}";
            this.Sprites["text"] = new Sprite(this.Viewport);
            this.Sprites["text"].X = 44;
            this.Sprites["text"].Y = 13;
            this.Sprites["visbg"] = new Sprite(this.Viewport, new SolidBitmap(26, 24, LayerIndex % 2 == 1 ? new Color(36, 38, 41) : new Color(48, 50, 53)));
            this.Sprites["visbg"].X = 6;
            this.Sprites["visbg"].Y = 9;
            this.Sprites["visible"] = new Sprite(this.Viewport);
            this.Sprites["visible"].X = 10;
            this.Sprites["visible"].Y = 13;
            this.VisibleIM = new MouseInputManager(this);
            this.VisibleIM.OnLeftClick += LayerVisibleClicked;
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
                this.Redraw();
            }
        }

        public override void Update()
        {
            int DiffX = this.Position.X - this.ScrolledPosition.X;
            int DiffY = this.Position.Y - this.ScrolledPosition.Y;
            int x = this.Viewport.X + 6 - DiffX;
            int y = this.Viewport.Y + 9 - DiffY;
            int w = 26 - DiffX;
            int h = 24 - DiffY;
            this.VisibleIM.Update(new Rect(x, y, w, h));
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
    }
}
