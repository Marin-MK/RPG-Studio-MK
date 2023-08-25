using System;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class EventCommandIcon : Widget
{
    public Color Color => Sprites["icon"].Color;

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
            CommandCode.ChangeTextOptions => (0, 2),
            CommandCode.ChangeWindowskin => (0, 3),
            CommandCode.InputNumber => (0, 4),
            CommandCode.Script => (0, 5),
            CommandCode.Comment => (0, 6),
            CommandCode.ChangeMoney => (0, 7),
            CommandCode.SetMoveRoute => (0, 8),
            CommandCode.WaitForMoveCompletion => (0, 9),
            CommandCode.Wait => (0, 10),

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
            CommandCode.SetEventLocation => (2, 1),
            CommandCode.ScrollMap => (2, 2),
            CommandCode.ChangeMapSettings => (2, 3),
            CommandCode.ChangeFogColorTone => (2, 4),
            CommandCode.ChangeFogOpacity => (2, 5),
            CommandCode.ShowAnimation => (2, 6),
            CommandCode.ChangeTransparencyFlag => (2, 7),
            CommandCode.ChangeScreenColorTone => (2, 8),
            CommandCode.SetWeatherEffects => (2, 9),

            CommandCode.ShowPicture => (3, 0),
            CommandCode.MovePicture => (3, 1),
            CommandCode.RotatePicture => (3, 2),
            CommandCode.ChangePictureColorTone => (3, 3),
            CommandCode.ErasePicture => (3, 4),
            CommandCode.PlayBGM => (3, 5),
            CommandCode.FadeOutBGM => (3, 6),
            CommandCode.PlayBGS => (3, 7),
            CommandCode.FadeOutBGS => (3, 8),
            CommandCode.MemorizeBGMBGS => (3, 9),
            CommandCode.RestoreBGMBGS => (3, 10),
            CommandCode.PlayME => (3, 11),
            CommandCode.PlaySE => (3, 12),
            CommandCode.StopSE => (3, 13),

            CommandCode.CallMenuScreen => (4, 0),
            CommandCode.CallSaveScreen => (4, 1),
            CommandCode.ReturnToTitleScreen => (4, 2),
            CommandCode.GameOver => (4, 3),
            CommandCode.HealAll => (4, 4),
            CommandCode.ChangeSaveAccess => (4, 5),
            CommandCode.ChangeMenuAccess => (4, 6),

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
