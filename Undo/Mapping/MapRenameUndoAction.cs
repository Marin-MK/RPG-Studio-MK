using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets;

namespace RPGStudioMK.Undo;

public class MapRenameUndoAction : BaseUndoAction
{
    public int MapID;
    public string OldName;
    public string NewName;

    public MapRenameUndoAction(int MapID, string OldName, string NewName)
    {
        this.MapID = MapID;
        this.OldName = OldName;
        this.NewName = NewName;
    }

    public static void Create(int MapID, string OldName, string NewName)
    {
        var c = new MapRenameUndoAction(MapID, OldName, NewName);
        c.Register();
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
        if (IsRedo)
        {
            Data.Maps[this.MapID].Name = NewName;
        }
        else
        {
            Data.Maps[this.MapID].Name = OldName;
        }
        TreeView mapview = Editor.MainWindow.MapWidget.MapSelectPanel.mapview;
        foreach (TreeNode Node in mapview.Nodes)
        {
            if ((int) Node.Object == this.MapID)
            {
                Node.Name = Data.Maps[this.MapID].Name;
                break;
            }
            else
            {
                TreeNode n = Node.FindNode(n => (int) n.Object == this.MapID);
                if (n != null)
                {
                    n.Name = Data.Maps[this.MapID].Name;
                    break;
                }
            }
        }
        mapview.Redraw();
        return true;
    }
}
