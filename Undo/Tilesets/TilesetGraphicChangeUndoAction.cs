using RPGStudioMK.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class TilesetGraphicChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Tileset graphic changed";
    public override string Description => $"Tileset: {Utilities.Digits(TilesetID, 3)}\nOld: {OldGraphicName}\nNew: {NewGraphicName}";

    int TilesetID;
    string OldGraphicName;
    string NewGraphicName;

    List<Passability> OldPassabilities;
    List<Passability> NewPassabilities;
    List<int> OldPriorities;
    List<int> NewPriorities;
    List<int> OldTags;
    List<int> NewTags;
    List<bool> OldBushFlags;
    List<bool> NewBushFlags;
    List<bool> OldCounterFlags;
    List<bool> NewCounterFlags;

    public TilesetGraphicChangeUndoAction(int TilesetID, string OldGraphicName, string NewGraphicName,
                                          List<Passability> OldPassabilities, List<Passability> NewPassabilities,
                                          List<int> OldPriorities, List<int> NewPriorities,
                                          List<int> OldTags, List<int> NewTags,
                                          List<bool> OldBushFlags, List<bool> NewBushFlags,
                                          List<bool> OldCounterFlags, List<bool> NewCounterFlags)
    {
        this.TilesetID = TilesetID;
        this.OldGraphicName = OldGraphicName;
        this.NewGraphicName = NewGraphicName;
        this.OldPassabilities = OldPassabilities;
        this.NewPassabilities = NewPassabilities;
        this.OldPriorities = OldPriorities;
        this.NewPriorities = NewPriorities;
        this.OldTags = OldTags;
        this.NewTags = NewTags;
        this.OldBushFlags = OldBushFlags;
        this.NewBushFlags = NewBushFlags;
        this.OldCounterFlags = OldCounterFlags;
        this.NewCounterFlags = NewCounterFlags;
    }

    public static void Create(int TilesetID, string OldGraphicName, string NewGraphicName,
                              List<Passability> OldPassabilities, List<Passability> NewPassabilities,
                              List<int> OldPriorities, List<int> NewPriorities,
                              List<int> OldTags, List<int> NewTags,
                              List<bool> OldBushFlags, List<bool> NewBushFlags,
                              List<bool> OldCounterFlags, List<bool> NewCounterFlags)
    {
        var c = new TilesetGraphicChangeUndoAction(TilesetID, OldGraphicName, NewGraphicName,
                                                   OldPassabilities, NewPassabilities, OldPriorities, NewPriorities,
                                                   OldTags, NewTags, OldBushFlags, NewBushFlags,
                                                   OldCounterFlags, NewCounterFlags);
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
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        Tileset Tileset = Data.Tilesets[TilesetID];
        dtt.GraphicBox.SetText(Tileset.GraphicName);
        dtt.SetTileset(Tileset, true);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Tileset Tileset = Data.Tilesets[TilesetID];
        if (IsRedo)
        {
            Tileset.GraphicName = NewGraphicName;
            Tileset.Passabilities = NewPassabilities;
            Tileset.Priorities = NewPriorities;
            Tileset.Tags = NewTags;
            Tileset.BushFlags = NewBushFlags;
            Tileset.CounterFlags = NewCounterFlags;
        }
        else
        {
            Tileset.GraphicName = OldGraphicName;
            Tileset.Passabilities = OldPassabilities;
            Tileset.Priorities = OldPriorities;
            Tileset.Tags = OldTags;
            Tileset.BushFlags = OldBushFlags;
            Tileset.CounterFlags = OldCounterFlags;
        }
        Tileset.CreateBitmap(true);
    }
}
