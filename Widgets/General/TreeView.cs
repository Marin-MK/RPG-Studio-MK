using System;
using System.Collections.Generic;
using ODL;

namespace MKEditor.Widgets
{
    public class TreeView : Widget
    {
        public List<TreeNode> Nodes        { get; protected set; } = new List<TreeNode>();
        public TreeNode       SelectedNode { get; protected set; }
        public TreeNode       HoveringNode { get; protected set; }

        public int TrailingBlank = 0;

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
            this.OnWidgetSelected += WidgetSelected;
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
            this.SetHeight(height + TrailingBlank);
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
            if (node == SelectedNode) Sprites["selector"].Y = y;
            node.PixelsIndented = x;
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            this.Sprites["list"].Bitmap.Font = f;
            string text = node.Name ?? node.Object.ToString();
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
                if (node.Nodes.Count > 0) count -= node.Nodes[node.Nodes.Count - 1].GetDisplayedNodeCount();
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

        public void SetSelectedNode(TreeNode node, bool CallEvent = true)
        {
            SelectedNode = node;
            if (CallEvent && OnSelectedNodeChanged != null) OnSelectedNodeChanged.Invoke(this, Graphics.LastMouseEvent);
            Redraw();
        }

        public override void HoverChanged(object sender, MouseEventArgs e)
        {
            base.HoverChanged(sender, e);
            Sprites["hover"].Visible = WidgetIM.Hovering;
            if (!WidgetIM.Hovering) HoveringNode = null;
            MouseMoving(sender, e);
        }

        public override void MouseMoving(object sender, MouseEventArgs e)
        {
            base.MouseMoving(sender, e);
            if (!WidgetIM.Hovering) return;
            int rx = e.X - this.Viewport.X + Position.X - ScrolledPosition.X;
            int ry = e.Y - this.Viewport.Y + Position.Y - ScrolledPosition.Y;
            if (ry >= Size.Height - TrailingBlank)
            {
                Sprites["hover"].Visible = false;
                HoveringNode = null;
                return;
            }
            int globalindex = (int) Math.Floor(ry / 24d);
            Sprites["hover"].Visible = true;
            Sprites["hover"].Y = globalindex * 24;
            int index = 0;
            TreeNode n = null;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                n = this.Nodes[i].FindNodeIndex(globalindex - index);
                if (n != null) break;
                index += this.Nodes[i].GetDisplayedNodeCount() + 1;
            }
            HoveringNode = n;
        }

        public override void MouseDown(object sender, MouseEventArgs e)
        {
            base.MouseDown(sender, e);
            if (!WidgetIM.Hovering) return;
            TreeNode oldselected = this.SelectedNode;
            if (e.LeftButton && !e.OldLeftButton || e.RightButton && !e.OldRightButton)
            {
                int rx = e.X - this.Viewport.X + Position.X - ScrolledPosition.X;
                int ry = e.Y - this.Viewport.Y + Position.Y - ScrolledPosition.Y;
                if (ry >= Size.Height - TrailingBlank) return;
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
                if (n.Nodes.Count > 0 && rx < nodex + 10 && rx > nodex - 10)
                {
                    n.Collapsed = !n.Collapsed;
                    int mapid = (n.Object as Game.Map).ID;
                    (Parent.Parent as MapSelectPanel).SetCollapsed(Editor.ProjectSettings.MapOrder, mapid, n.Collapsed);
                    if (n.Collapsed && n.ContainsNode(SelectedNode)) SelectedNode = n;
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
        public string Name;
        public object Object = "treeNode";
        public bool Collapsed = true;
        public List<TreeNode> Nodes = new List<TreeNode>();
        public int PixelsIndented = 0;

        public TreeNode()
        {

        }

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

        public TreeNode RemoveNode(TreeNode n)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == n)
                {
                    Nodes.RemoveAt(i);
                    if (Nodes.Count == 0) return this;
                    return i >= Nodes.Count ? Nodes[i - 1] : Nodes[i];
                }
                else if (Nodes[i].ContainsNode(n))
                {
                    return Nodes[i].RemoveNode(n);
                }
            }
            return null;
        }

        public TreeNode FindNode(Predicate<TreeNode> match)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                bool it = match.Invoke(Nodes[i]);
                if (it) return Nodes[i];
                else
                {
                    TreeNode n = Nodes[i].FindNode(match);
                    if (n != null) return n;
                }
            }
            return null;
        }
    }
}
