using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class LayerSwapUndoAction : BaseUndoAction
{
    public int MapID;
    public int LayerIndex;
    public bool MovedUp;

    public LayerSwapUndoAction(int MapID, int LayerIndex, bool MovedUp)
    {
        this.MapID = MapID;
        this.LayerIndex = LayerIndex;
        this.MovedUp = MovedUp;
    }

    public static void Create(int MapID, int LayerIndex, bool MovedUp)
    {
        var c = new LayerSwapUndoAction(MapID, LayerIndex, MovedUp);
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
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Data.Maps[this.MapID]);
            Continue = false;
        }
        if (!Continue) return false;
        Widgets.LayerPanel LayerPanel = Editor.MainWindow.MapWidget.MapViewer.LayerPanel;
        bool MoveUp = this.MovedUp == IsRedo;
        if (MoveUp)
        {
            LayerPanel.MoveLayerUp(LayerIndex - 1, true);
        }
        else
        {
            LayerPanel.MoveLayerDown(LayerIndex + 1, true);
        }
        return true;
    }
}
