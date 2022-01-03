using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK;

public class MapOrderChangeUndoAction : BaseUndoAction
{
    public List<TreeNode> OldNodes;
    public List<TreeNode> NewNodes;

    public MapOrderChangeUndoAction(List<TreeNode> OldNodes, List<TreeNode> NewNodes)
    {
        this.OldNodes = OldNodes;
        this.NewNodes = NewNodes;
    }

    public static void Create(List<TreeNode> OldNodes, List<TreeNode> NewNodes)
    {
        new MapOrderChangeUndoAction(OldNodes, NewNodes);
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
            mapview.SetNodes(NewNodes);
        }
        else
        {
            mapview.SetNodes(OldNodes);
        }
        Editor.UpdateOrder(mapview.Nodes);
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
        mapview.SetSelectedNode(SelectedNode);
        return true;
    }
}
