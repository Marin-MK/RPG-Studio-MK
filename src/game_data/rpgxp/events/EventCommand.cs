﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RPGStudioMK.Game;

public class EventCommand : ICloneable
{
    public CommandCode Code;
    public int Indent;
    public List<object> Parameters = new List<object>();

    /// <summary>
    /// DO NOT USE!
    /// </summary>
    public EventCommand()
    {

    }

    public EventCommand(CommandCode Code, int Indent, List<object> Parameters)
    {
        this.Code = Code;
        this.Indent = Indent;
        this.Parameters = Parameters;
    }

    public EventCommand(IntPtr data)
    {
        this.Code = (CommandCode)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@code"));
        this.Indent = (int)Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@indent"));
        IntPtr parameters = Ruby.GetIVar(data, "@parameters");
        for (int i = 0; i < Ruby.Array.Length(parameters); i++)
        {
            IntPtr param = Ruby.Array.Get(parameters, i);
            object obj = Utilities.RubyToNative(param);
            this.Parameters.Add(obj);
        }

        if (this.Code == CommandCode.Script)
        {
            string code = (string) Parameters[0];
            Match match = Regex.Match(code, @"^# rpg studio mk\n\$game_self_switches\[\[\$game_map\.map_id,(\d+|@event_id),'(.)'\]\]=(true|false)\n\$game_map\.need_refresh=true$");
            if (match.Success)
            {
                // Convert custom ControlSelfSwitch script back to the command
                this.Code = CommandCode.ControlSelfSwitch;
                string c = match.Groups[2].Value;
                long state = match.Groups[3].Value == "true" ? 0L : 1L;
                if (match.Groups[1].Value != "@event_id")
                {
                    long EventID = Convert.ToInt64(match.Groups[1].Value);
                    this.Parameters = new List<object>() { c, state, EventID };
                }
                else this.Parameters = new List<object>() { c, state };
            }
        }
    }

    public IntPtr Save()
    {
        if (this.Code == CommandCode.ControlSelfSwitch && (this.Parameters.Count == 3 || ((string) this.Parameters[0])[0] > 'D'))
        {
            // Convert unconventional ControlSelfSwitch command to script
            string eventid = Parameters.Count == 3 ? this.Parameters[2].ToString() : "@event_id";
            string switchid = "'" + this.Parameters[0].ToString() + "'";
            string value = ((long) this.Parameters[1] == 0) ? "true" : "false";
            return new EventCommand(CommandCode.Script, this.Indent, new List<object>() { $"# rpg studio mk\n$game_self_switches[[$game_map.map_id,{eventid},{switchid}]]={value}\n$game_map.need_refresh=true" }).Save();
        }

        IntPtr cmd = Ruby.Funcall(Compatibility.RMXP.EventCommand.Class, "new");
        Ruby.Pin(cmd);
        Ruby.SetIVar(cmd, "@code", Ruby.Integer.ToPtr((int)this.Code));
        Ruby.SetIVar(cmd, "@indent", Ruby.Integer.ToPtr(this.Indent));
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
        EventCommand c = new EventCommand(this.Code, this.Indent, null);
        c.Parameters = (List<object>) Utilities.CloneUnknown(this.Parameters);
        return c;
    }
}

public enum CommandCode
{
    Blank = 0,

    ShowText = 101,
    ShowChoices = 102,
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
    RestoreBGMBGS = 238,
    PlayBGM = 241,
    FadeOutBGM = 242,
    PlayBGS = 245,
    FadeOutBGS = 246,
    MemorizeBGMBGS = 247,
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
