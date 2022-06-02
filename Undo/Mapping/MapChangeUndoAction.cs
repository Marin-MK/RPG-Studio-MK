using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Game;
using RPGStudioMK.Widgets;

namespace RPGStudioMK.Undo;

public class MapChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Map {(Creation ? "creation" : "deletion")}";
    public override string Description
    {
        get
        {
            StringBuilder sb = new StringBuilder();
            Maps.ForEach(m =>
            {
                sb.AppendLine((Creation ? "Created" : "Deleted") + $" map {Utilities.Digits(m.ID, 3)}: {m.Name}, events: {m.Events.Count}, size: ({m.Width}x{m.Height})");
            });
            return sb.ToString();
        }
    }

    public List<Map> Maps;
    public Dictionary<int, (int Order, int Parent)> OldOrderParentList;
    public Dictionary<int, (int Order, int Parent)> NewOrderParentList;
    public bool Creation;

    public MapChangeUndoAction(List<Map> Maps, Dictionary<int, (int, int)> OldOrderParentList, Dictionary<int, (int, int)> NewOrderParentList, bool Creation)
    {
        this.Maps = Maps;
        this.OldOrderParentList = OldOrderParentList;
        this.NewOrderParentList = NewOrderParentList;
        this.Creation = Creation;
    }

    public static void Create(List<Map> Maps, Dictionary<int, (int, int)> OldOrderParentList, Dictionary<int, (int, int)> NewOrderParentList, bool Creation)
    {
        var c = new MapChangeUndoAction(Maps, OldOrderParentList, NewOrderParentList, Creation);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        if (!InMode(EditorMode.Mapping))
        {
            SetMode(EditorMode.Mapping);
            return false;
        }
        Dictionary<int, (int Order, int Parent)> OrderParentList = null;
        TreeView mapview = Editor.MainWindow.MapWidget.MapSelectPanel.mapview;
        int SelectedMapID = (int) mapview.SelectedNode.Object;
        int MinOrder = Maps.Min(m => m.Order);
        if (Creation)
        {
            if (IsRedo)
            {
                // Redo Creation
                Maps.ForEach(m => Data.Maps[m.ID] = m);
                OrderParentList = NewOrderParentList;
                Editor.MainWindow.MapWidget.SetHint($"Re-created map{(Maps.Count > 1 ? "s" : $" {Utilities.Digits(Maps[0].ID, 3)}")}");
            }
            else
            {
                // Undo Creation
                Maps.ForEach(m => Data.Maps.Remove(m.ID));
                OrderParentList = OldOrderParentList;
                Editor.MainWindow.MapWidget.SetHint($"Deleted map{(Maps.Count > 1 ? "s" : $" {Utilities.Digits(Maps[0].ID, 3)}")}");
            }
        }
        else
        {
            if (IsRedo)
            {
                // Redo deletion
                Maps.ForEach(m => Data.Maps.Remove(m.ID));
                OrderParentList = NewOrderParentList;
                Editor.MainWindow.MapWidget.SetHint($"Deleted map{(Maps.Count > 1 ? "s" : $" {Utilities.Digits(Maps[0].ID, 3)}")}");
            }
            else
            {
                // Undo deletion
                Maps.ForEach(m => Data.Maps[m.ID] = m);
                OrderParentList = OldOrderParentList;
                Editor.MainWindow.MapWidget.SetHint($"Re-created map{(Maps.Count > 1 ? "s" : $" {Utilities.Digits(Maps[0].ID, 3)}")}");
            }
        }
        foreach (KeyValuePair<int, (int Order, int Parent)> kvp in OrderParentList)
        {
            Data.Maps[kvp.Key].Order = kvp.Value.Order;
            Data.Maps[kvp.Key].ParentID = kvp.Value.Parent;
        }
        Editor.MainWindow.MapWidget.MapSelectPanel.PopulateList();
        TreeNode SelectedNode = null;
        // Attempt to find the previously selected node in the new node list to select it.
        foreach (TreeNode n in mapview.Nodes)
        {
            if ((int) n.Object == SelectedMapID)
            {
                SelectedNode = n;
                break;
            }
            else
            {
                TreeNode node = n.FindVisibleNode(n => (int) n.Object == SelectedMapID);
                if (node != null)
                {
                    SelectedNode = node;
                    break;
                }
            }
        }
        if (SelectedNode == null)
        {
            // SelectedNode still null, so our selected map has been removed.
            // Thus we select the map above our deleted map in the node list.
            foreach (TreeNode n in mapview.Nodes)
            {
                if (Data.Maps[(int) n.Object].Order == MinOrder)
                {
                    SelectedNode = n;
                    break;
                }
                else
                {
                    TreeNode node = n.FindVisibleNode(n => Data.Maps[(int) n.Object].Order == MinOrder);
                    if (node != null)
                    {
                        SelectedNode = node;
                        break;
                    }
                }
            }
        }
        if (SelectedNode == null) SelectedNode = mapview.Nodes[0];
        mapview.SetSelectedNode(SelectedNode);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        Dictionary<int, (int Order, int Parent)> OrderParentList = null;
        TreeView mapview = Editor.MainWindow.MapWidget.MapSelectPanel.mapview;
        int SelectedMapID = (int) mapview.SelectedNode.Object;
        int MinOrder = Maps.Min(m => m.Order);
        if (Creation)
        {
            if (IsRedo)
            {
                // Redo Creation
                Maps.ForEach(m => Data.Maps[m.ID] = m);
                OrderParentList = NewOrderParentList;
                Editor.MainWindow.MapWidget.SetHint($"Re-created map{(Maps.Count > 1 ? "s" : $" {Utilities.Digits(Maps[0].ID, 3)}")}");
            }
            else
            {
                // Undo Creation
                Maps.ForEach(m => Data.Maps.Remove(m.ID));
                OrderParentList = OldOrderParentList;
                Editor.MainWindow.MapWidget.SetHint($"Deleted map{(Maps.Count > 1 ? "s" : $" {Utilities.Digits(Maps[0].ID, 3)}")}");
            }
        }
        else
        {
            if (IsRedo)
            {
                // Redo deletion
                Maps.ForEach(m => Data.Maps.Remove(m.ID));
                OrderParentList = NewOrderParentList;
                Editor.MainWindow.MapWidget.SetHint($"Deleted map{(Maps.Count > 1 ? "s" : $" {Utilities.Digits(Maps[0].ID, 3)}")}");
            }
            else
            {
                // Undo deletion
                Maps.ForEach(m => Data.Maps[m.ID] = m);
                OrderParentList = OldOrderParentList;
                Editor.MainWindow.MapWidget.SetHint($"Re-created map{(Maps.Count > 1 ? "s" : $" {Utilities.Digits(Maps[0].ID, 3)}")}");
            }
        }
        foreach (KeyValuePair<int, (int Order, int Parent)> kvp in OrderParentList)
        {
            Data.Maps[kvp.Key].Order = kvp.Value.Order;
            Data.Maps[kvp.Key].ParentID = kvp.Value.Parent;
        }
    }
}
