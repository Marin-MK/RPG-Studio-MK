using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class TextWidget : ExpandableCommandWidget
{
    Label SingleLabel;
    MultilineLabel MultilineLabel;

    public TextWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex)
    {
        SingleLabel = new Label(this);
        SingleLabel.SetFont(Fonts.CabinMedium.Use(9));
        SingleLabel.SetDocked(true);
        MultilineLabel = new MultilineLabel(this);
        MultilineLabel.SetFont(Fonts.CabinMedium.Use(9));
        MultilineLabel.SetHDocked(true);
        MultilineLabel.SetHeight(1);
        MultilineLabel.SetPosition(8, StandardHeight);
        SetExpandable(false);
        OnHoverChanged += _ => SetExpandable(Mouse.Inside && (Commands.Count > 1 && Command.Code == CommandCode.Script || SingleLabel.ReachedWidthLimit));
        //OnDoubleLeftMouseDownInside += _ =>
        //{
        //    string text = "";
        //    for (int i = 0; i < Commands.Count; i++)
        //    {
        //        EventCommand cmd = Commands[i];
        //        text += (string) cmd.Parameters[0];
        //        if (i != Commands.Count - 1 && Command.Code == CommandCode.Script) text += "\n";
        //    }
        //    GenericMultilineTextBoxWindow win = new GenericMultilineTextBoxWindow("Show Text", text);
        //    win.OnClosed += _ =>
        //    {
        //        if (!win.Apply) return;
        //        // Set text
        //    };
        //};
    }

    protected override (bool Applied, bool ResetCommand, int GlobalIndexToCountFrom) Edit()
    {
        Commands = new List<EventCommand>()
        {
            new EventCommand(CommandCode.ShowText, 0, new List<object>() { "Line 1\n" }),
            new EventCommand(CommandCode.MoreText, 0, new List<object>() { "Line 2\n" }),
            new EventCommand(CommandCode.MoreText, 0, new List<object>() { "Line 3" })
        };
        return (true, false, -1);
    }

    private string MergeText()
    {
        string text = "";
        for (int i = 0; i < Commands.Count; i++)
        {
            EventCommand cmd = Commands[i];
            text += (string) cmd.Parameters[0];
            if (i != Commands.Count - 1 && Command.Code == CommandCode.Script) text += Expanded ? "\n" : " ";
        }
        return text;
    }

    public override void LoadCommand()
    {
        HeaderLabel.SetText(Command.Code switch
        {
            CommandCode.Script => "Script",
            CommandCode.ShowText => "Text",
            CommandCode.Comment => "Comment",
            _ => HeaderLabel.Text
        });
        SingleLabel.SetPosition(HeaderLabel.Position.X + HeaderLabel.Size.Width + 6, 3);
        SingleLabel.SetWidthLimit(Size.Width - SingleLabel.Position.X - 22);
        SingleLabel.SetFont(Command.Code == CommandCode.Script ? Fonts.FiraCode.Use(9) : Fonts.CabinMedium.Use(9));

        string text = MergeText();
        SingleLabel.SetText(text);

        SetExpandable(Mouse.Inside && (Commands.Count > 1 && Command.Code == CommandCode.Script || SingleLabel.ReachedWidthLimit));

        bool updateparent = false;
        if (!Expandable && Expanded)
        {
            SetExpanded(false, true);
            updateparent = true;
            text = MergeText();
            SingleLabel.SetText(text);
        }

        SingleLabel.SetVisible(!Expanded);
        MultilineLabel.SetVisible(Expanded);

        if (Expanded)
        {
            MultilineLabel.SetFont(Command.Code == CommandCode.Script ? Fonts.FiraCode.Use(9) : Fonts.CabinMedium.Use(9));
            MultilineLabel.SetText(text);
            HeightAdd = MultilineLabel.Size.Height;
        }
        else
        {
            HeightAdd = 0;
        }
        if (updateparent)
        {
            UpdateHeight();
            ((Widget) Parent).UpdateLayout();
        }
    }
}
