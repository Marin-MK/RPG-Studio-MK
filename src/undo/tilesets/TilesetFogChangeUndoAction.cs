using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class TilesetFogChangeUndoAction : BaseUndoAction
{
    public override string Title => "Fog changed";
    public override string Description
    {
        get
        {
            string s = $"Tileset: {Utilities.Digits(TilesetID, 3)}\n\n";
            s += $"Old Fog\n";
            s += $"Name: {OldFogName}\n";
            s += $"Hue: {OldFogHue}\n";
            s += $"Blend Type: {OldFogBlendType}\n";
            s += $"SX: {OldFogSX}\n";
            s += $"SY: {OldFogSY}\n";
            s += $"Zoom: {OldFogZoom}\n";
            s += $"Opacity: {OldFogOpacity}\n\n";
            
            s += $"New Tileset\n";
            s += $"Name: {NewFogName}\n";
            s += $"Hue: {NewFogHue}\n";
            s += $"Blend Type: {NewFogBlendType}\n";
            s += $"SX: {NewFogSX}\n";
            s += $"SY: {NewFogSY}\n";
            s += $"Zoom: {NewFogZoom}\n";
            s += $"Opacity: {NewFogOpacity}\n";
            return s;
        }
    }

    int TilesetID;
    string OldFogName;
    string NewFogName;
    int OldFogHue;
    int NewFogHue;
    byte OldFogOpacity;
    byte NewFogOpacity;
    int OldFogBlendType;
    int NewFogBlendType;
    int OldFogZoom;
    int NewFogZoom;
    int OldFogSX;
    int NewFogSX;
    int OldFogSY;
    int NewFogSY;

    public TilesetFogChangeUndoAction(int TilesetID, string OldFogName, string NewFogName, int OldFogHue, int NewFogHue,
                                      byte OldFogOpacity, byte NewFogOpacity, int OldFogBlendType, int NewFogBlendType,
                                      int OldFogZoom, int NewFogZoom, int OldFogSX, int NewFogSX, int OldFogSY, int NewFogSY)
    {
        this.TilesetID = TilesetID;
        this.OldFogName = OldFogName;
        this.NewFogName = NewFogName;
        this.OldFogHue = OldFogHue;
        this.NewFogHue = NewFogHue;
        this.OldFogOpacity = OldFogOpacity;
        this.NewFogOpacity = NewFogOpacity;
        this.OldFogBlendType = OldFogBlendType;
        this.NewFogBlendType = NewFogBlendType;
        this.OldFogZoom = OldFogZoom;
        this.NewFogZoom = NewFogZoom;
        this.OldFogSX = OldFogSX;
        this.NewFogSX = NewFogSX;
        this.OldFogSY = OldFogSY;
        this.NewFogSY = NewFogSY;
    }

    public static void Create(int TilesetID, string OldFogName, string NewFogName, int OldFogHue, int NewFogHue,
                              byte OldFogOpacity, byte NewFogOpacity, int OldFogBlendType, int NewFogBlendType,
                              int OldFogZoom, int NewFogZoom, int OldFogSX, int NewFogSX, int OldFogSY, int NewFogSY)
    {
        var c = new TilesetFogChangeUndoAction(TilesetID, OldFogName, NewFogName, OldFogHue, NewFogHue,
                                               OldFogOpacity, NewFogOpacity, OldFogBlendType, NewFogBlendType,
                                               OldFogZoom, NewFogZoom, OldFogSX, NewFogSX, OldFogSY, NewFogSY);
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
        dtt.FogBox.SetText(Game.Data.Tilesets[TilesetID].FogName);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        if (IsRedo)
        {
            Data.Tilesets[TilesetID].FogName = NewFogName;
            Data.Tilesets[TilesetID].FogHue = NewFogHue;
            Data.Tilesets[TilesetID].FogOpacity = NewFogOpacity;
            Data.Tilesets[TilesetID].FogBlendType = NewFogBlendType;
            Data.Tilesets[TilesetID].FogZoom = NewFogZoom;
            Data.Tilesets[TilesetID].FogSX = NewFogSX;
            Data.Tilesets[TilesetID].FogSY = NewFogSY;
        }
        else
        {
            Data.Tilesets[TilesetID].FogName = OldFogName;
            Data.Tilesets[TilesetID].FogHue = OldFogHue;
            Data.Tilesets[TilesetID].FogOpacity = OldFogOpacity;
            Data.Tilesets[TilesetID].FogBlendType = OldFogBlendType;
            Data.Tilesets[TilesetID].FogZoom = OldFogZoom;
            Data.Tilesets[TilesetID].FogSX = OldFogSX;
            Data.Tilesets[TilesetID].FogSY = OldFogSY;
        }
    }
}
