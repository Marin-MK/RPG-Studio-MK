using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MKEditor.Game;
using odl;
using amethyst;

namespace MKEditor.Widgets
{
    public class ConditionBox : Widget
    {
        public List<BasicCondition> Conditions;
        public bool Selectable { get; protected set; } = false;
        public int SelectedIndex
        {
            get
            {
                Widget w = StackPanel.Widgets.Find(w => ((ConditionEntryWidget) w).Selected);
                return StackPanel.Widgets.IndexOf(w);
            }
        }
        public bool Enabled { get; protected set; } = true;

        Container MainContainer;
        VStackPanel StackPanel;

        public BaseEvent OnDoubleClicked;
        public BaseEvent OnSelectionChanged;

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

        public void SetEnabled(bool Enabled)
        {
            if (this.Enabled != Enabled)
            {
                this.Enabled = Enabled;
                RedrawBox();
                foreach (ConditionEntryWidget w in StackPanel.Widgets) w.SetEnabled(Enabled);
            }
        }

        public void SetSelectable(bool Selectable)
        {
            if (this.Selectable != Selectable)
            {
                this.Selectable = Selectable;
                StackPanel.Widgets.ForEach(w => ((ConditionEntryWidget) w).SetSelectable(this.Selectable));
                if (Selectable && this.Conditions.Count > 0) this.SetSelectedIndex(0);
            }
        }

        public void SetSelectedIndex(int Index)
        {
            ((ConditionEntryWidget) StackPanel.Widgets[Index]).SetSelected(true);
        }

        public void Edit(BaseEvent Callback = null)
        {
            EditConditionsWindow edw = new EditConditionsWindow(this.Conditions);
            edw.OnClosed += delegate (BaseEventArgs e)
            {
                if (edw.NeedUpdate) SetConditions(edw.Conditions);
                Callback?.Invoke(e);
            };
        }

        public void RedrawBox()
        {
            Sprites["bg"].Bitmap?.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.DrawRect(Size, this.Enabled ? new Color(86, 108, 134) : new Color(36, 34, 36));
            Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, this.Enabled ? new Color(10, 23, 37) : new Color(72, 72, 72));
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Color DarkOutline = this.Enabled ? new Color(40, 62, 84) : new Color(36, 34, 36);
            Sprites["bg"].Bitmap.SetPixel(1, 1, DarkOutline);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, 1, DarkOutline);
            Sprites["bg"].Bitmap.SetPixel(1, Size.Height - 2, DarkOutline);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 2, Size.Height - 2, DarkOutline);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 1, Size.Width - 12, Size.Height - 2, DarkOutline);
            Sprites["bg"].Bitmap.Lock();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            RedrawBox();
            MainContainer.SetSize(Size.Width - 13, Size.Height - 4);
            StackPanel.SetWidth(MainContainer.Size.Width);
            MainContainer.VScrollBar.SetPosition(Size.Width - 10, 2);
            MainContainer.VScrollBar.SetSize(8, Size.Height - 4);
        }

        public void SetConditions(List<BasicCondition> Conditions)
        {
            this.Conditions = Conditions;
            while (StackPanel.Widgets.Count > 0) StackPanel.Widgets[0].Dispose();
            foreach (BasicCondition condition in Conditions)
            {
                ConditionEntryWidget cew = new ConditionEntryWidget(StackPanel);
                cew.SetSize(StackPanel.Size.Width, 20);
                cew.SetCondition(condition, null);
                cew.SetSelectable(this.Selectable);
                cew.SetEnabled(this.Enabled);
            }
            if (this.Selectable && this.Conditions.Count > 0) this.SetSelectedIndex(0);
        }
    }

    public class ConditionEntryWidget : Widget
    {
        public BasicCondition Condition;
        public bool Selectable { get; protected set; } = false;
        public bool Selected { get; protected set; } = false;
        public bool Enabled { get; protected set; } = true;

        DynamicLabel DynamicLabel;

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

            DynamicLabel = new DynamicLabel(this);
            DynamicLabel.SetPosition(16, 2);
        }

        public void SetEnabled(bool Enabled)
        {
            if (this.Enabled != Enabled)
            {
                this.Enabled = Enabled;
                this.DynamicLabel.SetEnabled(this.Enabled);
            }
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
                if (Selected) ((ConditionBox) Parent.Parent.Parent).OnSelectionChanged?.Invoke(new BaseEventArgs());
            }
        }

        public void SetCondition(BasicCondition Condition, ConditionUIParser Parser)
        {
            this.Condition = Condition;
            //this.DynamicLabel.SetParameters(Condition.Parameters);
            //this.DynamicLabel.SetTextFormat(Condition.Type.Text);
            //this.DynamicLabel.SetParser(Parser);
            this.DynamicLabel.SetText(Condition.Type.Text.ToString());
            this.DynamicLabel.SetColors(ConditionParser.Colors);
            this.DynamicLabel.RedrawText();
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
                if (!this.Selected)
                {
                    SetSelected(true);
                    if (TimerExists("double")) DestroyTimer("double");
                    SetTimer("double", 300);
                }
                else
                {
                    if (TimerExists("double") && !TimerPassed("double"))
                    {
                        ((ConditionBox) Parent.Parent.Parent).OnDoubleClicked?.Invoke(new BaseEventArgs());
                    }
                    else if (TimerExists("double") && TimerPassed("double"))
                    {
                        ResetTimer("double");
                    }
                    else if (!TimerExists("double"))
                    {
                        SetTimer("double", 300);
                    }
                }
            }
            else if (TimerExists("double"))
            {
                DestroyTimer("double");
            }
        }
    }
}
