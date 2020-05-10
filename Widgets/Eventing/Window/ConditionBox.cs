using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class ConditionBox : Widget
    {
        public Event EventData;
        public EventPage PageData;
        public bool Selectable { get; protected set; } = false;
        public int SelectedIndex
        {
            get
            {
                Widget w = StackPanel.Widgets.Find(w => ((ConditionEntryWidget) w).Selected);
                return StackPanel.Widgets.IndexOf(w);
            }
        }

        Container MainContainer;
        VStackPanel StackPanel;

        public ConditionBox(IContainer Parent) : base(Parent)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            MainContainer = new Container(this);
            MainContainer.SetPosition(1, 2);
            MainContainer.VAutoScroll = true;
            VScrollBar vs = new VScrollBar(this);
            vs.ScrollStep = 4;
            MainContainer.SetVScrollBar(vs);
            StackPanel = new VStackPanel(MainContainer);
        }

        public void SetSelectable(bool Selectable)
        {
            if (this.Selectable != Selectable)
            {
                this.Selectable = Selectable;
                StackPanel.Widgets.ForEach(w => ((ConditionEntryWidget) w).SetSelectable(this.Selectable));
                this.SetSelectedIndex(0);
            }
        }

        public void SetSelectedIndex(int Index)
        {
            ((ConditionEntryWidget) StackPanel.Widgets[Index]).SetSelected(true);
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            Sprites["bg"].Bitmap?.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.DrawRect(Size, 86, 108, 134);
            Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, 10, 23, 37);
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(1, 1, 40, 62, 84);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 1, 40, 62, 84);
            Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 2, 40, 62, 84);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, 40, 62, 84);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 1, Size.Width - 12, Size.Height - 2, 40, 62, 84);
            Sprites["bg"].Bitmap.Lock();
            MainContainer.SetSize(Size.Width - 13, Size.Height - 4);
            StackPanel.SetWidth(MainContainer.Size.Width);
            MainContainer.VScrollBar.SetPosition(Size.Width - 10, 2);
            MainContainer.VScrollBar.SetSize(8, Size.Height - 4);
        }

        public void SetEventPage(Event Event, EventPage Page)
        {
            this.EventData = Event;
            this.PageData = Page;
            while (StackPanel.Widgets.Count > 0) StackPanel.Widgets[0].Dispose();
            foreach (BasicCondition condition in Page.Conditions)
            {
                ConditionEntryWidget cew = new ConditionEntryWidget(StackPanel);
                cew.SetSize(StackPanel.Size.Width, 20);
                cew.SetCondition(condition);
                cew.SetSelectable(this.Selectable);
            }
            if (this.Selectable && Page.Conditions.Count > 0) this.SetSelectedIndex(0);
        }
    }

    public class ConditionEntryWidget : Widget
    {
        public BasicCondition Condition;
        public bool Selectable { get; protected set; } = false;
        public bool Selected { get; protected set; } = false;

        public ConditionEntryWidget(IContainer Parent) : base(Parent)
        {
            Sprites["box"] = new Sprite(this.Viewport);
            Sprites["box"].Bitmap = new Bitmap(4, 4);
            Sprites["box"].Bitmap.Unlock();
            Sprites["box"].Bitmap.FillRect(0, 1, 4, 2, Color.WHITE);
            Sprites["box"].Bitmap.FillRect(1, 0, 2, 4, Color.WHITE);
            Sprites["box"].Bitmap.Lock();
            Sprites["box"].X = 7;
            Sprites["box"].Y = 7;

            Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 20, new Color(55, 187, 255)));
            Sprites["hover"].Visible = false;

            Sprites["text"] = new Sprite(this.Viewport);
            Sprites["text"].X = 16;
            Sprites["text"].Y = 2;
        }

        public void SetSelectable(bool Selectable)
        {
            this.Selectable = Selectable;
            MouseMoving(Graphics.LastMouseEvent);
        }

        public void SetSelected(bool Selected)
        {
            if (this.Selected != Selected)
            {
                Parent.Widgets.ForEach(w =>
                {
                    if (w != this) ((ConditionEntryWidget) w).SetSelected(false);
                });
                this.Selected = Selected;
                SetBackgroundColor(this.Selected ? new Color(28, 50, 73) : Color.ALPHA);
                Sprites["hover"].Visible = this.Selectable && this.WidgetIM.Hovering;
            }
        }

        public void SetCondition(BasicCondition Condition)
        {
            this.Condition = Condition;
            Sprites["text"].Bitmap?.Dispose();
            Sprites["text"].Bitmap = new Bitmap(Size.Width - Sprites["text"].X, Size.Height - Sprites["text"].Y);
            Sprites["text"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Font = Font.Get("Fonts/ProductSans-M", 12);
            int x = 0;
            string text = Condition.ToString();
            Color color = ConditionParser.Colors[0];
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '[' && text[i + 1] == 'c' && text[i + 2] == '=')
                {
                    int startnum = i + 3;
                    int endnum = -1;
                    for (int j = startnum; j < text.Length; j++)
                    {
                        if (!Utilities.IsNumeric(text[j]))
                        {
                            endnum = j;
                            break;
                        }
                    }
                    if (endnum != -1)
                    {
                        int idx = 0;
                        if (endnum != startnum) idx = Convert.ToInt32(text.Substring(startnum, endnum - startnum));
                        if (idx < ConditionParser.Colors.Count) color = ConditionParser.Colors[idx];
                        else throw new Exception($"Only {ConditionParser.Colors.Count} defined colors; Index {idx} out of range.");
                    }
                    i = endnum;
                    continue;
                }
                Sprites["text"].Bitmap.DrawText(text[i].ToString(), x, 0, color);
                x += Sprites["text"].Bitmap.TextSize(text[i]).Width;
            }
            Sprites["text"].Bitmap.Lock();
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            Sprites["hover"].Visible = this.Selectable && this.WidgetIM.Hovering;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering && e.LeftButton != e.OldLeftButton && this.Selectable)
            {
                SetSelected(true);
            }
        }
    }
}
