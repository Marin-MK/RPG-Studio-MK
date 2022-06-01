using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class NodeCollapseChangeUndoAction : BaseUndoAction
{
    public int MapID;
    public bool OldCollapsed;
    public bool NewCollapsed;
    public int SelectedMapID;

    public NodeCollapseChangeUndoAction(int MapID, bool OldCollapsed, bool NewCollapsed, int SelectedMapID)
    {
        this.MapID = MapID;
        this.OldCollapsed = OldCollapsed;
        this.NewCollapsed = NewCollapsed;
        this.SelectedMapID = SelectedMapID;
    }

    public static void Create(int MapID, bool OldCollapsed, bool NewCollapsed, int SelectedMapID)
    {
        var c = new NodeCollapseChangeUndoAction(MapID, OldCollapsed, NewCollapsed, SelectedMapID);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        if (!InMode(EditorMode.Mapping))
        {
            SetMode(EditorMode.Mapping);
            return false;
        }
        TreeView mapview = Editor.MainWindow.MapWidget.MapSelectPanel.mapview;
        int SelectedMapID = (int) mapview.SelectedNode.Object;
        TreeNode Node = null;
        foreach (TreeNode n in mapview.Nodes)
        {
            if ((int) n.Object == this.MapID)
            {
                Node = n;
                break;
            }
            else
            {
                TreeNode foundnode = n.FindNode(n => (int) n.Object == this.MapID);
                if (foundnode != null)
                {
                    Node = foundnode;
                    break;
                }
            }
        }
        if (IsRedo)
        {
            Node.Collapsed = NewCollapsed;
            Game.Data.Maps[(int) Node.Object].Expanded = !NewCollapsed;
        }
        else
        {
            Node.Collapsed = OldCollapsed;
            Game.Data.Maps[(int) Node.Object].Expanded = !OldCollapsed;
        }
        if (Node.FindNode(n => (int) n.Object == SelectedMapID) != null)
        {
            mapview.SetSelectedNode(Node);
        }
        mapview.Redraw();
        return true;
    }
}
