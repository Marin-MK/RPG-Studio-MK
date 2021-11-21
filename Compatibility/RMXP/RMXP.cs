using System;
using System.Collections.Generic;
using System.Linq;
using amethyst;
using odl;
using rubydotnet;
using RPGStudioMK.Widgets;
using RPGStudioMK.Game;
using System.IO;

namespace RPGStudioMK.Compatibility
{
    public static partial class RMXP
    {
        public static void Setup()
        {
            Ruby.Initialize();
            RPG.Create();
            Table.Create();
            Tone.Create();
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
}
