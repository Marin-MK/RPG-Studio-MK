using RPGStudioMK.Game;

namespace RPGStudioMK.Widgets;

public class TilesetFilePicker : AbstractFilePicker
{
    public TilesetFilePicker(string InitialFilename = null)
        : base("Tilesets", Data.ProjectPath + "/Graphics/Tilesets", InitialFilename)
    {

    }
}
