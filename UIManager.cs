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
        public List<Timer> Timers = new List<Timer>();

        private Sprite BGSprite;
        private List<MouseInputManager> IMs = new List<MouseInputManager>();

        public IContainer Parent { get { throw new MethodNotSupportedException(this); } }

        public UIManager(MainEditorWindow Window)
        {
            this.Window = Window;
            BGSprite = new Sprite(this.Viewport);
            BGSprite.Bitmap = new SolidBitmap(this.Size, this.BackgroundColor);
            this.Window.SetActiveWidget(this);
            this.RegisterShortcut(new Shortcut(null, new Key(Keycode.Z, Keycode.CTRL), delegate (BaseEventArgs e) { Editor.Undo(); }, true));
            this.RegisterShortcut(new Shortcut(null, new Key(Keycode.Y, Keycode.CTRL), delegate (BaseEventArgs e) { Editor.Redo(); }, true));
        }

        public void Add(Widget w)
        {
            this.Widgets.Add(w);
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

        public void AddInput(MouseInputManager input)
        {
            IMs.Add(input);
        }

        public void RemoveInput(MouseInputManager input)
        {
            IMs.Remove(input);
        }

        public void MouseDown(MouseEventArgs e)
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
                MouseMoving(e);
            }
        }

        public void MousePress(MouseEventArgs e)
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

        public void MouseUp(MouseEventArgs e)
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

        public void MouseMoving(MouseEventArgs e)
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

        public void MouseWheel(MouseEventArgs e)
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

        public void Resized(BaseEventArgs e)
        {
            (BGSprite.Bitmap as SolidBitmap).SetSize(this.Size);
            this.Widgets.ForEach(w =>
            {
                w.OnParentSizeChanged(new BaseEventArgs());
                w.Redraw();
            });
        }

        public void Update()
        {
            foreach (Shortcut s in this.Shortcuts)
            {
                if (!s.GlobalShortcut) continue; // Handled by the Widget it's bound to

                if (s.Widget != null && (s.Widget.WindowLayer < Window.ActiveWidget.WindowLayer || !s.Widget.IsVisible() || s.Widget.Disposed)) continue;

                Key k = s.Key;
                bool Valid = false;
                if (Input.Press((SDL2.SDL.SDL_Keycode) k.MainKey))
                {
                    if (TimerPassed($"key_{s.Key.ID}"))
                    {
                        ResetTimer($"key_{s.Key.ID}");
                        Valid = true;
                    }
                    else if (TimerPassed($"key_{s.Key.ID}_initial"))
                    {
                        SetTimer($"key_{s.Key.ID}", 50);
                        DestroyTimer($"key_{s.Key.ID}_initial");
                        Valid = true;
                    }
                    else if (!TimerExists($"key_{s.Key.ID}") && !TimerExists($"key_{s.Key.ID}_initial"))
                    {
                        SetTimer($"key_{s.Key.ID}_initial", 300);
                        Valid = true;
                    }
                }
                else
                {
                    if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
                    if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
                }
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

                s.Event(new BaseEventArgs());
            }

            for (int i = 0; i < this.Widgets.Count; i++)
            {
                this.Widgets[i].Update();
            }
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

        public void SetSelectedWidget(Widget w)
        {
            if (this.SelectedWidget == w) return;
            if (this.SelectedWidget != null && !this.SelectedWidget.Disposed)
            {
                this.SelectedWidget.SelectedWidget = false;
                Widget selbefore = this.SelectedWidget;
                this.SelectedWidget.OnWidgetDeselected(new BaseEventArgs());
                if (!selbefore.Disposed) selbefore.Redraw();
                // Possible if OnWidgetDeselected itself called SetSelectedWidget on a different widget.
                // In that case we should skip the setting-bit below, as it would
                // set the selected widget to null AFTER the previous SetSelectedWidget call.
                if (selbefore != this.SelectedWidget)
                    return;
            }
            this.SelectedWidget = w;
            if (w != null)
            {
                this.SelectedWidget.SelectedWidget = true;
                this.SelectedWidget.Redraw();
            }
        }

        public void TextInput(TextEventArgs e)
        {
            this.SelectedWidget?.OnTextInput(e);
        }

        public void RegisterShortcut(Shortcut s)
        {
            this.Shortcuts.Add(s);
        }

        public void DeregisterShortcut(Shortcut s)
        {
            this.Shortcuts.Remove(s);
            if (TimerExists($"key_{s.Key.ID}")) DestroyTimer($"key_{s.Key.ID}");
            if (TimerExists($"key_{s.Key.ID}_initial")) DestroyTimer($"key_{s.Key.ID}_initial");
        }

        public void SetBackgroundColor(Color c)
        {
            (BGSprite.Bitmap as SolidBitmap).SetColor(c);
        }
        public void SetBackgroundColor(byte r, byte g, byte b, byte a = 255)
        {
            (BGSprite.Bitmap as SolidBitmap).SetColor(r, g, b, a);
        }
    }
}
