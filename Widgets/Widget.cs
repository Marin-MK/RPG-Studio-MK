using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class Widget : IDisposable, IContainer
    {
        public string                       Name;
        public int                          RealX           { get { return this.Position.X + this.Parent.RealX; } }
        public int                          RealY           { get { return this.Position.Y + this.Parent.RealY; } }
        public Viewport                     Viewport        { get; protected set; }
        public Size                         Size            { get; protected set; } = new Size(20, 20);
        public Point                        Position        { get; protected set; } = new Point(0, 0);
        public Rect                         RealRect        { get; protected set; } = new Rect(0, 0, 0, 0);
        public MKWindow                     Window          { get; protected set; }
        public bool                         AutoResize      { get; protected set; } = false;
        public Size                         MinimumSize     { get; protected set; } = new Size(1, 1);
        public Size                         MaximumSize     { get; protected set; } = new Size(9999, 9999);
        public Color                        BackgroundColor { get; protected set; } = new Color(255, 255, 255, 0);
        public Dictionary<string, Sprite>   Sprites         { get; protected set; } = new Dictionary<string, Sprite>();
        public List<Widget>                 Widgets         { get; protected set; } = new List<Widget>();
        public MouseInputManager            WidgetIM        { get; protected set; }
        public IContainer                   Parent          { get; protected set; }
        public bool                         Disposed        { get; protected set; } = false;

        protected bool Drawn        = false;
        protected bool SizeChanged  = false;
        
        public EventHandler<MouseEventArgs> OnLeftClick;
        public EventHandler<MouseEventArgs> OnMouseDown;
        public EventHandler<MouseEventArgs> OnMousePress;
        public EventHandler<MouseEventArgs> OnMouseUp;
        public EventHandler<MouseEventArgs> OnMouseWheel;
        public EventHandler<MouseEventArgs> OnMouseMoving;

        public Widget(object Parent, string Name = "widget")
        {
            if (Parent is MKWindow)
            {
                this.Window = Parent as MKWindow;
                this.Parent = this.Window.UI;
            }
            else if (Parent is Widget)
            {
                this.Window = (Parent as Widget).Window;
                this.Parent = Parent as IContainer;
            }
            this.Viewport = new Viewport(this.Window.Renderer, 0, 0, this.Size);
            this.Sprites["_bg"] = new Sprite(this.Viewport);
            this.Name = this.Parent.GetName(Name);
            this.OnLeftClick = new EventHandler<MouseEventArgs>(this.LeftClick);
            this.OnMouseMoving = new EventHandler<MouseEventArgs>(this.MouseMoving);
            this.WidgetIM = new MouseInputManager(this);
            this.Parent.Add(this);
        }

        public void Dispose()
        {
            AssertUndisposed();
            foreach (Sprite sprite in Sprites.Values)
            {
                sprite.Dispose();
            }
            this.Viewport.Dispose();
            this.Disposed = true;
        }

        private void AssertUndisposed()
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
        }

        public void Redraw()
        {
            AssertUndisposed();
            this.Drawn = false;
        }

        protected virtual void Draw()
        {
            AssertUndisposed();
            this.Drawn = true;
            this.SizeChanged = false;
        }

        public virtual void Update()
        {
            AssertUndisposed();
            this.WidgetIM.Update(this.RealRect);
            if (!this.Drawn) this.Draw();
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                this.Widgets[i].Update();
            }
        }

        public virtual void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != e.OldLeftButton && this.WidgetIM.Hovering)
            {
                Redraw();
            }
        }
        public virtual void MousePress(object sender, MouseEventArgs e) { }
        public virtual void MouseUp(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != e.OldLeftButton && this.WidgetIM.ClickedInArea == true) Redraw();
        }
        public virtual void MouseWheel(object sender, MouseEventArgs e) { }
        public virtual void MouseMoving(object sender, MouseEventArgs e) { }
        public virtual void HoverChanged(object sender, MouseEventArgs e)
        {
            Redraw();
        }
        public virtual void LeftClick(object sender, MouseEventArgs e) { }

        protected void UpdateBounds()
        {
            AssertUndisposed();
            int DiffX = 0, DiffY = 0;
            if (this.Viewport.X < this.Parent.RealX)
            {
                DiffX = this.Parent.RealX - this.Viewport.X;
                foreach (Sprite s in this.Sprites.Values) s.OX = DiffX;
                this.Viewport.X += DiffX;
            }
            else if (this.Viewport.X + this.Viewport.Width > this.Parent.RealX + this.Parent.Size.Width)
            {
                DiffX = this.Viewport.X + this.Viewport.Width - this.Parent.RealX - this.Parent.Size.Width;
                DiffX *= -1;
                foreach (Sprite s in this.Sprites.Values) s.OX = DiffX;
                this.Viewport.X += DiffX;
            }
            if (this.Viewport.Y < this.Parent.RealY)
            {
                DiffY = this.Parent.RealY - this.Viewport.Y;
                foreach (Sprite s in this.Sprites.Values) s.OY = DiffY;
                this.Viewport.Y += DiffY;
            }
            else if (this.Viewport.Y + this.Viewport.Height > this.Parent.RealY + this.Parent.Size.Height)
            {
                DiffY = this.Viewport.Y + this.Viewport.Height - this.Parent.RealY - this.Parent.Size.Height;
                DiffY *= -1;
                foreach (Sprite s in this.Sprites.Values) s.OY = DiffY;
                this.Viewport.Y += DiffY;
            }

            int x = this.RealX;
            int y = this.RealY;
            int w = this.Size.Width;
            int h = this.Size.Height;
            // Viewport is left/above the parent container
            if (DiffX < 0) w += DiffX;
            if (DiffY < 0) h += DiffY;
            // Viewport is right/under the parent container
            if (DiffX > 0) { x += DiffX; w -= DiffX; }
            if (DiffY > 0) { y += DiffY; h -= DiffY; }
            // Actual rect of this Widget on the screen (used for mouse input)
            this.RealRect = new Rect(x, y, w, h);
        }

        public void SetPosition(int X, int Y)
        {
            this.SetPosition(new Point(X, Y));
        }
        public virtual void SetPosition(Point p)
        {
            AssertUndisposed();
            this.Position = p;
            this.Viewport.X = p.X + this.Parent.RealX;
            this.Viewport.Y = p.Y + this.Parent.RealY;
            UpdateBounds();
        }

        public void SetSize(int Width, int Height)
        {
            SetSize(new Size(Width, Height));
        }
        public virtual void SetSize(Size size)
        {
            AssertUndisposed();
            Size oldsize = this.Size;
            size.Clamp(this.MinimumSize, this.MaximumSize);
            if (oldsize.Width != size.Width || oldsize.Height != size.Height)
            {
                this.Size = size;
                if (this.Sprites["_bg"].Bitmap != null) this.Sprites["_bg"].Bitmap.Dispose();
                this.Sprites["_bg"].Bitmap = new Bitmap(this.Size);
                this.Sprites["_bg"].Bitmap.FillRect(0, 0, this.Size, this.BackgroundColor);
                this.Viewport.Width = this.Size.Width;
                this.Viewport.Height = this.Size.Height;
                this.SizeChanged = true;
                this.SetPosition(this.Position);
                Redraw();
            }
        }

        public void SetBackgroundColor(byte r, byte g, byte b, byte a = 255)
        {
            this.SetBackgroundColor(new Color(r, g, b, a));
        }
        public void SetBackgroundColor(Color c)
        {
            AssertUndisposed();
            this.BackgroundColor = c;
            this.Sprites["_bg"].Bitmap = new Bitmap(this.Size);
            this.Sprites["_bg"].Bitmap.FillRect(0, 0, this.Size, this.BackgroundColor);
        }

        public void Add(Widget w)
        {
            if (this.Widgets.Exists(wgt => wgt.Name == w.Name))
            {
                throw new Exception("Already existing widget by the name of '" + w.Name + "'");
            }
            this.Widgets.Add(w);
        }

        public void Get(string Name)
        {
            throw new NotImplementedException();
        }

        public void Remove(Widget w)
        {
            throw new NotImplementedException();
        }

        public string GetName(string Name)
        {
            int i = 1;
            while (true)
            {
                Widget w = this.Widgets.Find(wgt => wgt.Name == Name + i.ToString());
                if (w == null) return Name + i.ToString();
                i++;
            }
        }
    }
}
