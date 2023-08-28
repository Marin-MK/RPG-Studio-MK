using System;
using System.Collections.Generic;
using RPGStudioMK.Game;
using System.Diagnostics;
using System.Linq;

namespace RPGStudioMK.Widgets;

public class MapSelectPanel : Widget
{
    public TreeView MapTree;
    public Map? SelectedMap => MapTree.SelectedNode is TreeNode ? Data.Maps[(int) ((TreeNode) MapTree.SelectedNode).Object] : null;
    public Map? HoveredMap => MapTree.HoveringNode is TreeNode ? Data.Maps[(int) ((TreeNode) MapTree.HoveringNode).Object] : null;

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

        MapTree = new TreeView(this);
        MapTree.SetDocked(true);
        MapTree.SetPadding(0, 35, 0, 0);
        MapTree.SetHResizeToFill(false);
        MapTree.SetExtraXScrollArea(24);
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
        MapTree.OnDragAndDropped += e => DragAndDropped(e.Object);
        MapTree.OnNodeExpansionChanged += delegate (GenericObjectEventArgs<TreeNode> e)
        {
            TreeNode Node = e.Object;
            int mapid = (int) Node.Object;
            Data.Maps[mapid].Expanded = Node.Expanded;
            // Works, but probably not desired
            //Undo.NodeCollapseChangeUndoAction.Create(mapid, Node.Expanded, !Node.Expanded);
        };
        MapTree.OnNodeGlobalIndexChanged += delegate (GenericObjectEventArgs<TreeNode> e)
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
                IsClickable = e => e.Value = HoveredMap != null && MapTree.HoveringNode is TreeNode && ((TreeNode) MapTree.HoveringNode).HasChildren
            },
            new MenuItem("Collapse All")
            {
                IsClickable = e => e.Value = HoveredMap != null && MapTree.HoveringNode is TreeNode && ((TreeNode) MapTree.HoveringNode).HasChildren
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

    public void DragAndDropped((ITreeNode DroppedNode, TreeNode OldRoot, TreeNode NewRoot) Data)
    {
        TreeNode DroppedNode = (TreeNode) Data.DroppedNode;
        Game.Data.Maps[(int) DroppedNode.Object].ParentID = DroppedNode.Parent == DroppedNode.Root ? 0 : Game.Data.Maps[(int) DroppedNode.Parent.Object].ID;
        // Works, but probably not desired
        //Undo.MapOrderChangeUndoAction.Create(Data.OldRoot, (OptimizedNode) Data.NewRoot.Clone());
    }

    public TreeNode PopulateList()
    {
        TreeNode root = new TreeNode("ROOT", 0);
        List<Map> sortedMaps = Data.Maps.Values.ToList();
        sortedMaps.Sort((a, b) => a.Order.CompareTo(b.Order));
        PopulateList(sortedMaps, 0).ForEach(c => root.AddChild(c));
        MapTree.SetRootNode(root);
        return root;
    }

    private List<TreeNode> PopulateList(List<Map> unvisitedMaps, int PopulateChildrenOfID)
    {
        List<TreeNode> nodes = new List<TreeNode>();
        for (int i = 0; i < unvisitedMaps.Count; i++)
        {
            Map map = unvisitedMaps[i];
            if (map.ParentID == PopulateChildrenOfID)
            {
                TreeNode Node = new TreeNode(map.ToString(), map.ID);
                unvisitedMaps.RemoveAt(i);
                i--;
                List<TreeNode> Children = PopulateList(unvisitedMaps, map.ID);
                Children.ForEach(n => Node.AddChild(n));
                Node.SetExpanded(map.Expanded);
                nodes.Add(Node);
            }
        }
        return nodes;
    }

    public void SetMap(Map Map)
    {
        if (Map == null) MapTree.SetSelectedNode(null, false);
        // No need to update the selected node if we already have the desired map active
        if (SelectedMap != null && SelectedMap == Map) return;
        int MapID = Map?.ID ?? -1;
        TreeNode Node = MapTree.Root.GetNode(n => (int) n.Object == MapID, true, true);
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
                InsertMap((TreeNode) MapTree.HoveringNode, mpw.Map);
            }
        };
    }

    private TreeNode InsertMap(TreeNode Parent, Map Map)
    {
        if (Parent is null) Parent = MapTree.Root;
        TreeNode NewNode = new TreeNode(Map.Name, Map.ID);
        MapTree.InsertNode(Parent, null, NewNode);
        Map.Order = NewNode.GlobalIndex;
        Map.ParentID = (int) Parent.Object;
        Data.Maps.Add(Map.ID, Map);
        NewNode.OnGlobalIndexChanged += _ => MapTree.OnNodeExpansionChanged.Invoke(new GenericObjectEventArgs<TreeNode>(NewNode));
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
                if (((TreeNode) MapTree.HoveringNode).Text != mpw.Map.Name)
                {
                    ((TreeNode) MapTree.HoveringNode).SetText(mpw.Map.Name);
                    MapTree.RedrawNodeText((TreeNode) MapTree.HoveringNode);
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
        if (MapTree.HoveringNode is not TreeNode) return;
        List<Map> Maps = new List<Map>() { HoveredMap };
        if (WithChildren) ((TreeNode) MapTree.HoveringNode).GetAllChildren(true).ForEach(n =>
        {
            if (n is not TreeNode) return;
            Map m = Data.Maps[(int) ((TreeNode) n).Object];
            Maps.Add(m);
        });
        Utilities.SetClipboard(Maps, BinaryData.MAPS);
    }
    
    private void PasteMap()
    {
        if (!Utilities.IsClipboardValidBinary(BinaryData.MAPS)) return;
        PrintIndices();
        List<Map> maps = Utilities.GetClipboard<List<Map>>();
        Dictionary<int, TreeNode> MapHash = new Dictionary<int, TreeNode>();
        // The list is maps is ordered from root to child, meaning if we encounter a node with a parent, that parent must already have been encountered or the data is invalid.
        for (int i = 0; i < maps.Count; i++)
        {
            TreeNode ParentNode = null;
            if (!MapHash.ContainsKey(maps[i].ParentID)) ParentNode = (TreeNode) MapTree.HoveringNode;
            else ParentNode = MapHash[maps[i].ParentID];
            int id = maps[i].ID;
            maps[i].ID = Editor.GetFreeMapID();
            TreeNode NewNode = InsertMap(ParentNode, maps[i]);
            MapHash.Add(id, NewNode);
        }
        PrintIndices();
    }

    void PrintIndices()
    {
        Logger.WriteLine(">>>>> START");
        MapTree.Root.GetAllChildren(true).ForEach(c =>
        {
            if (c is not TreeNode) return;
            TreeNode Node = (TreeNode)c;
            string depth = "";
            for (int i = 0; i < Node.Depth; i++) depth += " ";
            depth += "- ";
            Logger.WriteLine($"{depth}{Node.GlobalIndex}: {Node.Text} ({Node.Parent.GlobalIndex})");
        });
    }
    
    private void DeleteNode(ITreeNode Node, bool DeleteChildren)
    {
        List<ITreeNode> Children = new List<ITreeNode>();
        if (Node is TreeNode) Children = new List<ITreeNode>(((TreeNode) Node).Children);
        List<ITreeNode> DeletedNodes = MapTree.DeleteNode(Node, DeleteChildren);
        DeletedNodes.ForEach(n =>
        {
            if (n is not TreeNode) return;
            TreeNode Node = (TreeNode) n;
            Map DeletedMap = Data.Maps[(int) Node.Object];
            Data.Maps.Remove(DeletedMap.ID);
        });
        // If we're not deleting our children we're flattening them.
        // The node parent property updates automatically, but we have to manually update
        // the ParentID property of the map that's associated with the node.
        if (!DeleteChildren) Children.ForEach(c =>
        {
            if (c is not TreeNode) return;
            TreeNode n = (TreeNode) c;
            Data.Maps[(int) n.Object].ParentID = c.Parent.GlobalIndex;
        });
        // TODO: undo/redo
    }

    private void DeleteMap()
    {
        if (MapTree.Empty || HoveredMap == null) return;
        bool AskDeleteChildren = MapTree.HoveringNode is TreeNode && ((TreeNode) MapTree.HoveringNode).HasChildren;
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
