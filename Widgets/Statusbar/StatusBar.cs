using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class StatusBar : Widget
{
    public ZoomControl ZoomControl;

    List<Message> Queue = new List<Message>();

    int DrawnX = -1;
    int DrawnY = -1;
    int DrawnWidth = -1;
    int DrawnHeight = -1;

    public StatusBar(IContainer Parent) : base(Parent)
    {
        SetBackgroundColor(10, 23, 37);
        Sprites["map"] = new Sprite(this.Viewport);
        Sprites["map"].X = 4;
        Sprites["map"].Y = 3;
        Sprites["line1"] = new Sprite(this.Viewport, new SolidBitmap(1, 20, new Color(28, 50, 73)));
        Sprites["line1"].X = 296;
        Sprites["line1"].Y = 3;
        Sprites["line2"] = new Sprite(this.Viewport, new SolidBitmap(1, 20, new Color(28, 50, 73)));
        Sprites["line2"].Y = 3;
        Sprites["line3"] = new Sprite(this.Viewport, new SolidBitmap(1, 20, new Color(28, 50, 73)));
        Sprites["line3"].Y = 3;
        Sprites["text"] = new Sprite(this.Viewport);
        Sprites["text"].Y = 3;
        Sprites["cursor"] = new Sprite(this.Viewport);
        Sprites["cursor"].X = 304;
        Sprites["cursor"].Y = 3;

        ZoomControl = new ZoomControl(this);
    }

    public void SetMap(Map Map)
    {
        if (Sprites["map"].Bitmap != null) Sprites["map"].Bitmap.Dispose();
        Font f = Fonts.ProductSansMedium.Use(11);
        string text = $"{Utilities.Digits(Map.ID, 3)}: {Map.Name} ({Map.Width}x{Map.Height})";
        Size s = f.TextSize(text);
        Sprites["map"].Bitmap = new Bitmap(s);
        Sprites["map"].Bitmap.Font = f;
        Sprites["map"].Bitmap.Unlock();
        Sprites["map"].Bitmap.DrawText(text, Color.WHITE);
        Sprites["map"].Bitmap.Lock();
    }

    public void DrawText(string Text)
    {
        if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
        Font f = Fonts.ProductSansMedium.Use(11);
        Size s = f.TextSize(Text);
        Sprites["text"].Bitmap = new Bitmap(s);
        Sprites["text"].Bitmap.Font = f;
        Sprites["text"].Bitmap.Unlock();
        Sprites["text"].Bitmap.DrawText(Text, Color.WHITE);
        Sprites["text"].Bitmap.Lock();
    }

    public void RemoveText()
    {
        if (Sprites["text"].Bitmap != null) Sprites["text"].Bitmap.Dispose();
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        Sprites["line2"].X = Size.Width - 290 - ZoomControl.Size.Width - 6;
        Sprites["line3"].X = Size.Width - 284;
        Sprites["text"].X = Size.Width - 276;
        ZoomControl.SetPosition(Size.Width - 290 - ZoomControl.Size.Width, 0);
    }

    public void DrawCursor(int X, int Y, int width, int height)
    {
        if (Sprites["cursor"].Bitmap != null) Sprites["cursor"].Bitmap.Dispose();
        Font f = Fonts.ProductSansMedium.Use(11);
        string text = $"{Utilities.Digits(X, 3)}x{Utilities.Digits(Y, 3)}";
        MapViewer mv = Editor.MainWindow.MapWidget.MapViewer;
        if (mv.Mode == MapMode.Tiles)
        {
            if (width != 0 || height != 0) text += $" (size {width + 1},{height + 1})";
        }
        else if (mv.Mode == MapMode.Events)
        {
            foreach (Event e in mv.Map.Events.Values)
            {
                if (e.X == mv.MapTileX && e.Y == mv.MapTileY)
                {
                    text += $" (event {Utilities.Digits(e.ID, 3)})";
                    break;
                }
            }
        }
        Size s = f.TextSize(text);
        Sprites["cursor"].Bitmap = new Bitmap(s);
        Sprites["cursor"].Bitmap.Font = f;
        Sprites["cursor"].Bitmap.Unlock();
        Sprites["cursor"].Bitmap.DrawText(text, Color.WHITE);
        Sprites["cursor"].Bitmap.Lock();
        DrawnX = X;
        DrawnY = Y;
        DrawnWidth = width;
        DrawnHeight = height;
    }

    public void RemoveCursorText()
    {
        if (Sprites["cursor"].Bitmap != null) Sprites["cursor"].Bitmap.Dispose();
        DrawnX = -1;
        DrawnY = -1;
    }

    public override void Update()
    {
        base.Update();
        if (TimerPassed("timer"))
        {
            DestroyTimer("timer");
            Queue.RemoveAt(0);
            if (Queue.Count > 0)
            {
                bool started = false;
                for (int i = 0; i < Queue.Count; i++)
                {
                    if (Queue[i].Priority) // Show a priority message
                    {
                        SetTimer("timer", Queue[i].Time);
                        DrawText(Queue[i].Text);
                        started = true;
                        break;
                    }
                }
                if (!started) // No other priority messages queued
                {
                    SetTimer("timer", Queue[0].Time);
                    DrawText(Queue[0].Text);
                }
            }
            else // No other messages queued
            {
                RemoveText();
            }
        }
        if (Editor.MainWindow.MapWidget != null)
        {
            MapViewer MapViewer = Editor.MainWindow.MapWidget.MapViewer;
            if (!MapViewer.MainContainer.Mouse.Inside)
            {
                RemoveCursorText();
            }
            else if (MapViewer.TopLeftX != DrawnX || MapViewer.TopLeftY != DrawnY ||
                     MapViewer.CursorWidth != DrawnWidth || MapViewer.CursorHeight != DrawnHeight ||
                     Sprites["cursor"].Bitmap == null)
            {

                DrawCursor(MapViewer.TopLeftX, MapViewer.TopLeftY, MapViewer.CursorWidth, MapViewer.CursorHeight);
            }
        }
        else
        {
            RemoveCursorText();
        }
    }

    public void Refresh()
    {
        if (Editor.MainWindow.MapWidget != null)
        {
            SetMap(Editor.MainWindow.MapWidget.Map);
            ZoomControl.SetVisible(true);
            Sprites["line2"].Visible = true;
        }
        else
        {
            if (Sprites["map"].Bitmap != null) Sprites["map"].Bitmap.Dispose();
            ZoomControl.SetVisible(false);
            Sprites["line2"].Visible = false;
        }
    }

    public void QueueMessage(string Text, bool Priority = false, long Time = 2000)
    {
        Queue.Add(new Message(Text, Priority, Time));
        if (!TimerExists("timer"))
        {
            SetTimer("timer", Time);
            DrawText(Text);
        }
        else
        {
            if (!Queue[0].Priority && Priority)
            {
                DestroyTimer("timer");
                Queue.RemoveAt(0);
                SetTimer("timer", Time);
                DrawText(Text);
            }
        }
    }
}

class Message
{
    public string Text;
    public bool Priority;
    public long Time;

    public Message(string Text, bool Priority, long Time)
    {
        this.Text = Text;
        this.Priority = Priority;
        this.Time = Time;
    }
}
