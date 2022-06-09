using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class TextWidget : ExpandableCommandWidget
{
    Label SingleLabel;
    MultilineLabel MultilineLabel;
    //int CharactersPerCommand = 25;

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
    }

    protected override void Edit(EditEvent Continue)
    {
        GenericMultilineTextBoxWindow win = new GenericMultilineTextBoxWindow(HeaderLabel.Text, GetFullText(), Command.Code == CommandCode.Script);
        win.OnClosed += _ =>
        {
            if (!win.Apply)
            {
                Continue(false);
                return;
            }
            Commands = new List<EventCommand>();
            List<string> Lines = SplitText(win.Text);
            Lines.ForEach(l =>
            {
                CommandCode code = Command.Code switch
                {
                    CommandCode.ShowText => Commands.Count == 0 ? CommandCode.ShowText : CommandCode.MoreText,
                    CommandCode.Comment => Commands.Count == 0 ? CommandCode.Comment : CommandCode.MoreComment,
                    CommandCode.Script => Commands.Count == 0 ? CommandCode.Script : CommandCode.MoreScript,
                    _ => throw new Exception("Invalid command code")
                };
                Commands.Add(new EventCommand(code, 0, new List<object>() { l }));
            });
            Continue();
            SetExpanded(true);
        };
    }

    private List<string> SplitText(string Text)
    {
        if (Command.Code == CommandCode.Script)
        {
            return Text.Split('\n').ToList();
        }
        else
        {
            return Utilities.FormatString(Fonts.CabinMedium.Use(9), Text, 300);
            //List<string> Lines = new List<string>();
            //int LineCount = (int) Math.Ceiling((double) Text.Length / CharactersPerCommand);
            //for (int i = 0; i < LineCount; i++)
            //{
            //    if (i == LineCount - 1) Lines.Add(Text.Substring(i * CharactersPerCommand, Text.Length - i * CharactersPerCommand));
            //    else Lines.Add(Text.Substring(i * CharactersPerCommand, CharactersPerCommand));
            //}
            //return Lines;
        }
    }

    private string GetFullText()
    {
        string text = "";
        for (int i = 0; i < Commands.Count; i++)
        {
            EventCommand cmd = Commands[i];
            text += (string) cmd.Parameters[0];
            if (i != Commands.Count - 1) text += "\n";
        }
        return text;
    }

    private string MergeText()
    {
        string text = "";
        for (int i = 0; i < Commands.Count; i++)
        {
            EventCommand cmd = Commands[i];
            text += (string) cmd.Parameters[0];
            if (i != Commands.Count - 1) text += Expanded ? "\n" : " ";
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

        bool CanExpand = Commands.Count > 1 && Command.Code == CommandCode.Script || SingleLabel.ReachedWidthLimit;

        if (!CanExpand && Expanded)
        {
            SetExpanded(false, true);
            text = MergeText();
            SingleLabel.SetText(text);
        }

        SingleLabel.SetVisible(!Expanded);
        MultilineLabel.SetVisible(Expanded);

        SetExpandable(CanExpand && Mouse.Inside);

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

        UpdateHeight();
        ((Widget) Parent).UpdateLayout();
    }
}
