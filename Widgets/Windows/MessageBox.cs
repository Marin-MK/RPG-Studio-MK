using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class MessageBox : PopupWindow
    {
        public int Result = 0;

        public string Message { get; protected set; }

        MultilineLabel label;
        Button Button1;
        Button Button2;
        Button Button3;
        PictureBox Icon;

        public ButtonType ButtonType;
        public List<string> Buttons;
        public IconType IconType;

        public EventHandler<EventArgs> OnButtonPressed;

        public MessageBox(string Title, string Message, ButtonType type = ButtonType.OK, IconType IconType = IconType.None, List<string> _buttons = null)
        {
            this.ButtonType = type;
            this.Buttons = _buttons;
            this.IconType = IconType;

            SetTitle(Title);
            this.Message = Message;

            if (IconType != IconType.None)
            {
                Icon = new PictureBox(this);
                Icon.Sprite.Bitmap = new Bitmap("mbox_icons");
                Icon.Sprite.SrcRect.Width = 32;
                Icon.Sprite.SrcRect.X = 32 * (int) IconType;
            }

            label = new MultilineLabel(this);
            label.SetText(Message);
            
            switch (ButtonType)
            {
                case ButtonType.OK:
                    Buttons = new List<string>() { "OK" };
                    break;
                case ButtonType.OKCancel:
                    Buttons = new List<string>() { "OK", "Cancel" };
                    break;
                case ButtonType.YesNo:
                    Buttons = new List<string>() { "Yes", "No" };
                    break;
                case ButtonType.YesNoCancel:
                    Buttons = new List<string>() { "Yes", "No", "Cancel" };
                    break;
                case ButtonType.Custom: break;
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

        public MessageBox(string Title, string Message, List<string> Buttons, IconType IconType = IconType.None)
            : this(Title, Message, ButtonType.Custom, IconType, Buttons)
        {

        }

        public MessageBox(string Title, string Message, IconType IconType)
            : this(Title, Message, ButtonType.OK, IconType)
        {

        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            Button3.SetPosition(Size.Width - Button3.Size.Width - 1, Size.Height - Button3.Size.Height - 1);
            if (Button2 != null) Button2.SetPosition(Button3.Position.X - Button2.Size.Width, Button3.Position.Y);
            if (Button1 != null) Button1.SetPosition(Button2.Position.X - Button1.Size.Width, Button3.Position.Y);
            int x = 10;
            if (Icon != null)
            {
                x = 60;
            }
            label.SetSize(Size.Width - 10 - x, 1);
            label.RedrawText();
            label.SetPosition(x, Size.Height / 2 - label.Size.Height / 2 - 2);
            if (Icon != null)
            {
                Icon.SetPosition(14, Size.Height / 2 - 24);
            }
            if (Size.Height - label.Size.Height < 100) SetSize(Size.Width + 100, Size.Height + 50);
            Center();
        }
    }

    public enum ButtonType
    {
        Custom,
        OK,
        OKCancel,
        YesNo,
        YesNoCancel
    }

    public enum IconType
    {
        None = -1,
        Error = 0,
        Warning = 1,
        Info = 2
    }
}
