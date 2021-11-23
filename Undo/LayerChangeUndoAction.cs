using System;
using System.Collections.Generic;
using RPGStudioMK.Game;

namespace RPGStudioMK
{
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
            this.LayerData = (Layer) LayerData.Clone();
            this.Removal = Removal;
        }

        public static void Create(int MapID, int LayerIndex, Layer LayerData, bool Removal)
        {
            new LayerChangeUndoAction(MapID, LayerIndex, LayerData, Removal);
        }

        public override bool Trigger(bool IsRedo)
        {
            //  Removal &&  IsRedo : Redoing Removal  : Remove
            //  Removal && !IsRedo : Undoing Removal  : Create
            // !Removal &&  IsRedo : Redoing Creation : Create
            // !Removal && !IsRedo : Undoing Creation : Remove
            bool Create = Removal != IsRedo;
            if (Create)
            {
                Editor.MainWindow.MapWidget.MapViewerTiles.LayerPanel.NewLayer(LayerIndex - 1, LayerData, true);
            }
            else
            {
                Editor.MainWindow.MapWidget.MapViewerTiles.LayerPanel.DeleteLayer(LayerIndex, true);
            }
            return true;
        }
    }
}
