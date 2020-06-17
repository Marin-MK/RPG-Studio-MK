using System;
using odl;
using RPGStudioMK.Game;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class EventListEntryWidget : Widget
    {
        public bool Selected { get; protected set; } = false;
        public Event EventData;

        public EventListEntryWidget(IContainer Parent) : base(Parent)
        {
            Sprites["graphic"] = new Sprite(this.Viewport);
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].X = 34;
            Sprites["text"].Y = 5;
            SetSize(272, 32);
        }

        public void SetEvent(Event Event)
        {
            this.EventData = Event;
            string text = $"{Utilities.Digits(Event.ID, 3)}: {Event.Name}";
            Font f = Font.Get("Fonts/Ubuntu-B", 14);
            Size s = f.TextSize(text);
            Sprites["text"].Bitmap?.Dispose();
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.DrawText(text, Color.WHITE);
            Sprites["text"].Bitmap.Lock();
            Sprites["graphic"].Bitmap?.Dispose();
            if (Event.Pages.Count > 0)
            {
                EventGraphic gfx = Event.Pages[0].Graphic;
                if (gfx.Type == ":file")
                {
                    if (System.IO.File.Exists(Data.ProjectPath + "/" + gfx.Param.ToString() + ".png"))
                    {
                        Sprites["graphic"].Bitmap = new Bitmap(Data.ProjectPath + "/" + gfx.Param.ToString());
                        int dir = 0;
                        if (gfx.NumDirections == 4) dir = gfx.Direction / 2 - 1;
                        if (gfx.NumDirections == 8) dir = gfx.Direction - 1;
                        Sprites["graphic"].SrcRect.Width = Sprites["graphic"].Bitmap.Width / gfx.NumFrames;
                        Sprites["graphic"].SrcRect.Height = Sprites["graphic"].Bitmap.Height / gfx.NumDirections;
                        Sprites["graphic"].SrcRect.Y = Sprites["graphic"].SrcRect.Height * dir;
                    }
                }
                Sprites["graphic"].X = 4;
                Sprites["graphic"].Y = 4;
                if (Sprites["graphic"].Bitmap != null)
                {
                    int max = Math.Max(Sprites["graphic"].SrcRect.Width, Sprites["graphic"].SrcRect.Height);
                    double perc = 24d / max;
                    Sprites["graphic"].ZoomX = perc;
                    Sprites["graphic"].ZoomY = perc;
                }
            }
        }

        public void SetSelected(bool Selected)
        {
            if (this.Selected != Selected)
            {
                this.Selected = Selected;
                if (this.Selected)
                {
                    foreach (EventListEntryWidget w in Parent.Widgets)
                    {
                        if (w != this) w.SetSelected(false);
                    }
                }
                Sprites["text"].Color = Selected ? new Color(61, 184, 232) : Color.WHITE;
                SetBackgroundColor(Selected ? new Color(19, 36, 55) : Color.ALPHA);
            }
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering && e.LeftButton != e.OldLeftButton && !Selected)
            {
                SetSelected(true);
                Editor.MainWindow.EventingWidget.MapViewer.SelectEvent(EventData);
            }
        }
    }
}
