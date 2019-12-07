using System;
using System.Collections.Generic;
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

        public ButtonTypes ButtonType;
        public List<string> Buttons;

        public EventHandler<EventArgs> OnButtonPressed;

        public MessageBox(string Title, string Message, ButtonTypes type = ButtonTypes.OK, List<string> _buttons = null)
            : base(Graphics.Windows[0], "messageBox")
        {
            this.ButtonType = type;
            this.Buttons = _buttons;
            SetTitle(Title);

            label = new MultilineLabel(this);
            label.SetText(Message);
            
            switch (ButtonType)
            {
                case ButtonTypes.OK:
                    Buttons = new List<string>() { "OK" };
                    break;
                case ButtonTypes.OKCancel:
                    Buttons = new List<string>() { "OK", "Cancel" };
                    break;
                case ButtonTypes.YesNo:
                    Buttons = new List<string>() { "Yes", "No" };
                    break;
                case ButtonTypes.YesNoCancel:
                    Buttons = new List<string>() { "Yes", "No", "Cancel" };
                    break;
                case ButtonTypes.Custom: break;
                default:
                    throw new Exception("Invalid ButtonType");
            }

            if (Buttons.Count == 1)
            {
                Button3 = new Button(this);
                Button3.SetText(Buttons[0]);
                Button3.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 0;
                    if (OnButtonPressed != null) OnButtonPressed.Invoke(null, new EventArgs());
                    Close();
                };
            }
            else if (Buttons.Count == 2)
            {
                Button2 = new Button(this);
                Button2.SetText(Buttons[0]);
                Button2.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 0;
                    if (OnButtonPressed != null) OnButtonPressed.Invoke(null, new EventArgs());
                    Close();
                };
                Button3 = new Button(this);
                Button3.SetText(Buttons[1]);
                Button3.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 1;
                    if (OnButtonPressed != null) OnButtonPressed.Invoke(null, new EventArgs());
                    Close();
                };
            }
            else if (Buttons.Count == 3)
            {
                Button1 = new Button(this);
                Button1.SetText(Buttons[0]);
                Button1.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 0;
                    if (OnButtonPressed != null) OnButtonPressed.Invoke(null, new EventArgs());
                    Close();
                };
                Button2 = new Button(this);
                Button2.SetText(Buttons[1]);
                Button2.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 1;
                    if (OnButtonPressed != null) OnButtonPressed.Invoke(null, new EventArgs());
                    Close();
                };
                Button3 = new Button(this);
                Button3.SetText(Buttons[2]);
                Button3.OnClicked += delegate (object sender, EventArgs e)
                {
                    Result = 2;
                    if (OnButtonPressed != null) OnButtonPressed.Invoke(null, new EventArgs());
                    Close();
                };
            }

            SetSize(300, 150);
        }

        public MessageBox(string Title, string Message, List<string> Buttons)
            : this(Title, Message, ButtonTypes.Custom, Buttons)
        {

        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            Button3.SetPosition(Size.Width - Button3.Size.Width - 1, Size.Height - Button3.Size.Height - 1);
            if (Button2 != null) Button2.SetPosition(Button3.Position.X - Button2.Size.Width, Button3.Position.Y);
            if (Button1 != null) Button1.SetPosition(Button2.Position.X - Button1.Size.Width, Button3.Position.Y);
            label.SetSize(Size.Width - 20, 1);
            label.RedrawText();
            label.SetPosition(10, Size.Height / 2 - label.Size.Height / 2 - 2);
            if (Size.Height - label.Size.Height < 100) SetSize(Size.Width + 100, Size.Height + 50);
            Center();
        }
    }

    public enum ButtonTypes
    {
        Custom,
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }
}
