using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class ProgressWindow : PopupWindow
{
    public string Message { get; protected set; }
    public bool CloseWhenDone { get; protected set; }

    Label MessageLabel;
    ProgressBar ProgressBar;

    public ProgressWindow(string Title, string Message, bool CloseWhenDone = true)
    {
        this.Message = Message;
        this.CloseWhenDone = CloseWhenDone;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(300, CloseWhenDone ? 100 : 130);
        SetSize(MaximumSize);
        Center();

        MessageLabel = new Label(this);
        MessageLabel.SetFont(Fonts.UbuntuRegular.Use(14));
        MessageLabel.SetText(Message);
        MessageLabel.SetPosition(Size.Width / 2 - MessageLabel.Size.Width / 2, 24);

        ProgressBar = new ProgressBar(this);
        ProgressBar.SetPosition(14, 48);
        ProgressBar.SetSize(Size.Width - 14 - WindowEdges * 2, 32);

        if (!CloseWhenDone)
        {
            CreateButton("OK", _ => OK());
            Buttons[0].SetEnabled(false);
            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => OK(), true, e => e.Value = Buttons[0].Enabled)
            });
        }
    }

    private void OK()
    {
        Close();
    }

    public void SetMessage(string Message)
    {
        if (this.Message != Message)
        {
            this.Message = Message;
            MessageLabel.SetText(this.Message);
            MessageLabel.SetPosition(Size.Width / 2 - MessageLabel.Size.Width / 2, 24);
        }
    }

    public void SetProgress(float Progress)
    {
        if (ProgressBar.Progress == Progress) return;
        ProgressBar.SetProgress(Progress);
        if (CloseWhenDone && this.ProgressBar.Progress == 1) Close();
    }
}
