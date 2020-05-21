using MKEditor.Game;
using System;
using System.Collections.Generic;
using System.Text;
using ODL;

namespace MKEditor.Widgets
{
    public class CommandHandlerWidget : Widget
    {
        public BasicCommand Command;
        public CommandUIParser UIParser;
        public bool Selected { get; protected set; } = false;

        Label HeaderLabel;
        public Container WidgetContainer;

        public CommandHandlerWidget(IContainer Parent) : base(Parent)
        {
            Sprites["bullet"] = new Sprite(this.Viewport);
            Sprites["bullet"].Bitmap = new Bitmap(4, 4);
            Sprites["bullet"].Bitmap.Unlock();
            Sprites["bullet"].Bitmap.FillRect(1, 0, 2, 4, Color.WHITE);
            Sprites["bullet"].Bitmap.FillRect(0, 1, 4, 2, Color.WHITE);
            Sprites["bullet"].Bitmap.Lock();

            Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, this.Size.Height, new Color(55, 187, 255)));
            Sprites["hover"].Visible = false;

            HeaderLabel = new Label(this);
            HeaderLabel.SetPosition(19, 2);

            WidgetContainer = new Container(this);
            WidgetContainer.SetPosition(19, 20);
            WidgetContainer.AutoResize = true;
        }

        public void SetCommand(BasicCommand Command)
        {
            this.Command = Command;
            this.UIParser = new CommandUIParser(Command.Type.UI, this);
            this.UIParser.CreateReadonlyWidget();
            this.UIParser.Load(Command);
            this.Sprites["bullet"].X = Command.Indent * 24 + 6;
            this.Sprites["bullet"].Y = 9;
            this.Sprites["bullet"].Visible = this.UIParser.Bullet;
            this.HeaderLabel.SetText(this.UIParser.HeaderText?? Command.Type.Name);
            this.HeaderLabel.SetTextColor(this.UIParser.HeaderColor);
            this.HeaderLabel.SetVisible(this.UIParser.HeaderVisible);
            if (this.UIParser.Height != -1) WidgetContainer.SetHeight(this.UIParser.Height);
            else WidgetContainer.UpdateAutoScroll();
            WidgetContainer.SetPosition(WidgetContainer.Position.X, this.UIParser.HeaderVisible ? 20 : 2);
            this.SetHeight(WidgetContainer.Position.Y + WidgetContainer.Size.Height + 2);
            ((SolidBitmap) this.Sprites["hover"].Bitmap).SetSize(2, this.Size.Height);
        }

        public void UpdateSize()
        {
            WidgetContainer.UpdateAutoScroll();
            this.SetHeight(WidgetContainer.Position.Y + WidgetContainer.Size.Height + 2);
            ((SolidBitmap) this.Sprites["hover"].Bitmap).SetSize(2, this.Size.Height);
        }

        public void EditWindow()
        {
            this.UIParser.OpenEditWindow();
            this.UIParser.Window.SetTitle(this.UIParser.Title?? this.Command.Type.Name);
            this.UIParser.Load(this.Command);
        }

        public void SetSelected(bool Selected)
        {
            if (this.Selected != Selected)
            {
                Parent.Widgets.ForEach(w =>
                {
                    if (w != this) ((CommandHandlerWidget) w).SetSelected(false);
                });
                this.Selected = Selected;
                SetBackgroundColor(this.Selected ? new Color(28, 50, 73) : Color.ALPHA);
                Sprites["hover"].Visible = this.WidgetIM.Hovering;
                if (Selected) ((CommandBox) Parent.Parent.Parent).OnSelectionChanged?.Invoke(new BaseEventArgs());
            }
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
            Sprites["hover"].Visible = this.WidgetIM.Hovering;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (WidgetIM.Hovering && e.LeftButton != e.OldLeftButton)
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
                        ((CommandBox) Parent.Parent.Parent).OnDoubleClicked?.Invoke(new BaseEventArgs());
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

        public void UpdateParentList()
        {
            UIParser.Load(this.Command);
        }
    }
}
