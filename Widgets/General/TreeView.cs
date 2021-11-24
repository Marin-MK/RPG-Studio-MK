using System;
using System.Collections.Generic;
using odl;
using amethyst;

namespace RPGStudioMK.Widgets
{
    public class TreeView : Widget
    {
        public List<TreeNode> Nodes        { get; protected set; } = new List<TreeNode>();
        public TreeNode       SelectedNode { get; protected set; }
        public TreeNode       HoveringNode { get; protected set; }
        public TreeNode       DraggingNode { get; protected set; }

        public int TrailingBlank = 0;

        public MouseEvent OnSelectedNodeChanged;
        public BaseEvent OnDragAndDropped;

        public bool HoverTop = false;
        public bool HoverOver = false;
        public bool HoverBottom = false;

        public TreeView(IContainer Parent) : base(Parent)
        {
            Sprites["selector"] = new Sprite(this.Viewport, new SolidBitmap(1, 21, new Color(28, 50, 73)));
            Sprites["hover"] = new Sprite(this.Viewport, new SolidBitmap(2, 21, new Color(55, 187, 255)));
            Sprites["hover"].Visible = false;
            Sprites["list"] = new Sprite(this.Viewport);
            Sprites["list_drag"] = new Sprite(this.Viewport);
            Sprites["text"] = new Sprite(this.Viewport);
            this.OnWidgetSelected += WidgetSelected;
        }

        public void SetNodes(List<TreeNode> Nodes)
        {
            this.Nodes = Nodes;
            SelectedNode = Nodes[0];
            this.Redraw();
        }

        public override void SizeChanged(BaseEventArgs e)
        {
            base.SizeChanged(e);
            (this.Sprites["selector"].Bitmap as SolidBitmap).SetSize(this.Size.Width, 21);
        }

        protected override void Draw()
        {
            Sprites["list"].Bitmap?.Dispose();
            Sprites["text"].Bitmap?.Dispose();
            int items = 0;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                items++;
                items += this.Nodes[i].GetDisplayedNodeCount();
            }
            int height = items * 24;
            SetHeight(height + TrailingBlank);
            Sprites["list"].Bitmap = new Bitmap(Size.Width, height);
            Sprites["text"].Bitmap = new Bitmap(Size.Width, height);
            Sprites["list"].Bitmap.Unlock();
            Sprites["text"].Bitmap.Unlock();
            
            int y = 0;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                y = DrawNode(this.Nodes[i], 15, y, true, i == this.Nodes.Count - 1);
                y += 24;
            }

            Sprites["list"].Bitmap.Lock();
            Sprites["text"].Bitmap.Lock();

            base.Draw();
        }

        private int DrawNode(TreeNode node, int x, int y, bool FirstGeneration, bool LastNode)
        {
            if (node == SelectedNode) Sprites["selector"].Y = y;
            node.PixelsIndented = x;
            Font f = Font.Get("Fonts/ProductSans-M", 14);
            this.Sprites["text"].Bitmap.Font = f;
            Game.Map m = Game.Data.Maps[(int) node.Object];
            string text = $"({m.ParentID}, {m.Order}): " + node.Name ?? node.Object.ToString();
            Size s = f.TextSize(text);
            Color c = SelectedNode == node ? new Color(55, 187, 255) : Color.WHITE;
            this.Sprites["text"].Bitmap.DrawText(text, x + 12, y + 1, c);

            if (!FirstGeneration)
            {
                if (x - 6 < Sprites["list"].Bitmap.Width) Sprites["list"].Bitmap.DrawLine(x - 16, y + 8, x - 6, y + 8, new Color(64, 104, 146));
            }

            if (node.Nodes.Count > 0)
            {
                int count = node.GetDisplayedNodeCount();
                if (node.Nodes.Count > 0) count -= node.Nodes[node.Nodes.Count - 1].GetDisplayedNodeCount();
                if (count > 0 && x < Sprites["list"].Bitmap.Width) Sprites["list"].Bitmap.DrawLine(x, y + 8, x, y + 8 + count * 24, new Color(64, 104, 146));
                if (x + 5 < Sprites["list"].Bitmap.Width) Utilities.DrawCollapseBox(Sprites["list"].Bitmap, x - 5, y + 3, node.Collapsed);
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

        void UpdateHoverDrag()
        {
            Sprites["list_drag"].Bitmap?.Dispose();
            if (DraggingNode == null || HoveringNode == DraggingNode) return;
            Sprites["hover"].Visible = false;
            int y = 0;
            bool Found = false;
            for (int i = 0; i < Nodes.Count; i++)
            {
                (int Y, bool Continue) result = CalculateNodeYUntil(Nodes[i], HoveringNode);
                y += result.Y;
                if (!result.Continue)
                {
                    Found = true;
                    break;
                }
            }
            if (!Found) throw new Exception($"Failed to find DraggingNode.");
            if (HoverTop || HoverBottom)
            {
                Sprites["list_drag"].Bitmap = new SolidBitmap(Size.Width - 6, 1, new Color(55, 187, 255));
                Sprites["list_drag"].X = 3;
                if (HoverTop) Sprites["list_drag"].Y = y + 3;
                else Sprites["list_drag"].Y = y + 17;
            }
            else if (HoverOver)
            {
                Sprites["list_drag"].Bitmap = new Bitmap(7, 7);
                Sprites["list_drag"].X = 2;
                Sprites["list_drag"].Y = y + 7;
                Sprites["list_drag"].Bitmap.Unlock();
                Color c = new Color(55, 187, 255);
                Sprites["list_drag"].Bitmap.DrawLine(2, 0, 6, 0, c);
                Sprites["list_drag"].Bitmap.DrawLine(2, 0, 2, 6, c);
                Sprites["list_drag"].Bitmap.DrawLine(0, 4, 4, 4, c);
                Sprites["list_drag"].Bitmap.SetPixel(1, 5, c);
                Sprites["list_drag"].Bitmap.SetPixel(3, 5, c);
                Sprites["list_drag"].Bitmap.Lock();
            }
        }

        (int Y, bool Continue) CalculateNodeYUntil(TreeNode Node, TreeNode Target)
        {
            if (Node == Target) return (0, false);
            int sum = 24;
            foreach (TreeNode n in Node.Nodes)
            {
                (int Y, bool Continue) result = CalculateNodeYUntil(n, Target);
                sum += result.Y;
                if (!result.Continue) return (sum, false);
            }
            return (sum, true);
        }

        public void SetSelectedNode(TreeNode node, bool CallEvent = true)
        {
            SelectedNode = node;
            if (CallEvent) this.OnSelectedNodeChanged?.Invoke(Graphics.LastMouseEvent);
            Redraw();
        }

        public override void HoverChanged(MouseEventArgs e)
        {
            base.HoverChanged(e);
            Sprites["hover"].Visible = WidgetIM.Hovering;
            if (!WidgetIM.Hovering) HoveringNode = null;
            MouseMoving(e);
        }

        public override void MouseMoving(MouseEventArgs e)
        {
            base.MouseMoving(e);
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
            int part = ry % 24;
            Sprites["hover"].Visible = true;
            Sprites["hover"].Y = globalindex * 24;
            HoverTop = part < 7;
            HoverOver = !HoverTop && part < 17;
            HoverBottom = !HoverTop && !HoverOver;
            int index = 0;
            TreeNode n = null;
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                n = this.Nodes[i].FindNodeIndex(globalindex - index);
                if (n != null) break;
                index += this.Nodes[i].GetDisplayedNodeCount() + 1;
            }
            HoveringNode = n;
            if (DraggingNode != null) UpdateHoverDrag();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            if (!WidgetIM.Hovering) return;
            TreeNode oldselected = this.SelectedNode;
            if (e.LeftButton && !e.OldLeftButton && e.LeftButton)
            {
                int rx = e.X - this.Viewport.X + Position.X - ScrolledPosition.X;
                int ry = e.Y - this.Viewport.Y + Position.Y - ScrolledPosition.Y;
                if (ry >= Size.Height - TrailingBlank)
                {
                    DraggingNode = null;
                    UpdateHoverDrag();
                    return;
                }
                int globalindex = (int) Math.Floor(ry / 24d);
                int index = 0;
                TreeNode n = null;
                for (int i = 0; i < this.Nodes.Count; i++)
                {
                    n = this.Nodes[i].FindNodeIndex(globalindex - index);
                    if (n != null) break;
                    index += this.Nodes[i].GetDisplayedNodeCount() + 1;
                }
                if (n == null)
                {
                    DraggingNode = null;
                    UpdateHoverDrag();
                }
                int nodex = n.PixelsIndented;
                if (n.Nodes.Count > 0 && rx < nodex + 10 && rx > nodex - 10)
                {
                    n.Collapsed = !n.Collapsed;
                    int mapid = (int) n.Object;
                    Game.Data.Maps[mapid].Expanded = !n.Collapsed;
                    if (n.Collapsed && n.ContainsNode(SelectedNode)) SelectedNode = n;
                }
                else
                {
                    SelectedNode = n;
                    DraggingNode = n;
                }
            }
            if (SelectedNode != oldselected)
            {
                this.OnSelectedNodeChanged?.Invoke(e);
                Redraw();
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            if (e.LeftButton != e.OldLeftButton && !e.LeftButton)
            {
                if (DraggingNode != null && DraggingNode != HoveringNode) this.OnDragAndDropped?.Invoke(new BaseEventArgs());
                DraggingNode = null;
                UpdateHoverDrag();
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

        public TreeNode FindParentNode(Predicate<TreeNode> match)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                bool it = match.Invoke(Nodes[i]);
                if (it) return this;
                else
                {
                    TreeNode n = Nodes[i].FindParentNode(match);
                    if (n != null) return n;
                }
            }
            return null;
        }
    }
}
