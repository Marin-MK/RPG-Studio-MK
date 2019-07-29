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
            /*this.Nodes = new List<TreeNode>()
            {
                NewNode("Intro"),
                NewNode("Lappet Town", new List<TreeNode>()
                {
                    NewNode("\\PN's house"),
                    NewNode("Daisy's house", new List<TreeNode>()
                    {
                        NewNode("Daisy's Bedroom"),
                        NewNode("Daisy's Closet")
                    }),
                    NewNode("Pokémon Lab")
                }),
                NewNode("Route 1", new List<TreeNode>()
                {
                    NewNode("Kurt's house")
                }),
                NewNode("Cedolan City", new List<TreeNode>()
                {
                    NewNode("Cedolan City Poké Center"),
                    NewNode("Cedolan Gym"),
                    NewNode("Pokémon Insitute"),
                    NewNode("Cedolan City Condo"),
                    NewNode("Game Corner"),
                    NewNode("Cedolan Dept. 1F", new List<TreeNode>()
                    {
                        NewNode("Cedolan Dept. 2F"),
                        NewNode("Cedolan Dept. 3F"),
                        NewNode("Cedolan Dept. 4F"),
                        NewNode("Cedolan Dept. 5F"),
                        NewNode("Cedolan Dept. Rooftop"),
                        NewNode("Cedolan Dept. Elevator"),
                    })
                }),
                NewNode("Route 2"),
                NewNode("Lerucean Town", new List<TreeNode>()
                {
                    NewNode("Lerucean Town Poké Center"),
                    NewNode("Lerucean Town Mart"),
                    NewNode("Pokémon Fan Club"),
                    NewNode("Pokémon Day Care")
                }),
                NewNode("Natural Park", new List<TreeNode>()
                {
                    NewNode("Natural Park Entrance"),
                    NewNode("Natural Park Pavillion")
                })
            };
            SelectedNode = Nodes[0];*/
            
            this.Sprites["list"] = new Sprite(this.Viewport);
            this.WidgetIM.OnMouseDown += MouseDown;
            this.OnWidgetSelect += WidgetSelect;
        }

        public void SetNodes(List<TreeNode> Nodes)
        {
            this.Nodes = Nodes;
            SelectedNode = Nodes[0];
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
                y = DrawNode(this.Nodes[i], 3, y, i == 0, i == this.Nodes.Count - 1);
                y += 24;
            }

            this.Sprites["list"].Bitmap.Lock();

            base.Draw();
            if (this.Size.Width != oldwidth) this.Redraw();
        }

        private int DrawNode(TreeNode node, int x, int y, bool FirstNode, bool LastNode)
        {
            node.PixelsIndented = x;
            Font f = Font.Get("Fonts/Ubuntu-B", 14);
            this.Sprites["list"].Bitmap.Font = f;
            string text = node.Object.ToString();
            Size s = f.TextSize(text);
            node.PixelWidth = s.Width + 16;
            Color c = SelectedNode == node ? new Color(255, 168, 54) : new Color(134, 146, 158);
            this.Sprites["list"].Bitmap.DrawText(text, x + 16, y, c);
            if (x + 20 + s.Width > this.Size.Width) this.SetWidth(x + 20 + s.Width);

            #region Draws connecting lines
            Color LineColor = new Color(134, 146, 158);
            if (FirstNode && !LastNode) // First node
            {
                Sprites["list"].Bitmap.DrawLine(x, y + 11, x, y + 23, LineColor);
            }
            else if (LastNode) // Last node
            {
                Sprites["list"].Bitmap.DrawLine(x, y, x, y + 11, LineColor);
            }
            else // Middle node
            {
                Sprites["list"].Bitmap.DrawLine(x, y, x, y + 23, LineColor);
            }
            if (!LastNode)
            {
                int fullnodecount = node.GetDisplayedNodeCount();
                Sprites["list"].Bitmap.DrawLine(x, y, x, y + 23 + fullnodecount * 24, LineColor);
            }
            Sprites["list"].Bitmap.DrawLine(x, y + 11, x + 8, y + 11, LineColor);
            #endregion

            if (node.Nodes.Count > 0)
            {
                DrawCollapseBox(Sprites["list"].Bitmap as Bitmap, x - 3, y + 8, node.Collapsed);
                if (!node.Collapsed)
                {
                    for (int i = 0; i < node.Nodes.Count; i++)
                    {
                        y += 24;
                        y = DrawNode(node.Nodes[i], x + 20, y, i == 0, i == node.Nodes.Count - 1);
                    }
                }
            }
            return y;
        }

        private void DrawCollapseBox(Bitmap b, int x, int y, bool collapsed)
        {
            Color outer = new Color(55, 55, 55);
            Color middle = new Color(111, 111, 111);
            Color inner = new Color(132, 132, 132);
            Color gray = new Color(42, 42, 42);
            b.SetPixel(x + 1, y, outer);
            b.SetPixel(x, y + 1, outer);
            b.SetPixel(x + 5, y, outer);
            b.SetPixel(x + 6, y + 1, outer);
            b.SetPixel(x, y + 5, outer);
            b.SetPixel(x + 1, y + 6, outer);
            b.SetPixel(x + 5, y + 6, outer);
            b.SetPixel(x + 6, y + 5, outer);
            b.SetPixel(x + 2, y, middle);
            b.SetPixel(x + 4, y, middle);
            b.SetPixel(x, y + 2, middle);
            b.SetPixel(x, y + 4, middle);
            b.SetPixel(x + 6, y + 2, middle);
            b.SetPixel(x + 6, y + 4, middle);
            b.SetPixel(x + 2, y + 6, middle);
            b.SetPixel(x + 4, y + 6, middle);
            b.SetPixel(x + 3, y, inner);
            b.SetPixel(x, y + 3, inner);
            b.SetPixel(x + 6, y + 3, inner);
            b.SetPixel(x + 3, y + 6, inner);
            b.FillRect(x + 1, y + 1, 5, 5, inner);
            // Draws minus
            b.SetPixel(x + 1, y + 3, gray);
            b.SetPixel(x + 2, y + 3, Color.BLACK);
            b.SetPixel(x + 3, y + 3, Color.BLACK);
            b.SetPixel(x + 4, y + 3, Color.BLACK);
            b.SetPixel(x + 5, y + 3, gray);
            if (collapsed) // Draws extra line to make a plus
            {
                b.SetPixel(x + 3, y + 1, gray);
                b.SetPixel(x + 3, y + 2, Color.BLACK);
                b.SetPixel(x + 3, y + 4, Color.BLACK);
                b.SetPixel(x + 3, y + 5, gray);
            }
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            TreeNode oldselected = this.SelectedNode;
            if (e.LeftButton && !e.OldLeftButton)
            {
                int rx = e.X - this.Viewport.X + Position.X - ScrolledPosition.X;
                int ry = e.Y - this.Viewport.Y + Position.Y - ScrolledPosition.Y;
                int globalindex = (int) Math.Floor(ry / 24d);
                int index = 0;
                TreeNode n = null;
                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    n = this.Nodes[i].FindNodeIndex(globalindex - index);
                    if (n != null) break;
                    index += this.Nodes[i].GetDisplayedNodeCount() + 1;
                }
                if (n == null) return;
                int nodex = n.PixelsIndented;
                if (rx < nodex - 10 || rx > nodex + n.PixelWidth + 10) return;
                if (rx < nodex + 10)
                {
                    if (n.Nodes.Count > 0)
                    {
                        n.Collapsed = !n.Collapsed;
                        if (n.Collapsed && n.ContainsNode(SelectedNode))
                            SelectedNode = n;
                    }
                }
                else
                {
                    SelectedNode = n;
                }
            }
            if (SelectedNode != oldselected)
            {
                if (OnSelectedNodeChanged != null) OnSelectedNodeChanged.Invoke(this, e);
                Redraw();
            }
        }
    }

    public class TreeNode
    {
        public object Object = "treeNode";
        public bool Collapsed = false;
        public List<TreeNode> Nodes = new List<TreeNode>();
        public int PixelsIndented = 0;
        public int PixelWidth = 0;

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
