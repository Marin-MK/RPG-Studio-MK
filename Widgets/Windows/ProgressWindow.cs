using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class ProgressWindow : PopupWindow
{
    public string Message { get; protected set; }
    public bool CloseWhenDone { get; protected set; }

    public Action OnFinished;
    public Action OnCancelled;

    Label MessageLabel;
    ProgressBar ProgressBar;

    public ProgressWindow(string Title, string Message, bool CloseWhenDone = true, bool Cancellable = true)
    {
        this.Message = Message;
        this.CloseWhenDone = CloseWhenDone;

        SetTitle(Title);
        MinimumSize = MaximumSize = new Size(300, CloseWhenDone && !Cancellable ? 110 : 140);
        SetSize(MaximumSize);
        Center();

        MessageLabel = new Label(this);
        MessageLabel.SetFont(Fonts.Paragraph);
        MessageLabel.SetText(Message);
        MessageLabel.RedrawText(true);
        MessageLabel.SetPosition(Size.Width / 2 - MessageLabel.Size.Width / 2, 32);

        ProgressBar = new ProgressBar(this);
        ProgressBar.SetPosition(14, 58);
        ProgressBar.SetSize(Size.Width - 14 - WindowEdges * 2, 32);
        ProgressBar.OnValueChanged += _ =>
        {
            if (ProgressBar.Progress == 1)
            {
                if (CloseWhenDone && !Disposed) this.OnFinished?.Invoke();
                else if (Buttons.Count > 0) Buttons[0].SetEnabled(true);
            }
        };

        this.OnFinished += () => Close();
        this.OnCancelled += () => Close();

        if (!CloseWhenDone)
        {
            CreateButton("OK", _ => this.OnFinished?.Invoke());
            Buttons[0].SetEnabled(false);
            RegisterShortcuts(new List<Shortcut>()
            {
                new Shortcut(this, new Key(Keycode.ENTER, Keycode.CTRL), _ => this.OnFinished?.Invoke(), true, e => e.Value = Buttons[0].Enabled)
            });
        }

        if (Cancellable)
        {
            CreateButton("Cancel", _ => this.OnCancelled?.Invoke());
        }
    }

    public void SetMessage(string Message)
    {
        if (this.Message != Message)
        {
            this.Message = Message;
            MessageLabel.SetText(this.Message);
            MessageLabel.RedrawText(true);
            MessageLabel.SetPosition(Size.Width / 2 - MessageLabel.Size.Width / 2, 32);
        }
    }

    public void SetProgress(float Progress)
    {
        ProgressBar.SetValue(Progress);
    }
}
