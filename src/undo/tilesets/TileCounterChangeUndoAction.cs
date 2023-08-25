using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class TileCounterChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Changed counter flag";
    public override string Description => $"Tileset: {TilesetID}\nTile: {TileID}\nTile X: {TileX}\nTile Y: {TileY}\nOld: {OldCounter}\nNew: {NewCounter}";

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
        TriggerLogical(IsRedo);
        dtt.TilesetContainer.SetTileCounter(this.TileID, this.TileX, this.TileY, Game.Data.Tilesets[TilesetID].CounterFlags[TileID]);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Data.Tilesets[TilesetID].CounterFlags[TileID] = IsRedo ? NewCounter : OldCounter;
    }
}
