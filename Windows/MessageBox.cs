using System;
using ODL;

namespace MKEditor.Widgets
{
    public class MessageBox : PopupWindow
    {
        MultilineLabel label;
        Button OKButton;

        public MessageBox(string Title, string Message)
            : base(Graphics.Windows[0], "messageBox")
        {
            SetName(Title);
            
            label = new MultilineLabel(this);
            label.SetText(Message);

            OKButton = new Button(this);
            OKButton.SetText("OK");
            OKButton.OnClicked += delegate (object sender, EventArgs e)
            {
                Close();
            };

            SetSize(300, 150);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            OKButton.SetPosition(Size.Width - OKButton.Size.Width - 1, Size.Height - OKButton.Size.Height - 1);
            label.SetSize(Size.Width - 20, 1);
            label.OnSizeChanged += delegate (object s, SizeEventArgs ev)
            {
                label.SetPosition(10, Size.Height / 2 - label.Size.Height / 2 - 6);
            };
            base.SizeChanged(sender, e);
            Center();
        }
    }
}
