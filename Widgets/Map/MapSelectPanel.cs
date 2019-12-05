using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MapSelectPanel : Widget
    {
        public MapViewer MapViewer;
        public StatusBar StatusBar;

        public Container allmapcontainer;
        public TreeView mapview;

        public MapSelectPanel(object Parent, string Name = "mapSelectTab")
            : base(Parent, Name)
        {
            Sprites["bar"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height, new Color(28, 50, 73)));

            allmapcontainer = new Container(this);
            allmapcontainer.SetPosition(0, 1);
            allmapcontainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(this);
            allmapcontainer.SetVScrollBar(vs);

            mapview = new TreeView(allmapcontainer);
            mapview.SetWidth(212);
            mapview.OnSelectedNodeChanged += delegate (object sender, MouseEventArgs e)
            {
                SetMap(mapview.SelectedNode.Object as Map, true);
            };
            mapview.TrailingBlank = 64;
            allmapcontainer.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("New Map")
                {
                    OnLeftClick = NewMap
                },
                new MenuItem("Edit Map")
                {
                    OnLeftClick = EditMap,
                    IsClickable = delegate (object sender, ConditionEventArgs e)
                    {
                        e.ConditionValue = mapview.HoveringNode != null;
                    }
                },
                new MenuSeparator(),
                new MenuItem("Delete")
                {
                    OnLeftClick = DeleteMap,
                    Shortcut = "Del",
                    IsClickable = delegate (object sender, ConditionEventArgs e)
                    {
                        e.ConditionValue = mapview.HoveringNode != null && mapview.Nodes.Count > 1;
                    }
                }
            });
            OnWidgetSelected += WidgetSelected;
        }

        public List<TreeNode> PopulateList(List<object> Maps, bool first = false)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            for (int i = (first ? 0 : 2); i < Maps.Count; i++)
            {
                if (Maps[i] is int)
                {
                    nodes.Add(new TreeNode() { Object = Data.Maps[(int) Maps[i]] });
                    Data.Maps[(int) Maps[i]].Added = true;
                }
                else
                {
                    List<object> list = (List<object>) Maps[i];
                    TreeNode n = new TreeNode();
                    n.Object = Data.Maps[(int) list[0]];
                    n.Collapsed = (bool) list[1];
                    Data.Maps[(int) list[0]].Added = true;
                    n.Nodes = PopulateList(list);
                    nodes.Add(n);
                }
            }
            if (first)
            {
                foreach (KeyValuePair<int, Map> kvp in Data.Maps)
                {
                    if (!kvp.Value.Added)
                    {
                        nodes.Add(new TreeNode() { Object = kvp.Value });
                        kvp.Value.Added = true;
                        Editor.ProjectSettings.MapOrder.Add(kvp.Key);
                    }
                }
                mapview.SetNodes(nodes);
            }
            return nodes;
        }

        public void SetMap(Map Map, bool CalledFromTreeView = false)
        {
            MapViewer.SetMap(Map);
            Editor.ProjectSettings.LastMapID = Map.ID;
            Editor.ProjectSettings.LastLayer = 0;
            if (!CalledFromTreeView) // Has yet to update the selection
            {
                TreeNode node = null;
                for (int i = 0; i < mapview.Nodes.Count; i++)
                {
                    if ((mapview.Nodes[i].Object as Map).ID == Map.ID)
                    {
                        node = mapview.Nodes[i];
                        break;
                    }
                    else
                    {
                        TreeNode n = mapview.Nodes[i].FindNode(n => (n.Object as Map).ID == Map.ID);
                        if (n != null)
                        {
                            node = n;
                            break;
                        }
                    }
                }
                mapview.SetSelectedNode(node);
            }
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            allmapcontainer.SetSize(this.Size.Width - 11, this.Size.Height - 1);
            Sprites["bar"].X = Size.Width - 11;
            (Sprites["bar"].Bitmap as SolidBitmap).SetSize(1, Size.Height);
            allmapcontainer.VScrollBar.SetPosition(Size.Width - 9, 1);
            allmapcontainer.VScrollBar.SetSize(8, Size.Height - 2);
        }

        private void NewMap(object sender, MouseEventArgs e)
        {
            Editor.UnsavedChanges = true;
            Map Map = new Map();
            int i = 1;
            while (true)
            {
                if (!Data.Maps.ContainsKey(i))
                {
                    Map.ID = i;
                    break;
                }
                i++;
            }
            Map.DevName = "Untitled Map";
            Map.DisplayName = "Untitled Map";
            Map.SetSize(15, 15);
            MapPropertiesWindow mpw = new MapPropertiesWindow(Map, this.Window);
            mpw.OnClosed += delegate (object _, EventArgs ev)
            {
                if (mpw.UpdateMapViewer)
                {
                    Data.Maps.Add(Map.ID, Map);
                    TreeNode node = new TreeNode() { Object = Map };
                    if (mapview.HoveringNode != null)
                    {
                        AddIDToMap(Editor.ProjectSettings.MapOrder, (mapview.HoveringNode.Object as Map).ID, Map.ID);
                        mapview.HoveringNode.Nodes.Add(node);
                        mapview.HoveringNode.Collapsed = false;
                    }
                    else
                    {
                        Editor.ProjectSettings.MapOrder.Add(Map.ID);
                        mapview.Nodes.Add(node);
                    }
                    mapview.SetSelectedNode(node);
                }
            };
        }

        private bool AddIDToMap(List<object> collection, int ParentID, int ChildID)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                object o = collection[i];
                if (o is bool) continue;
                else if (o is int)
                {
                    if ((int) o == ParentID)
                    {
                        if (i == 0) // Already in this parent's node list
                            collection.Add(ChildID);
                        else // Create new node list
                            collection[i] = new List<object>() { ParentID, false, ChildID };
                        return true;
                    }
                }
                else
                {
                    if (AddIDToMap((List<object>)o, ParentID, ChildID)) break;
                }
            }
            return false;
        }

        public bool SetCollapsed(List<object> collection, int MapID, bool IsCollapsed)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                object o = collection[i];
                if (o is List<object>)
                {
                    List<object> sub = o as List<object>;
                    if ((int) sub[0] == MapID)
                    {
                        (collection[i] as List<object>)[1] = IsCollapsed;
                    }
                    else
                    {
                        SetCollapsed(collection[i] as List<object>, MapID, IsCollapsed);
                    }
                }
            }
            return false;
        }

        private void EditMap(object sender, MouseEventArgs e)
        {
            Map map = mapview.SelectedNode.Object as Map;
            MapPropertiesWindow mpw = new MapPropertiesWindow(map, this.Window);
            mpw.OnClosed += delegate (object _, EventArgs ev)
            {
                if (mpw.UnsavedChanges) Editor.UnsavedChanges = true;
                if (mpw.UpdateMapViewer)
                {
                    MapViewer.SetMap(map);
                }
            };
        }

        private void DeleteMap(object sender, MouseEventArgs e)
        {
            if (mapview.Nodes.Count <= 1) return;
            string message = "Are you sure you want to delete this map?";
            if (mapview.HoveringNode.Nodes.Count > 0) message += " All of its children will also be deleted.";
            MessageBox confirm = new MessageBox("Warning", message, ButtonTypes.YesNoCancel);
            confirm.OnClosed += delegate (object s, EventArgs ev)
            {
                if (confirm.Result == 0) // Yes
                {
                    Editor.UnsavedChanges = true;
                    DeleteMapRecursively(mapview.HoveringNode);
                    for (int i = 0; i < mapview.Nodes.Count; i++)
                    {
                        if (mapview.Nodes[i] == mapview.HoveringNode)
                        {
                            mapview.Nodes.RemoveAt(i);
                            mapview.SetSelectedNode(i >= mapview.Nodes.Count ? mapview.Nodes[i - 1] : mapview.Nodes[i]);
                        }
                        else if (mapview.Nodes[i].ContainsNode(mapview.HoveringNode))
                        {
                            mapview.SetSelectedNode(mapview.Nodes[i].RemoveNode(mapview.HoveringNode));
                        }
                    }
                    RemoveID(Editor.ProjectSettings.MapOrder, (mapview.HoveringNode.Object as Map).ID, true);
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
            Data.Maps.Remove((node.Object as Map).ID);
        }

        private void RemoveID(List<object> collection, int ID, bool first = false)
        {
            for (int i = (first ? 0 : 2); i < collection.Count; i++)
            {
                if (collection[i] is bool) continue;
                else if (collection[i] is int)
                {
                    if ((int) collection[i] == ID)
                    {
                        collection.RemoveAt(i);
                        break;
                    }
                }
                else
                {
                    List<object> sub = (List<object>) collection[i];
                    if (sub[0] is int && (int) sub[0] == ID)
                    {
                        collection.RemoveAt(i);
                    }
                    else if (sub.Count == 3 && sub[2] is int && (int) sub[2] == ID)
                    {
                        collection[i] = (int) sub[0];
                    }
                    else
                    {
                        RemoveID(sub, ID);
                    }
                }
            }
        }
    }
}
