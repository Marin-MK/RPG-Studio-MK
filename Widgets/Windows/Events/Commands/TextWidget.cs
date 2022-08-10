using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class TextWidget : BaseCommandWidget
{
    MultilineLabel MultilineLabel;

    public TextWidget(IContainer Parent, int ParentWidgetIndex) : base(Parent, ParentWidgetIndex)
    {
        MultilineLabel = new MultilineLabel(this);
        MultilineLabel.SetFont(Fonts.CabinMedium.Use(9));
        MultilineLabel.SetHeight(1);
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
            Commands = TextToCommands(Command.Code, win.Text);
            Continue();
        };
    }

    public static List<EventCommand> TextToCommands(CommandCode Code, string Text)
    {
        List<EventCommand> Commands = new List<EventCommand>();
        List<string> Lines = SplitText(Code, Text);
        Lines.ForEach(l =>
        {
            CommandCode code = Code switch
            {
                CommandCode.ShowText => Commands.Count == 0 ? CommandCode.ShowText : CommandCode.MoreText,
                CommandCode.Comment => Commands.Count == 0 ? CommandCode.Comment : CommandCode.MoreComment,
                CommandCode.Script => Commands.Count == 0 ? CommandCode.Script : CommandCode.MoreScript,
                _ => throw new Exception("Invalid command code")
            };
            Commands.Add(new EventCommand(code, 0, new List<object>() { l }));
        });
        return Commands;
    }

    private static List<string> SplitText(CommandCode Code, string Text)
    {
        if (Code == CommandCode.Script)
        {
            return Text.Split('\n').ToList();
        }
        else
        {
            return Utilities.FormatString(Fonts.CabinMedium.Use(9), Text, 300);
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
            if (i != Commands.Count - 1) text += "\n";
        }
        return text;
    }

    public override void LoadCommand()
    {
        base.LoadCommand();
        HeaderLabel.SetText(Command.Code switch
        {
            CommandCode.Script => "Script",
            CommandCode.ShowText => "Text",
            CommandCode.Comment => "Comment",
            _ => HeaderLabel.Text
        });
        
        string text = MergeText();

        ScaleGradientWithSize = false;

        MultilineLabel.SetPosition(HeaderLabel.Position.X + HeaderLabel.Size.Width + 10, 8);
        MultilineLabel.SetLineHeight(22);
        MultilineLabel.SetWidth(Size.Width - MultilineLabel.Position.X - 4);
        MultilineLabel.SetFont(Command.Code == CommandCode.Script ? Fonts.Monospace.Use(11) : Fonts.CabinMedium.Use(9));
        MultilineLabel.SetText(text);
        HeightAdd = Math.Max(0, MultilineLabel.Size.Height - StandardHeight);
        //SetWidth(GetStandardWidth(Indentation));
        //
        //UpdateSize();
        //((Widget) Parent).UpdateLayout();
    }

    public override void LeftMouseDownInside(MouseEventArgs e)
    {
        base.LeftMouseDownInside(e);
        if (e.Handled)
        {
            CancelDoubleClick();
            return;
        }
        SetSelected(true);
    }
}
