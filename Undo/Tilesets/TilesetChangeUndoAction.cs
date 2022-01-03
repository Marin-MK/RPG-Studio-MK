using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class TilesetChangeUndoAction : BaseUndoAction
{
    Tileset OldTileset;
    Tileset NewTileset;

    public TilesetChangeUndoAction(Tileset OldTileset, Tileset NewTileset)
    {
        this.OldTileset = OldTileset;
        this.NewTileset = NewTileset;
    }

    public static void Create(Tileset OldTileset, Tileset NewTileset)
    {
        new TilesetChangeUndoAction(OldTileset, NewTileset);
    }

    public override bool Trigger(bool IsRedo)
    {
        bool Continue = true;
        if (!InMode(EditorMode.Database))
        {
            SetDatabaseMode(Widgets.DatabaseMode.Tilesets);
            Continue = false;
        }
        if (!InDatabaseSubmode(Widgets.DatabaseMode.Tilesets))
        {
            SetDatabaseSubmode(Widgets.DatabaseMode.Tilesets);
            Continue = false;
        }
        Widgets.DataTypeTilesets dtt = (Widgets.DataTypeTilesets)Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget;
        if (dtt.SelectedItem == null || dtt.SelectedItem.ID != OldTileset.ID)
        {
            dtt.SetSelectedIndex(OldTileset.ID - 1);
            Continue = false;
        }
        if (!Continue) return false;
        if (IsRedo)
        {
            // Deletes the tileset
            Data.Tilesets[OldTileset.ID] = NewTileset;
        }
        else
        {
            // Restores the tileset
            Data.Tilesets[OldTileset.ID] = OldTileset;
        }
        dtt.RedrawList();
        dtt.SetTileset(Data.Tilesets[OldTileset.ID], true);
        return true;
    }
}
