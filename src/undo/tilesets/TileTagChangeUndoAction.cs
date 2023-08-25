using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class TileTagChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Changed tile tag";
    public override string Description => $"Tileset: {TilesetID}\nTile: {TileID}\nTile X: {TileX}\nTile Y: {TileY}\nOld: {OldTag}\nNew: {NewTag}";

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
        var c = new TileTagChangeUndoAction(TilesetID, TileID, TileX, TileY, OldTag, NewTag);
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
        if (dtt.Tabs.SelectedIndex != 5)
        {
            dtt.Tabs.SelectTab(5);
            Continue = false;
        }
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        dtt.TilesetContainer.SetTileTag(this.TileID, this.TileX, this.TileY, Data.Tilesets[TilesetID].Tags[TileID]);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Data.Tilesets[TilesetID].Tags[TileID] = IsRedo ? NewTag : OldTag;
    }
}
