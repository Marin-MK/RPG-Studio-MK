using MKEditor.Widgets;
using ODL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor
{
    [System.Diagnostics.DebuggerDisplay("{Widget} {Disposed}")]
    public class MouseInputManager
    {
        public readonly Widget Widget;
        public MainEditorWindow Window { get { return this.Widget.Window; } }

        public int Priority = 0;
        public int OldPriority = 0;

        public Rect Area;

        private bool _ClickedLeft = false;
        public bool ClickedLeft { get { return Widget.IsVisible() ? _ClickedLeft : false; } set { _ClickedLeft = value; } }
        private bool _ClickedRight = false;
        public bool ClickedRight { get { return Widget.IsVisible() ? _ClickedRight : false; } set { _ClickedRight = value; } }
        private bool _ClickedMiddle = false;
        public bool ClickedMiddle { get { return Widget.IsVisible() ? _ClickedMiddle : false; } set { _ClickedMiddle = value; } }

        private bool? _ClickedLeftInArea = null;
        public bool? ClickedLeftInArea { get { return Widget.IsVisible() ? _ClickedLeftInArea : false; } set { _ClickedLeftInArea = value; } }
        private bool? _ClickedRightInArea = null;
        public bool? ClickedRightInArea { get { return Widget.IsVisible() ? _ClickedRightInArea : false; } set { _ClickedRightInArea = value; } }
        private bool? _ClickedMiddleInArea = null;
        public bool? ClickedMiddleInArea { get { return Widget.IsVisible() ? _ClickedMiddleInArea : false; } set { _ClickedMiddleInArea = value; } }

        public bool SelectWithLeftClick = true;
        public bool SelectWithMiddleClick = true;
        public bool SelectWithRightClick = true;

        private bool _Hovering = false;
        public bool Hovering { get { return Widget.IsVisible() ? _Hovering : false; } set { _Hovering = value; } }

        public MouseInputManager(Widget Widget)
        {
            this.Widget = Widget;
            this.Window.UI.AddInput(this);
        }

        public bool ClickAnim()
        {
            if (!Widget.IsVisible()) return false;
            return ClickedLeftInArea == true && Hovering;
        }

        public bool HoverAnim()
        {
            if (!Widget.IsVisible()) return false;
            return Hovering && ClickedLeftInArea != false || ClickedLeftInArea == true;
        }

        public void Update(Rect Area)
        {
            this.Area = Area;
            if (this.Widget == null || this.Widget.Disposed)
            {
                this.Window.UI.RemoveInput(this);
            }
        }

        public bool WidgetAccessible()
        {
            if (Widget.MouseAlwaysActive) return true;
            if (Widget.WindowLayer < Widget.Window.ActiveWidget.WindowLayer) return false;
            return true;
        }

        protected virtual void SelectedEvent(MouseEventArgs e)
        {
            this.Widget.OnWidgetSelected?.Invoke(e);
        }

        protected virtual void MouseDownEvent(MouseEventArgs e)
        {
            this.Widget.OnMouseDown.Invoke(e);
        }

        protected virtual void MouseUpEvent(MouseEventArgs e)
        {
            this.Widget.OnMouseUp.Invoke(e);
        }

        protected virtual void MousePressEvent(MouseEventArgs e)
        {
            this.Widget.OnMousePress.Invoke(e);
        }

        protected virtual void MouseMovingEvent(MouseEventArgs e)
        {
            this.Widget.OnMouseMoving.Invoke(e);
        }

        protected virtual void HoverChangedEvent(MouseEventArgs e)
        {
            this.Widget.OnHoverChanged.Invoke(e);
        }

        protected virtual void MouseWheelEvent(MouseEventArgs e)
        {
            this.Widget.OnMouseWheel.Invoke(e);
        }

        protected virtual void LeftClickEvent(MouseEventArgs e)
        {
            this.Widget.OnLeftClick.Invoke(e);
        }

        protected virtual void MiddleClickEvent(MouseEventArgs e)
        {
            this.Widget.OnMiddleClick.Invoke(e);
        }

        protected virtual void RightClickEvent(MouseEventArgs e)
        {
            this.Widget.OnRightClick.Invoke(e);
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (!Ready()) return;
            if (!Widget.IsVisible()) return;

            if (!(Widget.Window.ActiveWidget is UIManager))
            {
                if (!(Widget.Window.ActiveWidget as Widget).WidgetIM.Hovering && Widget.Window.ActiveWidget is ContextMenu)
                {
                    (Widget.Window.ActiveWidget as Widget).Dispose();
                    e.Handled = true;
                    return;
                }
            }

            if (!WidgetAccessible()) return;

            if (e.LeftButton && !e.OldLeftButton)
            {
                this.ClickedLeft = true;
                this.ClickedLeftInArea = e.InArea(this.Area);
                if (this.ClickedLeftInArea == true && this.SelectWithLeftClick)
                {
                    SelectedEvent(e);
                }
            }

            if (e.RightButton && !e.OldRightButton)
            {
                this.ClickedRight = true;
                this.ClickedRightInArea = e.InArea(this.Area);
                if (this.ClickedRightInArea == true && this.SelectWithRightClick)
                {
                    SelectedEvent(e);
                }
            }

            if (e.MiddleButton && !e.OldMiddleButton)
            {
                this.ClickedMiddle = true;
                this.ClickedMiddleInArea = e.InArea(this.Area);
                if (this.ClickedMiddleInArea == true && this.SelectWithMiddleClick)
                {
                    SelectedEvent(e);
                }
            }

            MouseDownEvent(e);
        }

        public void MousePress(MouseEventArgs e)
        {
            if (!Ready()) return;
            if (!Widget.IsVisible()) return;
            if (!WidgetAccessible()) return;

            MousePressEvent(e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (!Ready()) return;
            if (!Widget.IsVisible()) return;
            //if (!WidgetAccessible()) return; // If a button opens up a context menu, the mouse-up event shouldn't be ignored because of that menu.

            if (!e.LeftButton && e.OldLeftButton)
            {
                if (ClickedLeftInArea == true && Hovering)
                {
                    LeftClickEvent(e);
                }
                this.ClickedLeft = false;
                this.ClickedLeftInArea = null;
            }

            if (!e.RightButton && e.OldRightButton)
            {
                if (ClickedRightInArea == true && Hovering)
                {
                    RightClickEvent(e);
                }
                this.ClickedRight = false;
                this.ClickedRightInArea = null;
            }

            if (!e.MiddleButton && e.MiddleButton)
            {
                if (ClickedMiddleInArea == true && Hovering)
                {
                    MiddleClickEvent(e);
                }
                this.ClickedMiddle = false;
                this.ClickedMiddleInArea = false;
            }

            MouseUpEvent(e);
        }

        public void MouseWheel(MouseEventArgs e)
        {
            if (!Ready()) return;
            if (!Widget.IsVisible()) return;
            if (!WidgetAccessible()) return;

            if (e.WheelY != 0) MouseWheelEvent(e);
        }

        public void MouseMoving(MouseEventArgs e)
        {
            if (!Ready()) return;
            if (!Widget.IsVisible()) return;
            if (!WidgetAccessible()) return;

            bool oldhover = this.Hovering;
            this.Hovering = e.InArea(this.Area);
            MouseMovingEvent(e);
            if (oldhover != this.Hovering)
            {
                HoverChangedEvent(e);
            }
        }

        public bool Ready()
        {
            if (this.Area == null || this.Widget == null || this.Widget.Disposed) return false;
            return true;
        }
    }
}
