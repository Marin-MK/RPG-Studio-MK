using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class TileBushChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Changed bush flag";
    public override string Description => $"Tileset: {TilesetID}\nTile: {TileID}\nTile X: {TileX}\nTile Y: {TileY}\nOld: {OldBush}\nNew: {NewBush}";

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
        var c = new TileBushChangeUndoAction(TilesetID, TileID, TileX, TileY, OldBush, NewBush);
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
        if (dtt.Tabs.SelectedIndex != 3)
        {
            dtt.Tabs.SelectTab(3);
            Continue = false;
        }
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        dtt.TilesetContainer.SetTileBush(this.TileID, this.TileX, this.TileY, Game.Data.Tilesets[TilesetID].BushFlags[TileID]);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Data.Tilesets[TilesetID].BushFlags[TileID] = IsRedo ? NewBush : OldBush;
    }
}
