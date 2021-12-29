using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

public class TilesetNameChangeUndoAction : BaseUndoAction
{
    int TilesetID;
    TextAreaState OldState;
    TextAreaState NewState;

    public TilesetNameChangeUndoAction(int TilesetID, TextAreaState OldState, TextAreaState NewState)
    {
        this.TilesetID = TilesetID;
        this.OldState = OldState;
        this.NewState = NewState;
    }

    public static void Create(int TilesetID, TextAreaState OldState, TextAreaState NewState)
    {
        new TilesetNameChangeUndoAction(TilesetID, OldState, NewState);
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
        Widgets.DataTypeTilesets dtt = (Widgets.DataTypeTilesets) Editor.MainWindow.DatabaseWidget.ActiveDatabaseWidget;
        if (dtt.SelectedItem == null || dtt.SelectedItem.ID != TilesetID)
        {
            dtt.SetSelectedIndex(TilesetID - 1);
            Continue = false;
        }
        if (!Continue) return false;
        if (IsRedo)
        {
            Game.Data.Tilesets[TilesetID].Name = NewState.Text;
            dtt.NameBox.SetState(NewState);
        }
        else
        {
            Game.Data.Tilesets[TilesetID].Name = OldState.Text;
            dtt.NameBox.SetState(OldState);
        }
        dtt.RedrawList();
        dtt.OldNameBoxState = dtt.NameBox.GetState();
        return true;
    }

    public override string ToString()
    {
        return OldState.Text + " -> " + NewState.Text;
    }
}
