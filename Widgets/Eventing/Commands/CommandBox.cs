using System;
using System.Collections.Generic;
using System.Text;
using ODL;
using MKEditor.Game;

namespace MKEditor.Widgets
{
    public class CommandBox : Widget
    {
        public Event EventData { get; protected set; }
        public EventPage PageData { get; protected set; }
        public int SelectedIndex
        {
            get
            {
                Widget w = StackPanel.Widgets.Find(w => ((CommandAPIHandlerWidget) w).Selected);
                return StackPanel.Widgets.IndexOf(w);
            }
        }

        Container MainContainer;
        VStackPanel StackPanel;

        public BaseEvent OnSelectionChanged;
        public BaseEvent OnDoubleClicked;

        public CommandBox(IContainer Parent) : base(Parent)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            MainContainer = new Container(this);
            MainContainer.SetPosition(1, 2);
            MainContainer.VAutoScroll = true;
            VScrollBar vs = new VScrollBar(this);
            vs.ScrollStep = 4;
            MainContainer.SetVScrollBar(vs);
            StackPanel = new VStackPanel(MainContainer);
            this.OnDoubleClicked += delegate (BaseEventArgs e)
            {
                EditCommand();
            };
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

        public void SetEventPage(Event EventData, EventPage PageData)
        {
            this.EventData = EventData;
            this.PageData = PageData;
            while (StackPanel.Widgets.Count > 0) StackPanel.Widgets[0].Dispose();
            for (int i = 0; i < PageData.Commands.Count; i++)
            {
                BasicCommand cmd = PageData.Commands[i];
                CommandAPIHandlerWidget cew = new CommandAPIHandlerWidget(StackPanel);
                cew.SetWidth(Size.Width - 13);
                cew.SetCommand(cmd);
            }
        }

        public void EditCommand()
        {
            CommandAPIHandlerWidget chw = (CommandAPIHandlerWidget) StackPanel.Widgets[SelectedIndex];
            chw.EditWindow();
        }
    }
}
