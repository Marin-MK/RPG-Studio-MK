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
            IntPtr load_path = Ruby.GetGlobal("$LOAD_PATH");
            Ruby.Funcall(load_path, "push", Ruby.String.ToPtr("./ruby/2.7.0"));
            if (Graphics.Platform == odl.Platform.Windows)
            {
                Ruby.Funcall(load_path, "push", Ruby.String.ToPtr("./ruby/2.7.0/x64-mingw32"));
            }
            else if (Graphics.Platform == odl.Platform.Linux)
            {
                Ruby.Funcall(load_path, "push", Ruby.String.ToPtr("./ruby/2.7.0/x86_64-linux"));
            }
            Ruby.Require("zlib");
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
