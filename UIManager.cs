using System;
using System.Collections.Generic;
using MKEditor.Widgets;
using ODL;

namespace MKEditor
{
    public class UIManager : IContainer
    {
        public MainEditorWindow Window { get; protected set; }
        public Point AdjustedPosition { get { return new Point(0, 0); } set { throw new MethodNotSupportedException(this); } }
        public Point Position { get { return new Point(0, 0); } }
        public int ScrolledX { get { return 0; } set { throw new MethodNotSupportedException(this); } }
        public int ScrolledY { get { return 0; } set { throw new MethodNotSupportedException(this); } }
        public int ZIndex { get { return 0; } }
        public Point ScrolledPosition { get { return new Point(0, 0); } }
        public Size Size { get { return new Size(this.Window.Width, this.Window.Height); } }
        public Viewport Viewport { get { return this.Window.Viewport; } }
        public List<Widget> Widgets { get; protected set; } = new List<Widget>();
        public Color BackgroundColor { get { return this.Window.BackgroundColor; } }
        public Widget SelectedWidget { get; protected set; }
        public HScrollBar HScrollBar { get { return null; } set { throw new MethodNotSupportedException(this); } }
        public VScrollBar VScrollBar { get { return null; } set { throw new MethodNotSupportedException(this); } }
        public List<Shortcut> Shortcuts { get; protected set; } = new List<Shortcut>();
        public int WindowLayer { get { return 0; } set { throw new MethodNotSupportedException(this); } }

        private Sprite BGSprite;
        private List<MouseInputManager> IMs = new List<MouseInputManager>();

        public IContainer Parent { get { throw new MethodNotSupportedException(this); } }

        public UIManager(MainEditorWindow Window)
        {
            this.Window = Window;
            BGSprite = new Sprite(this.Viewport);
            BGSprite.Bitmap = new SolidBitmap(this.Size, this.BackgroundColor);
            this.Window.SetActiveWidget(this);
        }

        public void Add(Widget w)
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

        public Widget Remove(Widget w)
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

        public void RemoveInput(MouseInputManager input)
        {
            IMs.Remove(input);
        }

        public void MouseDown(object sender, MouseEventArgs e)
        {
            bool DoMoveEvent = false;
            for (int i = 0; i < IMs.Count; i++)
            {
                if (IMs[i].Widget.Disposed)
                {
                    IMs.RemoveAt(i);
                    i--;
                    continue;
                }
                IMs[i].MouseDown(e);
                if (e.Handled)
                {
                    DoMoveEvent = true;
                    break;
                }
            }
            if (DoMoveEvent)
            {
                e.Handled = false;
                MouseMoving(sender, e);
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
                if (e.Handled) break;
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
                if (e.Handled) break;
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
                if (e.Handled) break;
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
                if (e.Handled) break;
            }
        }

        public void WindowResized(object sender, WindowEventArgs e)
        {
            (BGSprite.Bitmap as SolidBitmap).SetSize(this.Size);
            this.Widgets.ForEach(w =>
            {
                w.OnParentSizeChanged.Invoke(sender, new SizeEventArgs(e.Width, e.Height));
                w.Redraw();
            });
        }

        public void Update()
        {
            foreach (Shortcut s in this.Shortcuts)
            {
                if (!s.GlobalShortcut) continue; // Handled by the Widget it's bound to

                if (s.Widget.WindowLayer < Window.ActiveWidget.WindowLayer || !s.Widget.IsVisible() || s.Widget.Disposed) continue;

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

            for (int i = 0; i < this.Widgets.Count; i++)
            {
                this.Widgets[i].Update();
            }
        }

        public void SetSelectedWidget(Widget w)
        {
            if (this.SelectedWidget != null)
            {
                this.SelectedWidget.SelectedWidget = false;
                this.SelectedWidget.OnWidgetDeselected.Invoke(this, new EventArgs());
                this.SelectedWidget.Redraw();
            }
            this.SelectedWidget = w;
            if (w != null)
            {
                this.SelectedWidget.SelectedWidget = true;
                this.SelectedWidget.Redraw();
            }
        }

        public void TextInput(object sender, TextInputEventArgs e)
        {
            if (this.SelectedWidget != null)
            {
                this.SelectedWidget.OnTextInput.Invoke(sender, e);
            }
        }

        public void RegisterShortcut(Shortcut s)
        {
            this.Shortcuts.Add(s);
        }

        public void DeregisterShortcut(Shortcut s)
        {
            this.Shortcuts.Remove(s);
        }
    }
}
