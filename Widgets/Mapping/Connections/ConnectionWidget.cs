using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class ConnectionWidget : Widget
    {
        bool Initialized = false;

        Label MapLabel;
        Label XLabel;
        Label YLabel;

        public DropdownBox MapBox;
        public NumericBox XBox;
        public NumericBox YBox;
        public ExitButton ExitButton;

        public Game.MapConnection MapConnection;

        public bool Selected = false;

        public ConnectionWidget(IContainer Parent) : base(Parent)
        {
            Font f = Font.Get("Fonts/Ubuntu-B", 14);

            MapLabel = new Label(this);
            MapLabel.SetPosition(12, 6);
            MapLabel.SetText("Map");
            MapLabel.SetFont(f);

            XLabel = new Label(this);
            XLabel.SetPosition(15, 60);
            XLabel.SetText("X");
            XLabel.SetFont(f);

            YLabel = new Label(this);
            YLabel.SetPosition(94, 60);
            YLabel.SetText("Y");
            YLabel.SetFont(f);

            MapBox = new DropdownBox(this);
            MapBox.SetPosition(23, 27);
            MapBox.SetSize(145, 25);
            MapBox.SetFont(f);
            MapBox.SetReadOnly(true);

            XBox = new NumericBox(this);
            XBox.SetPosition(24, 79);
            XBox.SetSize(66, 27);
            XBox.OnValueChanged += delegate (object sender, EventArgs e)
            {
                if (Initialized) SetOffset(XBox.Value, YBox.Value);
            };

            YBox = new NumericBox(this);
            YBox.SetPosition(101, 79);
            YBox.SetSize(66, 27);
            YBox.OnValueChanged += delegate (object sender, EventArgs e)
            {
                if (Initialized) SetOffset(XBox.Value, YBox.Value);
            };

            ExitButton = new ExitButton(this);
            ExitButton.SetPosition(250, 7);
            ExitButton.SetSize(16, 16);

            Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 120, new Color(47, 160, 193)));
            Sprites["hover"].Visible = false;

            this.OnWidgetSelected += WidgetSelected;
            this.WidgetIM.OnMouseDown += MouseDown;
            this.WidgetIM.OnHoverChanged += HoverChanged;

            SetSize(272, 120);
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (this.WidgetIM.Hovering) this.SetSelected(true);
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            Sprites["hover"].Visible = this.WidgetIM.Hovering;
        }

        public void Disconnect()
        {
            Editor.MainWindow.MapWidget.MapViewerConnections.ConnectionsPanel.Disconnect(this.MapConnection.MapID);
        }

        public void SetSelected(bool Selected)
        {
            if (this.Selected != Selected)
            {
                this.Selected = Selected;
                if (this.Selected)
                {
                    foreach (Widget w in Parent.Parent.Widgets)
                    {
                        if (((LayoutContainer) w).Widget is ConnectionWidget && ((LayoutContainer) w).Widget != this)
                            ((ConnectionWidget) ((LayoutContainer) w).Widget).SetSelected(false);
                    }
                }
                Color Color = Selected ? new Color(47, 160, 193) : Color.WHITE;
                MapLabel.SetTextColor(Color);
                XLabel.SetTextColor(Color);
                YLabel.SetTextColor(Color);
                MapBox.SetTextColor(Color);
                XBox.SetTextColor(Color);
                YBox.SetTextColor(Color);
                SetBackgroundColor(Selected ? new Color(19, 36, 55) : Color.ALPHA);
                MapConnectionWidget mcw = Editor.MainWindow.MapWidget.MapViewerConnections.ConnectionWidgets.Find(w => w.MapID == this.MapConnection.MapID);
                mcw.SetDarkOverlay((byte) (Selected ? 32 : 192));
                mcw.Outline.SetOuterColor(Selected ? new Color(53, 210, 255) : Color.ALPHA);
                mcw.Outline.SetThickness(2);
                mcw.SetZIndex(Selected ? 1 : 0);
            }
        }

        public void SetOffset(int RelativeX, int RelativeY)
        {
            MapConnectionWidget mcw = Editor.MainWindow.MapWidget.MapViewerConnections.ConnectionWidgets.Find(w => w.MapID == this.MapConnection.MapID);
            this.MapConnection.RelativeX = RelativeX;
            this.MapConnection.RelativeY = RelativeY;
            Game.MapConnection c = Game.Data.Maps[this.MapConnection.MapID].Connections.Find(conn => conn.MapID == Editor.MainWindow.MapWidget.Map.ID);
            c.RelativeX = -RelativeX;
            c.RelativeY = -RelativeY;
            mcw.RelativeX = RelativeX;
            mcw.RelativeY = RelativeY;
            Editor.MainWindow.MapWidget.MapViewerConnections.PositionMap();
        }

        public void SetConnection(Game.MapConnection Connection)
        {
            Initialized = false;
            this.MapConnection = Connection;
            string text = "[Unknown Map]";
            if (Game.Data.Maps[Connection.MapID] != null) text = Game.Data.Maps[Connection.MapID].DevName;
            MapBox.SetInitialText(text);
            XBox.SetValue(Connection.RelativeX);
            YBox.SetValue(Connection.RelativeY);
            Initialized = true;
        }

        public override void Update()
        {
            base.Update();
            MapConnectionWidget mcw = Editor.MainWindow.MapWidget.MapViewerConnections.ConnectionWidgets.Find(w => w.MapID == this.MapConnection.MapID);
            if (this.Selected && (this.SelectedWidget || mcw.SelectedWidget))
            {
                int Diff = Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LSHIFT) || Input.Press(SDL2.SDL.SDL_Keycode.SDLK_RSHIFT) ? 10 : 1;
                if (Input.Trigger(SDL2.SDL.SDL_Keycode.SDLK_LEFT) || TimerPassed("left"))
                {
                    if (TimerPassed("left")) ResetTimer("left");
                    SetOffset(this.MapConnection.RelativeX - Diff, this.MapConnection.RelativeY);
                }
                if (Input.Trigger(SDL2.SDL.SDL_Keycode.SDLK_RIGHT) || TimerPassed("right"))
                {
                    if (TimerPassed("right")) ResetTimer("right");
                    SetOffset(this.MapConnection.RelativeX + Diff, this.MapConnection.RelativeY);
                }
                if (Input.Trigger(SDL2.SDL.SDL_Keycode.SDLK_UP) || TimerPassed("up"))
                {
                    if (TimerPassed("up")) ResetTimer("up");
                    SetOffset(this.MapConnection.RelativeX, this.MapConnection.RelativeY - Diff);
                }
                if (Input.Trigger(SDL2.SDL.SDL_Keycode.SDLK_DOWN) || TimerPassed("down"))
                {
                    if (TimerPassed("down")) ResetTimer("down");
                    SetOffset(this.MapConnection.RelativeX, this.MapConnection.RelativeY + Diff);
                }

                if (Input.Press(SDL2.SDL.SDL_Keycode.SDLK_LEFT))
                {
                    if (!TimerExists("left_initial") && !TimerExists("left"))
                    {
                        SetTimer("left_initial", 300);
                    }
                    else if (TimerPassed("left_initial"))
                    {
                        DestroyTimer("left_initial");
                        SetTimer("left", 50);
                    }
                }
                else
                {
                    if (TimerExists("left")) DestroyTimer("left");
                    if (TimerExists("left_initial")) DestroyTimer("left_initial");
                }
                if (Input.Press(SDL2.SDL.SDL_Keycode.SDLK_RIGHT))
                {
                    if (!TimerExists("right_initial") && !TimerExists("right"))
                    {
                        SetTimer("right_initial", 300);
                    }
                    else if (TimerPassed("right_initial"))
                    {
                        DestroyTimer("right_initial");
                        SetTimer("right", 50);
                    }
                }
                else
                {
                    if (TimerExists("right")) DestroyTimer("right");
                    if (TimerExists("right_initial")) DestroyTimer("right_initial");
                }
                if (Input.Press(SDL2.SDL.SDL_Keycode.SDLK_UP))
                {
                    if (!TimerExists("up_initial") && !TimerExists("up"))
                    {
                        SetTimer("up_initial", 300);
                    }
                    else if (TimerPassed("up_initial"))
                    {
                        DestroyTimer("up_initial");
                        SetTimer("up", 50);
                    }
                }
                else
                {
                    if (TimerExists("up")) DestroyTimer("up");
                    if (TimerExists("up_initial")) DestroyTimer("up_initial");
                }
                if (Input.Press(SDL2.SDL.SDL_Keycode.SDLK_DOWN))
                {
                    if (!TimerExists("down_initial") && !TimerExists("down"))
                    {
                        SetTimer("down_initial", 300);
                    }
                    else if (TimerPassed("down_initial"))
                    {
                        DestroyTimer("down_initial");
                        SetTimer("down", 50);
                    }
                }
                else
                {
                    if (TimerExists("down")) DestroyTimer("down");
                    if (TimerExists("down_initial")) DestroyTimer("down_initial");
                }
            }
        }
    }

    public class ExitButton : Widget
    {
        public ExitButton(IContainer Parent) : base(Parent)
        {
            Sprites["box"] = new Sprite(this.Viewport);
            this.WidgetIM.OnMouseDown += MouseDown;
            this.WidgetIM.OnHoverChanged += HoverChanged;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (WidgetIM.Hovering) ((ConnectionWidget) Parent).Disconnect();
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            Redraw();
        }

        protected override void Draw()
        {
            if (Sprites["box"].Bitmap != null) Sprites["box"].Bitmap.Dispose();
            Color Outline = new Color(10, 23, 37);
            Color OuterCorner = new Color(255, 255, 255, 0);
            Color InnerCorner = new Color(132, 139, 146);
            Color InnerOutline = new Color(255, 255, 255);
            Color Filler = new Color(255, 255, 255);
            if (WidgetIM.Hovering)
            {
                Outline = new Color(1, 16, 20);
                OuterCorner = new Color(9, 24, 37);
                InnerCorner = new Color(12, 119, 149);
                InnerOutline = new Color(62, 200, 239);
                Filler = new Color(47, 160, 193);
            }
            Bitmap b = new Bitmap(16, 16);
            b.Unlock();
            #region Draw exit cross
            // Outer corner pieces
            b.SetPixel(0, 0, OuterCorner);
            b.SetPixel(15, 0, OuterCorner);
            b.SetPixel(0, 15, OuterCorner);
            b.SetPixel(15, 15, OuterCorner);

            // Outline
            b.SetPixel(1, 0, Outline);
            b.SetPixel(2, 0, Outline);
            b.DrawLine(3, 0, 7, 4, Outline);
            b.DrawLine(9, 4, 12, 0, Outline);
            b.SetPixel(13, 0, Outline);
            b.SetPixel(14, 0, Outline);
            b.SetPixel(15, 1, Outline);
            b.SetPixel(15, 2, Outline);
            b.DrawLine(15, 3, 11, 7, Outline);
            b.DrawLine(11, 8, 15, 12, Outline);
            b.SetPixel(15, 13, Outline);
            b.SetPixel(15, 14, Outline);
            b.SetPixel(14, 15, Outline);
            b.SetPixel(13, 15, Outline);
            b.DrawLine(12, 15, 8, 11, Outline);
            b.DrawLine(7, 11, 4, 15, Outline);
            b.SetPixel(2, 15, Outline);
            b.SetPixel(1, 15, Outline);
            b.SetPixel(0, 14, Outline);
            b.SetPixel(0, 13, Outline);
            b.DrawLine(0, 12, 4, 8, Outline);
            b.DrawLine(4, 7, 0, 3, Outline);
            b.SetPixel(0, 1, Outline);
            b.SetPixel(0, 2, Outline);

            // Inner corner pieces
            b.SetPixel(1, 1, InnerCorner);
            b.SetPixel(1, 14, InnerCorner);
            b.SetPixel(14, 1, InnerCorner);
            b.SetPixel(14, 14, InnerCorner);

            // Inner outline
            b.SetPixel(2, 1, InnerOutline);
            b.DrawLine(3, 1, 7, 5, InnerOutline);
            b.DrawLine(8, 5, 12, 1, InnerOutline);
            b.SetPixel(13, 1, InnerOutline);
            b.SetPixel(14, 2, InnerOutline);
            b.DrawLine(14, 3, 10, 7, InnerOutline);
            b.DrawLine(10, 8, 14, 12, InnerOutline);
            b.SetPixel(14, 13, InnerOutline);
            b.SetPixel(13, 14, InnerOutline);
            b.DrawLine(12, 14, 8, 10, InnerOutline);
            b.DrawLine(7, 10, 3, 14, InnerOutline);
            b.SetPixel(2, 14, InnerOutline);
            b.SetPixel(1, 13, InnerOutline);
            b.DrawLine(1, 12, 5, 8, InnerOutline);
            b.DrawLine(5, 7, 1, 3, InnerOutline);
            b.SetPixel(1, 2, InnerOutline);

            // Filler
            b.DrawLine(2, 3, 12, 13, Filler);
            b.DrawLine(2, 2, 13, 13, Filler);
            b.DrawLine(3, 2, 13, 12, Filler);
            b.DrawLine(2, 12, 12, 2, Filler);
            b.DrawLine(2, 13, 13, 2, Filler);
            b.DrawLine(3, 13, 13, 3, Filler);
            #endregion
            b.Lock();
            Sprites["box"].Bitmap = b;
            base.Draw();
        }
    }
}
