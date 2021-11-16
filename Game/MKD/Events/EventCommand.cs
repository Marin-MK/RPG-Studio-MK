using System;
using System.Collections.Generic;
using rubydotnet;

namespace RPGStudioMK.Game
{
    public class EventCommand : ICloneable
    {
        public int Indent;
        public CommandCode Code;
        public List<object> Parameters = new List<object>();

        public EventCommand()
        {

        }

        public EventCommand(IntPtr data)
        {
            this.Indent = (int) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@indent"));
            this.Code = (CommandCode) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@code"));
            IntPtr parameters = Ruby.GetIVar(data, "@parameters");
            for (int i = 0; i < Ruby.Array.Length(parameters); i++)
            {
                IntPtr param = Ruby.Array.Get(parameters, i);
                object obj = Utilities.RubyToNative(param);
                this.Parameters.Add(obj);
            }
        }

        public IntPtr Save()
        {
            IntPtr cmd = Ruby.Funcall(Compatibility.RMXP.EventCommand.Class, "new");
            Ruby.Pin(cmd);
            Ruby.SetIVar(cmd, "@indent", Ruby.Integer.ToPtr(this.Indent));
            Ruby.SetIVar(cmd, "@code", Ruby.Integer.ToPtr((int) this.Code));
            IntPtr parameters = Ruby.Array.Create();
            Ruby.SetIVar(cmd, "@parameters", parameters);
            for (int i = 0; i < this.Parameters.Count; i++)
            {
                IntPtr param = Utilities.NativeToRuby(this.Parameters[i]);
                Ruby.Array.Set(parameters, i, param);
            }
            Ruby.Unpin(cmd);
            return cmd;
        }

        public override string ToString()
        {
            return Code.ToString();
        }

        public object Clone()
        {
            EventCommand c = new EventCommand();
            c.Indent = this.Indent;
            c.Code = this.Code;
            c.Parameters = new List<object>();
            this.Parameters.ForEach(p =>
            {
                if (p is int || p is long || p is string || p is true || p is false || p is null) c.Parameters.Add(p);
                else if (p is ICloneable) c.Parameters.Add(((ICloneable) p).Clone());
                else throw new Exception("Uncloneable object: " + p.GetType().ToString());
            });
            return c;
        }
    }

    public enum CommandCode
    {
        Blank = 0,

        ShowText = 101,
        ShowCoices = 102,
        InputNumber = 103,
        ChangeTextOptions = 104,
        Wait = 106,
        Comment = 108,
        ConditionalBranch = 111,
        Loop = 112,
        BreakLoop = 113,
        ExitEventProcessing = 115,
        EraseEvent = 116,
        CallCommonEvent = 117,
        Label = 118,
        JumpToLabel = 119,
        ControlSwitches = 121,
        ControlVariables = 122,
        ControlSelfSwitch = 123,
        ControlTimer = 124,
        ChangeMoney = 125,
        ChangeWindowskin = 131,
        ChangeBattleBGM = 132,
        ChangeBattleEndME = 133,
        ChangeSaveAccess = 134,
        ChangeMenuAccess = 135,
        ChangeEncounter = 136,

        TransferPlayer = 201,
        SetEventLocation = 202,
        ScrollMap = 203,
        ChangeMapSettings = 204,
        ChangeFogColorTone = 205,
        ChangeFogOpacity = 206,
        ShowAnimation = 207,
        ChangeTransparencyFlag = 208,
        SetMoveRoute = 209,
        WaitForMoveCompletion = 210,
        PrepareForTransition = 221,
        ExecuteTransition = 222,
        ChangeScreenColorTone = 223,
        ScreenFlash = 224,
        ScreenShake = 225,
        ShowPicture = 231,
        MovePicture = 232,
        RotatePicture = 233,
        ChangePictureColorTone = 234,
        ErasePicture = 235,
        SetWeatherEffects = 236,
        PlayBGM = 241,
        FadeOutBGM = 242,
        PlayBGS = 245,
        FadeOutBGS = 246,
        MemomorizeBGMBGS = 247,
        RestoreBGMBGS = 238,
        PlayME = 249,
        PlaySE = 250,
        StopSE = 251,

        NameInputProcessing = 303,
        HealAll = 314,
        CallMenuScreen = 351,
        CallSaveScreen = 352,
        GameOver = 353,
        ReturnToTitleScreen = 354,
        Script = 355,

        MoreText = 401,
        BranchWhenXXX = 402,
        BranchWhenCancel = 403,
        BranchEnd = 404,
        MoreComment = 408,
        BranchElse = 411,
        BranchConditionalEnd = 412,
        RepeatAbove = 413,

        MoreMoveRoute = 509,

        MoreScript = 655
    }
}
