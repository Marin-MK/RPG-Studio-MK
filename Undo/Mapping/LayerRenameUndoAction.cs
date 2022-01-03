using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class LayerRenameUndoAction : BaseUndoAction
{
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
        new LayerRenameUndoAction(MapID, LayerIndex, OldName, NewName);
    }

    public override bool Trigger(bool IsRedo)
    {
        if (!InMode(EditorMode.Mapping))
        {
            SetMode(EditorMode.Mapping);
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps[this.MapID]);
            return false;
        }
        bool ActiveMap = Editor.MainWindow.MapWidget.Map.ID == MapID;
        if (!ActiveMap)
        {
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps[this.MapID]);
            return false;
        }
        Map Map = Data.Maps[MapID];
        Map.Layers[LayerIndex].Name = IsRedo ? NewName : OldName;
        Editor.MainWindow.MapWidget.MapViewerTiles.LayerPanel.layerwidget.Redraw();
        return true;
    }
}
