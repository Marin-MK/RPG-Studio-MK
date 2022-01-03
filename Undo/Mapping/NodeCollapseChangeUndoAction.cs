using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class NodeCollapseChangeUndoAction : BaseUndoAction
{
    public TreeNode Node;
    public bool OldCollapsed;
    public bool NewCollapsed;
    public int SelectedMapID;

    public NodeCollapseChangeUndoAction(TreeNode Node, bool OldCollapsed, bool NewCollapsed, int SelectedMapID)
    {
        this.Node = Node;
        this.OldCollapsed = OldCollapsed;
        this.NewCollapsed = NewCollapsed;
        this.SelectedMapID = SelectedMapID;
    }

    public static void Create(TreeNode Node, bool OldCollapsed, bool NewCollapsed, int SelectedMapID)
    {
        var c = new NodeCollapseChangeUndoAction(Node, OldCollapsed, NewCollapsed, SelectedMapID);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        if (!InMode(EditorMode.Mapping))
        {
            SetMode(EditorMode.Mapping);
            Editor.MainWindow.MapWidget.MapSelectPanel.SetMap(Game.Data.Maps[Editor.ProjectSettings.LastMapID]);
            return false;
        }
        TreeView mapview = Editor.MainWindow.MapWidget.MapSelectPanel.mapview;
        int SelectedMapID = (int) mapview.SelectedNode.Object;
        if (IsRedo)
        {
            this.Node.Collapsed = NewCollapsed;
        }
        else
        {
            this.Node.Collapsed = OldCollapsed;
        }
        Game.Data.Maps[(int) this.Node.Object].Expanded = !this.Node.Collapsed;
        if (this.Node.FindNode(n => (int) n.Object == SelectedMapID) != null)
        {
            mapview.SetSelectedNode(this.Node);
        }
        mapview.Redraw();
        return true;
    }
}
