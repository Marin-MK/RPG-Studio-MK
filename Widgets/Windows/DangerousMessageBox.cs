using System;
using System.Collections.Generic;

namespace RPGStudioMK.Widgets;

public class DangerousMessageBox : PopupWindow
{
    public bool Yes;

    public string Message { get; protected set; }

    protected MultilineLabel label;
    protected ImageBox Icon;

    int ClickCount = 0;

    public DangerousMessageBox(string Title, string Message)
    {
        SetTitle(Title);
        this.Message = Message;

        Icon = new ImageBox(this);
        Icon.SetBitmap("assets/img/mbox_icons");
        Icon.SetSrcRect(new Rect(32 * ((int) IconType.Warning - 1), 0, 32, 32));

        label = new MultilineLabel(this);
        label.SetText(Message);

        CreateButton("No", _ => Close());
        Button YesButton = CreateButton("Yes", _ =>
        {
            ClickCount++;
            if (ClickCount > 80)
            {
                Yes = true;
                Close();
            }
        });
        YesButton.SetRepeatable(true);

        MinimumSize = MaximumSize = new Size(300, 150);
        SetSize(MaximumSize);

        YesButton.SetEnabled(false);
        YesButton.SetOpacity(128);

        SetCallback(5000, () =>
        {
            YesButton.SetEnabled(true);
            YesButton.SetOpacity(255);
        });
    }

    public override void LeftMouseUp(MouseEventArgs e)
    {
        base.LeftMouseUp(e);
        ClickCount = 0;
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        int x = 10 + WindowEdges;
        if (Icon != null)
        {
            x += 50;
        }
        label.SetSize(Size.Width - 10 - x, 1);
        label.RedrawText(true);
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