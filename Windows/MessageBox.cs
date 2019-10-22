using System;
using ODL;

namespace MKEditor.Widgets
{
    public class MessageBox : PopupWindow
    {
        public int Result = 0;

        MultilineLabel label;
        Button Button1;
        Button Button2;
        Button Button3;

        public MessageBox(string Title, string Message, ButtonTypes type = ButtonTypes.OK)
            : base(Graphics.Windows[0], "messageBox")
        {
            SetTitle(Title);
            
            label = new MultilineLabel(this);
            label.SetText(Message);

            if (type == ButtonTypes.OK)
            {
                Button3 = new Button(this);
                Button3.SetText("OK");
                Button3.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 0;
                    Close();
                };
            }
            else if (type == ButtonTypes.OKCancel || type == ButtonTypes.YesNo)
            {
                Button2 = new Button(this);
                Button2.SetText(type == ButtonTypes.OKCancel ? "OK" : "Yes");
                Button2.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 0;
                    Close();
                };
                Button3 = new Button(this);
                Button3.SetText(type == ButtonTypes.OKCancel ? "Cancel" : "No");
                Button3.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 1;
                    Close();
                };
            }
            else if (type == ButtonTypes.YesNoCancel)
            {
                Button1 = new Button(this);
                Button1.SetText("Yes");
                Button1.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 0;
                    Close();
                };
                Button2 = new Button(this);
                Button2.SetText("No");
                Button2.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 1;
                    Close();
                };
                Button3 = new Button(this);
                Button3.SetText("Cancel");
                Button3.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 2;
                    Close();
                };
            }

            SetSize(300, 150);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            Button3.SetPosition(Size.Width - Button3.Size.Width - 1, Size.Height - Button3.Size.Height - 1);
            if (Button2 != null) Button2.SetPosition(Button3.Position.X - Button2.Size.Width, Button3.Position.Y);
            if (Button1 != null) Button1.SetPosition(Button2.Position.X - Button1.Size.Width, Button3.Position.Y);
            label.SetSize(Size.Width - 20, 1);
            label.OnSizeChanged += delegate (object s, SizeEventArgs ev)
            {
                label.SetPosition(10, Size.Height / 2 - label.Size.Height / 2 - 6);
            };
            base.SizeChanged(sender, e);
            Center();
        }
    }

    public enum ButtonTypes
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }
}
