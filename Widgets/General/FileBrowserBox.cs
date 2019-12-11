using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class FileBrowserBox : Widget
    {
        public string Text { get { return TextArea.Text; } }

        public TextArea TextArea;

        public EventHandler<EventArgs> OnTextChanged { get { return TextArea.OnTextChanged; } set { TextArea.OnTextChanged = value; } }

        public EventHandler<ObjectEventArgs> OnFileChosen;

        public FileBrowserBox(object Parent, string Name = "fileBrowserBox")
            : base(Parent, Name)
        {
            Sprites["bg"] = new Sprite(this.Viewport);
            TextArea = new TextArea(this);
            TextArea.SetPosition(3, 3);
            TextArea.SetCaretHeight(13);
            SetSize(100, 21);

            WidgetIM.OnMouseDown += MouseDown;
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TextArea.SetSize(this.Size.Width - 21, this.Size.Height - 3);
        }

        public void SetInitialText(string Text)
        {
            this.TextArea.SetInitialText(Text);
        }

        public void SetFont(Font f)
        {
            this.TextArea.SetFont(f);
        }

        protected override void Draw()
        {
            if (Sprites["bg"].Bitmap != null) Sprites["bg"].Bitmap.Dispose();
            Sprites["bg"].Bitmap = new Bitmap(Size);
            Sprites["bg"].Bitmap.Unlock();
            Sprites["bg"].Bitmap.DrawRect(0, 0, Size.Width, Size.Height, 10, 23, 37);
            Sprites["bg"].Bitmap.SetPixel(0, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, 0, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(0, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 1, Size.Height - 1, Color.ALPHA);
            Sprites["bg"].Bitmap.FillRect(1, 1, Size.Width - 2, Size.Height - 2, 54, 81, 108);
            Sprites["bg"].Bitmap.FillRect(Size.Width - 18, 1, 17, 19, 28, 50, 73);
            Sprites["bg"].Bitmap.FillRect(Size.Width - 13, 9, 7, 2, Color.WHITE);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 12, 11, Size.Width - 8, 11, Color.WHITE);
            Sprites["bg"].Bitmap.DrawLine(Size.Width - 11, 12, Size.Width - 9, 12, Color.WHITE);
            Sprites["bg"].Bitmap.SetPixel(Size.Width - 10, 13, Color.WHITE);
            Sprites["bg"].Bitmap.Lock();
            base.Draw();
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (!TextArea.WidgetIM.Hovering && TextArea.SelectedWidget)
            {
                Window.UI.SetSelectedWidget(null);
            }
            int rx = e.X - Viewport.X;
            int ry = e.Y - Viewport.Y;
            if (rx >= Size.Width - 18 && rx < Size.Width - 1 &&
                ry >= 1 && ry < Size.Height - 1)
            {
                OpenFile of = new OpenFile();
                of.SetFilters(new List<FileFilter>()
                {
                    new FileFilter("PNG Image", "png")
                });
                of.SetInitialDirectory(Game.Data.ProjectPath + "\\gfx\\tilesets");
                of.SetTitle("Pick a tileset...");
                object result = of.Show();
                if (result != null)
                {
                    if (OnFileChosen != null) OnFileChosen.Invoke(null, new ObjectEventArgs(result));
                }
            };
        }
    }
}
