using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class LayerRenameUndoAction : BaseUndoAction
{
    public override string Title => $"Renamed Layer";
    public override string Description => $"Map: {(Data.Maps.ContainsKey(MapID) ? Data.Maps[MapID].Name : Utilities.Digits(MapID, 3))}\nLayer: {LayerIndex}\nOld Name: {OldName}\nNew Name: {NewName}";

    public int MapID;
    public int LayerIndex;
    public string OldName;
    public string NewName;

    public LayerRenameUndoAction(int MapID, int LayerIndex, string OldName, string NewName)
    {
        this.MapID = MapID;
        this.LayerIndex = LayerIndex;
        this.OldName = OldName;
        this.NewName = NewName;
    }

    public static void Create(int MapID, int LayerIndex, string OldName, string NewName)
    {
        var c = new LayerRenameUndoAction(MapID, LayerIndex, OldName, NewName);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        // Ensure we're in the Mapping mode
        bool Continue = true;
        if (!InMode(EditorMode.Mapping))
        {
            SetMappingMode(MapMode.Tiles);
            Continue = false;
        }
        // Ensure we're in the Tiles submode
        if (!InMappingSubmode(MapMode.Tiles))
        {
            SetMappingMode(MapMode.Tiles);
            Continue = false;
        }
        // Ensure we're on the map this action was taken on
        if (Editor.MainWindow.MapWidget.Map.ID != MapID)
        {
            Editor.MainWindow.MapWidget.SetMap(Data.Maps[this.MapID]);
            Continue = false;
        }
        if (!Continue) return false;
        TriggerLogical(IsRedo);
        Editor.MainWindow.MapWidget.MapViewer.LayerPanel.layerwidget.Redraw();
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Data.Maps[MapID].Layers[LayerIndex].Name = IsRedo ? NewName : OldName;
    }
}
