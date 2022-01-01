using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class FogFilePicker : AbstractFilePicker
{
    Tileset Tileset;

    public FogFilePicker(Tileset Tileset)
        : base("Fogs", Data.ProjectPath + "/Graphics/Fogs", Tileset.FogName)
    {
        this.Tileset = Tileset;
    }
}
