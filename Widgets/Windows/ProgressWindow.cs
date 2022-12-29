using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class ProgressWindow : PopupWindow
{
    public string Message { get; protected set; }
    public bool CloseWhenDone { get; protected set; }
    public bool ShowETA { get; protected set; } = false;

    public Action OnFinished;
    public Action OnCancelled;

    Label MessageLabel;
    Label ETALabel;
    ProgressBar ProgressBar;
    int LastSecond;
    List<float> LastProgress = new List<float>();

    public ProgressWindow(string Title, string Message, bool CloseWhenDone = true, bool Cancellable = true, bool ShowOKButton = true, bool ShowETA = false)
    {
        this.Message = Message;
        this.CloseWhenDone = CloseWhenDone;
        this.ShowETA = ShowETA;

        SetTitle(Title);
        int height = 140;
        if (CloseWhenDone && !Cancellable) height = 110;
        if (!CloseWhenDone && !ShowOKButton) height = 110;
        MinimumSize = MaximumSize = new Size(300, height);
        SetSize(MaximumSize);
        Center();

        MessageLabel = new Label(this);
        MessageLabel.SetFont(Fonts.Paragraph);
        MessageLabel.SetText(Message);
        MessageLabel.RedrawText(true);
        MessageLabel.SetPosition(Size.Width / 2 - MessageLabel.Size.Width / 2, 32);

        if (ShowETA)
        {
            ETALabel = new Label(this);
            ETALabel.SetFont(Fonts.Paragraph);
            ETALabel.SetPosition(20, Size.Height - 40);
        }

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

        if (!CloseWhenDone && ShowOKButton)
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

        LastSecond = DateTime.Now.Second;
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

    public override void Update()
    {
        base.Update();
        if (!ShowETA) return;
        if (DateTime.Now.Second != LastSecond)
        {
            LastSecond = DateTime.Now.Second;
            if (LastProgress.Count == 20) LastProgress.RemoveAt(0);
            if (ProgressBar.Progress > 0)
            {
                LastProgress.Add(ProgressBar.Progress);
            }
            UpdateETA();
        }
    }

    private void UpdateETA()
    {
        ProgressSpeed s = new ProgressSpeed();
        float DiffSum = 0;
        for (int i = 1; i < LastProgress.Count; i++)
        {
            DiffSum += LastProgress[i] - LastProgress[i - 1];
        }
        s.ProgressPerSecond = DiffSum / (LastProgress.Count - 1);
        if (ProgressBar.Progress == 0 || ProgressBar.Progress == 1) ETALabel.SetText("");
        else
        {
            string eta = s.CalculateETAString(ProgressBar.Progress);
            if (string.IsNullOrEmpty(eta)) ETALabel.SetText("");
            else ETALabel.SetText($"ETA: {eta}");
        }
    }
}

public struct ProgressSpeed
{
    public float ProgressPerSecond;

    public int CalculateETA(float CurrentProgress)
    {
        // ProgressPerSecond: 5%/s
        // CurrentProgress: 70%
        // (100% - CurrentProgress) / ProgressPerSecond
        // => (100% - 70%) / 5%/s
        // => 30% / 5%/s
        // => 6s
        return (int) Math.Floor((1 - CurrentProgress) / ProgressPerSecond);
    }

    public string CalculateETAString(float CurrentProgress)
    {
        int Seconds = CalculateETA(CurrentProgress);
        string ETA = "";
        if (Seconds >= 86400)
        {
            int days = Seconds / 86400;
            ETA += $"{days} days ";
            Seconds -= days * 86400;
        }
        if (Seconds >= 3600)
        {
            int hours = Seconds / 3600;
            ETA += $"{hours} hours ";
            Seconds -= hours * 3600;
        }
        if (Seconds >= 60)
        {
            int mins = Seconds / 60;
            ETA += $"{mins} minutes ";
            Seconds -= mins * 60;
        }
        if (Seconds > 0)
        {
            ETA += $"{Seconds} seconds";
        }
        return ETA;
    }
}