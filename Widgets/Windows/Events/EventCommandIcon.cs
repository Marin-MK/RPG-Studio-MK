using System;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventCommandIcon : Widget
{
    public EventCommandIcon(IContainer Parent) : base(Parent)
    {
        Sprites["shadow"] = new Sprite(this.Viewport);
        Sprites["shadow"].Bitmap = Utilities.EventCommandIconSheet;
        Sprites["shadow"].DestroyBitmap = false;
        Sprites["shadow"].SrcRect.Width = 24;
        Sprites["shadow"].SrcRect.Height = 24;
        Sprites["shadow"].Visible = false;
        Sprites["icon"] = new Sprite(this.Viewport);
        Sprites["icon"].Bitmap = Utilities.EventCommandIconSheet;
        Sprites["icon"].DestroyBitmap = false;
        Sprites["icon"].SrcRect.Width = 24;
        Sprites["icon"].SrcRect.Height = 24;
        Sprites["icon"].Visible = false;
    }

    public void SetEventCommand(CommandCode Code)
    {
        (int x, int y) = Code switch
        {
            CommandCode.ShowText => (0, 0),
            CommandCode.ShowChoices => (0, 1),
            CommandCode.Script => (0, 2),
            CommandCode.ControlSwitches => (1, 0),
            CommandCode.ControlVariables => (1, 1),
            CommandCode.ControlSelfSwitch => (1, 2),
            CommandCode.ConditionalBranch => (1, 3),
            CommandCode.Loop => (1, 4),
            CommandCode.BreakLoop => (1, 5),
            CommandCode.ExitEventProcessing => (1, 6),
            CommandCode.EraseEvent => (1, 7),
            CommandCode.CallCommonEvent => (1, 8),
            CommandCode.Label => (1, 9),
            CommandCode.JumpToLabel => (1, 10),
            CommandCode.TransferPlayer => (2, 0),
            CommandCode.ChangePictureColorTone => (3, 0),
            CommandCode.HealAll => (4, 4),
            CommandCode.Blank => (5, 0),
            _ => (-1, -1)
        };
        if (x == -1 || y == -1)
        {
            Sprites["shadow"].Visible = false;
            Sprites["icon"].Visible = false;
            return;
        }
        Sprites["shadow"].Visible = true;
        Sprites["icon"].Visible = true;
        Sprites["shadow"].SrcRect.X = Sprites["shadow"].Bitmap.Width / 2 + x * Sprites["shadow"].SrcRect.Width;
        Sprites["shadow"].SrcRect.Y = y * Sprites["shadow"].SrcRect.Height;
        Sprites["icon"].SrcRect.X = x * Sprites["icon"].SrcRect.Width;
        Sprites["icon"].SrcRect.Y = y * Sprites["icon"].SrcRect.Height;
    }

    public void SetColor(Color Color)
    {
        Sprites["icon"].Color = Color;
    }
}
