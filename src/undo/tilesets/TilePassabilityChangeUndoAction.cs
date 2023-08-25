using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class TilePassabilityChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Changed passability";
    public override string Description => $"Tileset: {TilesetID}\nTile: {TileID}\nTile X: {TileX}\nTile Y: {TileY}\nOld: {OldPassability}\nNew: {NewPassability}";

    int TilesetID;
    int TileID;
    int TileX;
    int TileY;
    Passability OldPassability;
    Passability NewPassability;
    bool Directional;

    public TilePassabilityChangeUndoAction(int TilesetID, int TileID, int TileX, int TileY, Passability OldPassability, Passability NewPassability, bool Directional)
    {
        this.TilesetID = TilesetID;
        this.TileID = TileID;
        this.TileX = TileX;
        this.TileY = TileY;
        this.OldPassability = OldPassability;
        this.NewPassability = NewPassability;
        this.Directional = Directional;
    }

    public static void Create(int TilesetID, int TileID, int TileX, int TileY, Passability OldPassability, Passability NewPassability, bool Directional)
    {
        var c = new TilePassabilityChangeUndoAction(TilesetID, TileID, TileX, TileY, OldPassability, NewPassability, Directional);
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
        int tab = Directional ? 1 : 0;
        if (dtt.Tabs.SelectedIndex != tab)
        {
            dtt.Tabs.SelectTab(tab);
            Continue = false;
        }
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        dtt.TilesetContainer.SetTilePassability(this.TileID, this.TileX, this.TileY, Game.Data.Tilesets[TilesetID].Passabilities[TileID]);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Data.Tilesets[TilesetID].Passabilities[TileID] = IsRedo ? NewPassability : OldPassability;
    }
}
