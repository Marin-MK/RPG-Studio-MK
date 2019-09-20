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

        public TreeView(object Parent, string Name = "treeView")
            : base(Parent, Name)
        {
            this.Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(1, 21, new Color(28, 50, 73)));
            this.Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 21, new Color(55, 187, 255)));
            this.Sprites["hover"].Visible = false;
            this.Sprites["list"] = new Sprite(this.Viewport);
            this.WidgetIM.OnHoverChanged += HoverChanged;
            this.WidgetIM.OnMouseMoving += MouseMoving;
            this.WidgetIM.OnMouseDown += MouseDown;
            this.OnWidgetSelect += WidgetSelect;
        }

        public void SetNodes(List<TreeNode> Nodes)
        {
            this.Nodes = Nodes;
            SelectedNode = Nodes[0];
            this.Redraw();
        }

        public override void SizeChanged(object sender, SizeEventArgs e)
        {
            base.SizeChanged(sender, e);
            (this.Sprites["selector"].Bitmap as SolidBitmap).SetSize(this.Size.Width, 21);
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
            
            int y = 0;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                y = DrawNode(this.Nodes[i], 15, y, true, i == this.Nodes.Count - 1);
                y += 24;
            }

            this.Sprites["list"].Bitmap.Lock();

            base.Draw();
        }

        private int DrawNode(TreeNode node, int x, int y, bool FirstGeneration, bool LastNode)
        {
            node.PixelsIndented = x;
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            this.Sprites["list"].Bitmap.Font = f;
            string text = node.Object.ToString();
            Size s = f.TextSize(text);
            Color c = SelectedNode == node ? new Color(55, 187, 255) : Color.WHITE;
            this.Sprites["list"].Bitmap.DrawText(text, x + 12, y + 1, c);

            if (!FirstGeneration)
            {
                Sprites["list"].Bitmap.DrawLine(x - 16, y + 8, x - 6, y + 8, new Color(64, 104, 146));
            }

            if (node.Nodes.Count > 0)
            {
                int count = node.GetDisplayedNodeCount();
                if (count > 0) Sprites["list"].Bitmap.DrawLine(x, y + 8, x, y + 8 + count * 24, new Color(64, 104, 146));
                Utilities.DrawCollapseBox(Sprites["list"].Bitmap as Bitmap, x - 5, y + 3, node.Collapsed);
                if (!node.Collapsed)
                {
                    for (int i = 0; i < node.Nodes.Count; i++)
                    {
                        y += 24;
                        y = DrawNode(node.Nodes[i], x + 16, y, false, i == node.Nodes.Count - 1);
                    }
                }
            }
            return y;
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            if (Window.UI.OverContextMenu != null) return;
            Sprites["hover"].Visible = WidgetIM.Hovering;
            MouseMoving(sender, e);
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!WidgetIM.Hovering || Window.UI.OverContextMenu != null) return;
            int ry = e.Y - this.Viewport.Y + Position.Y - ScrolledPosition.Y;
            int globalindex = (int) Math.Floor(ry / 24d);
            Sprites["hover"].Visible = true;
            Sprites["hover"].Y = globalindex * 24;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (!WidgetIM.Hovering || Window.UI.OverContextMenu != null) return;
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
                if (rx < nodex + 10 && rx > nodex - 10)
                {
                    n.Collapsed = !n.Collapsed;
                    if (n.Collapsed && n.ContainsNode(SelectedNode))
                    {
                        SelectedNode = n;
                        this.Sprites["selector"].Y = globalindex * 24;
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
                        this.Sprites["selector"].Y = sindex * 24;
                    }
                }
                else
                {
                    SelectedNode = n;
                    this.Sprites["selector"].Y = globalindex * 24;
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
