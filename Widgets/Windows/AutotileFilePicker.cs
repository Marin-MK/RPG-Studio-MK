using System.Collections.Generic;
using System.IO;
using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class AutotileFilePicker : AbstractFilePicker
{
    public AutotileFilePicker(string InitialFilename = null)
        : base("Autotiles", Data.ProjectPath + "/Graphics/Autotiles", InitialFilename)
    {
        SetSize(506, 400);
        Center();
    }
}
