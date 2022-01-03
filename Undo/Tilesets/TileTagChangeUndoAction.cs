using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TileTagChangeUndoAction : BaseUndoAction
{
    int TilesetID;
    int TileID;
    int TileX;
    int TileY;
    int OldTag;
    int NewTag;

    public TileTagChangeUndoAction(int TilesetID, int TileID, int TileX, int TileY, int OldTag, int NewTag)
    {
        this.TilesetID = TilesetID;
        this.TileID = TileID;
        this.TileX = TileX;
        this.TileY = TileY;
        this.OldTag = OldTag;
        this.NewTag = NewTag;
    }

    public static void Create(int TilesetID, int TileID, int TileX, int TileY, int OldTag, int NewTag)
    {
        new TileTagChangeUndoAction(TilesetID, TileID, TileX, TileY, OldTag, NewTag);
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
        if (dtt.Tabs.SelectedIndex != 5)
        {
            dtt.Tabs.SelectTab(5);
            Continue = false;
        }
        if (!Continue) return false;
        if (IsRedo)
        {
            Game.Data.Tilesets[TilesetID].Tags[TileID] = NewTag;
        }
        else
        {
            Game.Data.Tilesets[TilesetID].Tags[TileID] = OldTag;
        }
        dtt.TilesetContainer.SetTileTag(this.TileID, this.TileX, this.TileY, Game.Data.Tilesets[TilesetID].Tags[TileID]);
        return true;
    }
}
