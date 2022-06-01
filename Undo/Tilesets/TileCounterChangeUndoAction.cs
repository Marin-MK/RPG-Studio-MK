using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TileCounterChangeUndoAction : BaseUndoAction
{
    int TilesetID;
    int TileID;
    int TileX;
    int TileY;
    bool OldCounter;
    bool NewCounter;

    public TileCounterChangeUndoAction(int TilesetID, int TileID, int TileX, int TileY, bool OldCounter, bool NewCounter)
    {
        this.TilesetID = TilesetID;
        this.TileID = TileID;
        this.TileX = TileX;
        this.TileY = TileY;
        this.OldCounter = OldCounter;
        this.NewCounter = NewCounter;
    }

    public static void Create(int TilesetID, int TileID, int TileX, int TileY, bool OldCounter, bool NewCounter)
    {
        var c = new TileCounterChangeUndoAction(TilesetID, TileID, TileX, TileY, OldCounter, NewCounter);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        bool Continue = true;
        if (!InMode(EditorMode.Database))
        {
            SetDatabaseMode(DatabaseMode.Tilesets);
            Continue = false;
        }
        if (!InDatabaseSubmode(DatabaseMode.Tilesets))
        {
            SetDatabaseSubmode(DatabaseMode.Tilesets);
            Continue = false;
        }
        Widgets.DataTypeTilesets dtt = (Widgets.DataTypeTilesets)Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget;
        if (dtt.SelectedItem == null || dtt.SelectedItem.ID != TilesetID)
        {
            dtt.SetSelectedIndex(TilesetID - 1);
            Continue = false;
        }
        if (dtt.Tabs.SelectedIndex != 4)
        {
            dtt.Tabs.SelectTab(4);
            Continue = false;
        }
        if (!Continue) return false;
        if (IsRedo)
        {
            Game.Data.Tilesets[TilesetID].CounterFlags[TileID] = NewCounter;
        }
        else
        {
            Game.Data.Tilesets[TilesetID].CounterFlags[TileID] = OldCounter;
        }
        dtt.TilesetContainer.SetTileCounter(this.TileID, this.TileX, this.TileY, Game.Data.Tilesets[TilesetID].CounterFlags[TileID]);
        return true;
    }
}
