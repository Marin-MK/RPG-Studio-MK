using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class StatusBar : Widget
    {
        public MapViewer MapViewer;

        public ZoomControl ZoomControl;

        List<Message> Queue = new List<Message>();

        int DrawnX = -1;
        int DrawnY = -1;

        public StatusBar(object Parent, string Name = "statusBar")
            : base(Parent, Name)
        {
            Viewport.Name = "StatusBar";
            SetBackgroundColor(10, 23, 37);
            Sprites["map"] = new Sprite(this.Viewport);
            Sprites["map"].X = 4;
            Sprites["map"].Y = 3;
            Sprites["map"].Name = "MapText";
            Sprites["line1"] = new Sprite(this.Viewport, new SolidBitmap(1, 20, new Color(28, 50, 73)));
            Sprites["line1"].X = 296;
            Sprites["line1"].Y = 3;
            Sprites["line1"].Name = "Divider 1";
            Sprites["line2"] = new Sprite(this.Viewport, new SolidBitmap(1, 20, new Color(28, 50, 73)));
            Sprites["line2"].X = Size.Width - 284;
            Sprites["line2"].Y = 3;
            Sprites["line2"].Name = "Divider 2";
            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].X = 300;
            Sprites["text"].Y = 3;
            Sprites["text"].Name = "MessageText";
            Sprites["cursor"] = new Sprite(this.Viewport);
            Sprites["cursor"].Y = 3;
            Sprites["cursor"].Name = "CursorText";

            ZoomControl = new ZoomControl(this);
        }

        public void SetMap(Data.Map Map)
        {
            if (Sprites["map"].Bitmap != null) Sprites["map"].Bitmap.Dispose();
            Font f = Font.Get("Fonts/ProductSans-M", 14);
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
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            Size s = f.TextSize(Text);
            Sprites["text"].Bitmap = new Bitmap(s);
            Sprites["text"].Bitmap.Font = f;
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.DrawText(Text, Color.WHITE);
            Sprites["text"].Bitmap.Lock();
        }

        public void RemoveText()
        {
            Sprites["text"].Bitmap.Dispose();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            Sprites["line2"].X = Size.Width - 284;
            Sprites["cursor"].X = Size.Width - 280;
            ZoomControl.SetPosition(Size.Width - 290 - 88, 0);
        }

        public void DrawCursorPosition(int X, int Y)
        {
            if (Sprites["cursor"].Bitmap != null) Sprites["cursor"].Bitmap.Dispose();
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            string text = $"Cursor position ({Utilities.Digits(X, 3)}x{Utilities.Digits(Y, 3)})";
            Size s = f.TextSize(text);
            Sprites["cursor"].Bitmap = new Bitmap(s);
            Sprites["cursor"].Bitmap.Font = f;
            Sprites["cursor"].Bitmap.Unlock();
            Sprites["cursor"].Bitmap.DrawText(text, Color.WHITE);
            Sprites["cursor"].Bitmap.Lock();
            DrawnX = X;
            DrawnY = Y;
        }

        public void RemoveCursorText()
        {
            Sprites["cursor"].Bitmap.Dispose();
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
            if (MapViewer != null)
            {
                if (MapViewer.MapTileX < 0 || MapViewer.MapTileY < 0 || MapViewer.MapTileX >= MapViewer.Map.Width || MapViewer.MapTileY >= MapViewer.Map.Height)
                {
                    RemoveCursorText();
                }
                else if (MapViewer.MapTileX != DrawnX || MapViewer.MapTileY != DrawnY || Sprites["cursor"].Bitmap == null)
                {
                    DrawCursorPosition(MapViewer.MapTileX, MapViewer.MapTileY);
                }
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
}
