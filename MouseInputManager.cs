using MKEditor.Widgets;
using ODL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKEditor
{
    public class MouseInputManager
    {
        public readonly Widget Widget;
        public WidgetWindow Window { get { return this.Widget.Window; } }

        public EventHandler<MouseEventArgs> OnMouseMoving;
        public EventHandler<MouseEventArgs> OnMouseDown;
        public EventHandler<MouseEventArgs> OnMousePress;
        public EventHandler<MouseEventArgs> OnMouseUp;
        public EventHandler<MouseEventArgs> OnLeftClick;
        public EventHandler<MouseEventArgs> OnRightClick;
        public EventHandler<MouseEventArgs> OnMouseWheel;
        public EventHandler<MouseEventArgs> OnHoverChanged;

        private Rect Area;

        private bool _ClickedLeft = false;
        public bool ClickedLeft { get { return Widget.Visible ? _ClickedLeft : false; } set { _ClickedLeft = value; } }
        private bool _ClickedRight = false;
        public bool ClickedRight { get { return Widget.Visible ? _ClickedRight : false; } set { _ClickedRight = value; } }

        private bool? _ClickedLeftInArea = null;
        public bool? ClickedLeftInArea { get { return Widget.Visible ? _ClickedLeftInArea : false; } set { _ClickedLeftInArea = value; } }
        private bool? _ClickedRightInArea = null;
        public bool? ClickedRightInArea { get { return Widget.Visible ? _ClickedRightInArea : false; } set { _ClickedRightInArea = value; } }

        private bool _Hovering = false;
        public bool Hovering { get { return Widget.Visible ? _Hovering : false; } set { _Hovering = value; } }

        public MouseInputManager(Widget Widget)
        {
            this.Widget = Widget;
            this.Window.UI.AddInput(this);
        }

        public bool ClickAnim()
        {
            if (!Widget.Visible) return false;
            return ClickedLeftInArea == true && Hovering;
        }

        public bool HoverAnim()
        {
            if (!Widget.Visible) return false;
            return Hovering && ClickedLeftInArea != false || ClickedLeftInArea == true;
        }

        public void Update(Rect Area)
        {
            this.Area = Area;
        }

        public bool OverContextMenu()
        {
            if (Widget.Window.UI.OverContextMenu != null && Widget.Window.UI.OverContextMenu != Widget && Widget.Window.UI.OverContextMenu.WidgetIM.Hovering)
                return true;
            return false;
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
            if (OverContextMenu()) return;
            
            if (Widget.Window.UI.OverContextMenu != null && Widget.Window.UI.OverContextMenu != Widget)
            {
                Widget.Window.UI.OverContextMenu.Dispose();
            }

            if (e.LeftButton && !e.OldLeftButton)
            {
                this.ClickedLeft = true;
                this.ClickedLeftInArea = e.InArea(this.Area);
                if (this.ClickedLeftInArea == true)
                {
                    if (this.Widget.OnWidgetSelect != null) this.Widget.OnWidgetSelect.Invoke(this, e);
                }
            }

            if (e.RightButton && !e.OldRightButton)
            {
                this.ClickedRight = true;
                this.ClickedRightInArea = e.InArea(this.Area);
            }

            if (this.OnMouseDown != null) this.OnMouseDown.Invoke(this, e);
        }

        public void MousePress(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
            if (OverContextMenu()) return;
            if (this.OnMousePress != null) this.OnMousePress.Invoke(this, e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
            if (OverContextMenu()) return;

            if (!e.LeftButton && e.OldLeftButton)
            {
                if (ClickedLeftInArea == true && Hovering)
                {
                    if (this.OnLeftClick != null) this.OnLeftClick.Invoke(this, e);
                }
                this.ClickedLeft = false;
                this.ClickedLeftInArea = null;
            }

            if (!e.RightButton && e.OldRightButton)
            {
                if (ClickedRightInArea == true && Hovering)
                {
                    if (this.OnRightClick != null) this.OnRightClick.Invoke(this, e);
                }
                this.ClickedRight = false;
                this.ClickedRightInArea = null;
            }

            if (this.OnMouseUp != null) this.OnMouseUp.Invoke(this, e);
        }

        public void MouseWheel(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
            if (OverContextMenu()) return;
            if (e.WheelY != 0 && this.OnMouseWheel != null) this.OnMouseWheel.Invoke(this, e);
        }

        public void MouseMoving(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
            if (OverContextMenu()) return;
            bool oldhover = this.Hovering;
            this.Hovering = e.InArea(this.Area);
            if (this.OnMouseMoving != null) this.OnMouseMoving.Invoke(this, e);
            if (oldhover != this.Hovering && this.OnHoverChanged != null)
            {
                this.OnHoverChanged.Invoke(this, e);
            }
        }
    }
}
