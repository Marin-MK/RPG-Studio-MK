using System;
using System.Collections.Generic;
using RPGStudioMK.Game;
using odl;
using amethyst;
using System.Diagnostics;
using System.Linq;

namespace RPGStudioMK.Widgets
{
    public class MapSelectPanel : Widget
    {
        public Container allmapcontainer;
        public TreeView mapview;

        Stopwatch Stopwatch = new Stopwatch();

        public MapSelectPanel(IContainer Parent) : base(Parent)
        {
            Label Header = new Label(this);
            Header.SetText("All Maps");
            Header.SetFont(Font.Get("Fonts/Ubuntu-B", 16));
            Header.SetPosition(5, 5);

            Sprites["sep"] = new Sprite(this.Viewport, new SolidBitmap(288, 1, new Color(28, 50, 73)));
            Sprites["sep"].Y = 30;

            Sprites["bar"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height - 30, new Color(28, 50, 73)));
            Sprites["bar"].Y = 30;

            allmapcontainer = new Container(this);
            allmapcontainer.SetPosition(0, 35);
            allmapcontainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(this);
            allmapcontainer.SetVScrollBar(vs);

            mapview = new TreeView(allmapcontainer);
            mapview.SetWidth(212);
            mapview.OnSelectedNodeChanged += delegate (MouseEventArgs e)
            {
                Stopwatch.Start();
                SetMap(Data.Maps[(int) mapview.SelectedNode.Object], true);
                Stopwatch.Stop();
                Editor.MainWindow.StatusBar.QueueMessage($"Loaded Map #{mapview.SelectedNode.Object} ({Stopwatch.ElapsedMilliseconds}ms)", false, 1000);
                Stopwatch.Reset();
            };
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
                    OnLeftClick = CutMap
                },
                new MenuItem("Copy Map")
                {
                    OnLeftClick = CopyMap
                },
                new MenuItem("Paste Map")
                {
                    OnLeftClick = PasteMap
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
        }

        void SortNodeList(List<TreeNode> Nodes)
        {
            Nodes.Sort((TreeNode n1, TreeNode n2) =>
            {
                return Data.Maps[(int) n1.Object].Order.CompareTo(Data.Maps[(int) n2.Object].Order);
            });
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
                    if ((int) mapview.Nodes[i].Object == Map.ID)
                    {
                        node = mapview.Nodes[i];
                        break;
                    }
                    else
                    {
                        TreeNode n = mapview.Nodes[i].FindNode(n => (int) n.Object == Map.ID);
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
            allmapcontainer.SetSize(this.Size.Width - 11, this.Size.Height - allmapcontainer.Position.Y);
            Sprites["bar"].X = Size.Width - 11;
            (Sprites["bar"].Bitmap as SolidBitmap).SetSize(1, Size.Height - 30);
            allmapcontainer.VScrollBar.SetPosition(Size.Width - 9, 33);
            allmapcontainer.VScrollBar.SetSize(8, Size.Height - 35);
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
                    Editor.AddMap(mpw.Map, mapview.HoveringNode == null ? 0 : (int) mapview.HoveringNode.Object);
                }
            };
        }

        private void EditMap(BaseEventArgs e)
        {
            Map map = Data.Maps[(int) mapview.HoveringNode.Object];
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
                    if (Editor.MainWindow.MapWidget != null) Editor.MainWindow.MapWidget.SetMap(mpw.Map);
                }
            };
        }

        private void CutMap(BaseEventArgs e)
        {
            CopyMap(e);
            int MapID = (int) mapview.HoveringNode.Object;
            Map Map = Data.Maps[MapID];
            Data.Maps.Remove(MapID);
            Editor.DecrementMapOrderFrom(Map.Order);
            Editor.DeletedMaps.Add(Map);
            for (int i = 0; i < mapview.Nodes.Count; i++)
            {
                if ((int) mapview.Nodes[i].Object == MapID)
                {
                    if (mapview.HoveringNode.Nodes.Count > 0)
                    {
                        mapview.HoveringNode.Nodes.ForEach(n => Data.Maps[(int) n.Object].ParentID = 0);
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
                        mapview.HoveringNode.Nodes.ForEach(n => Data.Maps[(int) n.Object].ParentID = 0);
                        Node.Nodes.AddRange(mapview.HoveringNode.Nodes);
                    }
                    Node.Nodes.Remove(mapview.HoveringNode);
                    SortNodeList(Node.Nodes);
                    mapview.SetSelectedNode(Node);
                    break;
                }
            }
            mapview.Redraw();
        }

        private void CopyMap(BaseEventArgs e)
        {
            Map map = Data.Maps[(int) mapview.HoveringNode.Object];
            Utilities.SetClipboard(map, BinaryData.MAP);
        }

        private void PasteMap(BaseEventArgs e)
        {
            if (!Utilities.IsClipboardValidBinary(BinaryData.MAP)) return;
            Map map = Utilities.GetClipboard<Map>();
            map.ID = Editor.GetFreeMapID();
            Editor.AddMap(map, mapview.HoveringNode == null ? 0 : (int) mapview.HoveringNode.Object);
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
                    if (DeleteChildMaps)
                    {
                        DeleteMapRecursively(mapview.HoveringNode);
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
                    }
                    else
                    {
                        int MapID = (int) mapview.HoveringNode.Object;
                        Map Map = Data.Maps[MapID];
                        Data.Maps.Remove(MapID);
                        Editor.DecrementMapOrderFrom(Map.Order);
                        Editor.DeletedMaps.Add(Map);
                        for (int i = 0; i < mapview.Nodes.Count; i++)
                        {
                            if ((int) mapview.Nodes[i].Object == MapID)
                            {
                                if (mapview.HoveringNode.Nodes.Count > 0)
                                {
                                    mapview.HoveringNode.Nodes.ForEach(n => Data.Maps[(int) n.Object].ParentID = 0);
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
                                    mapview.HoveringNode.Nodes.ForEach(n => Data.Maps[(int) n.Object].ParentID = 0);
                                    Node.Nodes.AddRange(mapview.HoveringNode.Nodes);
                                }
                                Node.Nodes.Remove(mapview.HoveringNode);
                                SortNodeList(Node.Nodes);
                                mapview.SetSelectedNode(Node);
                                break;
                            }
                        }
                    }
                    mapview.Redraw();
                }
            };
        }

        private void DeleteMapRecursively(TreeNode node)
        {
            for (int i = 0; i < node.Nodes.Count; i++)
            {
                DeleteMapRecursively(node.Nodes[i]);
            }
            int MapID = (int) node.Object;
            Map Map = Data.Maps[MapID];
            Data.Maps.Remove(MapID);
            Editor.DeletedMaps.Add(Map);
        }
    }

}
