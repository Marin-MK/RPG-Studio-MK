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
            this.Sprites["selector"].Y = 3;
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
            this.Sprites["selector"].Bitmap.Unlock();
            (this.Sprites["selector"].Bitmap as SolidBitmap).SetSize(this.Size.Width, 21);
            this.Sprites["selector"].Bitmap.Lock();
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
                y = DrawNode(this.Nodes[i], 15, y, true, i == this.Nodes.Count - 1);
                y += 24;
            }

            this.Sprites["list"].Bitmap.Lock();

            base.Draw();
            if (this.Size.Width != oldwidth) this.Redraw();
        }

        private int DrawNode(TreeNode node, int x, int y, bool FirstGeneration, bool LastNode)
        {
            node.PixelsIndented = x;
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            this.Sprites["list"].Bitmap.Font = f;
            string text = node.Object.ToString();
            Size s = f.TextSize(text);
            node.PixelWidth = s.Width + 12;
            Color c = SelectedNode == node ? new Color(55, 187, 255) : Color.WHITE;
            this.Sprites["list"].Bitmap.DrawText(text, x + 12, y, c);
            if (x + 16 + s.Width > this.Size.Width) this.SetWidth(x + 16 + s.Width);

            #region Draws connecting lines
            if (!FirstGeneration)
            {
                Sprites["list"].Bitmap.DrawLine(x - 16, y + 8, x - 6, y + 8, new Color(64, 104, 146));
            }
            #endregion

            if (node.Nodes.Count > 0)
            {
                int count = node.GetDisplayedNodeCount();
                if (count > 0) Sprites["list"].Bitmap.DrawLine(x, y + 8, x, y + 8 + count * 24, new Color(64, 104, 146));
                DrawCollapseBox(Sprites["list"].Bitmap as Bitmap, x - 5, y + 3, node.Collapsed);
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

        private void DrawCollapseBox(Bitmap b, int x, int y, bool collapsed)
        {
            b.SetPixel(x, y + 2, 17, 33, 50);
            b.SetPixel(x + 2, y, 17, 33, 50);
            b.SetPixel(x + 8, y, 17, 33, 50);
            b.SetPixel(x + 10, y + 2, 17, 33, 50);
            b.SetPixel(x, y + 8, 17, 33, 50);
            b.SetPixel(x + 2, y + 10, 17, 33, 50);
            b.SetPixel(x + 8, y + 10, 17, 33, 50);
            b.SetPixel(x + 10, y + 8, 17, 33, 50);
            b.SetPixel(x + 1, y + 1, 26, 45, 66);
            b.SetPixel(x + 1, y + 9, 26, 45, 66);
            b.SetPixel(x + 9, y + 1, 26, 45, 66);
            b.SetPixel(x + 9, y + 9, 26, 45, 66);
            b.SetPixel(x, y + 3, 39, 64, 90);
            b.SetPixel(x, y + 7, 39, 64, 90);
            b.SetPixel(x + 10, y + 3, 39, 64, 90);
            b.SetPixel(x + 10, y + 7, 39, 64, 90);
            b.SetPixel(x + 3, y + 10, 39, 64, 90);
            b.SetPixel(x + 7, y + 10, 39, 64, 90);
            b.SetPixel(x, y + 4, 53, 83, 114);
            b.SetPixel(x, y + 6, 53, 83, 114);
            b.SetPixel(x + 10, y + 4, 53, 83, 114);
            b.SetPixel(x + 10, y + 6, 53, 83, 114);
            b.SetPixel(x + 4, y + 10, 53, 83, 114);
            b.SetPixel(x + 6, y + 10, 53, 83, 114);
            b.SetPixel(x, y + 5, 58, 90, 122);
            b.SetPixel(x + 10, y + 5, 58, 90, 122);
            b.SetPixel(x + 3, y, 49, 78, 107);
            b.SetPixel(x + 7, y, 49, 78, 107);
            b.DrawLine(x + 4, y, x + 6, y, 64, 104, 146);
            b.DrawLine(x + 2, y + 1, x + 8, y + 1, 64, 104, 146);
            b.DrawLine(x + 1, y + 2, x + 9, y + 2, 64, 104, 146);
            b.DrawLine(x + 1, y + 3, x + 9, y + 3, 64, 104, 146);
            b.DrawLine(x + 1, y + 7, x + 9, y + 7, 64, 104, 146);
            b.DrawLine(x + 1, y + 8, x + 9, y + 8, 64, 104, 146);
            b.DrawLine(x + 2, y + 9, x + 8, y + 9, 64, 104, 146);
            b.DrawLine(x + 2, y + 9, x + 8, y + 9, 64, 104, 146);
            b.SetPixel(x + 5, y + 10, 64, 104, 146);
            b.SetPixel(x + 1, y + 4, 35, 55, 76);
            b.SetPixel(x + 1, y + 6, 35, 55, 76);
            b.SetPixel(x + 9, y + 4, 35, 55, 76);
            b.SetPixel(x + 9, y + 6, 35, 55, 76);
            b.DrawLine(x + 2, y + 4, x + 8, y + 4, 17, 27, 38);
            b.DrawLine(x + 2, y + 6, x + 8, y + 6, 17, 27, 38);
            b.SetPixel(x + 1, y + 5, 17, 27, 38);
            b.SetPixel(x + 9, y + 5, 17, 27, 38);
            b.SetPixel(x + 2, y + 5, 181, 193, 206);
            b.SetPixel(x + 8, y + 5, 181, 193, 206);
            b.DrawLine(x + 3, y + 5, x + 7, y + 5, Color.WHITE);
            if (collapsed)
            {
                b.SetPixel(x + 3, y, 39, 64, 90);
                b.SetPixel(x + 7, y, 39, 64, 90);
                b.SetPixel(x + 4, y, 53, 83, 114);
                b.SetPixel(x + 6, y, 53, 83, 114);
                b.SetPixel(x + 5, y, 58, 90, 122);
                b.SetPixel(x + 5, y + 10, 58, 90, 122);
                b.SetPixel(x + 4, y + 1, 35, 55, 76);
                b.SetPixel(x + 4, y + 9, 35, 55, 76);
                b.SetPixel(x + 6, y + 1, 35, 55, 76);
                b.SetPixel(x + 6, y + 9, 35, 55, 76);
                b.SetPixel(x + 4, y + 3, 17, 27, 38);
                b.SetPixel(x + 4, y + 2, 17, 27, 38);
                b.SetPixel(x + 5, y + 1, 17, 27, 38);
                b.SetPixel(x + 6, y + 2, 17, 27, 38);
                b.SetPixel(x + 6, y + 3, 17, 27, 38);
                b.SetPixel(x + 4, y + 7, 17, 27, 38);
                b.SetPixel(x + 4, y + 8, 17, 27, 38);
                b.SetPixel(x + 5, y + 9, 17, 27, 38);
                b.SetPixel(x + 6, y + 8, 17, 27, 38);
                b.SetPixel(x + 6, y + 7, 17, 27, 38);
                b.SetPixel(x + 5, y + 2, 181, 193, 206);
                b.SetPixel(x + 5, y + 8, 181, 193, 206);
                b.DrawLine(x + 5, y + 3, x + 5, y + 7, Color.WHITE);
            }
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            Sprites["hover"].Visible = WidgetIM.Hovering;
            MouseMoving(sender, e);
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!WidgetIM.Hovering) return;
            int ry = e.Y - this.Viewport.Y + Position.Y - ScrolledPosition.Y;
            int globalindex = (int) Math.Floor((ry - 5) / 24d);
            Sprites["hover"].Visible = true;
            Sprites["hover"].Y = 3 + globalindex * 24;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (!WidgetIM.Hovering) return;
            TreeNode oldselected = this.SelectedNode;
            if (e.LeftButton && !e.OldLeftButton)
            {
                int rx = e.X - this.Viewport.X + Position.X - ScrolledPosition.X;
                int ry = e.Y - this.Viewport.Y + Position.Y - ScrolledPosition.Y;
                int globalindex = (int) Math.Floor((ry - 5) / 24d);
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
                        this.Sprites["selector"].Y = 3 + globalindex * 24;
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
                        this.Sprites["selector"].Y = 3 + sindex * 24;
                    }
                }
                else
                {
                    SelectedNode = n;
                    this.Sprites["selector"].Y = 3 + globalindex * 24;
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
