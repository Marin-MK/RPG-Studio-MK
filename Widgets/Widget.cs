using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class Widget : IDisposable, IContainer
    {
        /// <summary>
        /// The unique name of the widget by which it is stored and referenced in its parent widget.
        /// </summary>
        public string Name;

        /// <summary>
        /// The viewport of this widget. Influenced by position, size, parent position and size, scroll values, etc.
        /// </summary>
        public Viewport Viewport { get; set; }

        /// <summary>
        /// Full size of this widget. Can be smaller if viewport exceeds parent container, but never bigger.
        /// </summary>
        public Size Size { get; protected set; } = new Size(50, 50);

        /// <summary>
        /// Relative position to parent container.
        /// </summary>
        public Point Position { get; protected set; } = new Point(0, 0);

        /// <summary>
        /// Main window associated with this widget.
        /// </summary>
        public MainEditorWindow Window { get; protected set; }

        /// <summary>
        /// Whether or not the widget should automatically resize based on its children's positions and sizes.
        /// </summary>
        public bool AutoResize = false;

        /// <summary>
        /// Minimum possible size for this widget.
        /// </summary>
        public Size MinimumSize { get; protected set; } = new Size(1, 1);

        /// <summary>
        /// Maximum possible size for this widget.
        /// </summary>
        public Size MaximumSize { get; protected set; } = new Size(9999, 9999);

        /// <summary>
        /// Background color of this widget.
        /// </summary>
        public Color BackgroundColor { get; protected set; } = new Color(255, 255, 255, 0);

        /// <summary>
        /// The list of sprites that create the graphics of this widget. All sprites MUST be stored in here to be properly displayed.
        /// </summary>
        public Dictionary<string, ISprite> Sprites { get; protected set; } = new Dictionary<string, ISprite>();

        /// <summary>
        /// Children widgets of this widget.
        /// </summary>
        public List<Widget> Widgets { get; protected set; } = new List<Widget>();

        /// <summary>
        /// This object aids in fetching mouse input.
        /// </summary>
        public MouseInputManager WidgetIM { get; protected set; }

        /// <summary>
        /// The parent of this widget.
        /// </summary>
        public IContainer Parent { get; set; }

        /// <summary>
        /// Whether or not this widget has been disposed.
        /// </summary>
        public bool Disposed { get; protected set; } = false;

        /// <summary>
        /// Used for determining the viewport boundaries.
        /// </summary>
        public Point AdjustedPosition { get; protected set; } = new Point(0, 0);

        /// <summary>
        /// Whether or not this widget itself is visible. Actual visibility may vary based on parent visibility.
        /// </summary>
        public bool Visible { get; protected set; } = true;

        /// <summary>
        /// Whether or not this widget is the currently active and selected widget.
        /// </summary>
        public bool SelectedWidget = false;

        private int _ZIndex = 0;
        /// <summary>
        /// Relative Z Index of this widget and its viewport. Actual Z Index may vary based on parent Z Index.
        /// </summary>
        public int ZIndex
        {
            get
            {
                if (Parent is UIManager) return _ZIndex;
                return (Parent as Widget).ZIndex + _ZIndex;
            }
            protected set { _ZIndex = value; }
        }

        /// <summary>
        /// The list of right-click menu options to show when this widget is right-clicked.
        /// </summary>
        public List<IMenuItem> ContextMenuList { get; protected set; }

        /// <summary>
        /// Whether or not to show the context menu if this widget is right-clicked.
        /// </summary>
        public bool ShowContextMenu { get; protected set; } = false;

        /// <summary>
        /// The list of keyboard shortcuts associated with this widget. Can be global shortcuts.
        /// </summary>
        public List<Shortcut> Shortcuts { get; protected set; } = new List<Shortcut>();

        /// <summary>
        /// Whether or not this widget should be considered when determining scrollbar size and position for autoscroll.
        /// </summary>
        public bool ConsiderInAutoScroll = true;

        private int _WindowLayer = 0;
        /// <summary>
        /// Which pseudo-window or layer this widget is on.
        /// </summary>
        public int WindowLayer
        {
            get
            {
                return Parent.WindowLayer > _WindowLayer ? Parent.WindowLayer : _WindowLayer;
            }
            set
            {
                _WindowLayer = value;
            }
        }

        /// <summary>
        /// Whether or not this widget should scroll horizontally if its children exceeds this widget's boundaries.
        /// </summary>
        public bool HAutoScroll = false;

        /// <summary>
        /// Whether or not this widget should scroll vertically if its children exceeds this widget's boundaries.
        /// </summary>
        public bool VAutoScroll = false;

        /// <summary>
        /// How far this widget has scrolled horizontally with autoscroll.
        /// </summary>
        public int ScrolledX { get; set; } = 0;

        /// <summary>
        /// How far this widget has scrolled horizontally with autoscroll.
        /// </summary>
        public int ScrolledY { get; set; } = 0;

        /// <summary>
        /// Relative position of this widget including scroll values.
        /// </summary>
        public Point ScrolledPosition
        {
            get
            {
                return new Point(
                    this.Position.X - Parent.ScrolledX,
                    this.Position.Y - Parent.ScrolledY
                );
            }
        }

        /// <summary>
        /// The total width occupied by this widget's children.
        /// </summary>
        public int MaxChildWidth = 0;

        /// <summary>
        /// The total height occupied by this widget's children.
        /// </summary>
        public int MaxChildHeight = 0;

        /// <summary>
        /// The ScrollBar for scrolling horizontally.
        /// </summary>
        public HScrollBar HScrollBar { get; protected set; }

        /// <summary>
        /// The ScrollBar for scrolling vertically.
        /// </summary>
        public VScrollBar VScrollBar { get; protected set; }

        /// <summary>
        /// The margin to use for grids and stackpanels.
        /// </summary>
        public Margin Margin { get; protected set; } = new Margin();

        /// <summary>
        /// Which grid row this widget starts in.
        /// </summary>
        public int GridRowStart = 0;

        /// <summary>
        /// Which grid row this widget ends in.
        /// </summary>
        public int GridRowEnd = 0;

        /// <summary>
        /// Which grid column this widget starts in.
        /// </summary>
        public int GridColumnStart = 0;

        /// <summary>
        /// Which grid column this widget ends in.
        /// </summary>
        public int GridColumnEnd = 0;

        /// <summary>
        /// A list of timers used for time-sensitive events
        /// </summary>
        public List<Timer> Timers { get; protected set; } = new List<Timer>();

        /// <summary>
        /// Whether or not to redraw this widget.
        /// </summary>
        protected bool Drawn = false;
        
        /// <summary>
        /// Called whenever this widget is left clicked.
        /// </summary>
        public EventHandler<MouseEventArgs> OnLeftClick;

        /// <summary>
        /// Called once whenever a new mouse button is pressed.
        /// </summary>
        public EventHandler<MouseEventArgs> OnMouseDown;

        /// <summary>
        /// Called while a mouse button is being held down.
        /// </summary>
        public EventHandler<MouseEventArgs> OnMousePress;

        /// <summary>
        /// Called once whenever a mouse button is released.
        /// </summary>
        public EventHandler<MouseEventArgs> OnMouseUp;

        /// <summary>
        /// Called once per mouse wheel scroll.
        /// </summary>
        public EventHandler<MouseEventArgs> OnMouseWheel;

        /// <summary>
        /// Called while the mouse is moving.
        /// </summary>
        public EventHandler<MouseEventArgs> OnMouseMoving;

        /// <summary>
        /// Called when this widget becomes the active widget.
        /// </summary>
        public EventHandler<MouseEventArgs> OnWidgetSelected;

        /// <summary>
        /// Called when this widget is no longer the active widget.
        /// </summary>
        public EventHandler<EventArgs> OnWidgetDeselected;

        /// <summary>
        /// Called when a button is being is pressed.
        /// </summary>
        public EventHandler<TextInputEventArgs> OnTextInput;

        /// <summary>
        /// Called when this widget's relative position changes.
        /// </summary>
        public EventHandler<EventArgs> OnPositionChanged;

        /// <summary>
        /// Called when this widget's relative size changes.
        /// </summary>
        public EventHandler<SizeEventArgs> OnSizeChanged;

        /// <summary>
        /// Called when this widget's parent relative size changes.
        /// </summary>
        public EventHandler<SizeEventArgs> OnParentSizeChanged;

        /// <summary>
        /// Called when a child's relative size changes.
        /// </summary>
        public EventHandler<SizeEventArgs> OnChildBoundsChanged;

        /// <summary>
        /// Called before this widget is disposed.
        /// </summary>
        public EventHandler<EventArgs> OnDisposing;

        /// <summary>
        /// Called after this widget is disposed.
        /// </summary>
        public EventHandler<EventArgs> OnDisposed;

        /// <summary>
        /// Called when the autoscroll scrollbars are being scrolled.
        /// </summary>
        public EventHandler<EventArgs> OnScrolling;

        /// <summary>
        /// Called before the right-click menu would open. Is cancellable.
        /// </summary>
        public EventHandler<CancelEventArgs> OnContextMenuOpening;

        /// <summary>
        /// Creates a new Widget object.
        /// </summary>
        /// <param name="Parent">The Parent widget.</param>
        /// <param name="Name">The unique name to give this widget by which to store it.</param>
        /// <param name="Index">Optional index parameter. Used internally for stackpanels.</param>
        public Widget(object Parent, string Name = "widget", int Index = -1)
        {
            // Children of ILayout widgets (Grid, StackPanel) aren't added directly, but rather get a LayoutContainer as a parent and copy their viewport.
            if (Parent is ILayout && !(this is LayoutContainer))
            {
                if (Parent is VStackPanel && Index != -1) (Parent as VStackPanel).Insert(Index, this);
                else (Parent as Widget).Add(this);
            }
            else
            {
                this.SetParent(Parent, Index);
                // Create new viewport directly on the window's renderer.
                this.Viewport = new Viewport(this.Window.Renderer, 0, 0, this.Size);
                // Z index by default copies parent viewport's z. If changed later, SetZIndex will modify the value.
                this.Viewport.Z = this.ZIndex;
                if (this.Parent is Widget) this.Viewport.Visible = (this.Parent as Widget).IsVisible() ? this.Visible : false;
            }
            // Ensures this name is unique by adding an integer at the end.
            this.Name = this.Parent.GetName(Name);
            // The background sprite responsible for the BackgroundColor.
            this.Sprites["_bg"] = new Sprite(this.Viewport);
            this.Sprites["_bg"].Bitmap = new SolidBitmap(this.Size, this.BackgroundColor);
            // In the same viewport as all other sprites, but at a very large negative Z index.
            this.Sprites["_bg"].Z = -999999999;
            // Set some default events.
            this.OnLeftClick = new EventHandler<MouseEventArgs>(this.LeftClick);
            this.OnMouseMoving = new EventHandler<MouseEventArgs>(this.MouseMoving);
            this.OnWidgetDeselected = new EventHandler<EventArgs>(this.WidgetDeselected);
            this.OnTextInput = new EventHandler<TextInputEventArgs>(this.TextInput);
            this.OnPositionChanged = new EventHandler<EventArgs>(this.PositionChanged);
            this.OnSizeChanged = new EventHandler<SizeEventArgs>(this.SizeChanged);
            this.OnParentSizeChanged = new EventHandler<SizeEventArgs>(this.ParentSizeChanged);
            this.OnChildBoundsChanged = new EventHandler<SizeEventArgs>(this.ChildBoundsChanged);
            // Creates the input manager object responsible for fetching mouse input.
            this.WidgetIM = new MouseInputManager(this);
            this.WidgetIM.OnRightClick += RightClick_ContextMenu;
        }

        /// <summary>
        /// Initializes the list of right-click menu options.
        /// </summary>
        /// <param name="Items">The list of menu items.</param>
        public virtual void SetContextMenuList(List<IMenuItem> Items)
        {
            AssertUndisposed();
            this.ContextMenuList = Items;
            this.ShowContextMenu = Items.Count > 0;
        }

        /// <summary>
        /// Initializes the list of shortcuts.
        /// </summary>
        /// <param name="Shortcuts">The list of shortcuts.</param>
        public void RegisterShortcuts(List<Shortcut> Shortcuts)
        {
            AssertUndisposed();
            // De-register old global shortcuts in the UIManager object.
            foreach (Shortcut s in this.Shortcuts)
            {
                if (s.GlobalShortcut) this.Window.UI.DeregisterShortcut(s);
            }
            this.Shortcuts = Shortcuts;
            // Register global shortcuts in the UIManager object.
            foreach (Shortcut s in this.Shortcuts)
            {
                if (s.GlobalShortcut) this.Window.UI.RegisterShortcut(s);
            }
        }

        /// <summary>
        /// Sets the Z Index of this widget and viewport.
        /// </summary>
        /// <param name="ZIndex">The new Z Index.</param>
        public void SetZIndex(int ZIndex)
        {
            AssertUndisposed();
            // Only update if value changed.
            if (this.ZIndex != ZIndex)
            {
                this.ZIndex = ZIndex;
                // this.ZIndex takes parent Z Index into account.
                this.Viewport.Z = this.ZIndex;
            }
        }

        /// <summary>
        /// Registers this widget under the parent widget.
        /// </summary>
        /// <param name="Parent">The Parent widget.</param>
        /// <param name="Index">Optional index for stackpanel parents.</param>
        public void SetParent(object Parent, int Index = -1)
        {
            AssertUndisposed();
            // De-registers this widget from former parent if present.
            if (this.Parent != null) this.Parent.Remove(this);
            // MainEditorWindow isn't a widget, instead use its UI (UIManager) field.
            if (Parent is MainEditorWindow)
            {
                this.Window = Parent as MainEditorWindow;
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
            // Insert widget at specific index for stackpanels
            if (Index != -1 && this.Parent is VStackPanel) (this.Parent as VStackPanel).Insert(Index, this);
            // Or add to the end of the list otherwise
            else this.Parent.Add(this);
            if (this.Viewport != null)
            {
                this.Viewport.Z = this.ZIndex;
                this.Viewport.Visible = (this.Parent as Widget).IsVisible() ? this.Visible : false;
            }
        }

        /// <summary>
        /// Returns the pseudo-parent window. PopupWindow objects are Widgets, but seen as "Pseudo-windows".
        /// </summary>
        public object GetParentWindow()
        {
            AssertUndisposed();
            if (Parent is PopupWindow || Parent is MainEditorWindow) return Parent;
            else if (Parent is UIManager) return Window;
            else return (Parent as Widget).GetParentWindow();
        }

        /// <summary>
        /// Set visibility for this widget and all children.
        /// </summary>
        /// <param name="Visible">Boolean visibility value.</param>
        public void SetVisible(bool Visible)
        {
            AssertUndisposed();
            this.Visible = Visible;
            Widget parent = Parent as Widget;
            // Since LayoutContainer copies the viewport of this Widget, it will always
            // match the visibility of this widget and therefore give undesireable results.
            // Use its parent instead, which is the stackpanel or grid.
            if (parent is LayoutContainer) parent = parent.Parent as Widget;
            if ((parent as Widget).IsVisible())
            {
                Viewport.Visible = Visible;
            }
            else
            {
                Viewport.Visible = false;
            }
            SetViewportVisible(Visible);
        }

        /// <summary>
        /// Used for setting viewport visibility without changing actual visible property (used for children-parent visbility)
        /// </summary>
        /// <param name="Visible">Boolean visibilty value.</param>
        protected void SetViewportVisible(bool Visible, bool Initial = false)
        {
            Widget parent = Parent as Widget;
            // Since LayoutContainer copies the viewport of this Widget, it will always
            // match the visibility of this widget and therefore give undesireable results.
            // Use its parent instead, which is the stackpanel or grid.
            if (parent is LayoutContainer) parent = parent.Parent as Widget;
            if ((parent as Widget).IsVisible())
            {
                if (this.Visible && Viewport.Visible != Visible)
                    Viewport.Visible = Visible;
            }
            else this.Viewport.Visible = false;
            this.Widgets.ForEach(w => w.SetViewportVisible(Visible));
        }

        /// <summary>
        /// Returns whether this widget is visible.
        /// </summary>
        public bool IsVisible()
        {
            AssertUndisposed();
            return Viewport.Visible;
        }

        /// <summary>
        /// Updates the scrollbars based on children boundaries.
        /// </summary>
        public void UpdateAutoScroll()
        {
            if (!HAutoScroll && !VAutoScroll) return;
            // Calculate total child width
            int OldMaxChildWidth = MaxChildWidth;
            MaxChildWidth = 0;
            this.Widgets.ForEach(wdgt =>
            {
                if (!wdgt.Visible || !wdgt.ConsiderInAutoScroll) return;
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
                if (!w.Visible || !w.ConsiderInAutoScroll) return;
                int h = w.Size.Height;
                if (w.Parent is LayoutContainer) h += (w.Parent as LayoutContainer).Position.Y;
                else h += w.Position.Y;
                if (h > MaxChildHeight) MaxChildHeight = h;
            });
            if (AutoResize)
            {
                int w = this.Size.Width;
                if (MaxChildWidth > w) w = MaxChildWidth;
                int h = this.Size.Height;
                if (MaxChildHeight > h) h = MaxChildHeight;
                SetSize(w, h);
            }
            else
            {
                // ScrollBarX
                if (HAutoScroll)
                {
                    if (MaxChildWidth > this.Size.Width)
                    {
                        if (HScrollBar == null)
                        {
                            throw new Exception("Autoscroll was enabled, but no scrollbar has been defined.");
                        }
                        if (OldMaxChildWidth - this.Viewport.Width > 0 && this.ScrolledX > OldMaxChildWidth - this.Viewport.Width)
                        {
                            this.ScrolledX = OldMaxChildWidth - this.Viewport.Width;
                        }
                        HScrollBar.SetValue((double)this.ScrolledX / (MaxChildWidth - this.Viewport.Width));
                        HScrollBar.SetSliderSize((double)this.Viewport.Width / MaxChildWidth);
                        HScrollBar.MouseInputRect = this.Viewport.Rect;
                        HScrollBar.SetVisible(true);
                    }
                    else if (HScrollBar != null)
                    {
                        HScrollBar.SetVisible(false);
                        ScrolledX = 0;
                    }
                }
                // ScrollBarY
                if (VAutoScroll)
                {
                    if (MaxChildHeight > this.Size.Height)
                    {
                        bool ActuallyVisible = MaxChildHeight > this.Size.Height;
                        if (VScrollBar == null)
                        {
                            throw new Exception("Autoscroll was enabled, but no scrollbar has been defined.");
                        }
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
                        VScrollBar.SetValue((double)this.ScrolledY / (MaxChildHeight - this.Viewport.Height));
                        VScrollBar.SetSliderSize((double)this.Viewport.Height / MaxChildHeight);
                        VScrollBar.MouseInputRect = this.Viewport.Rect;
                        VScrollBar.SetSliderVisible(ActuallyVisible);
                        VScrollBar.SetVisible(true);
                    }
                    else if (VScrollBar != null)
                    {
                        VScrollBar.SetVisible(false);
                        ScrolledY = 0;
                    }
                }
            }
            // Update positions
            this.UpdateBounds();
        }

        /// <summary>
        /// Links the given HScrollBar with this widget.
        /// </summary>
        public void SetHScrollBar(HScrollBar hsb)
        {
            this.HScrollBar = hsb;
            hsb.MouseInputRect = this.Viewport.Rect;
            hsb.LinkedWidget = this;
        }

        /// <summary>
        /// Links the given VScrollBar with this widget.
        /// </summary>
        public void SetVScrollBar(VScrollBar vsb)
        {
            this.VScrollBar = vsb;
            vsb.MouseInputRect = this.Viewport.Rect;
            vsb.LinkedWidget = this;
        }

        /// <summary>
        /// Updates this viewport's boundaries if it exceeds the parent viewport's boundaries and applies scrolling.
        /// </summary>
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
            //if (this is HScrollBar || this is VScrollBar) ScrolledX = ScrolledY = 0;

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
            foreach (Widget w in this.Widgets)
            {
                w.UpdateBounds();
            }
        }

        /// <summary>
        /// Changes the relative position of this widget.
        /// </summary>
        public void SetPosition(int X, int Y)
        {
            this.SetPosition(new Point(X, Y));
        }
        /// <summary>
        /// Changes the relative position of this widget.
        /// </summary>
        public virtual void SetPosition(Point p)
        {
            AssertUndisposed();
            this.Position = p;
            // Update the viewport boundaries
            UpdateBounds();
            this.OnPositionChanged.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Sets the width of this widget.
        /// </summary>
        public void SetWidth(int Width)
        {
            this.SetSize(Width, this.Size.Height);
        }
        /// <summary>
        /// Sets the height of this widget.
        /// </summary>
        public void SetHeight(int Height)
        {
            this.SetSize(this.Size.Width, Height);
        }
        /// <summary>
        /// Sets the size of this widget.
        /// </summary>
        public void SetSize(int Width, int Height)
        {
            this.SetSize(new Size(Width, Height));
        }
        /// <summary>
        /// Sets the size of this widget.
        /// </summary>
        public virtual void SetSize(Size size)
        {
            AssertUndisposed();
            Size oldsize = this.Size;
            // Ensures the new size doesn't exceed the set minimum and maximum values.
            if (size.Width < MinimumSize.Width) size.Width = MinimumSize.Width;
            else if (size.Width > MaximumSize.Width) size.Width = MaximumSize.Width;
            if (size.Height < MinimumSize.Height) size.Height = MinimumSize.Height;
            else if (size.Height > MaximumSize.Height) size.Height = MaximumSize.Height;
            if (oldsize.Width != size.Width || oldsize.Height != size.Height)
            {
                this.Size = size;
                // Update the background sprite's size
                (this.Sprites["_bg"].Bitmap as SolidBitmap).SetSize(this.Size);
                this.Viewport.Width = this.Size.Width;
                this.Viewport.Height = this.Size.Height;
                // Updates the viewport boundaries
                this.UpdateBounds();
                // Executes all events associated with resizing a widget.
                this.OnSizeChanged.Invoke(this, new SizeEventArgs(this.Size, oldsize));
                this.Widgets.ForEach(w =>
                {
                    Widget wdgt = w;
                    if (wdgt is LayoutContainer) wdgt = (w as LayoutContainer).Widget;
                    wdgt.OnParentSizeChanged.Invoke(this, new SizeEventArgs(this.Size, oldsize));
                    wdgt.OnSizeChanged.Invoke(this, new SizeEventArgs(this.Size));
                });
                Redraw();
                if (this.Parent is Widget && !(this is HScrollBar) && !(this is VScrollBar))
                {
                    Widget prnt = this.Parent as Widget;
                    prnt.OnChildBoundsChanged.Invoke(this, new SizeEventArgs(this.Size, oldsize));
                }
            }
        }

        /// <summary>
        /// Sets the background color of this widget.
        /// </summary>
        public void SetBackgroundColor(byte r, byte g, byte b, byte a = 255)
        {
            this.SetBackgroundColor(new Color(r, g, b, a));
        }
        /// <summary>
        /// Sets the background color of this widget.
        /// </summary>
        public void SetBackgroundColor(Color c)
        {
            AssertUndisposed();
            this.BackgroundColor = c;
            (this.Sprites["_bg"].Bitmap as SolidBitmap).SetColor(c);
        }

        /// <summary>
        /// Sets a timer.
        /// </summary>
        /// <param name="identifier">Unique string identifier.</param>
        /// <param name="milliseconds">Number of milliseconds to run the timer for.</param>
        public void SetTimer(string identifier, long milliseconds)
        {
            Timers.Add(new Timer(identifier, DateTime.Now.Ticks, 10000 * milliseconds));
        }

        /// <summary>
        /// Returns whether or not the specified timer's time has elapsed.
        /// </summary>
        public bool TimerPassed(string identifier)
        {
            Timer t = Timers.Find(timer => timer.Identifier == identifier);
            if (t == null) return false;
            return DateTime.Now.Ticks >= t.StartTime + t.Timespan;
        }

        /// <summary>
        /// Returns whether or not the specified timer exists.
        /// </summary>
        public bool TimerExists(string identifier)
        {
            return Timers.Exists(t => t.Identifier == identifier);
        }

        /// <summary>
        /// Destroys the specified timer object.
        /// </summary>
        public void DestroyTimer(string identifier)
        {
            Timer t = Timers.Find(timer => timer.Identifier == identifier);
            if (t == null) throw new Exception("No timer by the identifier of '" + identifier + "' was found.");
            Timers.Remove(t);
        }

        /// <summary>
        /// Resets the specified timer with the former timespan.
        /// </summary>
        public void ResetTimer(string identifier)
        {
            Timer t = Timers.Find(timer => timer.Identifier == identifier);
            if (t == null) throw new Exception("No timer by the identifier of '" + identifier + "' was found.");
            t.StartTime = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Adds a Widget object to this widget.
        /// </summary>
        public virtual void Add(Widget w)
        {
            if (this.Widgets.Exists(wgt => wgt.Name == w.Name))
            {
                throw new Exception("Already existing widget by the name of '" + w.Name + "'");
            }
            this.Widgets.Add(w);
        }

        /// <summary>
        /// Fetches a child widget by name.
        /// </summary>
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

        /// <summary>
        /// Removes a child widget and returns the deregistered widget.
        /// </summary>
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

        /// <summary>
        /// Finds a unique name for the given string by adding an integer to the end.
        /// </summary>
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

        /// <summary>
        /// Sets the margin for this widget. Used for grids and stackpanels.
        /// </summary>
        public void SetMargin(int all)
        {
            this.SetMargin(all, all, all, all);
        }
        /// <summary>
        /// Sets the margin for this widget. Used for grids and stackpanels.
        /// </summary>
        public void SetMargin(int horizontal, int vertical)
        {
            this.SetMargin(horizontal, vertical, horizontal, vertical);
        }
        /// <summary>
        /// Sets the margin for this widget. Used for grids and stackpanels.
        /// </summary>
        public void SetMargin(int left, int up, int right, int down)
        {
            this.Margin = new Margin(left, up, right, down);
            this.UpdateLayout();
        }

        /// <summary>
        /// Sets the grid row this widget is in. Used for grids.
        /// </summary>
        /// <param name="Row"></param>
        /// <returns></returns>
        public void SetGridRow(int Row)
        {
            this.SetGridRow(Row, Row);
        }
        /// <summary>
        /// Sets the grid row this widget starts and ends in. Used for grids.
        /// </summary>
        /// <param name="RowStart"></param>
        /// <param name="RowEnd"></param>
        /// <returns></returns>
        public void SetGridRow(int RowStart, int RowEnd)
        {
            this.GridRowStart = RowStart;
            this.GridRowEnd = RowEnd;
            this.UpdateLayout();
        }

        /// <summary>
        /// Sets the grid column this widget is in. Used for grids.
        /// </summary>
        /// <param name="Column"></param>
        /// <returns></returns>
        public void SetGridColumn(int Column)
        {
            this.SetGridColumn(Column, Column);
        }
        /// <summary>
        /// Sets the grid column this widget starts and ends in. Used for grids.
        /// </summary>
        public void SetGridColumn(int ColumnStart, int ColumnEnd)
        {
            this.GridColumnStart = ColumnStart;
            this.GridColumnEnd = ColumnEnd;
            this.UpdateLayout();
        }

        /// <summary>
        /// Sets the grid row and column this widget is in. Used for grids.
        /// </summary>
        /// <param name="Row"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        public void SetGrid(int Row, int Column)
        {
            this.SetGrid(Row, Row, Column, Column);
        }
        /// <summary>
        /// Sets the grid row and column this widget starts and ends in. Used for grids.
        /// </summary>
        public void SetGrid(int RowStart, int RowEnd, int ColumnStart, int ColumnEnd)
        {
            this.GridRowStart = RowStart;
            this.GridRowEnd = RowEnd;
            this.GridColumnStart = ColumnStart;
            this.GridColumnEnd = ColumnEnd;
            this.UpdateLayout();
        }

        /// <summary>
        /// Updates the grid or stackpanel's layout.
        /// </summary>
        public virtual void UpdateLayout()
        {
            if (this.Parent is ILayout)
            {
                (this.Parent as ILayout).NeedUpdate = true;
            }
        }

        /// <summary>
        /// Disposes this widget, its viewport and all its children.
        /// </summary>
        public virtual void Dispose()
        {
            AssertUndisposed();
            if (this.OnDisposing != null) this.OnDisposing.Invoke(this, new EventArgs());
            // Mark this widget as disposed.
            this.Disposed = true;
            // Dispose the viewport and all its sprites.
            // Viewport may already be null if a child of a layoutcontainer has been disposed
            // because they share the same viewport.
            if (this.Viewport != null && !this.Viewport.Disposed) this.Viewport.Dispose();
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                if (this.Widgets[i] != null)
                {
                    this.Widgets[i].Dispose();
                    // Disposing a widget automatically removes it from its parent's widget list,
                    // Hence we have to update our iterator.
                    i--;
                }
            }
            // Remove this widget from the parent's widget list.
            this.Parent.Widgets.Remove(this);
            // Set viewport and sprites to null to ensure no methods can use them anymore.
            this.Viewport = null;
            this.Sprites = null;
            if (Parent is LayoutContainer) 
                (Parent as Widget).Dispose();
            if (this.OnDisposed != null) this.OnDisposed.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Ensures this widget is not disposed by raising an exception if it is.
        /// </summary>
        protected void AssertUndisposed()
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
        }

        /// <summary>
        /// Marks the sprites as needing an redraw next iteration.
        /// This allows you to call Redraw multiple times at once, as it will only redraw once in the next iteration.
        /// </summary>
        public void Redraw()
        {
            AssertUndisposed();
            this.Drawn = false;
        }

        /// <summary>
        /// The method that redraws this widget and its sprites.
        /// </summary>
        protected virtual void Draw()
        {
            AssertUndisposed();
            this.Drawn = true;
        }

        /// <summary>
        /// Updates this widget and its children. Do not put excessive logic in here.
        /// </summary>
        public virtual void Update()
        {
            AssertUndisposed();

            // If this widget is active
            if (this.SelectedWidget)
            {
                // Execute shortcuts if their buttons is being triggered.
                foreach (Shortcut s in this.Shortcuts)
                {
                    if (s.GlobalShortcut) continue; // Handled by the UIManager

                    Key k = s.Key;
                    bool Valid = Input.Trigger((SDL2.SDL.SDL_Keycode)k.MainKey);
                    if (!Valid) continue;

                    // Modifiers
                    foreach (Keycode mod in k.Modifiers)
                    {
                        bool onefound = false;
                        List<SDL2.SDL.SDL_Keycode> codes = new List<SDL2.SDL.SDL_Keycode>();
                        if (mod == Keycode.CTRL) { codes.Add(SDL2.SDL.SDL_Keycode.SDLK_LCTRL); codes.Add(SDL2.SDL.SDL_Keycode.SDLK_RCTRL); }
                        else if (mod == Keycode.SHIFT) { codes.Add(SDL2.SDL.SDL_Keycode.SDLK_LSHIFT); codes.Add(SDL2.SDL.SDL_Keycode.SDLK_RSHIFT); }
                        else if (mod == Keycode.ALT) { codes.Add(SDL2.SDL.SDL_Keycode.SDLK_LALT); codes.Add(SDL2.SDL.SDL_Keycode.SDLK_RALT); }
                        else codes.Add((SDL2.SDL.SDL_Keycode)mod);

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

                    // Execute this shortcut's event.
                    s.Event.Invoke(this, new EventArgs());
                }
            }

            // Updates the MouseInputManager in case this widget's boundaries changes.
            this.WidgetIM.Update(this.Viewport.Rect);
            // If this widget needs a redraw, perform the redraw
            if (!this.Drawn) this.Draw();
            // Update child widgets
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                this.Widgets[i].Update();
            }
        }

        /// <summary>
        /// Responsible for opening the right-click menu when this widget is right-clicked.
        /// </summary>
        private void RightClick_ContextMenu(object sender, MouseEventArgs e)
        {
            if (ShowContextMenu && ContextMenuList != null && ContextMenuList.Count > 0)
            {
                bool cont = true;
                if (OnContextMenuOpening != null)
                {
                    CancelEventArgs args = new CancelEventArgs();
                    OnContextMenuOpening.Invoke(sender, args);
                    if (args.Cancel) cont = false;
                }
                if (cont)
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
        public virtual void WidgetSelected(object sender, MouseEventArgs e)
        {
            this.Window.UI.SetSelectedWidget(this);
        }
        public virtual void WidgetDeselected(object sender, EventArgs e) { }
        public virtual void TextInput(object sender, TextInputEventArgs e) { }
        public virtual void PositionChanged(object sender, EventArgs e)
        {
            UpdateAutoScroll();
        }
        public virtual void SizeChanged(object sender, SizeEventArgs e)
        {
            UpdateAutoScroll();
            UpdateLayout();
        }
        public virtual void ParentSizeChanged(object sender, SizeEventArgs e) { }
        public virtual void ChildBoundsChanged(object sender, SizeEventArgs e)
        {
            UpdateAutoScroll();
        }
    }

    public class Shortcut
    {
        public Widget Widget;
        public Key Key;
        public EventHandler<EventArgs> Event;
        public bool GlobalShortcut = false;

        /// <summary>
        /// Creates a new Shortcut object.
        /// </summary>
        /// <param name="Widget">The widget that needs to be visible and available for global shortcuts to work.</param>
        /// <param name="Key">The actual key combination required to activate this shortcut.</param>
        /// <param name="Event">The event to trigger when the key is pressed.</param>
        /// <param name="GlobalShortcut">Whether the given widget needs to be active, or if this is a global shortcut.</param>
        public Shortcut(Widget Widget, Key Key, EventHandler<EventArgs> Event, bool GlobalShortcut = false)
        {
            this.Widget = Widget;
            this.Key = Key;
            this.Event = Event;
            this.GlobalShortcut = GlobalShortcut;
        }
    }

    public class Timer
    {
        public string Identifier;
        public long StartTime;
        public long Timespan;

        /// <summary>
        /// Creates a new Timer object.
        /// </summary>
        /// <param name="identifier">The unique identifier string.</param>
        /// <param name="starttime">The time at which this Timer started.</param>
        /// <param name="timespan">The timespan this Timer is active for.</param>
        public Timer(string identifier, long starttime, long timespan)
        {
            this.Identifier = identifier;
            this.StartTime = starttime;
            this.Timespan = timespan;
        }
    }
}
