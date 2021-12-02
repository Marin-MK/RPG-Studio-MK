using System;
using System.Collections.Generic;
using odl;

namespace RPGStudioMK.Widgets;

public class MessageBox : PopupWindow
{
    public int Result = 0;

    public string Message { get; protected set; }

    protected MultilineLabel label;
    protected Button Button1;
    protected Button Button2;
    protected Button Button3;
    protected PictureBox Icon;

    public ButtonType ButtonType;
    public new List<string> Buttons;
    public IconType IconType;

    public BaseEvent OnButtonPressed;

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
            Icon.Sprite.Bitmap = new Bitmap("assets/img/mbox_icons");
            Icon.Sprite.SrcRect.Width = 32;
            Icon.Sprite.SrcRect.X = 32 * ((int)IconType - 1);
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
            Button3.OnClicked += delegate (BaseEventArgs e)
            {
                Result = 0;
                this.OnButtonPressed?.Invoke(new BaseEventArgs());
                Close();
            };
        }
        else if (Buttons.Count == 2)
        {
            Button2 = new Button(this);
            Button2.SetText(Buttons[0]);
            Button2.OnClicked += delegate (BaseEventArgs e)
            {
                Result = 0;
                this.OnButtonPressed?.Invoke(new BaseEventArgs());
                Close();
            };
            Button3 = new Button(this);
            Button3.SetText(Buttons[1]);
            Button3.OnClicked += delegate (BaseEventArgs e)
            {
                Result = 1;
                this.OnButtonPressed?.Invoke(new BaseEventArgs());
                Close();
            };
        }
        else if (Buttons.Count == 3)
        {
            Button1 = new Button(this);
            Button1.SetText(Buttons[0]);
            Button1.OnClicked += delegate (BaseEventArgs e)
            {
                Result = 0;
                this.OnButtonPressed?.Invoke(new BaseEventArgs());
                Close();
            };
            Button2 = new Button(this);
            Button2.SetText(Buttons[1]);
            Button2.OnClicked += delegate (BaseEventArgs e)
            {
                Result = 1;
                this.OnButtonPressed?.Invoke(new BaseEventArgs());
                Close();
            };
            Button3 = new Button(this);
            Button3.SetText(Buttons[2]);
            Button3.OnClicked += delegate (BaseEventArgs e)
            {
                Result = 2;
                this.OnButtonPressed?.Invoke(new BaseEventArgs());
                Close();
            };
        }

        MinimumSize = MaximumSize = new Size(300, 150);
        SetSize(MaximumSize);
    }

    public MessageBox(string Title, string Message, List<string> Buttons, IconType IconType = IconType.None)
        : this(Title, Message, ButtonType.Custom, IconType, Buttons)
    {

    }

    public MessageBox(string Title, string Message, IconType IconType)
        : this(Title, Message, ButtonType.OK, IconType)
    {

    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
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
        if (Size.Height - label.Size.Height < 100)
        {
            MinimumSize = MaximumSize = new Size(Size.Width + 100, Size.Height + 50);
            SetSize(MaximumSize);
        }
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
    None = 0,
    Error = 1,
    Warning = 2,
    Info = 3
}
