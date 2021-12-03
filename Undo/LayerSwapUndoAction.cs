namespace RPGStudioMK;

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
        new LayerSwapUndoAction(MapID, LayerIndex, MovedUp);
    }

    public override bool Trigger(bool IsRedo)
    {
        Widgets.LayerPanel LayerPanel = Editor.MainWindow.MapWidget.MapViewerTiles.LayerPanel;
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
