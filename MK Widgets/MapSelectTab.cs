using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class MapSelectTab : Widget
    {
        public MapViewer MapViewer;

        TabView TabView;

        Container allmapcontainer;
        TreeView mapview;

        public MapSelectTab(object Parent, string Name = "mapSelectTab")
            : base(Parent, Name)
        {
            SetBackgroundColor(10, 23, 37);

            TabView = new TabView(this);
            TabView.CreateTab("Maps");

            Sprites["header"] = new Sprite(this.Viewport, new SolidBitmap(Size.Width - 12, 4, new Color(28, 50, 73)));
            Sprites["header"].Y = 25;
            Sprites["bar1"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height, new Color(28, 50, 73)));
            Sprites["bar1"].X = Size.Width - 1;
            Sprites["bar2"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height, new Color(28, 50, 73)));
            Sprites["bar2"].X = Size.Width - 12;

            allmapcontainer = new Container(TabView.GetTab(0));
            allmapcontainer.SetPosition(0, 3);
            allmapcontainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(this);
            vs.SetPosition(Size.Width - 10, 1);
            vs.SetSize(8, Size.Height - 2);
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
                // Changes mapviewer, layerstab and tilesettab to match the new map
                MapViewer.SetMap(mapview.SelectedNode.Object as Data.Map);
            };
            mapview.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("New Map"),
                new MenuSeparator(),
                new MenuItem("Edit Map"),
                new MenuItem("Cut"),
                new MenuItem("Copy") { Shortcut = "Ctrl+C" },
                new MenuItem("Paste") { Shortcut = "Ctrl+V", IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = false; } },
                new MenuSeparator(),
                new MenuItem("Delete") { Shortcut = "Del" }
            });
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            TabView.SetSize(this.Size);
            allmapcontainer.SetSize(this.Size.Width - 12, this.Size.Height - 36);
            Sprites["header"].Bitmap.Unlock();
            (Sprites["header"].Bitmap as SolidBitmap).SetSize(Size.Width - 12, 4);
            Sprites["header"].Bitmap.Lock();
            Sprites["bar1"].X = Size.Width - 1;
            Sprites["bar1"].Bitmap.Unlock();
            (Sprites["bar1"].Bitmap as SolidBitmap).SetSize(1, Size.Height);
            Sprites["bar1"].Bitmap.Lock();
            Sprites["bar2"].X = Size.Width - 12;
            Sprites["bar2"].Bitmap.Unlock();
            (Sprites["bar2"].Bitmap as SolidBitmap).SetSize(1, Size.Height);
            Sprites["bar2"].Bitmap.Lock();
            allmapcontainer.VScrollBar.SetPosition(Size.Width - 10, 1);
            allmapcontainer.VScrollBar.SetSize(8, Size.Height - 2);
        }
    }
}
