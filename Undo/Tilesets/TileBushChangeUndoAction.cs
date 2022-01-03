using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TileBushChangeUndoAction : BaseUndoAction
{
    int TilesetID;
    int TileID;
    int TileX;
    int TileY;
    bool OldBush;
    bool NewBush;

    public TileBushChangeUndoAction(int TilesetID, int TileID, int TileX, int TileY, bool OldBush, bool NewBush)
    {
        this.TilesetID = TilesetID;
        this.TileID = TileID;
        this.TileX = TileX;
        this.TileY = TileY;
        this.OldBush = OldBush;
        this.NewBush = NewBush;
    }

    public static void Create(int TilesetID, int TileID, int TileX, int TileY, bool OldBush, bool NewBush)
    {
        new TileBushChangeUndoAction(TilesetID, TileID, TileX, TileY, OldBush, NewBush);
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
        if (dtt.SelectedItem == null || dtt.SelectedItem.ID != TilesetID)
        {
            dtt.SetSelectedIndex(TilesetID - 1);
            Continue = false;
        }
        if (dtt.Tabs.SelectedIndex != 3)
        {
            dtt.Tabs.SelectTab(3);
            Continue = false;
        }
        if (!Continue) return false;
        if (IsRedo)
        {
            Game.Data.Tilesets[TilesetID].BushFlags[TileID] = NewBush;
        }
        else
        {
            Game.Data.Tilesets[TilesetID].BushFlags[TileID] = OldBush;
        }
        dtt.TilesetContainer.SetTileBush(this.TileID, this.TileX, this.TileY, Game.Data.Tilesets[TilesetID].BushFlags[TileID]);
        return true;
    }
}
