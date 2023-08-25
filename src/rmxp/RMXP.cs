using System;

namespace RPGStudioMK.Compatibility;

public static partial class RMXP
{
    public static void Setup()
    {
        RPG.Create();
        Table.Create();
        Tone.Create();
        Color.Create();
        Map.Create();
        AudioFile.Create();
        Event.Create();
        Page.Create();
        EventCommand.Create();
        Condition.Create();
        MoveRoute.Create();
        MoveCommand.Create();
        Graphic.Create();
        MapInfo.Create();
        Tileset.Create();
        System.Create();
        Words.Create();
        TestBattler.Create();
        CommonEvent.Create();
    }
}
