using System;
using System.Collections.Generic;

namespace RPGStudioMK.Game;

public class MoveCommand : ICloneable
{
    public MoveCode Code;
    public List<object> Parameters = new List<object>();

    public MoveCommand()
    {

    }

    public MoveCommand(MoveCode Code, List<object> Parameters)
    {
        this.Code = Code;
        this.Parameters = Parameters;
    }

    public MoveCommand(IntPtr data)
    {
        this.Code = (MoveCode) Ruby.Integer.FromPtr(Ruby.GetIVar(data, "@code"));
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
        IntPtr cmd = Ruby.Funcall(Compatibility.RMXP.MoveCommand.Class, "new");
        Ruby.Pin(cmd);
        Ruby.SetIVar(cmd, "@code", Ruby.Integer.ToPtr((int) Code));
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

    public object Clone()
    {
        MoveCommand c = new MoveCommand(this.Code, null);
        c.Parameters = (List<object>) Utilities.CloneUnknown(this.Parameters);
        return c;
    }

    public override string ToString()
    {
        return this.Code switch
        {
            MoveCode.None => "$>",
            MoveCode.Down => "Move Down",
            MoveCode.Left => "Move Left",
            MoveCode.Right => "Move Right",
            MoveCode.Up => "Move Up",
            MoveCode.LowerLeft => "Move Lower Left",
            MoveCode.LowerRight => "Move Lower Right",
            MoveCode.UpperLeft => "Move Upper Left",
            MoveCode.UpperRight => "Move Upper Right",
            MoveCode.Random => "Move at Random",
            MoveCode.TowardPlayer => "Move toward Player",
            MoveCode.AwayFromPlayer => "Move away from Player",
            MoveCode.Forward => "Move Forward",
            MoveCode.Backward => "Move Backward",
            MoveCode.Jump => $"Jump: {((long) Parameters[0] > 0 ? "+" : "")}{Parameters[0]}, {((long) Parameters[1] > 0 ? "+" : "")}{Parameters[1]}",
            MoveCode.Wait => $"Wait: {Parameters[0]} frame{((long) Parameters[0] > 1 ? "s": "")}",
            MoveCode.TurnDown => "Turn Down",
            MoveCode.TurnLeft => "Turn Left",
            MoveCode.TurnRight => "Turn Right",
            MoveCode.TurnUp => "Turn Up",
            MoveCode.TurnRight90 => "Turn 90° Right",
            MoveCode.TurnLeft90 => "Turn 90° Left",
            MoveCode.Turn180 => "Turn 180°",
            MoveCode.TurnRightOrLeft90 => "Turn 90° Right or Left",
            MoveCode.TurnRandom => "Turn at Random",
            MoveCode.TurnTowardPlayer => "Turn toward Player",
            MoveCode.TurnAwayFromPlayer => "Turn away from Player",
            MoveCode.SwitchOn => $"Switch ON: {Parameters[0]}",
            MoveCode.SwitchOff => $"Switch OFF: {Parameters[0]}",
            MoveCode.ChangeSpeed => $"Change Speed: {Parameters[0]}",
            MoveCode.ChangeFreq => $"Change Freq: {Parameters[0]}",
            MoveCode.WalkAnimeOn => "Move Animation ON",
            MoveCode.WalkAnimeOff => "Move Animation OFF",
            MoveCode.StepAnimeOn => "Stop Animation ON",
            MoveCode.StepAnimeOff => "Stop Animation OFF",
            MoveCode.DirectionFixOn => "Direction Fix ON",
            MoveCode.DirectionFixOff => "Direction Fix OFF",
            MoveCode.ThroughOn => "Through ON",
            MoveCode.ThroughOff => "Through OFF",
            MoveCode.AlwaysOnTopOn => "Always on Top ON",
            MoveCode.AlwaysOnTopOff => "Always on Top OFF",
            MoveCode.Graphic => $"Graphic: '{Parameters[0]}', {Parameters[1]}, {Parameters[2]}, {Parameters[3]}",
            MoveCode.Opacity => $"Change Opacity: {Parameters[0]}",
            MoveCode.Blending => $"Change Blending: {new string[] { "Normal", "Add", "Sub" }[(long) Parameters[0]]}",
            MoveCode.PlaySE => $"SE: '{((AudioFile) Parameters[0]).Name}', {((AudioFile) Parameters[0]).Volume}, {((AudioFile) Parameters[0]).Pitch}",
            MoveCode.Script => $"Script: {Parameters[0]}",
            MoveCode.ScriptAsync => $"Script: {Parameters[0]}",
            _ => throw new Exception($"Invalid Move Command Code: {this.Code}")
        };
    }
}

public enum MoveCode
{
    None = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    Up = 4,
    LowerLeft = 5,
    LowerRight = 6,
    UpperLeft = 7,
    UpperRight = 8,
    Random = 9,
    TowardPlayer = 10,
    AwayFromPlayer = 11,
    Forward = 12,
    Backward = 13,
    Jump = 14,
    Wait = 15,
    TurnDown = 16,
    TurnLeft = 17,
    TurnRight = 18,
    TurnUp = 19,
    TurnRight90 = 20,
    TurnLeft90 = 21,
    Turn180 = 22,
    TurnRightOrLeft90 = 23,
    TurnRandom = 24,
    TurnTowardPlayer = 25,
    TurnAwayFromPlayer = 26,
    SwitchOn = 27,
    SwitchOff = 28,
    ChangeSpeed = 29,
    ChangeFreq = 30,
    WalkAnimeOn = 31,
    WalkAnimeOff = 32,
    StepAnimeOn = 33,
    StepAnimeOff = 34,
    DirectionFixOn = 35,
    DirectionFixOff = 36,
    ThroughOn = 37,
    ThroughOff = 38,
    AlwaysOnTopOn = 39,
    AlwaysOnTopOff = 40,
    Graphic = 41,
    Opacity = 42,
    Blending = 43,
    PlaySE = 44,
    Script = 45,
    ScriptAsync = 101
}