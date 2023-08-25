using System.Collections.Generic;
using System.Text;
using RPGStudioMK.Game;

namespace RPGStudioMK.Undo;

public class MapTilesetsChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Map tilesets changed";
    public override string Description
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Old Tilesets:");
            for (int i = 0; i < OldTilesetIDs.Count; i++)
            {
                int id = OldTilesetIDs[i];
                if (id >= Data.Tilesets.Count || Data.Tilesets[id] == null) sb.Append(Utilities.Digits(id, 3));
                else sb.Append(Data.Tilesets[id].Name);
                if (i != OldTilesetIDs.Count - 1) sb.Append(", ");
            }
            sb.AppendLine();
            sb.AppendLine("New Tilesets:");
            for (int i = 0; i < NewTilesetIDs.Count; i++)
            {
                int id = NewTilesetIDs[i];
                if (id >= Data.Tilesets.Count || Data.Tilesets[id] == null) sb.Append(Utilities.Digits(id, 3));
                else sb.Append(Data.Tilesets[id].Name);
                if (i != NewTilesetIDs.Count - 1) sb.Append(", ");
            }
            return sb.ToString();
        }
    }

    public int MapID;
    public List<int> OldTilesetIDs;
    public List<int> NewTilesetIDs;

    public MapTilesetsChangeUndoAction(int MapID, List<int> OldTilesetIDs, List<int> NewTilesetIDs)
    {
        this.MapID = MapID;
        this.OldTilesetIDs = OldTilesetIDs;
        this.NewTilesetIDs = NewTilesetIDs;
    }

    public static void Create(int MapID, List<int> OldTilesetIDs, List<int> NewTilesetIDs)
    {
        var c = new MapTilesetsChangeUndoAction(MapID, OldTilesetIDs, NewTilesetIDs);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        // Ensure we're in the Mapping mode
        bool Continue = true;
        if (!InMode(EditorMode.Mapping))
        {
            SetMode(EditorMode.Mapping);
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
        Editor.MainWindow.MapWidget.SetMap(Data.Maps[this.MapID]);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Data.Maps[this.MapID].TilesetIDs = IsRedo ? NewTilesetIDs : OldTilesetIDs;
        // Update autotiles
        for (int i = 0; i < 7; i++)
        {
            Data.Maps[this.MapID].AutotileIDs[i] = Data.Maps[this.MapID].TilesetIDs[0] * 7 + i;
        }
    }
}
