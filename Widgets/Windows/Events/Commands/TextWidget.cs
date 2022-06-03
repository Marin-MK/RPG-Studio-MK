using System;
using System.Collections.Generic;
using System.Linq;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets.CommandWidgets;

public class TextWidget : ExpandableCommandWidget
{
    Label SingleLabel;
    MultilineLabel MultilineLabel;

    public TextWidget(IContainer Parent) : base(Parent)
    {
        SingleLabel = new Label(this);
        SingleLabel.SetFont(Fonts.ProductSansMedium.Use(9));
        SingleLabel.SetDocked(true);
        MultilineLabel = new MultilineLabel(this);
        MultilineLabel.SetFont(Fonts.ProductSansMedium.Use(9));
        MultilineLabel.SetHDocked(true);
        MultilineLabel.SetHeight(1);
        MultilineLabel.SetPosition(8, 24);
        SetExpandable(false);
        OnHoverChanged += _ => SetExpandable(Mouse.Inside && (Commands.Count > 1 && Command.Code == CommandCode.Script || SingleLabel.ReachedWidthLimit));
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
        SingleLabel.SetPosition(HeaderLabel.Position.X + HeaderLabel.Size.Width + 6, 5);
        SingleLabel.SetWidthLimit(Size.Width - SingleLabel.Position.X - 22);
        SingleLabel.SetVisible(!Expanded);
        MultilineLabel.SetVisible(Expanded);
        string text = "";
        for (int i = 0; i < Commands.Count; i++)
        {
            EventCommand cmd = Commands[i];
            text += (string) cmd.Parameters[0];
            if (i != Commands.Count - 1 && Command.Code == CommandCode.Script) text += Expanded ? "\n" : " ";
        }

        SingleLabel.SetText(text);
        SetExpandable(Mouse.Inside && (Commands.Count > 1 && Command.Code == CommandCode.Script || SingleLabel.ReachedWidthLimit));

        if (Expanded)
        {
            MultilineLabel.SetText(text);
            HeightAdd = MultilineLabel.Size.Height;
        }
        else
        {
            HeightAdd = 0;
        }
    }
}
