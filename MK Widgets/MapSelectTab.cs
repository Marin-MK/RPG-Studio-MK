using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class MapSelectTab : Widget
    {
        public MapViewer MapViewer;

        Container allmapcontainer;
        TreeView mapview;

        public MapSelectTab(object Parent, string Name = "mapSelectTab")
            : base(Parent, Name)
        {
            SetBackgroundColor(10, 23, 37);
            this.Sprites["header"] = new Sprite(this.Viewport);
            Font f = Font.Get("Fonts/ProductSans-B", 16);
            Size s = f.TextSize("Maps");
            this.Sprites["header"].Bitmap = new Bitmap(s);
            this.Sprites["header"].Bitmap.Unlock();
            this.Sprites["header"].Bitmap.Font = f;
            this.Sprites["header"].Bitmap.DrawText("Maps", Color.WHITE);
            this.Sprites["header"].Bitmap.Lock();
            this.Sprites["header"].X = 10;
            this.Sprites["header"].Y = 9;

            Sprites["bar1"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height, new Color(28, 50, 73)));
            Sprites["bar1"].X = Size.Width - 1;
            Sprites["bar2"] = new Sprite(this.Viewport, new SolidBitmap(1, Size.Height, new Color(28, 50, 73)));
            Sprites["bar2"].X = Size.Width - 12;

            allmapcontainer = new Container(this);
            allmapcontainer.SetPosition(1, 34);
            allmapcontainer.VAutoScroll = true;

            VScrollBar vs = new VScrollBar(this);
            vs.SetPosition(Size.Width - 10, 1);
            vs.SetSize(8, Size.Height - 2);
            allmapcontainer.SetVScrollBar(vs);

            Container bgcontainer = new Container(allmapcontainer);

            mapview = new TreeView(allmapcontainer);
            mapview.SetWidth(205);
            List<TreeNode> Nodes = new List<TreeNode>();
            foreach (KeyValuePair<int, Data.Map> kvp in Data.GameData.Maps)
            {
                Nodes.Add(new TreeNode() { Object = kvp.Value });
            }
            // temp
            Nodes = new List<TreeNode>()
            {
                new TreeNode()
                {
                    Object = "Pallet Town",
                    Nodes = new List<TreeNode>()
                    {
                        new TreeNode()
                        {
                            Object = "Player's House",
                            Nodes = new List<TreeNode>()
                            {
                                new TreeNode() { Object = "Players Room" }
                            }
                        },
                        new TreeNode() { Object = "Rival's House" },
                        new TreeNode() { Object = "Professor's Lab" }
                    }
                },
                new TreeNode() { Object = "Route 1" },
                new TreeNode()
                {
                    Object = "Viridian City",
                    Nodes = new List<TreeNode>()
                    {
                        new TreeNode() { Object = "Pokémon Centre" },
                        new TreeNode() { Object = "Poké Mart" },
                        new TreeNode() { Object = "Pokémon Gym" },
                        new TreeNode() { Object = "House 1" },
                        new TreeNode() { Object = "House 2" }
                    }
                },
                new TreeNode() { Object = "Route 2" },
                new TreeNode() { Object = "Hau'oli City Shopping District" },
                new TreeNode() { Object = "Viridian Forest" },
                new TreeNode()
                {
                    Object = "Pewter City",
                    Nodes = new List<TreeNode>()
                    {
                        new TreeNode() { Object = "Pewter Child" }
                    }
                },
                new TreeNode() { Object = "Route 3" },
                new TreeNode()
                {
                    Object = "Mt. Moon",
                    Nodes = new List<TreeNode>()
                    {
                        new TreeNode() { Object = "Floor 1" }
                    }
                },
                new TreeNode() { Object = "Route 4" },
                new TreeNode()
                {
                    Object = "Cerulean City",
                    Nodes = new List<TreeNode>()
                    {
                        new TreeNode() { Object = "Cerulean Child" }
                    }
                },
                new TreeNode() { Object = "Route 24" },
                new TreeNode() { Object = "Route 25" },
                new TreeNode() { Object = "Route 5" },
                new TreeNode() { Object = "Underground" },
                new TreeNode() { Object = "Route 6" },
                new TreeNode()
                {
                    Object = "Vermillion City",
                    Nodes = new List<TreeNode>()
                    {
                        new TreeNode() { Object = "Vermillion Child" }
                    }
                },
                new TreeNode()
                {
                    Object = "Diglett's Cave",
                    Nodes = new List<TreeNode>()
                    {
                        new TreeNode() { Object = "Floor 1" }
                    }
                }
            };
            mapview.SetNodes(Nodes);
            mapview.OnSelectedNodeChanged += delegate (object sender, MouseEventArgs e)
            {
                // Changes mapviewer, layerstab and tilesettab to match the new map
                //MapViewer.SetMap(mapview.SelectedNode.Object as Data.Map);
            };
            this.SetContextMenuList(new List<IMenuItem>()
            {
                new MenuItem("Map Properties"),
                new MenuItem("New Map"),
                new MenuItem("Copy") { Shortcut = "Ctrl+C" },
                new MenuItem("Paste") { Shortcut = "Ctrl+V", IsClickable = delegate (object sender, ConditionEventArgs e) { e.ConditionValue = false; } },
                new MenuItem("Delete") { Shortcut = "Del" }
            });
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            allmapcontainer.SetSize(this.Size.Width - 13 - 1, this.Size.Height - 36);
            base.SizeChanged(sender, e);
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
