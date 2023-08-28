﻿using RPGStudioMK.Game;
using RPGStudioMK.Widgets;

namespace RPGStudioMK.Undo;

public class MapOrderChangeUndoAction : BaseUndoAction
{
    public override string Title => $"Map order change";
    public override string Description => "Describes a change in the order of the map list, e.g. a map changed its parent-child relation.";

    public TreeNode OldRoot;
    public TreeNode NewRoot;

    public MapOrderChangeUndoAction(TreeNode OldRoot, TreeNode NewRoot)
    {
        this.OldRoot = OldRoot;
        this.NewRoot = NewRoot;
    }

    public static void Create(TreeNode OldRoot, TreeNode NewRoot)
    {
        var c = new MapOrderChangeUndoAction(OldRoot, NewRoot);
        c.Register();
    }

    public override bool Trigger(bool IsRedo)
    {
        if (!InMode(EditorMode.Mapping))
        {
            SetMode(EditorMode.Mapping);
            return false;
        }
        // TODO
        TreeView mapview = Editor.MainWindow.MapWidget.MapSelectPanel.MapTree;
        TriggerLogical(IsRedo);
        /*TreeNode SelectedNode = null;
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
        if (SelectedNode == null) throw new Exception("Could not find selected node.");*/
        Editor.MainWindow.MapWidget.SetHint($"{(IsRedo ? "Redid" : "Undid")} map list order changes");
        int mapID = (int) ((TreeNode) mapview.SelectedNode).Object;
        TreeNode root = IsRedo ? NewRoot : OldRoot;
        TreeNode? newSelectedNode = root.GetNode(n => n.Object is int && (int) n.Object == mapID);
        mapview.SetRootNode(root, newSelectedNode);
        return true;
    }

    public override void TriggerLogical(bool IsRedo)
    {
        TreeNode root = IsRedo ? NewRoot : OldRoot;
        root.GetAllChildren(true).ForEach(c =>
        {
            if (c is not TreeNode) return;
            TreeNode n = (TreeNode) c;
            int mapID = (int) n.Object;
            Data.Maps[mapID].Order = n.GlobalIndex;
            Data.Maps[mapID].ParentID = n.Parent == n.Root ? 0 : (int) n.Parent.Object;
        });
    }
}