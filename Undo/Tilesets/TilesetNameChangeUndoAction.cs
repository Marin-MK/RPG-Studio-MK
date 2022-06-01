using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TilesetNameChangeUndoAction : BaseUndoAction
{
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
        if (IsRedo)
        {
            Game.Data.Tilesets[TilesetID].Name = NewText;
        }
        else
        {
            Game.Data.Tilesets[TilesetID].Name = OldText;
        }
        dtt.NameBox.SetText(Game.Data.Tilesets[TilesetID].Name);
        dtt.RedrawList();
        return true;
    }
}
