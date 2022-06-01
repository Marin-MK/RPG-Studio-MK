using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class LayerChangeUndoAction : BaseUndoAction
{
    public int MapID;
    public int LayerIndex;
    public Layer LayerData;
    public bool Removal;

    public LayerChangeUndoAction(int MapID, int LayerIndex, Layer LayerData, bool Removal)
    {
        this.MapID = MapID;
        this.LayerIndex = LayerIndex;
        this.LayerData = (Layer)LayerData.Clone();
        this.Removal = Removal;
    }

    public static void Create(int MapID, int LayerIndex, Layer LayerData, bool Removal)
    {
        var c = new LayerChangeUndoAction(MapID, LayerIndex, LayerData, Removal);
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
        //  Removal &&  IsRedo : Redoing Removal  : Remove
        //  Removal && !IsRedo : Undoing Removal  : Create
        // !Removal &&  IsRedo : Redoing Creation : Create
        // !Removal && !IsRedo : Undoing Creation : Remove
        bool Create = Removal != IsRedo;
        if (Create)
        {
            Editor.MainWindow.MapWidget.MapViewer.LayerPanel.NewLayer(LayerIndex - 1, LayerData, true);
        }
        else
        {
            Editor.MainWindow.MapWidget.MapViewer.LayerPanel.DeleteLayer(LayerIndex, true);
        }
        return true;
    }
}
