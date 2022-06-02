using RPGStudioMK.Game;
using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Undo;

public class NodeCollapseChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Map collapsed";
    public override string Description => $"Map {(Data.Maps.ContainsKey(MapID) ? Data.Maps[MapID].Name : Utilities.Digits(MapID, 3))}\nOld Collapsed: {OldCollapsed}\nNew Collapsed: {NewCollapsed}";

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
        Node.Collapsed = IsRedo ? NewCollapsed : OldCollapsed;
        TriggerLogical(IsRedo);
        if (Node.FindNode(n => (int) n.Object == SelectedMapID) != null)
        {
            mapview.SetSelectedNode(Node);
        }
        mapview.Redraw();
        Editor.MainWindow.MapWidget.SetHint($"{(IsRedo ? "Redid" : "Undid")} map collapsed state changes");
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Data.Maps[MapID].Expanded = IsRedo ? !NewCollapsed : !OldCollapsed;
    }
}
