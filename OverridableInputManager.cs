using System;
using System.Collections.Generic;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class OverridableInputManager : MouseInputManager
    {
        public MouseEvent OnLeftClick;
        public MouseEvent OnMiddleclick;
        public MouseEvent OnRightClick;
        public MouseEvent OnMouseDown;
        public MouseEvent OnMousePress;
        public MouseEvent OnMouseUp;
        public MouseEvent OnMouseWheel;
        public MouseEvent OnMouseMoving;
        public MouseEvent OnHoverChanged;

        public OverridableInputManager(Widget Widget) : base(Widget) { }

        protected override void MouseDownEvent(MouseEventArgs e)
        {
            this.OnMouseDown?.Invoke(e);
        }

        protected override void MouseUpEvent(MouseEventArgs e)
        {
            this.OnMouseUp?.Invoke(e);
        }

        protected override void MousePressEvent(MouseEventArgs e)
        {
            this.OnMousePress?.Invoke(e);
        }

        protected override void HoverChangedEvent(MouseEventArgs e)
        {
            this.OnHoverChanged?.Invoke(e);
        }

        protected override void MouseMovingEvent(MouseEventArgs e)
        {
            this.OnMouseMoving?.Invoke(e);
        }

        protected override void MouseWheelEvent(MouseEventArgs e)
        {
            this.OnMouseWheel?.Invoke(e);
        }

        protected override void LeftClickEvent(MouseEventArgs e)
        {
            this.OnLeftClick?.Invoke(e);
        }

        protected override void MiddleClickEvent(MouseEventArgs e)
        {
            this.OnMiddleclick?.Invoke(e);
        }

        protected override void RightClickEvent(MouseEventArgs e)
        {
            this.OnRightClick?.Invoke(e);
        }
    }
}
