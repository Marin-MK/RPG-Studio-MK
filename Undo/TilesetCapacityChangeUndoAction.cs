using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

public class TilesetCapacityChangeUndoAction : BaseUndoAction
{
    List<Game.Tileset> OldTilesets;
    List<Game.Tileset> NewTilesets;
    int OldCapacity;
    int NewCapacity;

    public TilesetCapacityChangeUndoAction(List<Game.Tileset> OldTilesets, List<Game.Tileset> NewTilesets, int OldCapacity, int NewCapacity)
    {
        this.OldTilesets = OldTilesets;
        this.NewTilesets = NewTilesets;
        this.OldCapacity = OldCapacity;
        this.NewCapacity = NewCapacity;
    }

    public static void Create(List<Game.Tileset> OldTilesets, List<Game.Tileset> NewTilesets, int OldCapacity, int NewCapacity)
    {
        new TilesetCapacityChangeUndoAction(OldTilesets, NewTilesets, OldCapacity, NewCapacity);
    }

    public override bool Trigger(bool IsRedo)
    {
        if (!InMode(EditorMode.Database)) return SetDatabaseMode(Widgets.DatabaseMode.Tilesets);
        if (!InDatabaseSubmode(Widgets.DatabaseMode.Tilesets)) return SetDatabaseSubmode(Widgets.DatabaseMode.Tilesets);
        Widgets.DatabaseWidget DatabaseWidget = (Widgets.DatabaseWidget)Editor.MainWindow.MainEditorWidget;
        bool IsActiveTab = DatabaseWidget.Mode == Widgets.DatabaseMode.Tilesets;
        if (!IsActiveTab)
        {
            DatabaseWidget.SetMode(Widgets.DatabaseMode.Tilesets);
            return false;
        }
        if (IsRedo)
        {
            Game.Data.Tilesets = NewTilesets;
            Editor.ProjectSettings.TilesetCapacity = NewCapacity;
        }
        else
        {
            Game.Data.Tilesets = OldTilesets;
            Editor.ProjectSettings.TilesetCapacity = OldCapacity;
        }
        ((Widgets.DataTypeTilesets) DatabaseWidget.ActiveDatabaseWidget).RedrawList();
        ((Widgets.DataTypeTilesets) DatabaseWidget.ActiveDatabaseWidget).TilesetList.ListMaximum = Editor.ProjectSettings.TilesetCapacity;
        return true;
    }
}
