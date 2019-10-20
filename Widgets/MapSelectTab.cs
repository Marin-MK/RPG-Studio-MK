using System;
using System.Collections.Generic;
using MKEditor.Game;
using ODL;

namespace MKEditor.Widgets
{
    public class MapSelectTab : Widget
    {
        public MapViewer MapViewer;
        public StatusBar StatusBar;

        TabView TabView;

        Container allmapcontainer;
        public TreeView mapview;

        public MapSelectTab(object Parent, string Name = "mapSelectTab")
            : base(Parent, Name)
        {
            SetBackgroundColor(10, 23, 37);

            TabView = new TabView(this);
            TabView.CreateTab("Maps");

            Sprites["header"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width, 4, new Color(28, 50, 73)));
            Sprites["header"].Y = 25;
            Sprites["bar2"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height, new Color(28, 50, 73)));
            Sprites["bar2"].Y = 29;

            allmapcontainer = new Container(TabView.GetTab(0));
            allmapcontainer.SetPosition(0, 4);
            allmapcontainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(this);
            allmapcontainer.SetVScrollBar(vs);

            mapview = new TreeView(allmapcontainer);
            mapview.SetWidth(212);
            mapview.OnSelectedNodeChanged += delegate (object sender, MouseEventArgs e)
            {
                SetMap(mapview.SelectedNode.Object as Game.Map);
            };
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
                    Shortcut = "Del",
                    IsClickable = delegate (object sender, ConditionEventArgs e)
                    {
                        e.ConditionValue = mapview.HoveringNode != null;
                    }
                }
            });
            OnWidgetSelected += WidgetSelected;
        }

        public List<TreeNode> PopulateList(List<object> Maps, bool first = false)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            for (int i = (first ? 0 : 1); i < Maps.Count; i++)
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

        public void SetMap(Map Map)
        {
            MapViewer.SetMap(Map);
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TabView.SetSize(this.Size);
            allmapcontainer.SetSize(this.Size.Width - 11, this.Size.Height - 30);
            (Sprites["header"].Bitmap as SolidBitmap).SetSize(Size.Width, 4);
            Sprites["bar2"].X = Size.Width - 11;
            (Sprites["bar2"].Bitmap as SolidBitmap).SetSize(1, Size.Height - 29);
            allmapcontainer.VScrollBar.SetPosition(Size.Width - 9, 30);
            allmapcontainer.VScrollBar.SetSize(8, Size.Height - 31);
        }

        private void NewMap(object sender, MouseEventArgs e)
        {
            Map Map = new Map();
            Map.DevName = "Untitled Map";
            Map.DisplayName = "Untitled Map";
            Map.SetSize(15, 15);
            MapPropertiesWindow mpw = new MapPropertiesWindow(Map, this.Window);
            mpw.OnClosed += delegate (object _, EventArgs ev)
            {
                if (mpw.UpdateMapViewer)
                {
                    if (mapview.HoveringNode != null)
                    {

                    }
                }
            };
        }

        private void EditMap(object sender, MouseEventArgs e)
        {
            Map map = mapview.SelectedNode.Object as Map;
            MapPropertiesWindow mpw = new MapPropertiesWindow(map, this.Window);
            mpw.OnClosed += delegate (object _, EventArgs ev)
            {
                if (mpw.UpdateMapViewer)
                {
                    MapViewer.SetMap(map);
                }
            };
        }
    }
}
