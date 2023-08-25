using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class TilesetNameChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Tileset name changed";
    public override string Description => $"Tileset: {Utilities.Digits(TilesetID, 3)}\nOld Name: {OldText}\nNew Name: {NewText}";

    int TilesetID;
    string OldText;
    string NewText;

    public TilesetNameChangeUndoAction(int TilesetID, string OldText, string NewText)
    {
        this.TilesetID = TilesetID;
        this.OldText = OldText;
        this.NewText = NewText;
    }

    public static void Create(int TilesetID, string OldText, string NewText)
    {
        var c = new TilesetNameChangeUndoAction(TilesetID, OldText, NewText);
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
        Widgets.DataTypeTilesets dtt = (Widgets.DataTypeTilesets) Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget;
        if (dtt.SelectedItem == null || dtt.SelectedItem.ID != TilesetID)
        {
            dtt.SetSelectedIndex(TilesetID - 1);
            Continue = false;
        }
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        dtt.NameBox.SetText(Game.Data.Tilesets[TilesetID].Name);
        dtt.RedrawList();
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Data.Tilesets[TilesetID].Name = IsRedo ? NewText : OldText;
    }
}
