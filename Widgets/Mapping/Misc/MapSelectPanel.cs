using System;
using System.Collections.Generic;
using RPGStudioMK.Game;
using System.Diagnostics;

namespace RPGStudioMK.Widgets;

public class MapSelectPanel : Widget
{
    public Container allmapcontainer;
    public TreeView mapview;

    Stopwatch Stopwatch = new Stopwatch();

    public MapSelectPanel(IContainer Parent) : base(Parent)
    {
        Label Header = new Label(this);
        Header.SetText("All Maps");
        Header.SetFont(Fonts.UbuntuBold.Use(16));
        Header.SetPosition(5, 5);

        Sprites["sep"] = new Sprite(this.Viewport, new SolidBitmap(288, 1, new Color(28, 50, 73)));
        Sprites["sep"].Y = 30;

        Sprites["bar1"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height - 30, new Color(28, 50, 73)));
        Sprites["bar1"].Y = 30;

        Sprites["bar2"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width - 11, 1, new Color(28, 50, 73)));

        Sprites["block"] = new Sprite(this.Viewport, new SolidBitmap(11, 11, new Color(64, 104, 146)));

        allmapcontainer = new Container(this);
        allmapcontainer.SetPosition(0, 35);
        allmapcontainer.VAutoScroll = true;
        allmapcontainer.HAutoScroll = true;

        VScrollBar vs = new VScrollBar(this);
        allmapcontainer.SetVScrollBar(vs);

        HScrollBar hs = new HScrollBar(this);
        allmapcontainer.SetHScrollBar(hs);

        mapview = new TreeView(allmapcontainer);
        mapview.SetWidth(212);
        mapview.OnSelectedNodeChanged += delegate (MouseEventArgs e)
        {
            Stopwatch.Start();
            SetMap(Data.Maps[(int)mapview.SelectedNode.Object], true);
            Stopwatch.Stop();
            Editor.MainWindow.StatusBar.QueueMessage($"Loaded Map #{mapview.SelectedNode.Object} ({Stopwatch.ElapsedMilliseconds}ms)", false, 1000);
            Stopwatch.Reset();
        };
        mapview.OnDragAndDropped += delegate (BaseEventArgs e) { DragAndDropped(); };
        mapview.TrailingBlank = 32;
        allmapcontainer.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New Map")
            {
                OnLeftClick = NewMap
            },
            new MenuItem("Edit Map")
            {
                OnLeftClick = EditMap,
                IsClickable = delegate (BoolEventArgs e)
                {
                    e.Value = mapview.HoveringNode != null;
                }
            },
            new MenuSeparator(),
            new MenuItem("Cut Map")
            {
                OnLeftClick = CutMap,
                IsClickable = delegate (BoolEventArgs e) { e.Value = mapview.HoveringNode != null && mapview.Nodes.Count > 1; }
            },
            new MenuItem("Copy Map")
            {
                OnLeftClick = CopyMap,
                IsClickable = delegate (BoolEventArgs e) { e.Value = mapview.HoveringNode != null; }
            },
            new MenuItem("Paste Map")
            {
                OnLeftClick = PasteMap,
                IsClickable = delegate (BoolEventArgs e) { e.Value = Utilities.IsClipboardValidBinary(BinaryData.MAP); }
            },
            new MenuSeparator(),
            new MenuItem("Delete")
            {
                OnLeftClick = DeleteMap,
                Shortcut = "Del",
                IsClickable = delegate (BoolEventArgs e)
                {
                    e.Value = mapview.HoveringNode != null && mapview.Nodes.Count > 1;
                }
            }
        });
        OnWidgetSelected += WidgetSelected;
        SetBackgroundColor(10, 23, 37);
    }

    void SortNodeList(List<TreeNode> Nodes)
    {
        Nodes.Sort((TreeNode n1, TreeNode n2) =>
        {
            return Data.Maps[(int)n1.Object].Order.CompareTo(Data.Maps[(int)n2.Object].Order);
        });
    }

    public void DragAndDropped()
    {
        List<TreeNode> OldNodes = mapview.Nodes.ConvertAll(n => (TreeNode) n.Clone());
        TreeNode DraggingNode = mapview.DraggingNode;
        TreeNode HoveringNode = mapview.HoveringNode;
        bool Top = mapview.HoverTop;
        bool Over = mapview.HoverOver;
        bool Bottom = mapview.HoverBottom;
        // If the hovering node is a child of our dragging node,
        // we are trying to move the map inside a child map, which
        // obviously is not possible.
        if (DraggingNode.FindNode(n => n == HoveringNode) != null) return;
        // Remove the node
        if (mapview.Nodes.Contains(DraggingNode)) mapview.Nodes.Remove(DraggingNode);
        else
        {
            TreeNode Parent = GetParent(DraggingNode);
            if (Parent == null) throw new Exception("No parent node found.");
            Parent.Nodes.Remove(DraggingNode);
        }
        // Add the node
        if (Top)
        {
            // Root-level
            if (mapview.Nodes.Contains(HoveringNode))
            {
                int index = mapview.Nodes.IndexOf(HoveringNode);
                mapview.Nodes.Insert(index, DraggingNode);
            }
            else
            {
                TreeNode Parent = GetParent(HoveringNode);
                if (Parent == null) throw new Exception("No parent node found.");
                int index = Parent.Nodes.IndexOf(HoveringNode);
                Parent.Nodes.Insert(index, DraggingNode);
                if (Parent.Collapsed)
                {
                    Parent.Collapsed = false;
                    Data.Maps[(int)Parent.Object].Expanded = true;
                }
            }
        }
        else if (Over)
        {
            HoveringNode.Nodes.Add(DraggingNode);
            if (HoveringNode.Collapsed)
            {
                HoveringNode.Collapsed = false;
                Data.Maps[(int)HoveringNode.Object].Expanded = true;
            }
        }
        else if (Bottom)
        {
            // Has and showing children; add to children instead
            if (HoveringNode.Nodes.Count > 0 && !HoveringNode.Collapsed)
            {
                HoveringNode.Nodes.Insert(0, DraggingNode);
            }
            // Root-level node to add below
            else if (mapview.Nodes.Contains(HoveringNode))
            {
                // Add below node in parent list
                int index = mapview.Nodes.IndexOf(HoveringNode);
                mapview.Nodes.Insert(index + 1, DraggingNode);
            }
            else // Deeper node to add below
            {
                TreeNode Parent = GetParent(HoveringNode);
                if (Parent == null) throw new Exception("No parent node found.");
                int index = Parent.Nodes.IndexOf(HoveringNode);
                Parent.Nodes.Insert(index + 1, DraggingNode);
                if (Parent.Collapsed)
                {
                    Parent.Collapsed = false;
                    Data.Maps[(int)Parent.Object].Expanded = true;
                }
            }
        }
        else return;
        // Now update all map ParentID/Order fields to reflect the current Node structure
        Editor.UpdateOrder(mapview.Nodes);
        mapview.Redraw();
        Undo.MapOrderChangeUndoAction.Create(OldNodes, mapview.Nodes.ConvertAll(n => (TreeNode) n.Clone()));
    }

    public TreeNode GetParent(TreeNode NodeToFindParentOf)
    {
        if (mapview.Nodes.Contains(NodeToFindParentOf)) throw new Exception("Must handle root level node manually");
        foreach (TreeNode Node in mapview.Nodes)
        {
            TreeNode result = Node.FindParentNode(n => n == NodeToFindParentOf);
            if (result != null) return result;
        }
        return null;
    }

    public List<TreeNode> PopulateList(int PopulateChildrenOfID = 0)
    {
        List<TreeNode> nodes = new List<TreeNode>();
        foreach (Map map in Data.Maps.Values)
        {
            if (map.ParentID == PopulateChildrenOfID)
            {
                TreeNode node = new TreeNode() { Name = map.ToString(), Object = map.ID };
                List<TreeNode> Nodes = PopulateList(map.ID);
                node.Nodes = Nodes;
                node.Collapsed = !map.Expanded;
                nodes.Add(node);
            }
        }
        SortNodeList(nodes);
        if (PopulateChildrenOfID == 0) mapview.SetNodes(nodes);
        return nodes;
    }

    public void SetMap(Map Map, bool CalledFromTreeView = false)
    {
        if (Editor.MainWindow.MapWidget != null) Editor.MainWindow.MapWidget.SetMap(Map);
        Editor.ProjectSettings.LastMapID = Map.ID;
        Editor.ProjectSettings.LastLayer = 0;
        if (!CalledFromTreeView) // Has yet to update the selection
        {
            TreeNode node = null;
            for (int i = 0; i < mapview.Nodes.Count; i++)
            {
                if ((int)mapview.Nodes[i].Object == Map.ID)
                {
                    node = mapview.Nodes[i];
                    break;
                }
                else
                {
                    TreeNode n = mapview.Nodes[i].FindNode(n => (int)n.Object == Map.ID);
                    if (n != null)
                    {
                        node = n;
                        break;
                    }
                }
            }
            mapview.SetSelectedNode(node, false);
        }
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (Size.Width == 50 && Size.Height == 50) return;
        mapview.MinimumSize.Width = Size.Width - 11;
        allmapcontainer.SetSize(this.Size.Width - 11, this.Size.Height - allmapcontainer.Position.Y - 11);
        Sprites["bar1"].X = Size.Width - 11;
        (Sprites["bar1"].Bitmap as SolidBitmap).SetSize(1, Size.Height - 41);
        Sprites["bar2"].Y = Size.Height - 11;
        (Sprites["bar2"].Bitmap as SolidBitmap).SetSize(Size.Width - 11, 1);
        Sprites["block"].X = Size.Width - 11;
        Sprites["block"].Y = Size.Height - 11;
        allmapcontainer.VScrollBar.SetPosition(Size.Width - 9, 33);
        allmapcontainer.VScrollBar.SetSize(8, Size.Height - 46);
        allmapcontainer.HScrollBar.SetPosition(1, Size.Height - 9);
        allmapcontainer.HScrollBar.SetSize(Size.Width - 13, 8);
    }

    private void NewMap(BaseEventArgs e)
    {
        Map Map = new Map();
        Map.ID = Editor.GetFreeMapID();
        Map.Name = "Untitled Map";
        Map.SetSize(15, 15);
        MapPropertiesWindow mpw = new MapPropertiesWindow(Map);
        mpw.OnClosed += delegate (BaseEventArgs ev)
        {
            if (mpw.UpdateMapViewer)
            {
                Editor.UnsavedChanges = true;
                mpw.Map.Layers = new List<Layer>();
                for (int z = 0; z < 3; z++)
                {
                    Layer Layer = new Layer($"Layer {z + 1}", mpw.Map.Width, mpw.Map.Height);
                    mpw.Map.Layers.Add(Layer);
                }
                Dictionary<int, (int Order, int Parent)> OldOrderParentList = GetTreeState();
                Editor.AddMap(mpw.Map, mapview.HoveringNode == null ? 0 : (int) mapview.HoveringNode.Object);
                Dictionary<int, (int Order, int Parent)> NewOrderParentList = GetTreeState();
                Undo.MapChangeUndoAction.Create(new List<Map>() { mpw.Map }, OldOrderParentList, NewOrderParentList, true);
            }
        };
    }

    private void EditMap(BaseEventArgs e)
    {
        Map map = Data.Maps[(int)mapview.HoveringNode.Object];
        bool activemap = Editor.MainWindow.MapWidget.Map.ID == map.ID;
        MapPropertiesWindow mpw = new MapPropertiesWindow(map);
        mpw.OnClosed += delegate (BaseEventArgs ev)
        {
            if (mpw.UpdateMapViewer)
            {
                Data.Maps[map.ID] = mpw.Map;
                if (mapview.HoveringNode.Name != mpw.Map.Name)
                {
                    mapview.HoveringNode.Name = mpw.Map.Name;
                    mapview.Redraw();
                }
                Editor.UnsavedChanges = mpw.UnsavedChanges;
                if (Editor.MainWindow.MapWidget != null && activemap) Editor.MainWindow.MapWidget.SetMap(mpw.Map);
            }
        };
    }

    private void CutMap(BaseEventArgs e)
    {
        CopyMap(e);
        DeleteMapAndKeepChildren();
    }

    private void CopyMap(BaseEventArgs e)
    {
        Map map = Data.Maps[(int)mapview.HoveringNode.Object];
        Utilities.SetClipboard(map, BinaryData.MAP);
    }

    private void PasteMap(BaseEventArgs e)
    {
        if (!Utilities.IsClipboardValidBinary(BinaryData.MAP)) return;
        Map map = Utilities.GetClipboard<Map>();
        map.ID = Editor.GetFreeMapID();
        Dictionary<int, (int Order, int Parent)> OldOrderParentList = GetTreeState();
        Editor.AddMap(map, mapview.HoveringNode == null ? 0 : (int)mapview.HoveringNode.Object);
        Dictionary<int, (int Order, int Parent)> NewOrderParentList = GetTreeState();
        Undo.MapChangeUndoAction.Create(new List<Map>() { map }, OldOrderParentList, NewOrderParentList, true);
    }

    /// <summary>
    /// Deletes the hovered map and moves the children to the parent.
    /// </summary>
    void DeleteMapAndKeepChildren()
    {
        Dictionary<int, (int Order, int Parent)> OldOrderParentList = GetTreeState();
        int MapID = (int)mapview.HoveringNode.Object;
        Map Map = Data.Maps[MapID];
        Data.Maps.Remove(MapID);
        Editor.DecrementMapOrderFrom(Map.Order);
        Editor.DeletedMaps.Add(Map);
        for (int i = 0; i < mapview.Nodes.Count; i++)
        {
            if ((int)mapview.Nodes[i].Object == MapID)
            {
                if (mapview.HoveringNode.Nodes.Count > 0)
                {
                    mapview.HoveringNode.Nodes.ForEach(n => Data.Maps[(int)n.Object].ParentID = 0);
                    mapview.Nodes.AddRange(mapview.HoveringNode.Nodes);
                }
                mapview.Nodes.Remove(mapview.HoveringNode);
                SortNodeList(mapview.Nodes);
                mapview.SetSelectedNode(mapview.Nodes[i > 0 ? i - 1 : 0]);
                break;
            }
            else
            {
                TreeNode Node = mapview.Nodes[i].FindParentNode(n => n.Object == mapview.HoveringNode.Object);
                if (Node == null) continue;
                if (mapview.HoveringNode.Nodes.Count > 0)
                {
                    mapview.HoveringNode.Nodes.ForEach(n => Data.Maps[(int)n.Object].ParentID = 0);
                    Node.Nodes.AddRange(mapview.HoveringNode.Nodes);
                }
                Node.Nodes.Remove(mapview.HoveringNode);
                SortNodeList(Node.Nodes);
                mapview.SetSelectedNode(Node);
                break;
            }
        }
        mapview.Redraw();
        Dictionary<int, (int Order, int Parent)> NewOrderParentList = GetTreeState();
        Undo.MapChangeUndoAction.Create(new List<Map>() { Map }, OldOrderParentList, NewOrderParentList, false);
    }

    Dictionary<int, (int Order, int Parent)> GetTreeState()
    {
        Dictionary<int, (int Order, int Parent)> OrderParentList = new Dictionary<int, (int Order, int Parent)>();
        foreach (KeyValuePair<int, Map> kvp in Data.Maps)
        {
            OrderParentList.Add(kvp.Key, (kvp.Value.Order, kvp.Value.ParentID));
        }
        return OrderParentList;
    }

    /// <summary>
    /// Deletes the hovered map and all its children.
    /// </summary>
    void DeleteMapAndDeleteChildren()
    {
        Dictionary<int, (int Order, int Parent)> OldOrderParentList = GetTreeState();
        List<Map> Maps = DeleteMapRecursively(mapview.HoveringNode);
        for (int i = 0; i < mapview.Nodes.Count; i++)
        {
            if (mapview.Nodes[i] == mapview.HoveringNode)
            {
                mapview.Nodes.RemoveAt(i);
                mapview.SetSelectedNode(i >= mapview.Nodes.Count ? mapview.Nodes[i - 1] : mapview.Nodes[i]);
                break;
            }
            else if (mapview.Nodes[i].ContainsNode(mapview.HoveringNode))
            {
                mapview.SetSelectedNode(mapview.Nodes[i].RemoveNode(mapview.HoveringNode));
                break;
            }
        }
        Editor.OptimizeOrder();
        Dictionary<int, (int Order, int Parent)> NewOrderParentList = GetTreeState();
        Undo.MapChangeUndoAction.Create(Maps, OldOrderParentList, NewOrderParentList, false);
    }

    private void DeleteMap(BaseEventArgs e)
    {
        if (mapview.Nodes.Count <= 1) return;
        string message = "Are you sure you want to delete this map?";
        if (mapview.HoveringNode.Nodes.Count > 0) message += " All of its children will also be deleted.";
        DeleteMapPopup confirm = new DeleteMapPopup(mapview.HoveringNode.Nodes.Count > 0, "Warning", message, ButtonType.YesNoCancel, IconType.Warning);
        confirm.OnClosed += delegate (BaseEventArgs ev)
        {
            if (confirm.Result == 0) // Yes
            {
                bool DeleteChildMaps = mapview.HoveringNode.Nodes.Count > 0 ? confirm.DeleteChildMaps.Checked : false;
                Editor.UnsavedChanges = true;
                if (DeleteChildMaps) // Delete children
                {
                    DeleteMapAndDeleteChildren();
                }
                else // Keep and move children
                {
                    DeleteMapAndKeepChildren();
                }
                mapview.Redraw();
            }
        };
    }

    private List<Map> DeleteMapRecursively(TreeNode node)
    {
        List<Map> DeletedMaps = new List<Map>();
        for (int i = 0; i < node.Nodes.Count; i++)
        {
            DeletedMaps.AddRange(DeleteMapRecursively(node.Nodes[i]));
        }
        int MapID = (int)node.Object;
        Map Map = Data.Maps[MapID];
        Data.Maps.Remove(MapID);
        Editor.DeletedMaps.Add(Map);
        DeletedMaps.Add(Map);
        return DeletedMaps;
    }
}
