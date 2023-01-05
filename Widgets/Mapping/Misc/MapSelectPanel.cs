using System;
using System.Collections.Generic;
using RPGStudioMK.Game;
using System.Diagnostics;
using RPGStudioMK.Undo;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class MapSelectPanel : Widget
{
    public OptimizedTreeView MapTree;
    public Map? SelectedMap => MapTree.SelectedNode is OptimizedNode ? Data.Maps[(int) ((OptimizedNode) MapTree.SelectedNode).Object] : null;
    public Map? HoveredMap => MapTree.HoveringNode is OptimizedNode ? Data.Maps[(int) ((OptimizedNode) MapTree.HoveringNode).Object] : null;

    Stopwatch Stopwatch = new Stopwatch();

    public MapSelectPanel(IContainer Parent) : base(Parent)
    {
        Label Header = new Label(this);
        Header.SetText("All Maps");
        Header.SetFont(Fonts.Header);
        Header.SetPosition(5, 5);

        Sprites["sep"] = new Sprite(this.Viewport, new SolidBitmap(288, 1, new Color(28, 50, 73)));
        Sprites["sep"].Y = 30;

        Sprites["bar1"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height - 30, new Color(28, 50, 73)));
        Sprites["bar1"].Y = 30;

        Sprites["bar2"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width - 11, 1, new Color(28, 50, 73)));

        Sprites["block"] = new Sprite(this.Viewport, new SolidBitmap(11, 11, new Color(64, 104, 146)));

        MapTree = new OptimizedTreeView(this);
        MapTree.SetDocked(true);
        MapTree.SetPadding(0, 35, 0, 0);
        MapTree.SetHResizeToFill(false);
        MapTree.SetExtraXScrollArea(4);
        MapTree.SetExtraYScrollArea(32);
        MapTree.OnSelectionChanged += e =>
        {
            if (MapTree.SelectedNode == null) Editor.MainWindow.MapWidget.SetMap(null);
            else
            {
                Stopwatch.Start();
                Editor.MainWindow.MapWidget.SetMap(this.SelectedMap);
                Stopwatch.Stop();
                Editor.MainWindow.StatusBar.QueueMessage($"Loaded Map #{this.SelectedMap.ID} ({Stopwatch.ElapsedMilliseconds}ms)", false, 1000);
                Stopwatch.Reset();
            }
        };
        MapTree.OnDragAndDropped += e => throw new NotImplementedException();
        MapTree.OnNodeExpansionChanged += delegate (GenericObjectEventArgs<OptimizedNode> e)
        {
            OptimizedNode Node = e.Object;
            int mapid = (int) Node.Object;
            Data.Maps[mapid].Expanded = Node.Expanded;
            Undo.NodeCollapseChangeUndoAction.Create(mapid, Node.Expanded, !Node.Expanded);
        };
        MapTree.OnNodeGlobalIndexChanged += delegate (GenericObjectEventArgs<OptimizedNode> e)
        {
            Data.Maps[(int) e.Object.Object].Order = e.Object.GlobalIndex;
        };
        MapTree.SetContextMenuList(new List<IMenuItem>()
        {
            new MenuItem("New")
            {
                OnClicked = e => NewMap()
            },
            new MenuItem("Edit")
            {
                OnClicked = e => EditMap(),
                IsClickable = e => e.Value = HoveredMap != null
            },
            new MenuItem("Shift")
            {
                OnClicked = e => ShiftMap(),
                IsClickable = e => e.Value = HoveredMap != null
            },
            new MenuSeparator(),
            new MenuItem("Cut")
            {
                OnClicked = e => CutMap(false),
                IsClickable = e => e.Value = HoveredMap != null && !MapTree.Empty
            },
            new MenuItem("Cut with children")
            {
                OnClicked = e => CutMap(true),
                IsClickable = e => e.Value = HoveredMap != null && !MapTree.Empty
            },
            new MenuItem("Copy")
            {
                OnClicked = e => CopyMap(false),
                IsClickable = e => e.Value = HoveredMap != null
            },
            new MenuItem("Copy with children")
            {
                OnClicked = e => CopyMap(true),
                IsClickable = e => e.Value = HoveredMap != null
            },
            new MenuItem("Paste")
            {
                OnClicked = _ => PasteMap(),
                IsClickable = e => e.Value = Utilities.IsClipboardValidBinary(BinaryData.MAPS)
            },
            new MenuSeparator(),
            new MenuItem("Expand All")
            {
                IsClickable = e => e.Value = HoveredMap != null && MapTree.HoveringNode is OptimizedNode && ((OptimizedNode) MapTree.HoveringNode).HasChildren
            },
            new MenuItem("Collapse All")
            {
                IsClickable = e => e.Value = HoveredMap != null && MapTree.HoveringNode is OptimizedNode && ((OptimizedNode) MapTree.HoveringNode).HasChildren
            },
            new MenuSeparator(),
            new MenuItem("Delete")
            {
                OnClicked = e => DeleteMap(),
                Shortcut = "Del",
                IsClickable = e => e.Value = HoveredMap != null && !MapTree.Empty
            }
        });
        OnWidgetSelected += WidgetSelected;
        SetBackgroundColor(10, 23, 37);
    }

    void SortNodeList(List<OptimizedNode> Nodes)
    {
        Nodes.Sort((OptimizedNode n1, OptimizedNode n2) =>
        {
            return n1.GlobalIndex.CompareTo(n2.GlobalIndex);
        });
    }

    /*public void DragAndDropped()
    {
        List<TreeNode> OldNodes = MapTree.Nodes.ConvertAll(n => (TreeNode) n.Clone());
        TreeNode DraggingNode = MapTree.DraggingNode;
        TreeNode HoveringNode = MapTree.HoveringNode;
        bool Top = MapTree.HoverTop;
        bool Over = MapTree.HoverOver;
        bool Bottom = MapTree.HoverBottom;
        // If the hovering node is a child of our dragging node,
        // we are trying to move the map inside a child map, which
        // obviously is not possible.
        if (DraggingNode.FindNode(n => n == HoveringNode) != null) return;
        // Remove the node
        if (MapTree.Nodes.Contains(DraggingNode)) MapTree.Nodes.Remove(DraggingNode);
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
            if (MapTree.Nodes.Contains(HoveringNode))
            {
                int index = MapTree.Nodes.IndexOf(HoveringNode);
                MapTree.Nodes.Insert(index, DraggingNode);
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
                    TreeNode oldnode = null;
                    foreach (TreeNode node in OldNodes)
                    {
                        if (node.Object == Parent.Object)
                        {
                            oldnode = node;
                            break;
                        }
                        else
                        {
                            TreeNode n = node.FindNode(n => n.Object == Parent.Object);
                            if (n != null)
                            {
                                oldnode = n;
                                break;
                            }
                        }
                    }
                    if (oldnode == null) throw new Exception("Could not find old node.");
                    oldnode.Collapsed = false;
                    Data.Maps[(int) Parent.Object].Expanded = true;
                    Undo.NodeCollapseChangeUndoAction.Create((int) Parent.Object, true, false, (int) MapTree.SelectedNode.Object);
                }
            }
        }
        else if (Over)
        {
            HoveringNode.Nodes.Add(DraggingNode);
            if (HoveringNode.Collapsed)
            {
                HoveringNode.Collapsed = false;
                TreeNode oldnode = null;
                foreach (TreeNode node in OldNodes)
                {
                    if (node.Object == HoveringNode.Object)
                    {
                        oldnode = node;
                        break;
                    }
                    else
                    {
                        TreeNode n = node.FindNode(n => n.Object == HoveringNode.Object);
                        if (n != null)
                        {
                            oldnode = n;
                            break;
                        }
                    }
                }
                if (oldnode == null) throw new Exception("Could not find old node.");
                oldnode.Collapsed = false;
                Data.Maps[(int) HoveringNode.Object].Expanded = true;
                Undo.NodeCollapseChangeUndoAction.Create((int) HoveringNode.Object, true, false, (int) MapTree.SelectedNode.Object);
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
            else if (MapTree.Nodes.Contains(HoveringNode))
            {
                // Add below node in parent list
                int index = MapTree.Nodes.IndexOf(HoveringNode);
                MapTree.Nodes.Insert(index + 1, DraggingNode);
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
                    TreeNode oldnode = null;
                    foreach (TreeNode node in OldNodes)
                    {
                        if (node.Object == Parent.Object)
                        {
                            oldnode = node;
                            break;
                        }
                        else
                        {
                            TreeNode n = node.FindNode(n => n.Object == Parent.Object);
                            if (n != null)
                            {
                                oldnode = n;
                                break;
                            }
                        }
                    }
                    if (oldnode == null) throw new Exception("Could not find old node.");
                    oldnode.Collapsed = false;
                    Data.Maps[(int) Parent.Object].Expanded = true;
                    Undo.NodeCollapseChangeUndoAction.Create((int) Parent.Object, true, false, (int) MapTree.SelectedNode.Object);
                }
            }
        }
        else return;
        if (!OldNodes.Equals(MapTree.Nodes))
        {
            // Now update all map ParentID/Order fields to reflect the current Node structure
            Editor.UpdateOrder(MapTree.Nodes);
            MapTree.Redraw();
            Undo.MapOrderChangeUndoAction.Create(OldNodes, MapTree.Nodes.ConvertAll(n => (TreeNode)n.Clone()));
        }
    }*/

    public List<OptimizedNode> PopulateList(int PopulateChildrenOfID = 0)
    {
        List<OptimizedNode> nodes = new List<OptimizedNode>();
        foreach (Map map in Data.Maps.Values)
        {
            if (map.ParentID == PopulateChildrenOfID)
            {
                OptimizedNode Node = new OptimizedNode(map.ToString(), map.ID);
                List<OptimizedNode> Children = PopulateList(map.ID);
                Children.ForEach(n => Node.AddChild(n));
                Node.SetExpanded(map.Expanded);
                nodes.Add(Node);
            }
        }
        SortNodeList(nodes);
        if (PopulateChildrenOfID == 0) MapTree.SetNodes(nodes);
        return nodes;
    }

    public void SetMap(Map Map)
    {
        if (Map == null) MapTree.SetSelectedNode(null, false);
        // No need to update the selected node if we already have the desired map active
        if (SelectedMap != null && SelectedMap == Map) return;
        int MapID = Map?.ID ?? -1;
        OptimizedNode Node = MapTree.Root.GetNode(n => (int) n.Object == MapID, true, true);
        MapTree.SetSelectedNode(Node, false);
    }

    public override void SizeChanged(BaseEventArgs e)
    {
        base.SizeChanged(e);
        if (Size.Width == 50 && Size.Height == 50) return;
        Sprites["bar1"].X = Size.Width - 11;
        (Sprites["bar1"].Bitmap as SolidBitmap).SetSize(1, Size.Height - 41);
        Sprites["bar2"].Y = Size.Height - 11;
        (Sprites["bar2"].Bitmap as SolidBitmap).SetSize(Size.Width - 11, 1);
        Sprites["block"].X = Size.Width - 11;
        Sprites["block"].Y = Size.Height - 11;
    }

    public void NewMap()
    {
        Map Map = new Map("Untitled Map", Editor.GetFreeMapID());
        Map.TilesetIDs.Add(1);
        Map.AutotileIDs.Clear();
        for (int i = 0; i < 7; i++)
        {
            Map.AutotileIDs.Add(Data.Tilesets[Map.TilesetIDs[0]].ID * 7 + i);
        }
        Map.SetSize(15, 15);
        MapPropertiesWindow mpw = new MapPropertiesWindow(Map, false);
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
                InsertMap((OptimizedNode) MapTree.HoveringNode, mpw.Map);
            }
        };
    }

    private OptimizedNode InsertMap(OptimizedNode Parent, Map Map)
    {
        OptimizedNode NewNode = new OptimizedNode(Map.Name, Map.ID);
        MapTree.InsertNode(Parent, null, NewNode);
        Map.Order = NewNode.GlobalIndex;
        Map.ParentID = (int) Parent.Object;
        Data.Maps.Add(Map.ID, Map);
        NewNode.OnGlobalIndexChanged += _ => MapTree.OnNodeExpansionChanged.Invoke(new GenericObjectEventArgs<OptimizedNode>(NewNode));
        MapTree.SetSelectedNode(NewNode, false);
        // TODO: undo/redo
        return NewNode;
    }

    private void EditMap()
    {
        if (HoveredMap == null) return;
        Map Map = HoveredMap;
        bool activemap = Editor.MainWindow.MapWidget.Map.ID == Map.ID;
        MapPropertiesWindow mpw = new MapPropertiesWindow(Map);
        mpw.OnClosed += delegate (BaseEventArgs ev)
        {
            if (mpw.UpdateMapViewer)
            {
                Data.Maps[Map.ID] = mpw.Map;
                if (((OptimizedNode) MapTree.HoveringNode).Text != mpw.Map.Name)
                {
                    ((OptimizedNode) MapTree.HoveringNode).SetText(mpw.Map.Name);
                    MapTree.RedrawNodeText((OptimizedNode) MapTree.HoveringNode);
                }
                Editor.UnsavedChanges = mpw.UnsavedChanges;
                if (activemap) Editor.MainWindow.MapWidget.SetMap(mpw.Map); // Redraw the map if it's currently active
            }
        };
    }
    
    private void ShiftMap()
    {
        if (HoveredMap == null) return;
        Map Map = HoveredMap;
        bool activemap = Editor.MainWindow.MapWidget.Map.ID == Map.ID;
        if (!activemap) Editor.MainWindow.MapWidget.SetMap(Map);
        ShiftMapWindow win = new ShiftMapWindow(Map);
        win.OnClosed += _ =>
        {
            if (!win.Apply) return;
            // TODO: Change all ConvertAll uses to Select for consistency
            List<Layer> OldLayers = Map.Layers.ConvertAll(l => (Layer) l.Clone());
            Map.Shift(win.Direction, win.Value, win.ShiftEvents);
            Editor.MainWindow.MapWidget.SetMap(Map); // Redraw the map
            Size s = new Size(Map.Width, Map.Height);
            Undo.MapSizeChangeUndoAction.Create(Map.ID, OldLayers, s, Map.Layers.ConvertAll(l => (Layer) l.Clone()), s);
        };
    }
    
    private void CutMap(bool WithChildren)
    {
        Map map = HoveredMap;
        if (map == null) return;
        CopyMap(WithChildren);
        DeleteNode(MapTree.HoveringNode, WithChildren);
    }
    
    private void CopyMap(bool WithChildren)
    {
        if (MapTree.HoveringNode is not OptimizedNode) return;
        List<Map> Maps = new List<Map>() { HoveredMap };
        if (WithChildren) ((OptimizedNode) MapTree.HoveringNode).GetAllChildren(true).ForEach(n =>
        {
            if (n is not OptimizedNode) return;
            Map m = Data.Maps[(int) ((OptimizedNode) n).Object];
            Maps.Add(m);
        });
        Utilities.SetClipboard(Maps, BinaryData.MAPS);
    }
    
    private void PasteMap()
    {
        if (!Utilities.IsClipboardValidBinary(BinaryData.MAPS)) return;
        PrintIndices();
        List<Map> maps = Utilities.GetClipboard<List<Map>>();
        Dictionary<int, OptimizedNode> MapHash = new Dictionary<int, OptimizedNode>();
        // The list is maps is ordered from root to child, meaning if we encounter a node with a parent, that parent must already have been encountered or the data is invalid.
        for (int i = 0; i < maps.Count; i++)
        {
            OptimizedNode ParentNode = null;
            if (!MapHash.ContainsKey(maps[i].ParentID)) ParentNode = (OptimizedNode) MapTree.HoveringNode;
            else ParentNode = MapHash[maps[i].ParentID];
            int id = maps[i].ID;
            maps[i].ID = Editor.GetFreeMapID();
            OptimizedNode NewNode = InsertMap(ParentNode, maps[i]);
            MapHash.Add(id, NewNode);
        }
        PrintIndices();
    }

    void PrintIndices()
    {
        Console.WriteLine(">>>>> START");
        MapTree.Root.GetAllChildren(true).ForEach(c =>
        {
            if (c is not OptimizedNode) return;
            OptimizedNode Node = (OptimizedNode)c;
            string depth = "";
            for (int i = 0; i < Node.Depth; i++) depth += " ";
            depth += "- ";
            Console.WriteLine($"{depth}{Node.GlobalIndex}: {Node.Text} ({Node.Parent.GlobalIndex})");
        });
    }
    
    private void DeleteNode(IOptimizedNode Node, bool DeleteChildren)
    {
        List<IOptimizedNode> Children = new List<IOptimizedNode>();
        if (Node is OptimizedNode) Children = new List<IOptimizedNode>(((OptimizedNode) Node).Children);
        List<IOptimizedNode> DeletedNodes = MapTree.DeleteNode(Node, DeleteChildren);
        DeletedNodes.ForEach(n =>
        {
            if (n is not OptimizedNode) return;
            OptimizedNode Node = (OptimizedNode) n;
            Map DeletedMap = Data.Maps[(int) Node.Object];
            Data.Maps.Remove(DeletedMap.ID);
        });
        // If we're not deleting our children we're flattening them.
        // The node parent property updates automatically, but we have to manually update
        // the ParentID property of the map that's associated with the node.
        if (!DeleteChildren) Children.ForEach(c =>
        {
            if (c is not OptimizedNode) return;
            OptimizedNode n = (OptimizedNode) c;
            Data.Maps[(int) n.Object].ParentID = c.Parent.GlobalIndex;
        });
        // TODO: undo/redo
    }

    private void DeleteMap()
    {
        if (MapTree.Empty || HoveredMap == null) return;
        bool AskDeleteChildren = MapTree.HoveringNode is OptimizedNode && ((OptimizedNode) MapTree.HoveringNode).HasChildren;
        DeleteMapPopup confirm = new DeleteMapPopup(AskDeleteChildren, "Warning", "Are you sure you want to delete this map?", ButtonType.YesNoCancel, IconType.Warning);
        confirm.OnClosed += _ =>
        {
            if (confirm.Result == 0) // Yes
            {
                Editor.UnsavedChanges = true;
                Console.WriteLine(MapTree.Root);
                DeleteNode(MapTree.HoveringNode, confirm.DeleteChildren);
            }
        };
    }
}
