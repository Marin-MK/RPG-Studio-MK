using System;

namespace MKEditor.Widgets
{
    public class TextWidget : Widget
    {
        public string Text { get; protected set; } = "";

        public TextWidget(object Parent, string Name = "textWidget")
            : base(Parent, Name)
        {
            this.Text = this.Name;
        }

        public void SetText(string text)
        {
            if (this.Text != text)
            {
                this.Text = text;
                Redraw();
            }
        }
    }
}
