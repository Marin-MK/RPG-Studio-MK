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
            SetBackgroundColor(27, 28, 32);
            this.Sprites["header"] = new Sprite(this.Viewport, new Bitmap(314, 22));
            this.Sprites["header"].Bitmap.Unlock();
            this.Sprites["header"].Bitmap.FillRect(0, 0, 314, 22, new Color(40, 44, 52));
            this.Sprites["header"].Bitmap.Font = Font.Get("Fonts/Ubuntu-R", 16);
            this.Sprites["header"].Bitmap.DrawText("Maps", 6, 1, Color.WHITE);
            this.Sprites["header"].Bitmap.Lock();

            allmapcontainer = new Container(this);
            allmapcontainer.SetPosition(8, 26);
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
                // Changes mapviewer, layerstab and tilesettab to match the new map
                MapViewer.SetMap(mapview.SelectedNode.Object as Data.Map);
            };
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            allmapcontainer.SetSize(this.Size.Width - 12, this.Size.Height - 30);
            base.SizeChanged(sender, e);
        }
    }
}
