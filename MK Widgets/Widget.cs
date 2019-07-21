using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class Widget : IDisposable, IContainer
    {
        public static Bitmap IconSheet;

        public static void Setup()
        {
            IconSheet = new Bitmap("icons.png");
        }


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
        public bool                         Visible          { get; protected set; } = true;
        public bool                         Selected         = false;
        public bool                         Dock             = false;
        public int                          ZIndex           { get; protected set; } = 0;
        public List<IMenuItem>              ContextMenuList  { get; protected set; }
        public bool                         ShowContextMenu  { get; protected set; } = false;
        public List<Shortcut>               Shortcuts        { get; protected set; } = new List<Shortcut>();

        public  bool   AutoScroll        = false;
        public  bool   ShowScrollBars    = false;
        public  int    ScrolledX         { get; set; } = 0;
        public  int    ScrolledY         { get; set; } = 0;
        public  Point  ScrolledPosition
        {
            get
            {
                return new Point(
                    this.Position.X - Parent.ScrolledX,
                    this.Position.Y - Parent.ScrolledY
                );
            }
        }
        public  int    MaxChildWidth     = 0;
        public  int    MaxChildHeight    = 0;

        public Margin Margin          { get; protected set; } = new Margin();
        public int    GridRowStart    = 0;
        public int    GridRowEnd      = 0;
        public int    GridColumnStart = 0;
        public int    GridColumnEnd   = 0;

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
        public EventHandler<EventArgs> OnPositionChanged;
        public EventHandler<SizeEventArgs> OnSizeChanged;
        public EventHandler<SizeEventArgs> OnParentSizeChanged;
        public EventHandler<SizeEventArgs> OnChildBoundsChanged;
        public EventHandler<EventArgs> OnDisposed;

        public MinimalHScrollBar ScrollBarX { get; protected set; }
        public MinimalVScrollBar ScrollBarY { get; protected set; }

        public Widget(object Parent, string Name = "widget", int Index = -1)
        {
            if (Parent is ILayout && !(this is LayoutContainer))
            {
                if (Parent is VStackPanel && Index != -1) (Parent as VStackPanel).Insert(Index, this);
                else (Parent as Widget).Add(this);
            }
            else
            {
                this.SetParent(Parent, Index);
                this.Viewport = new Viewport(this.Window.Renderer, 0, 0, this.Size);
                this.Viewport.Z = this.Parent.Viewport.Z;
            }
            this.Name = this.Parent.GetName(Name);
            this.Sprites["_bg"] = new Sprite(this.Viewport);
            this.Sprites["_bg"].Bitmap = new SolidBitmap(this.Size, this.BackgroundColor);
            this.Sprites["_bg"].Z = -999999999;
            this.OnLeftClick = new EventHandler<MouseEventArgs>(this.LeftClick);
            this.OnMouseMoving = new EventHandler<MouseEventArgs>(this.MouseMoving);
            this.OnSelected = new EventHandler<EventArgs>(this.WidgetSelected);
            this.OnDeselected = new EventHandler<EventArgs>(this.WidgetDeselected);
            this.OnTextInput = new EventHandler<TextInputEventArgs>(this.TextInput);
            this.OnPositionChanged = new EventHandler<EventArgs>(this.PositionChanged);
            this.OnSizeChanged = new EventHandler<SizeEventArgs>(this.SizeChanged);
            this.OnParentSizeChanged = new EventHandler<SizeEventArgs>(this.ParentSizeChanged);
            this.OnChildBoundsChanged = new EventHandler<SizeEventArgs>(this.ChildBoundsChanged);
            this.WidgetIM = new MouseInputManager(this);
            this.WidgetIM.OnRightClick += RightClick_ContextMenu;
        }

        public void SetParent(object Parent, int Index = -1)
        {
            if (this.Parent != null) this.Parent.Remove(this);
            if (Parent is WidgetWindow)
            {
                this.Window = Parent as WidgetWindow;
                this.Parent = this.Window.UI;
            }
            else if (Parent is UIManager)
            {
                this.Window = (Parent as UIManager).Window;
                this.Parent = Parent as UIManager;
            }
            else if (Parent is Widget)
            {
                this.Window = (Parent as Widget).Window;
                this.Parent = Parent as IContainer;
            }
            if (Index != -1 && this.Parent is VStackPanel) (this.Parent as VStackPanel).Insert(Index, this);
            else this.Parent.Add(this);
        }

        public void SetVisible(bool Visible)
        {
            if (this.Visible != Visible)
            {
                this.Visible = Visible;
                this.Viewport.Visible = Visible;
                this.Widgets.ForEach(w => w.SetVisible(Visible));
            }
        }

        public bool IsVisible()
        {
            if (!this.Visible) return false;
            if (Parent is Widget) return (Parent as Widget).IsVisible();
            return true;
        }

        public void SetZIndex(int ZIndex)
        {
            if (this.ZIndex != ZIndex)
            {
                this.ZIndex = ZIndex;
                this.Viewport.Z = this.Parent.Viewport.Z + this.ZIndex;
            }
        }

        public void SetContextMenuList(List<IMenuItem> Items)
        {
            this.ContextMenuList = Items;
            this.ShowContextMenu = Items.Count > 0;
        }

        public void RegisterShortcuts(List<Shortcut> Shortcuts)
        {
            foreach (Shortcut s in this.Shortcuts)
            {
                if (s.GlobalShortcut) this.Window.UI.DeregisterShortcut(s);
            }
            this.Shortcuts = Shortcuts;
            foreach (Shortcut s in this.Shortcuts)
            {
                if (s.GlobalShortcut) this.Window.UI.RegisterShortcut(s);
            }
        }

        public virtual void Dispose()
        {
            AssertUndisposed();
            this.Disposed = true;
            this.Viewport.Dispose();
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                if (this.Widgets[i] != null)
                {
                    this.Widgets[i].Dispose();
                    i--;
                }
            }
            this.Parent.Widgets.Remove(this);
            this.Viewport = null;
            this.Sprites = null;
            if (this.OnDisposed != null) this.OnDisposed.Invoke(this, new EventArgs());
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

            if (this.Selected)
            {
                foreach (Shortcut s in this.Shortcuts)
                {
                    if (s.GlobalShortcut) continue; // Handled by the UIManager

                    Key k = s.Key;
                    bool Valid = Input.Trigger((SDL2.SDL.SDL_Keycode) k.MainKey);
                    if (!Valid) continue;

                    // Modifiers
                    foreach (Keycode mod in k.Modifiers)
                    {
                        bool onefound = false;
                        List<SDL2.SDL.SDL_Keycode> codes = new List<SDL2.SDL.SDL_Keycode>();
                        if (mod == Keycode.CTRL) { codes.Add(SDL2.SDL.SDL_Keycode.SDLK_LCTRL); codes.Add(SDL2.SDL.SDL_Keycode.SDLK_RCTRL); }
                        else if (mod == Keycode.SHIFT) { codes.Add(SDL2.SDL.SDL_Keycode.SDLK_LSHIFT); codes.Add(SDL2.SDL.SDL_Keycode.SDLK_RSHIFT); }
                        else if (mod == Keycode.ALT) { codes.Add(SDL2.SDL.SDL_Keycode.SDLK_LALT); codes.Add(SDL2.SDL.SDL_Keycode.SDLK_RALT); }
                        else codes.Add((SDL2.SDL.SDL_Keycode) mod);

                        for (int i = 0; i < codes.Count; i++)
                        {
                            if (Input.Press(codes[i]))
                            {
                                onefound = true;
                                break;
                            }
                        }

                        if (!onefound)
                        {
                            Valid = false;
                            break;
                        }
                    }

                    if (!Valid) continue;

                    s.Event.Invoke(this, new EventArgs());
                }
            }

            this.WidgetIM.Update(this.Viewport.Rect);
            if (!this.Drawn) this.Draw();
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                this.Widgets[i].Update();
            }
        }

        private void RightClick_ContextMenu(object sender, MouseEventArgs e)
        {
            if (ShowContextMenu && ContextMenuList != null && ContextMenuList.Count > 0)
            {
                ContextMenu cm = new ContextMenu(this.Window);
                cm.SetItems(ContextMenuList);
                Size s = cm.Size;
                int x = e.X;
                int y = e.Y;
                if (e.X + s.Width >= Window.Width) x -= s.Width;
                if (e.Y + s.Height >= Window.Height) y -= s.Height;
                x = Math.Max(0, x);
                y = Math.Max(0, y);
                cm.SetPosition(x, y);
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
            if (e.LeftButton != e.OldLeftButton && this.WidgetIM.ClickedLeftInArea == true) Redraw();
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
        public virtual void PositionChanged(object sender, EventArgs e)
        {
            UpdateAutoScroll();
        }
        public virtual void SizeChanged(object sender, SizeEventArgs e)
        {
            // As size is recalculated here, we first have to make sure the sliders are in
            // the correct position, otherwise its old position will exceed the old container size
            // (because it's directly on the edge)
            if (ScrollBarX != null) ScrollBarX.SetPosition(2, this.Size.Height - 13);
            if (ScrollBarY != null) ScrollBarY.SetPosition(this.Size.Width - 13, 2);
            // Now that they're in the proper place, calculate for the remaining child widgets
            UpdateAutoScroll();
            UpdateLayout();
            for (int i = 0; i < this.Widgets.Count;i ++)
            {
                Widget w = this.Widgets[i];
                // Docking; to do
                // if (!(w is AutoVScrollBar) && w.Dock) w.SetSize(this.Size);
            }
        }
        public virtual void ParentSizeChanged(object sender, SizeEventArgs e) { }
        public virtual void ChildBoundsChanged(object sender, SizeEventArgs e)
        {
            UpdateAutoScroll();
        }

        public void UpdateAutoScroll()
        {
            if (!AutoScroll) return;
            // Calculate total child width
            int OldMaxChildWidth = MaxChildWidth;
            MaxChildWidth = 0;
            this.Widgets.ForEach(wdgt =>
            {
                if (!wdgt.Visible || wdgt is AutoVScrollBar || wdgt is AutoHScrollBar) return;
                int w = wdgt.Size.Width;
                if (wdgt.Parent is LayoutContainer) w += (wdgt.Parent as LayoutContainer).Position.X;
                else w += wdgt.Position.X;
                if (w > MaxChildWidth) MaxChildWidth = w;
            });
            // Calculate total child height
            int OldMaxChildHeight = MaxChildHeight;
            MaxChildHeight = 0;
            this.Widgets.ForEach(w =>
            {
                if (!w.Visible || w is AutoVScrollBar || w is AutoHScrollBar) return;
                int h = w.Size.Height;
                if (w.Parent is LayoutContainer) h += (w.Parent as LayoutContainer).Position.Y;
                else h += w.Position.Y;
                if (h > MaxChildHeight) MaxChildHeight = h;
            });
            // ScrollBarX
            if (MaxChildWidth > this.Size.Width)
            {
                if (ScrollBarX == null)
                {
                    ScrollBarX = new AutoHScrollBar(this);
                    ScrollBarX.SetZIndex(999);
                }
                ScrollBarX.SetPosition(0, this.Size.Height - 17);
                // 17 if ScrollBarY is present, 0 if not.
                int extra = (MaxChildHeight > this.Size.Height) ? 17 : 0;
                ScrollBarX.SetSize(this.Size.Width - 4 - extra, 11);
                if (OldMaxChildWidth - this.Viewport.Width > 0 && this.ScrolledX > OldMaxChildWidth - this.Viewport.Width)
                {
                    this.ScrolledX = OldMaxChildWidth - this.Viewport.Width;
                }
                ScrollBarX.SetValue((double) this.ScrolledX / (MaxChildWidth - this.Viewport.Width));
                ScrollBarX.SetSliderSize((double) this.Viewport.Width / MaxChildWidth);
                ScrollBarX.MouseInputRect = this.Viewport.Rect;
            }
            else if (ScrollBarX != null)
            {
                this.ScrollBarX.Dispose();
                this.ScrollBarX = null;
                this.ScrolledX = 0;
            }
            // ScrollBarY
            if (MaxChildHeight > this.Size.Height || ShowScrollBars)
            {
                bool ActuallyVisible = MaxChildHeight > this.Size.Height;
                if (ScrollBarY == null)
                {
                    ScrollBarY = new AutoVScrollBar(this);
                    ScrollBarY.SetZIndex(999);
                }
                ScrollBarY.SetPosition(this.Size.Width - 17, 0);
                // 17 if ScrollBarX is present, 0 if not.
                int extra = (MaxChildWidth > this.Size.Width) ? 17 : 0;
                ScrollBarY.SetSize(11, this.Size.Height - extra);
                if (ActuallyVisible)
                {
                    if (OldMaxChildHeight - this.Viewport.Height > 0 && this.ScrolledY > OldMaxChildHeight - this.Viewport.Height)
                    {
                        this.ScrolledY = OldMaxChildHeight - this.Viewport.Height;
                    }
                    if (this.ScrolledY > MaxChildHeight - this.Viewport.Height)
                    {
                        this.ScrolledY = MaxChildHeight - this.Viewport.Height;
                    }
                }
                ScrollBarY.SetValue((double) this.ScrolledY / (MaxChildHeight - this.Viewport.Height));
                ScrollBarY.SetSliderSize((double) this.Viewport.Height / MaxChildHeight);
                ScrollBarY.MouseInputRect = this.Viewport.Rect;
                ScrollBarY.SetSliderVisible(ActuallyVisible);
            }
            else if (ScrollBarY != null)
            {
                this.ScrollBarY.Dispose();
                this.ScrollBarY = null;
                this.ScrolledY = 0;
            }
            // Update positions
            this.UpdateBounds();
        }

        public void UpdateBounds()
        {
            AssertUndisposed();
            foreach (ISprite s in this.Sprites.Values)
            {
                if (s is MultiSprite) foreach (Sprite ms in (s as MultiSprite).SpriteList.Values) ms.OX = ms.OY = 0;
                else s.OX = s.OY = 0;
            }

            int ScrolledX = this.Position.X - this.ScrolledPosition.X;
            int ScrolledY = this.Position.Y - this.ScrolledPosition.Y;
            if (this is AutoHScrollBar || this is AutoVScrollBar) ScrolledX = ScrolledY = 0;

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
            this.OnPositionChanged.Invoke(this, new EventArgs());
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
                this.Sprites["_bg"].Bitmap.Unlock();
                (this.Sprites["_bg"].Bitmap as SolidBitmap).SetSize(this.Size);
                this.Sprites["_bg"].Bitmap.Lock();
                this.Viewport.Width = this.Size.Width;
                this.Viewport.Height = this.Size.Height;
                this.RedrawSize = true;
                this.UpdateBounds();
                this.OnSizeChanged.Invoke(this, new SizeEventArgs(this.Size, oldsize));
                this.Widgets.ForEach(w =>
                {
                    Widget wdgt = w;
                    if (wdgt is LayoutContainer) wdgt = (w as LayoutContainer).Widget;
                    wdgt.OnParentSizeChanged.Invoke(this, new SizeEventArgs(this.Size, oldsize));
                    wdgt.OnSizeChanged.Invoke(this, new SizeEventArgs(this.Size));
                });
                Redraw();
                if (this.Parent is Widget && !(this is AutoHScrollBar) && !(this is AutoVScrollBar))
                {
                    Widget prnt = this.Parent as Widget;
                    prnt.OnChildBoundsChanged.Invoke(this, new SizeEventArgs(this.Size, oldsize));
                }
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
            this.Sprites["_bg"].Bitmap.Unlock();
            (this.Sprites["_bg"].Bitmap as SolidBitmap).SetColor(c);
            this.Sprites["_bg"].Bitmap.Lock();
            return this;
        }

        public virtual void Add(Widget w)
        {
            if (this.Widgets.Exists(wgt => wgt.Name == w.Name))
            {
                throw new Exception("Already existing widget by the name of '" + w.Name + "'");
            }
            this.Widgets.Add(w);
        }

        public virtual Widget Get(string Name)
        {
            foreach (Widget w in this.Widgets)
            {
                if (w is LayoutContainer)
                {
                    if ((w as LayoutContainer).Widget.Name == Name) return w;
                }
                else
                {
                    if (w.Name == Name) return w;
                }
            }
            return null;
        }

        public virtual Widget Remove(Widget w)
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

    public class Shortcut
    {
        public Key Key;
        public EventHandler<EventArgs> Event;
        public bool GlobalShortcut = false;

        public Shortcut(Key Key, EventHandler<EventArgs> Event, bool GlobalShortcut = false)
        {
            this.Key = Key;
            this.Event = Event;
            this.GlobalShortcut = GlobalShortcut;
        }
    }

    public enum Icon
    {
        NONE,

        Plus = '\uf067',
        New = Plus,
        Add = Plus,

        Eye = '\uf06e',
        Visible = Eye,

        TrashCan = '\uf2ed',
        Delete = TrashCan,
        Remove = TrashCan,

        Down = '\uf063',
        Up = '\uf062',

        Eraser = '\uf12d'
    }
}
