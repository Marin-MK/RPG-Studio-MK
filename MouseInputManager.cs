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
        public EventHandler<MouseEventArgs> OnMouseWheel;
        public EventHandler<MouseEventArgs> OnHoverChanged;

        private Rect Area;

        public bool Clicked = false;
        public bool? ClickedInArea = null;
        public bool Hovering = false;

        public MouseInputManager(Widget Widget)
        {
            this.Widget = Widget;
            this.Window.UI.AddInput(this);
        }

        public bool ClickAnim()
        {
            return ClickedInArea == true && Hovering;
        }

        public bool HoverAnim()
        {
            return Hovering && ClickedInArea != false || ClickedInArea == true;
        }

        public void Update(Rect Area)
        {
            this.Area = Area;
        }

        public void MouseDown(MouseEventArgs e)
        {
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
            if (this.OnMousePress != null) this.OnMousePress.Invoke(this, e);
        }

        public void MouseUp(MouseEventArgs e)
        {
            if (!e.LeftButton && e.OldLeftButton)
            {
                if (this.OnMouseUp != null) this.OnMouseUp.Invoke(this, e);
                if (ClickedInArea == true && Hovering)
                {
                    if (this.Widget.OnLeftClick != null) this.Widget.OnLeftClick.Invoke(this, e);
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
            if (e.WheelY != 0 && this.OnMouseWheel != null) this.OnMouseWheel.Invoke(this, e);
        }

        public void MouseMoving(MouseEventArgs e)
        {
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
