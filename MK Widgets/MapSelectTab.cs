using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class MapSelectTab : Widget
    {
        public MapViewer MapViewer;
        public StatusBar StatusBar;

        TabView TabView;

        Container allmapcontainer;
        TreeView mapview;

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
            List<TreeNode> Nodes = new List<TreeNode>();
            foreach (KeyValuePair<int, Data.Map> kvp in Data.GameData.Maps)
            {
                Nodes.Add(new TreeNode() { Object = kvp.Value });
            }
            mapview.SetNodes(Nodes);
            mapview.OnSelectedNodeChanged += delegate (object sender, MouseEventArgs e)
            {
                SetMap(mapview.SelectedNode.Object as Data.Map);
            };
            mapview.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("New Map"),
                new MenuSeparator(),
                new MenuItem("Edit Map") { OnLeftClick = OpenMapProperties },
                new MenuItem("Cut"),
                new MenuItem("Copy") { Shortcut = "Ctrl+C" },
                new MenuItem("Paste") { Shortcut = "Ctrl+V", IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = false; } },
                new MenuSeparator(),
                new MenuItem("Delete") { Shortcut = "Del" }
            });
        }

        public void SetMap(Data.Map Map)
        {
            // Changes mapviewer, layerstab and tilesettab to match the new map
            MapViewer.SetMap(Map);
            StatusBar.SetMap(Map);
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

        private void OpenMapProperties(object sender, MouseEventArgs e)
        {
            MapPropertiesWindow mpw = new MapPropertiesWindow(this.Window);
        }
    }
}
