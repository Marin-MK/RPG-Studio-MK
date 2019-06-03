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
        public EventHandler<MouseEventArgs> OnMouseWheel;
        public EventHandler<MouseEventArgs> OnHoverChanged;

        private Rect Area;

        private bool _Clicked = false;
        public bool Clicked { get { return Widget.Visible ? _Clicked : false; } set { _Clicked = value; } }
        private bool? _ClickedInArea = null;
        public bool? ClickedInArea { get { return Widget.Visible ? _ClickedInArea : false; } set { _ClickedInArea = value; } }
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
            return ClickedInArea == true && Hovering;
        }

        public bool HoverAnim()
        {
            if (!Widget.Visible) return false;
            return Hovering && ClickedInArea != false || ClickedInArea == true;
        }

        public void Update(Rect Area)
        {
            this.Area = Area;
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
            if (e.LeftButton && !e.OldLeftButton)
            {
                this.Clicked = true;
                this.ClickedInArea = e.InArea(this.Area);
                if (this.ClickedInArea == true)
                {
                    if (this.Widget.OnWidgetSelect != null) this.Widget.OnWidgetSelect.Invoke(this, e);
                }
            }
            if (this.OnMouseDown != null) this.OnMouseDown.Invoke(this, e);
        }

        public void MousePress(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
            if (this.OnMousePress != null) this.OnMousePress.Invoke(this, e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
            if (!e.LeftButton && e.OldLeftButton)
            {
                if (this.OnMouseUp != null) this.OnMouseUp.Invoke(this, e);
                if (ClickedInArea == true && Hovering)
                {
                    if (this.OnLeftClick != null) this.OnLeftClick.Invoke(this, e);
                }
                this.Clicked = false;
                this.ClickedInArea = null;
            }
            else
            {
                if (this.OnMouseUp != null) this.OnMouseUp.Invoke(this, e);
            }
        }

        public void MouseWheel(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
            if (e.WheelY != 0 && this.OnMouseWheel != null) this.OnMouseWheel.Invoke(this, e);
        }

        public void MouseMoving(MouseEventArgs e)
        {
            if (!Widget.Visible) return;
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
