using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class MapOrderChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Map order change";
    public override string Description => "Describes a change in the order of the map list, e.g. a map changed its parent-child relation.";

    public List<TreeNode> OldNodes;
    public List<TreeNode> NewNodes;

    public MapOrderChangeUndoAction(List<TreeNode> OldNodes, List<TreeNode> NewNodes)
    {
        this.OldNodes = OldNodes;
        this.NewNodes = NewNodes;
    }

    public static void Create(List<TreeNode> OldNodes, List<TreeNode> NewNodes)
    {
        var c = new MapOrderChangeUndoAction(OldNodes, NewNodes);
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
        TriggerLogical(IsRedo);
        TreeNode SelectedNode = null;
        for (int i = 0; i < mapview.Nodes.Count; i++)
        {
            if ((int) mapview.Nodes[i].Object == SelectedMapID)
            {
                SelectedNode = mapview.Nodes[i];
                break;
            }
            else
            {
                TreeNode node = mapview.Nodes[i].FindNode(n => (int) n.Object == SelectedMapID);
                if (node != null)
                {
                    SelectedNode = node;
                    break;
                }
            }
        }
        if (SelectedNode == null) throw new Exception("Could not find selected node.");
        Editor.MainWindow.MapWidget.SetHint($"{(IsRedo ? "Redid" : "Undid")} map list order changes");
        mapview.SetSelectedNode(SelectedNode);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Editor.UpdateOrder(IsRedo ? NewNodes : OldNodes);
    }
}
