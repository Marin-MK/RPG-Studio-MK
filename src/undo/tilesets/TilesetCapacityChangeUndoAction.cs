using RPGStudioMK.Game;
using System.Collections.Generic;

namespace RPGStudioMK.Undo;

public class TilesetCapacityChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Tileset capacity changed";
    public override string Description => $"Old capacity: {OldCapacity}\nNew capacity: {NewCapacity}";

    List<Tileset> OldTilesets;
    List<Tileset> NewTilesets;
    int OldCapacity;
    int NewCapacity;

    public TilesetCapacityChangeUndoAction(List<Tileset> OldTilesets, List<Tileset> NewTilesets, int OldCapacity, int NewCapacity)
    {
        this.OldTilesets = OldTilesets;
        this.NewTilesets = NewTilesets;
        this.OldCapacity = OldCapacity;
        this.NewCapacity = NewCapacity;
    }

    public static void Create(List<Tileset> OldTilesets, List<Tileset> NewTilesets, int OldCapacity, int NewCapacity)
    {
        var c = new TilesetCapacityChangeUndoAction(OldTilesets, NewTilesets, OldCapacity, NewCapacity);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        if (!InMode(EditorMode.Database)) return SetDatabaseMode(DatabaseMode.Tilesets);
        if (!InDatabaseSubmode(DatabaseMode.Tilesets)) return SetDatabaseSubmode(DatabaseMode.Tilesets);
        Widgets.DatabaseWidget DatabaseWidget = Editor.MainWindow.DatabaseWidget;
        if (!InDatabaseSubmode(DatabaseMode.Tilesets))
        {
            SetDatabaseSubmode(DatabaseMode.Tilesets);
            return false;
        }
        TriggerLogical(IsRedo);
        ((Widgets.DataTypeTilesets) DatabaseWidget.ActiveDatabaseWidget).RedrawList();
        ((Widgets.DataTypeTilesets) DatabaseWidget.ActiveDatabaseWidget).TilesetList.ListMaximum = Editor.ProjectSettings.TilesetCapacity;
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        if (IsRedo)
        {
            Data.Tilesets = NewTilesets;
            Editor.ProjectSettings.TilesetCapacity = NewCapacity;
        }
        else
        {
            Data.Tilesets = OldTilesets;
            Editor.ProjectSettings.TilesetCapacity = OldCapacity;
        }
    }
}
