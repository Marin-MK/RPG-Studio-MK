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
            this.Sprites["text"] = new Sprite(this.Viewport);
            Font f = new Font("Fonts/Quicksand Bold", 16);
            Size s = f.TextSize("Maps");
            this.Sprites["text"].Bitmap = new Bitmap(s);
            this.Sprites["text"].Bitmap.Font = f;
            this.Sprites["text"].X = 6;
            this.Sprites["text"].Y = 14;
            this.Sprites["text"].Bitmap.Unlock();
            this.Sprites["text"].Bitmap.DrawText("Maps", Color.WHITE);
            this.Sprites["text"].Bitmap.Lock();

            allmapcontainer = new Container(this);
            allmapcontainer.SetPosition(20, 40);
            allmapcontainer.SetWidth(205);
            allmapcontainer.AutoScroll = true;

            Container bgcontainer = new Container(allmapcontainer);

            mapview = new TreeView(allmapcontainer);
            mapview.SetWidth(205);
            List<TreeNode> Nodes = new List<TreeNode>();
            foreach (KeyValuePair<int, Data.Map> kvp in Data.GameData.Maps)
            {
                Nodes.Add(new TreeNode() { Object = kvp.Value });
            }
            mapview.SetNodes(Nodes);
            mapview.OnSelectedNodeChanged += delegate (object sender, MouseEventArgs e)
            {
                //MapViewer.SetMap(mapview.SelectedNode.Object as Data.Map);
            };
            mapview.OnSizeChanged += delegate (object sender, SizeEventArgs e)
            {
                //bgcontainer.SetSize(mapview.Size.Width + 20, mapview.Size.Height + 20);
            };
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            allmapcontainer.SetHeight(this.Size.Height - 50);
            base.SizeChanged(sender, e);
        }
    }
}
