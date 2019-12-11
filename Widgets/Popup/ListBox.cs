using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class ListBox : Widget
    {
        public int SelectedIndex { get { return ListDrawer.SelectedIndex; } }
        public ListItem SelectedItem { get { return ListDrawer.SelectedItem; } }
        public List<ListItem> Items { get { return ListDrawer.Items; } }

        public EventHandler<EventArgs> OnSelectionChanged
        {
            get 
            {
                return ListDrawer.OnSelectionChanged; 
            }
            set
            {
                ListDrawer.OnSelectionChanged = value;
            }
        } 

        public Container MainContainer;
        public ListDrawer ListDrawer;

        public ListBox(object Parent, string Name = "listBox")
            : base(Parent, Name)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            MainContainer = new Container(this);
            MainContainer.SetPosition(1, 2);
            MainContainer.VAutoScroll = true;
            ListDrawer = new ListDrawer(MainContainer);
            VScrollBar vs = new VScrollBar(this);
            MainContainer.SetVScrollBar(vs);
            SetSize(132, 174);
        }

        public void SetButtonText(string Text)
        {
            ListDrawer.SetButton(true, Text);
        }

        public void SetItems(List<ListItem> Items)
        {
            ListDrawer.SetItems(Items);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
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
            MainContainer.VScrollBar.SetPosition(Size.Width - 10, 2);
            ListDrawer.SetWidth(MainContainer.Size.Width);
            MainContainer.VScrollBar.SetSize(8, Size.Height - 4);
        }

        public void SetSelectedIndex(int idx)
        {
            ListDrawer.SetSelectedIndex(idx);
        }

        public override void Redraw()
        {
            base.Redraw();
            ListDrawer.Redraw();
        }
    }
}
