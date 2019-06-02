using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class Widget : IDisposable, IContainer
    {
        public string                       Name;
        public Viewport                     Viewport         { get; set; }
        public Size                         Size             { get; protected set; } = new Size(50, 50);
        public Point                        Position         { get; protected set; } = new Point(0, 0);
        public WidgetWindow                 Window           { get; protected set; }
        public bool                         AutoResize       { get; protected set; } = false;
        public Size                         MinimumSize      { get; protected set; } = new Size(1, 1);
        public Size                         MaximumSize      { get; protected set; } = new Size(9999, 9999);
        public Color                        BackgroundColor  { get; protected set; } = new Color(255, 255, 255, 0);
        public Dictionary<string, ISprite>  Sprites          { get; protected set; } = new Dictionary<string, ISprite>();
        public List<Widget>                 Widgets          { get; protected set; } = new List<Widget>();
        public MouseInputManager            WidgetIM         { get; protected set; }
        public IContainer                   Parent           { get; set; }
        public bool                         Disposed         { get; protected set; } = false;
        public Point                        AdjustedPosition { get; protected set; } = new Point(0, 0);
        public Size                         AdjustedSize     { get; protected set; } = new Size(50, 50);
        public bool                         Selected         = false;

        public  bool   AutoScroll        = false;
        private double _ScrollStep       = 0;
        public  double ScrollStep        { get { return _ScrollStep; } set { _ScrollStep = value; if (this.ScrollBar != null) this.ScrollBar.ScrollStep = value; } }
        public  double ScrollPercentageX { get; set; } = 0;
        public  double ScrollPercentageY { get; set; } = 0;
        public  Point  ScrolledPosition
        {
            get
            {
                return new Point(
                    this.Position.X - (int) Math.Round((this.Size.Width - this.Viewport.Width) * this.Parent.ScrollPercentageX),
                    this.Position.Y - (int) Math.Round((this.Size.Height - this.Viewport.Height) * this.Parent.ScrollPercentageY)
                );
            }
        }

        public Margin Margin { get; protected set; } = new Margin();
        public int GridRowStart     = 0;
        public int GridRowEnd       = 0;
        public int GridColumnStart  = 0;
        public int GridColumnEnd    = 0;

        protected bool Drawn      = false;
        protected bool RedrawSize = false;
        
        public EventHandler<MouseEventArgs> OnLeftClick;
        public EventHandler<MouseEventArgs> OnMouseDown;
        public EventHandler<MouseEventArgs> OnMousePress;
        public EventHandler<MouseEventArgs> OnMouseUp;
        public EventHandler<MouseEventArgs> OnMouseWheel;
        public EventHandler<MouseEventArgs> OnMouseMoving;
        public EventHandler<MouseEventArgs> OnWidgetSelect;
        public EventHandler<EventArgs> OnSelected;
        public EventHandler<EventArgs> OnDeselected;
        public EventHandler<TextInputEventArgs> OnTextInput;
        public EventHandler<SizeEventArgs> OnSizeChanged;
        public EventHandler<SizeEventArgs> OnParentSizeChanged;
        public EventHandler<SizeEventArgs> OnChildSizeChanged;

        public MinimalVScrollBar ScrollBar;

        public Widget(object Parent, string Name = "widget")
        {
            if (Parent is Grid && !(this is GridContainer))
            {
                (Parent as Grid).Add(this);
            }
            else
            {
                this.SetParent(Parent);
                this.Viewport = new Viewport(this.Window.Renderer, 0, 0, this.Size);
            }
            this.Name = this.Parent.GetName(Name);
            this.Sprites["_bg"] = new Sprite(this.Viewport);
            this.Sprites["_bg"].Z = -999999999;
            this.OnLeftClick = new EventHandler<MouseEventArgs>(this.LeftClick);
            this.OnMouseMoving = new EventHandler<MouseEventArgs>(this.MouseMoving);
            this.OnSelected = new EventHandler<EventArgs>(this.WidgetSelected);
            this.OnDeselected = new EventHandler<EventArgs>(this.WidgetDeselected);
            this.OnTextInput = new EventHandler<TextInputEventArgs>(this.TextInput);
            this.OnSizeChanged = new EventHandler<SizeEventArgs>(this.SizeChanged);
            this.OnParentSizeChanged = new EventHandler<SizeEventArgs>(this.ParentSizeChanged);
            this.OnChildSizeChanged = new EventHandler<SizeEventArgs>(this.ChildSizeChanged);
            this.WidgetIM = new MouseInputManager(this);
        }

        public void SetParent(object Parent)
        {
            if (this.Parent != null) this.Parent.Remove(this);
            if (Parent is WidgetWindow)
            {
                this.Window = Parent as WidgetWindow;
                this.Parent = this.Window.UI;
            }
            else if (Parent is Widget)
            {
                this.Window = (Parent as Widget).Window;
                this.Parent = Parent as IContainer;
            }
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
            this.Viewport = null;
            this.Sprites = null;
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
            this.RedrawSize = false;
        }

        public virtual void Update()
        {
            AssertUndisposed();
            this.WidgetIM.Update(this.Viewport.Rect);
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
        public virtual void WidgetSelect(object sender, MouseEventArgs e)
        {
            this.Window.UI.SetSelectedWidget(this);
        }
        public virtual void WidgetSelected(object sender, EventArgs e) { }
        public virtual void WidgetDeselected(object sender, EventArgs e) { }
        public virtual void TextInput(object sender, TextInputEventArgs e) { }
        public virtual void SizeChanged(object sender, SizeEventArgs e)
        {
            if (ScrollBar != null)
            {
                ScrollBar.SetPosition(this.Size.Width - 10, 2);
                ScrollBar.SetSize(11, this.Size.Height - 4);
                ScrollBar.MouseInputRect = this.Viewport.Rect;
            }
            UpdateLayout();
        }
        public virtual void ParentSizeChanged(object sender, SizeEventArgs e) { }
        public virtual void ChildSizeChanged(object sender, SizeEventArgs e)
        {
            if (!AutoScroll) return;
            int maxheight = 0;
            this.Widgets.ForEach(w =>
            {
                if (w.Position.Y + w.Size.Height > maxheight) maxheight = w.Position.Y + w.Size.Height;
            });
            if (maxheight > this.Size.Height)
            {
                if (ScrollBar == null)
                {
                    ScrollBar = new MinimalVScrollBar(this);
                    ScrollBar.Name = "ContainerScrollBar";
                    ScrollBar.MouseInputRect = this.Viewport.Rect;
                    ScrollBar.ScrollStep = this.ScrollStep == 0 ? (this.Size.Height / (double) maxheight) / 10d : this.ScrollStep;
                    ScrollBar.OnValueChanged += delegate (object sender2, EventArgs e2)
                    {
                        this.ScrollPercentageY = ScrollBar.Value;
                        this.Widgets.ForEach(w =>
                        {
                            if (w.Name != "ContainerScrollBar") w.UpdateBounds();
                        });
                    };
                }
                ScrollBar.SetPosition(this.Size.Width - 10, 2);
                ScrollBar.SetSize(11, this.Size.Height - 4);
            }
            else if (ScrollBar != null)
            {
                ScrollBar.Dispose();
                ScrollBar = null;
            }
        }

        protected void UpdateBounds()
        {
            AssertUndisposed();
            foreach (ISprite s in this.Sprites.Values)
            {
                if (s is MultiSprite) foreach (Sprite ms in (s as MultiSprite).SpriteList.Values) ms.OX = ms.OY = 0;
                else s.OX = s.OY = 0;
            }

            int ScrolledX = this.Position.X - this.ScrolledPosition.X;
            int ScrolledY = this.Position.Y - this.ScrolledPosition.Y;
            int ParentScrolledX = (this.Parent.Position.X - this.Parent.ScrolledPosition.X);
            int ParentScrolledY = (this.Parent.Position.Y - this.Parent.ScrolledPosition.Y);

            this.Viewport.X = this.Position.X + this.Parent.Viewport.X - Parent.AdjustedPosition.X - ScrolledX;
            this.Viewport.Y = this.Position.Y + this.Parent.Viewport.Y - Parent.AdjustedPosition.Y - ScrolledY;
            this.Viewport.Width = this.Size.Width;
            this.Viewport.Height = this.Size.Height;
            int DiffX = 0;
            int DiffY = 0;
            int DiffWidth = 0;
            int DiffHeight = 0;
            /* Handles X positioning */
            if (this.Viewport.X < this.Parent.Viewport.X)
            {
                DiffX = this.Parent.Viewport.X - this.Viewport.X;
                foreach (ISprite s in this.Sprites.Values)
                {
                    if (s is MultiSprite) foreach (Sprite ms in (s as MultiSprite).SpriteList.Values) ms.OX += DiffX;
                    else s.OX += DiffX;
                }
                this.Viewport.X = this.Position.X + this.Parent.Viewport.X + DiffX - ScrolledX - Parent.AdjustedPosition.X;
            }
            /* Handles width manipulation */
            if (this.Viewport.X + this.Size.Width > this.Parent.Viewport.X + this.Parent.Viewport.Width)
            {
                DiffWidth = this.Viewport.X + this.Size.Width - (this.Parent.Viewport.X + this.Parent.Viewport.Width);
                this.Viewport.Width -= DiffWidth;
            }
            /* Handles Y positioning */
            if (this.Viewport.Y < this.Parent.Viewport.Y)
            {
                DiffY = this.Parent.Viewport.Y - this.Viewport.Y;
                foreach (ISprite s in this.Sprites.Values)
                {
                    if (s is MultiSprite) foreach (Sprite ms in (s as MultiSprite).SpriteList.Values) ms.OY += DiffY;
                    else s.OY += DiffY;
                }
                this.Viewport.Y = this.Position.Y + this.Parent.Viewport.Y + DiffY - ScrolledY - Parent.AdjustedPosition.Y;
            }
            /* Handles height manipulation */
            if (this.Viewport.Y + this.Size.Height > this.Parent.Viewport.Y + this.Parent.Viewport.Height)
            {
                DiffHeight = this.Viewport.Y + this.Size.Height - (this.Parent.Viewport.Y + this.Parent.Viewport.Height);
                this.Viewport.Height -= DiffHeight;
            }
            this.AdjustedPosition = new Point(DiffX, DiffY);
            this.AdjustedSize = new Size(DiffWidth, DiffHeight);
            foreach (Widget w in this.Widgets)
            {
                w.UpdateBounds();
            }
        }

        public Widget SetPosition(int X, int Y)
        {
            return this.SetPosition(new Point(X, Y));
        }
        public virtual Widget SetPosition(Point p)
        {
            AssertUndisposed();
            this.Position = p;
            UpdateBounds();
            return this;
        }

        public Widget SetWidth(int Width)
        {
            return this.SetSize(Width, this.Size.Height);
        }
        public Widget SetHeight(int Height)
        {
            return this.SetSize(this.Size.Width, Height);
        }
        public Widget SetSize(int Width, int Height)
        {
            return SetSize(new Size(Width, Height));
        }
        public virtual Widget SetSize(Size size)
        {
            AssertUndisposed();
            Size oldsize = this.Size;
            size.Clamp(this.MinimumSize, this.MaximumSize);
            if (oldsize.Width != size.Width || oldsize.Height != size.Height)
            {
                this.Size = size;
                if (this.Sprites["_bg"].Bitmap != null) this.Sprites["_bg"].Bitmap.Dispose();
                this.Sprites["_bg"].Bitmap = new SolidBitmap(this.Size, this.BackgroundColor);
                this.Viewport.Width = this.Size.Width;
                this.Viewport.Height = this.Size.Height;
                this.RedrawSize = true;
                this.SetPosition(this.Position);
                this.OnSizeChanged.Invoke(this, new SizeEventArgs(this.Size, oldsize));
                this.Widgets.ForEach(w =>
                {
                    w.OnParentSizeChanged.Invoke(this, new SizeEventArgs(this.Size, oldsize));
                });
                if (this.Parent is Widget) (this.Parent as Widget).OnChildSizeChanged.Invoke(this, new SizeEventArgs(this.Size, oldsize));
                Redraw();
            }
            return this;
        }

        public Widget SetBackgroundColor(byte r, byte g, byte b, byte a = 255)
        {
            return this.SetBackgroundColor(new Color(r, g, b, a));
        }
        public Widget SetBackgroundColor(Color c)
        {
            AssertUndisposed();
            this.BackgroundColor = c;
            this.Sprites["_bg"].Bitmap = new SolidBitmap(this.Size, this.BackgroundColor);
            return this;
        }

        public virtual IContainer Add(Widget w)
        {
            if (this.Widgets.Exists(wgt => wgt.Name == w.Name))
            {
                throw new Exception("Already existing widget by the name of '" + w.Name + "'");
            }
            this.Widgets.Add(w);
            return this;
        }

        public virtual IContainer Get(string Name)
        {
            foreach (Widget w in this.Widgets)
            {
                if (w.Name == Name) return w;
            }
            return null;
        }

        public virtual IContainer Remove(Widget w)
        {
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                if (this.Widgets[i] == w)
                {
                    this.Widgets.RemoveAt(i);
                    return w;
                }
            }
            return null;
        }

        public virtual string GetName(string Name)
        {
            int i = 1;
            while (true)
            {
                Widget w = this.Widgets.Find(wgt => wgt.Name == Name + i.ToString());
                if (w == null) return Name + i.ToString();
                i++;
            }
        }

        public Widget SetMargin(int all)
        {
            return this.SetMargin(all, all, all, all);
        }
        public Widget SetMargin(int horizontal, int vertical)
        {
            return this.SetMargin(horizontal, vertical, horizontal, vertical);
        }
        public Widget SetMargin(int left, int up, int right, int down)
        {
            this.Margin = new Margin(left, up, right, down);
            this.UpdateLayout();
            return this;
        }

        public Widget SetGridRow(int Row)
        {
            return this.SetGridRow(Row, Row);
        }
        public Widget SetGridRow(int RowStart, int RowEnd)
        {
            this.GridRowStart = RowStart;
            this.GridRowEnd = RowEnd;
            this.UpdateLayout();
            return this;
        }

        public Widget SetGridColumn(int Column)
        {
            return this.SetGridColumn(Column, Column);
        }
        public Widget SetGridColumn(int ColumnStart, int ColumnEnd)
        {
            this.GridColumnStart = ColumnStart;
            this.GridColumnEnd = ColumnEnd;
            this.UpdateLayout();
            return this;
        }

        public Widget SetGrid(int Row, int Column)
        {
            return this.SetGrid(Row, Row, Column, Column);
        }
        public Widget SetGrid(int RowStart, int RowEnd, int ColumnStart, int ColumnEnd)
        {
            this.GridRowStart = RowStart;
            this.GridRowEnd = RowEnd;
            this.GridColumnStart = ColumnStart;
            this.GridColumnEnd = ColumnEnd;
            this.UpdateLayout();
            return this;
        }

        public void UpdateLayout()
        {
            if (this.Parent is ILayout)
            {
                (this.Parent as ILayout).NeedUpdate = true;
            }
        }
    }
}
