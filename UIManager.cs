using System;
using System.Collections.Generic;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class UIManager : IContainer
    {
        public Window Window { get; protected set; }
        public Point AdjustedPosition { get { return new Point(0, 0); } set { throw new MethodNotSupportedException(this); } }
        public Size AdjustedSize { get { return new Size(0, 0); } }
        public Point Position { get { return new Point(0, 0); } }
        public double ScrollPercentageX { get { return 0; } set { throw new MethodNotSupportedException(this); } }
        public double ScrollPercentageY { get { return 0; } set { throw new MethodNotSupportedException(this); } }
        public Point ScrolledPosition { get { return new Point(0, 0); } }
        public Size Size { get { return new Size(this.Window.Width, this.Window.Height); } }
        public Viewport Viewport { get { return this.Window.Viewport; } }
        public List<Widget> Widgets { get; protected set; } = new List<Widget>();
        public Color BackgroundColor { get { return this.Window.BackgroundColor; } }
        public Widget SelectedWidget { get; protected set; }

        private Sprite BGSprite;
        private List<MouseInputManager> IMs = new List<MouseInputManager>();

        public UIManager(Window Window)
        {
            this.Window = Window;
            BGSprite = new Sprite(this.Viewport);
            BGSprite.Bitmap = new SolidBitmap(this.Size, this.BackgroundColor);
        }

        public IContainer Add(Widget w)
        {
            if (this.Widgets.Exists(wgt => wgt.Name == w.Name))
            {
                throw new Exception("Already existing widget by the name of '" + w.Name + "'");
            }
            this.Widgets.Add(w);
            return this;
        }

        public IContainer Get(string Name)
        {
            foreach (Widget w in this.Widgets)
            {
                if (w.Name == Name) return w;
            }
            return null;
        }

        public IContainer Remove(Widget w)
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

        public void AddInput(MouseInputManager input)
        {
            IMs.Add(input);
        }

        public void MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < IMs.Count; i++)
            {
                if (IMs[i].Widget.Disposed)
                {
                    IMs.RemoveAt(i);
                    i--;
                    continue;
                }
                IMs[i].MouseDown(e);
            }
        }

        public void MousePress(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < IMs.Count; i++)
            {
                if (IMs[i].Widget.Disposed)
                {
                    IMs.RemoveAt(i);
                    i--;
                    continue;
                }
                IMs[i].MousePress(e);
            }
        }

        public void MouseUp(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < IMs.Count; i++)
            {
                if (IMs[i].Widget.Disposed)
                {
                    IMs.RemoveAt(i);
                    i--;
                    continue;
                }
                IMs[i].MouseUp(e);
            }
            for (int i = 0; i < IMs.Count; i++)
            {
                if (IMs[i].Widget.Disposed)
                {
                    IMs.RemoveAt(i);
                    i--;
                    continue;
                }
                if (IMs[i].OnHoverChanged != null) IMs[i].OnHoverChanged.Invoke(sender, e);
            }
        }

        public void MouseMoving(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < IMs.Count; i++)
            {
                if (IMs[i].Widget.Disposed)
                {
                    IMs.RemoveAt(i);
                    i--;
                    continue;
                }
                IMs[i].MouseMoving(e);
            }
        }

        public void MouseWheel(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < IMs.Count; i++)
            {
                if (IMs[i].Widget.Disposed)
                {
                    IMs.RemoveAt(i);
                    i--;
                    continue;
                }
                IMs[i].MouseWheel(e);
            }
        }

        public void WindowResized(object sender, WindowEventArgs e)
        {
            BGSprite.Bitmap.Unlock();
            (BGSprite.Bitmap as SolidBitmap).SetSize(this.Size);
            BGSprite.Bitmap.Lock();
            this.Widgets.ForEach(w =>
            {
                w.OnParentSizeChanged.Invoke(sender, new SizeEventArgs(e.Width, e.Height));
                w.Redraw();
            });
        }

        public void Update()
        {
            for (int i = 0; i < this.Widgets.Count; i++)
            {
                this.Widgets[i].Update();
            }
        }

        public void SetSelectedWidget(Widget w)
        {
            if (this.SelectedWidget != null)
            {
                this.SelectedWidget.Selected = false;
                this.SelectedWidget.OnDeselected.Invoke(this, new EventArgs());
                this.SelectedWidget.Redraw();
            }
            this.SelectedWidget = w;
            this.SelectedWidget.Selected = true;
            this.SelectedWidget.OnSelected(this, new EventArgs());
            this.SelectedWidget.Redraw();
        }

        public void TextInput(object sender, TextInputEventArgs e)
        {
            if (this.SelectedWidget != null)
            {
                this.SelectedWidget.OnTextInput.Invoke(sender, e);
            }
        }
    }
}
