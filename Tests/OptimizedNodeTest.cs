using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGStudioMK.Tests;

public class OptimizedNodeTest : TestSuite
{
    [TestMethod]
    void TestNested()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //       - four
        var one = new OptimizedNode("One");
        var two = new OptimizedNode("Two");
        var three = new OptimizedNode("Three");
        var four = new OptimizedNode("Four");
        assertEqual(0, one.GlobalIndex);
        assertEqual(0, two.GlobalIndex);
        assertEqual(0, three.GlobalIndex);
        assertEqual(0, four.GlobalIndex);

        assertEqual(one, one.Root);
        assertEqual(two, two.Root);
        assertEqual(three, three.Root);
        assertEqual(four, four.Root);

        one.AddChild(two);
        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(one, two.Parent);
        assertEqual(one, one.Root);
        assertEqual(one, two.Root);

        two.AddChild(three);
        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(two, three.Parent);
        assertEqual(one, two.Parent);
        assertEqual(one, three.Root);
        assertEqual(one, two.Root);

        two.AddChild(four);
        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(3, four.GlobalIndex);
        assertEqual(two, four.Parent);
        assertEqual(two, three.Parent);

        assertEqual(three, four.GetPreviousSibling());
        assertEqual(four, three.GetNextSibling());

        assertNull(one.GetPreviousSibling());
        assertNull(two.GetPreviousSibling());
        assertNull(three.GetPreviousSibling());
        assertNull(one.GetNextSibling());
        assertNull(two.GetNextSibling());
        assertNull(four.GetNextSibling());

        assertNull(one.GetPreviousNode());
        assertEqual(one, two.GetPreviousNode());
        assertEqual(two, three.GetPreviousNode());
        assertEqual(three, four.GetPreviousNode());

        assertEqual(two, one.GetNextNode());
        assertEqual(three, two.GetNextNode());
        assertEqual(four, three.GetNextNode());
        assertNull(four.GetNextNode());
    }

    [TestMethod]
    void TestFlat()
    {
        // final tree:
        // - one
        //   - two
        //   - three
        //   - four
        var one = new OptimizedNode("One");
        var two = new OptimizedNode("Two");
        var three = new OptimizedNode("Three");
        var four = new OptimizedNode("Four");

        one.AddChild(two);
        one.AddChild(three);
        one.AddChild(four);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(3, four.GlobalIndex);

        assertEqual(3, one.Children.Count);

        assertEqual(two, one.GetNextNode());
        assertEqual(three, two.GetNextNode());
        assertEqual(four, three.GetNextNode());
        assertNull(four.GetNextNode());

        assertNull(one.GetPreviousNode());
        assertEqual(one, two.GetPreviousNode());
        assertEqual(two, three.GetPreviousNode());
        assertEqual(three, four.GetPreviousNode());

        assertNull(one.GetPreviousSibling());
        assertNull(one.GetNextSibling());
        assertNull(two.GetPreviousSibling());
        assertEqual(three, two.GetNextSibling());
        assertEqual(two, three.GetPreviousSibling());
        assertEqual(four, three.GetNextSibling());
        assertEqual(three, four.GetPreviousSibling());
        assertNull(four.GetNextSibling());

        assertEqual(one, one.Root);
        assertEqual(one, two.Root);
        assertEqual(one, three.Root);
        assertEqual(one, four.Root);

        assertNull(one.Parent);
        assertEqual(one, two.Parent);
        assertEqual(one, three.Parent);
        assertEqual(one, four.Parent);
    }

    [TestMethod]
    void TestInsert()
    {
        // final tree:
        // - root
        //   - nc
        //     - nd
        //   - na
        //   - nb

        var root = new OptimizedNode("ROOT");
        var na = new OptimizedNode("A");
        var nb = new OptimizedNode("B");
        var nc = new OptimizedNode("C");
        var nd = new OptimizedNode("D");

        assertEqual(1, root.GetTotalNodeCount());

        root.InsertChild(0, na);
        assertEqual(1, na.GlobalIndex);
        assertEqual(root, na.Root);
        assertEqual(root, na.Parent);

        assertEqual(2, root.GetTotalNodeCount());

        root.InsertChild(1, nb);
        assertEqual(2, nb.GlobalIndex);
        assertEqual(root, nb.Root);
        assertEqual(root, nb.Parent);

        assertEqual(3, root.GetTotalNodeCount());

        root.InsertChild(0, nc);
        assertEqual(1, nc.GlobalIndex);
        assertEqual(root, nc.Root);
        assertEqual(root, nc.Parent);
        assertEqual(2, na.GlobalIndex);
        assertEqual(3, nb.GlobalIndex);
        assertEqual(na, nc.GetNextNode());
        assertEqual(nb, na.GetNextNode());
        assertEqual(root, nc.GetPreviousNode());
        assertEqual(nc, na.GetPreviousNode());
        assertEqual(na, nb.GetPreviousNode());

        assertEqual(4, root.GetTotalNodeCount());

        nc.AddChild(nd);
        assertEqual(1, nc.GlobalIndex);
        assertEqual(root, nc.Root);
        assertEqual(root, nc.Parent);
        assertEqual(2, nd.GlobalIndex);
        assertEqual(root, nd.Root);
        assertEqual(nc, nd.Parent);
        assertEqual(3, na.GlobalIndex);
        assertEqual(4, nb.GlobalIndex);
        assertEqual(nd, nc.GetNextNode());
        assertEqual(na, nc.GetNextSibling());
        assertEqual(na, nd.GetNextNode());
        assertEqual(nd, na.GetPreviousNode());
        assertEqual(nc, na.GetPreviousSibling());

        assertEqual(5, root.GetTotalNodeCount());
    }
}
