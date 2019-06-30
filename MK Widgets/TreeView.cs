using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class TreeView : Widget
    {
        public List<TreeNode> Nodes        { get; protected set; } = new List<TreeNode>();
        public TreeNode       SelectedNode { get; protected set; }

        public EventHandler<MouseEventArgs> OnSelectedNodeChanged;

        public TreeNode NewNode(string Name, List<TreeNode> Nodes = null)
        {
            return new TreeNode() { Object = Name, Nodes = Nodes ?? new List<TreeNode>() };
        }

        public TreeView(object Parent, string Name = "treeView")
            : base(Parent, Name)
        {
            //this.Nodes = new List<TreeNode>()
            //{
            //    NewNode("Intro"),
            //    NewNode("Lappet Town", new List<TreeNode>()
            //    {
            //        NewNode("\\PN's house"),
            //        NewNode("Daisy's house"),
            //        NewNode("Pokémon Lab")
            //    }),
            //    NewNode("Route 1", new List<TreeNode>()
            //    {
            //        NewNode("Kurt's house")
            //    }),
            //    NewNode("Cedolan City", new List<TreeNode>()
            //    {
            //        NewNode("Cedolan City Poké Center"),
            //        NewNode("Cedolan Gym"),
            //        NewNode("Pokémon Insitute"),
            //        NewNode("Cedolan City Condo"),
            //        NewNode("Game Corner"),
            //        NewNode("Cedolan Dept. 1F", new List<TreeNode>()
            //        {
            //            NewNode("Cedolan Dept. 2F"),
            //            NewNode("Cedolan Dept. 3F"),
            //            NewNode("Cedolan Dept. 4F"),
            //            NewNode("Cedolan Dept. 5F"),
            //            NewNode("Cedolan Dept. Rooftop"),
            //            NewNode("Cedolan Dept. Elevator"),
            //        })
            //    }),
            //    NewNode("Route 2"),
            //    NewNode("Lerucean Town", new List<TreeNode>()
            //    {
            //        NewNode("Lerucean Town Poké Center"),
            //        NewNode("Lerucean Town Mart"),
            //        NewNode("Pokémon Fan Club"),
            //        NewNode("Pokémon Day Care")
            //    }),
            //    NewNode("Natural Park", new List<TreeNode>()
            //    {
            //        NewNode("Natural Park Entrance"),
            //        NewNode("Natural Park Pavillion")
            //    })
            //};

            this.Sprites["selection"] = new Sprite(this.Viewport, new SolidBitmap(205, 24, new Color(0, 120, 215)));
            this.Sprites["list"] = new Sprite(this.Viewport);
            this.WidgetIM.OnMouseDown += MouseDown;
            this.OnWidgetSelect += WidgetSelect;
        }

        public void SetNodes(List<TreeNode> Nodes)
        {
            this.Nodes = Nodes;
            this.Redraw();
        }

        protected override void Draw()
        {
            if (this.Sprites["list"].Bitmap != null) this.Sprites["list"].Bitmap.Dispose();
            int items = 0;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                items++;
                items += this.Nodes[i].GetDisplayedNodeCount();
            }
            int height = items * 24;
            this.SetHeight(height);
            this.Sprites["list"].Bitmap = new Bitmap(this.Size.Width, height);
            this.Sprites["list"].Bitmap.Unlock();

            int oldwidth = this.Size.Width;
            this.SetWidth(205);
            int y = 5;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                y = DrawNode(this.Nodes[i], 0, y);
                y += 24;
            }

            this.Sprites["list"].Bitmap.Lock();

            this.Sprites["selection"].Bitmap.Unlock();
            (this.Sprites["selection"].Bitmap as SolidBitmap).SetSize(this.Size.Width, 24);
            this.Sprites["selection"].Bitmap.Lock();

            base.Draw();
            if (this.Size.Width != oldwidth) this.Redraw();
        }

        private int DrawNode(TreeNode node, int x = 0, int y = 0)
        {
            node.PixelsIndented = x;
            Font f = Font.Get("Fonts/Quicksand Bold", 14);
            this.Sprites["list"].Bitmap.Font = f;
            string text = node.Object.ToString();
            Size s = f.TextSize(text);
            this.Sprites["list"].Bitmap.DrawText(text, x + 16, y, Color.WHITE);
            if (x + 16 + s.Width > this.Size.Width) this.SetWidth(x + 16 + s.Width);

            if (node.Nodes.Count > 0)
            {
                Font fa = Font.Get("Fonts/FontAwesome Solid", 14);
                char glyph = node.Collapsed ? '\uf105' : '\uf107';
                this.Sprites["list"].Bitmap.Font = fa;
                this.Sprites["list"].Bitmap.DrawGlyph(glyph, x + 2, y - 2, Color.WHITE);

                if (!node.Collapsed)
                {
                    for (int i = 0; i < node.Nodes.Count; i++)
                    {
                        y += 24;
                        y = DrawNode(node.Nodes[i], x + 16, y);
                    }
                }
            }
            return y;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            TreeNode oldselected = this.SelectedNode;
            if (this.WidgetIM.Hovering && e.LeftButton && !e.OldLeftButton)
            {
                int rx = e.X - this.Viewport.X;
                int ry = e.Y - this.Viewport.Y;
                int globalindex = (int) Math.Floor(ry / 24d);
                int index = 0;
                TreeNode n = null;
                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    n = this.Nodes[i].FindNodeIndex(globalindex - index);
                    if (n != null) break;
                    index += this.Nodes[i].GetDisplayedNodeCount() + 1;
                }
                if (n != null)
                {
                    int nx = n.PixelsIndented;
                    int ny = globalindex * 24;
                    if (rx >= nx && ry >= ny && rx < nx + 16 && ry < ny + 24)
                    {
                        n.Collapsed = !n.Collapsed;
                        if (n.Collapsed && n.ContainsNode(SelectedNode))
                        {
                            SelectedNode = n;
                            this.Sprites["selection"].Y = globalindex * 24;
                        }
                        else if (n != SelectedNode)
                        {
                            int sindex = 0;
                            for (int i = 0; i < this.Nodes.Count; i++)
                            {
                                if (this.Nodes[i] == SelectedNode) break;
                                sindex++;
                                if (!this.Nodes[i].ContainsNode(SelectedNode))
                                {
                                    sindex += this.Nodes[i].GetDisplayedNodeCount();
                                }
                                else
                                {
                                    sindex += this.Nodes[i].GetNodeIndex(SelectedNode);
                                }
                            }
                            this.Sprites["selection"].Y = sindex * 24;
                        }
                    }
                    else
                    {
                        SelectedNode = n;
                        this.Sprites["selection"].Y = globalindex * 24;
                    }
                }
            }
            if (SelectedNode != oldselected)
            {
                if (OnSelectedNodeChanged != null) OnSelectedNodeChanged.Invoke(this, e);
            }
        }
    }

    public class TreeNode
    {
        public object Object = "treeNode";
        public bool Collapsed = true;
        public List<TreeNode> Nodes = new List<TreeNode>();
        public int PixelsIndented = 0;

        public int GetDisplayedNodeCount()
        {
            int count = 0;
            if (Collapsed) return 0;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                count++;
                count += this.Nodes[i].GetDisplayedNodeCount();
            }
            return count;
        }

        public TreeNode FindNodeIndex(int Index = 0)
        {
            if (Index == 0) return this;
            if (Collapsed)
            {
                return null;
            }
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                Index -= 1;
                TreeNode n = this.Nodes[i].FindNodeIndex(Index);
                if (n != null) return n;
                Index -= this.Nodes[i].GetDisplayedNodeCount();
            }
            return null;
        }

        public bool ContainsNode(TreeNode n)
        {
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                if (this.Nodes[i] == n) return true;
                if (this.Nodes[i].ContainsNode(n)) return true;
            }
            return false;
        }

        public int GetNodeIndex(TreeNode n)
        {
            int index = 1;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                index++;
                if (this.Nodes[i] == n) return index;
                int idx = this.Nodes[i].GetNodeIndex(n);
                if (idx != -1) return index + idx;
            }
            return -1;
        }
    }
}
