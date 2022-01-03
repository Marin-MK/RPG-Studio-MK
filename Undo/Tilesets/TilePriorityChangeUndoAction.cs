using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TilePriorityChangeUndoAction : BaseUndoAction
{
    int TilesetID;
    int TileID;
    int TileX;
    int TileY;
    int OldPriority;
    int NewPriority;

    public TilePriorityChangeUndoAction(int TilesetID, int TileID, int TileX, int TileY, int OldPriority, int NewPriority)
    {
        this.TilesetID = TilesetID;
        this.TileID = TileID;
        this.TileX = TileX;
        this.TileY = TileY;
        this.OldPriority = OldPriority;
        this.NewPriority = NewPriority;
    }

    public static void Create(int TilesetID, int TileID, int TileX, int TileY, int OldPriority, int NewPriority)
    {
        var c = new TilePriorityChangeUndoAction(TilesetID, TileID, TileX, TileY, OldPriority, NewPriority);
        c.Register();
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
        if (dtt.Tabs.SelectedIndex != 2)
        {
            dtt.Tabs.SelectTab(2);
            Continue = false;
        }
        if (!Continue) return false;
        if (IsRedo)
        {
            Game.Data.Tilesets[TilesetID].Priorities[TileID] = NewPriority;
        }
        else
        {
            Game.Data.Tilesets[TilesetID].Priorities[TileID] = OldPriority;
        }
        dtt.TilesetContainer.SetTilePriority(this.TileID, this.TileX, this.TileY, Game.Data.Tilesets[TilesetID].Priorities[TileID]);
        return true;
    }
}
