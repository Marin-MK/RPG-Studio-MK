using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Widgets;

namespace RPGStudioMK.Tests;

//[DisableTestClass]
public class OptimizedNodeTest : TestSuite
{
    [TestMethod]
    void TestNested()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //     - four
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

        assertEqual(0, one.Depth);
        assertEqual(1, two.Depth);
        assertEqual(2, three.Depth);
        assertEqual(2, four.Depth);
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

        assertEqual(nb, root.GetLastNode());
        assertEqual(nb, nb.GetLastNode());
        assertEqual(na, na.GetLastNode());
        assertEqual(nd, nd.GetLastNode());
        assertEqual(nd, nc.GetLastNode());
    }

    [TestMethod]
    void TestContains()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //     - four
        //       - five
        //     - six
        //   - seven
        //     - eight
        //     - nine
        //       - ten
        //       - eleven
        //     - twelve
        //   - thirteen
        //     - fourteen

        var one = new OptimizedNode("One");
        var two = new OptimizedNode("Two");
        var three = new OptimizedNode("Three");
        var four = new OptimizedNode("Four");
        var five = new OptimizedNode("Five");
        var six = new OptimizedNode("Six");
        var seven = new OptimizedNode("Seven");
        var eight = new OptimizedNode("Eight");
        var nine = new OptimizedNode("Nine");
        var ten = new OptimizedNode("Ten");
        var eleven = new OptimizedNode("Eleven");
        var twelve = new OptimizedNode("Twelve");
        var thirteen = new OptimizedNode("Thirteen");
        var fourteen = new OptimizedNode("Fourteen");

        nine.AddChild(ten);
        nine.AddChild(eleven);

        seven.AddChild(eight);
        seven.AddChild(nine);
        seven.AddChild(twelve);

        four.AddChild(five);
        two.AddChild(three);
        two.AddChild(four);
        two.AddChild(six);

        thirteen.AddChild(fourteen);

        one.AddChild(two);
        one.AddChild(seven);
        one.AddChild(thirteen);

        assertTrue(one.Contains(two));
        assertTrue(two.Contains(three));
        assertTrue(four.Contains(five));
        assertTrue(two.Contains(five));
        assertTrue(one.Contains(five));
        assertTrue(one.Contains(fourteen));
        assertTrue(one.Contains(eleven));
        assertTrue(one.Contains(thirteen));

        assertFalse(two.Contains(seven));
        assertFalse(two.Contains(eleven));
        assertFalse(two.Contains(two));
        assertFalse(five.Contains(four));
        assertFalse(eight.Contains(nine));
        assertFalse(nine.Contains(eight));
        assertFalse(eleven.Contains(fourteen));
        assertFalse(eleven.Contains(thirteen));
        assertFalse(eleven.Contains(seven));
    }

    [TestMethod]
    void TestBigTree()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //     - four
        //       - five
        //     - six
        //   - seven
        //     - eight
        //     - nine
        //       - ten
        //       - eleven
        //     - twelve
        //   - thirteen
        //     - fourteen

        var one = new OptimizedNode("One");
        var two = new OptimizedNode("Two");
        var three = new OptimizedNode("Three");
        var four = new OptimizedNode("Four");
        var five = new OptimizedNode("Five");
        var six = new OptimizedNode("Six");
        var seven = new OptimizedNode("Seven");
        var eight = new OptimizedNode("Eight");
        var nine = new OptimizedNode("Nine");
        var ten = new OptimizedNode("Ten");
        var eleven = new OptimizedNode("Eleven");
        var twelve = new OptimizedNode("Twelve");
        var thirteen = new OptimizedNode("Thirteen");
        var fourteen = new OptimizedNode("Fourteen");

        nine.AddChild(ten);
        nine.AddChild(eleven);

        seven.AddChild(eight);
        seven.AddChild(nine);
        seven.AddChild(twelve);

        four.AddChild(five);
        two.AddChild(three);
        two.AddChild(four);
        two.AddChild(six);

        thirteen.AddChild(fourteen);

        one.AddChild(two);
        one.AddChild(seven);
        one.AddChild(thirteen);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(3, four.GlobalIndex);
        assertEqual(4, five.GlobalIndex);
        assertEqual(5, six.GlobalIndex);
        assertEqual(6, seven.GlobalIndex);
        assertEqual(7, eight.GlobalIndex);
        assertEqual(8, nine.GlobalIndex);
        assertEqual(9, ten.GlobalIndex);
        assertEqual(10, eleven.GlobalIndex);
        assertEqual(11, twelve.GlobalIndex);
        assertEqual(12, thirteen.GlobalIndex);
        assertEqual(13, fourteen.GlobalIndex);

        assertEqual(one, one.Root);
        assertEqual(one, two.Root);
        assertEqual(one, three.Root);
        assertEqual(one, four.Root);
        assertEqual(one, five.Root);
        assertEqual(one, six.Root);
        assertEqual(one, seven.Root);
        assertEqual(one, eight.Root);
        assertEqual(one, nine.Root);
        assertEqual(one, ten.Root);
        assertEqual(one, eleven.Root);
        assertEqual(one, twelve.Root);
        assertEqual(one, thirteen.Root);
        assertEqual(one, fourteen.Root);

        assertEqual(one, two.Parent);
        assertEqual(two, three.Parent);
        assertEqual(two, four.Parent);
        assertEqual(four, five.Parent);
        assertEqual(two, six.Parent);
        assertEqual(one, seven.Parent);
        assertEqual(seven, eight.Parent);
        assertEqual(seven, nine.Parent);
        assertEqual(nine, ten.Parent);
        assertEqual(nine, eleven.Parent);
        assertEqual(one, thirteen.Parent);
        assertEqual(thirteen, fourteen.Parent);

        assertEqual(0, one.Depth);
        assertEqual(1, two.Depth);
        assertEqual(2, three.Depth);
        assertEqual(2, four.Depth);
        assertEqual(3, five.Depth);
        assertEqual(2, six.Depth);
        assertEqual(1, seven.Depth);
        assertEqual(2, eight.Depth);
        assertEqual(2, nine.Depth);
        assertEqual(3, ten.Depth);
        assertEqual(3, eleven.Depth);
        assertEqual(2, twelve.Depth);
        assertEqual(1, thirteen.Depth);
        assertEqual(2, fourteen.Depth);
    }

    [TestMethod]
    void TestComplexWithExpansion()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //     - four (collapsed!)
        //       - five
        //     - six
        //   - seven
        //     - eight
        //     - nine (collapsed!)
        //       - ten
        //       - eleven
        //     - twelve
        //   - thirteen (collapsed!)
        //     - fourteen

        var one = new OptimizedNode("One");
        var two = new OptimizedNode("Two");
        var three = new OptimizedNode("Three");
        var four = new OptimizedNode("Four");
        var five = new OptimizedNode("Five");
        var six = new OptimizedNode("Six");
        var seven = new OptimizedNode("Seven");
        var eight = new OptimizedNode("Eight");
        var nine = new OptimizedNode("Nine");
        var ten = new OptimizedNode("Ten");
        var eleven = new OptimizedNode("Eleven");
        var twelve = new OptimizedNode("Twelve");
        var thirteen = new OptimizedNode("Thirteen");
        var fourteen = new OptimizedNode("Fourteen");

        nine.AddChild(ten);
        nine.AddChild(eleven);

        seven.AddChild(eight);
        seven.AddChild(nine);
        seven.AddChild(twelve);

        four.AddChild(five);
        two.AddChild(three);
        two.AddChild(four);
        two.AddChild(six);

        thirteen.AddChild(fourteen);

        one.AddChild(two);
        one.AddChild(seven);
        one.AddChild(thirteen);

        four.SetExpanded(false);
        nine.SetExpanded(false);
        thirteen.SetExpanded(false);

        // This tree was already tested in the method above; now we test
        // the nextnode and previousnodemethods with expansion states factored in.

        assertEqual(fourteen, thirteen.GetNextNode());
        assertNull(thirteen.GetNextNode(false));

        assertEqual(twelve, thirteen.GetPreviousNode(false));
        assertEqual(twelve, thirteen.GetPreviousNode(true));
        assertEqual(nine, twelve.GetPreviousNode(false));
        assertEqual(eleven, twelve.GetPreviousNode(true));
        assertEqual(nine, eight.GetNextNode(true));
        assertEqual(nine, eight.GetNextNode(false));
        assertEqual(ten, nine.GetNextNode(true));
        assertEqual(twelve, nine.GetNextNode(false));

        assertTrue(one.Contains(four, true));
        assertTrue(one.Contains(four, false));
        assertTrue(one.Contains(five, true));
        assertFalse(one.Contains(five, false));
        assertTrue(one.Contains(fourteen, true));
        assertFalse(one.Contains(fourteen, false));
    }

    [TestMethod]
    void TestAddWithSeparatorMid()
    {
        // final tree:
        // - one
        //   - two
        //   - XXXXX
        //   - three
        var one = new OptimizedNode("One");
        var two = new OptimizedNode("Two");
        var sep = new OptimizedNodeSeparator();
        var three = new OptimizedNode("Three");

        assertEqual(0, one.GlobalIndex);
        assertEqual(0, two.GlobalIndex);
        assertEqual(0, three.GlobalIndex);

        one.AddChild(two);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(one, one.Root);
        assertNull(one.Parent);

        assertEqual(one, two.Root);
        assertEqual(one, two.Parent);

        one.AddChild(sep);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(one, sep.Parent);
        assertEqual(one, sep.Root);

        assertNull(two.GetNextNode());
        assertNull(two.GetNextSibling());

        one.AddChild(three);

        assertEqual(2, three.GlobalIndex);
        assertEqual(one, three.Root);
        assertEqual(one, three.Parent);

        assertEqual(two, three.GetPreviousNode());
        assertEqual(two, three.GetPreviousSibling());
        assertEqual(three, two.GetNextNode());
        assertEqual(three, two.GetNextSibling());
        assertEqual(one, two.GetPreviousNode());
        assertNull(two.GetPreviousSibling());
        assertNull(three.GetNextNode());
        assertNull(three.GetNextSibling());
    }

    [TestMethod]
    void TestAddWithSeparatorEnd()
    {
        // final tree:
        // - one
        //   - two
        //   - three
        //   - XXXXX
        var one = new OptimizedNode("One");
        var two = new OptimizedNode("Two");
        var three = new OptimizedNode("Three");
        var sep = new OptimizedNodeSeparator();

        one.AddChild(two);
        one.AddChild(three);
        one.AddChild(sep);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);

        assertEqual(one, one.Root);
        assertEqual(one, two.Root);
        assertEqual(one, three.Root);
        assertEqual(one, sep.Root);

        assertNull(one.Parent);
        assertEqual(one, two.Parent);
        assertEqual(one, three.Parent);
        assertEqual(one, sep.Parent);

        assertNull(one.GetPreviousNode());
        assertEqual(two, one.GetNextNode());
        assertNull(one.GetPreviousSibling());
        assertNull(one.GetNextSibling());

        assertEqual(one, two.GetPreviousNode());
        assertEqual(three, two.GetNextNode());
        assertNull(two.GetPreviousSibling());
        assertEqual(three, two.GetNextSibling());

        assertEqual(two, three.GetPreviousNode());
        assertNull(three.GetNextNode());
        assertEqual(two, three.GetPreviousSibling());
        assertNull(three.GetNextSibling());
    }

    [TestMethod]
    void TestAddWithSeparatorStart()
    {
        // final tree:
        // - one
        //   - XXXXX
        //   - two
        //   - three
        var one = new OptimizedNode("One");
        var sep = new OptimizedNodeSeparator();
        var two = new OptimizedNode("Two");
        var three = new OptimizedNode("Three");

        one.AddChild(two);
        one.AddChild(three);
        one.AddChild(sep);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);

        assertEqual(one, one.Root);
        assertEqual(one, two.Root);
        assertEqual(one, three.Root);
        assertEqual(one, sep.Root);

        assertNull(one.Parent);
        assertEqual(one, two.Parent);
        assertEqual(one, three.Parent);
        assertEqual(one, sep.Parent);

        assertNull(one.GetPreviousNode());
        assertEqual(two, one.GetNextNode());
        assertNull(one.GetPreviousSibling());
        assertNull(one.GetNextSibling());

        assertEqual(one, two.GetPreviousNode());
        assertNull(two.GetPreviousSibling());
        assertEqual(three, two.GetNextNode());
        assertEqual(three, two.GetNextSibling());

        assertEqual(two, three.GetPreviousNode());
        assertEqual(two, three.GetPreviousSibling());
        assertNull(three.GetNextNode());
        assertNull(three.GetNextSibling());
    }

    [TestMethod]
    void TestBigInsert()
    {
        // final tree:
        // - root
        //   - one
        //   - two
        //     - three
        //       - XXXXX
        //       - four
        //     - five
        //   - six
        //     - seven
        //     - eight
        //     - nine
        //   - ten
        //   - eleven

        var root = new OptimizedNode("ROOT");
        var one = new OptimizedNode("One");
        var two = new OptimizedNode("Two");
        var three = new OptimizedNode("Three");
        var xxx = new OptimizedNodeSeparator();
        var four = new OptimizedNode("Four");
        var five = new OptimizedNode("Five");
        var six = new OptimizedNode("Six");
        var seven = new OptimizedNode("Seven");
        var eight = new OptimizedNode("Eight");
        var nine = new OptimizedNode("Nine");
        var ten = new OptimizedNode("Ten");
        var eleven = new OptimizedNode("Eleven");

        three.AddChild(four);
        two.AddChild(three);
        two.AddChild(five);

        six.AddChild(seven);
        six.AddChild(eight);
        six.AddChild(nine);

        root.AddChild(one);
        root.AddChild(two);
        root.AddChild(six);
        root.AddChild(ten);
        root.AddChild(eleven);

        three.InsertChild(0, xxx);

        assertEqual(0, root.GlobalIndex);
        assertEqual(1, one.GlobalIndex);
        assertEqual(2, two.GlobalIndex);
        assertEqual(3, three.GlobalIndex);
        assertEqual(4, four.GlobalIndex);
        assertEqual(5, five.GlobalIndex);
        assertEqual(6, six.GlobalIndex);
        assertEqual(7, seven.GlobalIndex);
        assertEqual(8, eight.GlobalIndex);
        assertEqual(9, nine.GlobalIndex);
        assertEqual(10, ten.GlobalIndex);
        assertEqual(11, eleven.GlobalIndex);

        assertEqual(three, two.GetNextNode());
        assertEqual(six, two.GetNextSibling());

        assertEqual(five, six.GetPreviousNode());
        assertEqual(two, six.GetPreviousSibling());

        assertEqual(two, three.GetPreviousNode());
        assertNull(three.GetPreviousSibling());
        assertEqual(four, three.GetNextNode());
        assertEqual(five, three.GetNextSibling());

        assertEqual(three, four.GetPreviousNode());
        assertNull(four.GetPreviousSibling());
        assertEqual(five, four.GetNextNode());
        assertNull(four.GetNextSibling());

        assertEqual(three, xxx.Parent);
        assertEqual(root, xxx.Root);
        assertEqual(3, four.Depth);
        assertEqual(3, xxx.Depth);
    }

    [TestMethod]
    void TestBigGetAllChildrenWithExpansion()
    {
        // final tree:
        // - root
        //   - one
        //   - two
        //     - three
        //       - XXXXX
        //       - four
        //     - five
        //   - six (collapsed)
        //     - seven
        //     - eight
        //     - nine
        //   - ten
        //   - eleven

        var root = new OptimizedNode("ROOT");
        var one = new OptimizedNode("One");
        var two = new OptimizedNode("Two");
        var three = new OptimizedNode("Three");
        var xxx = new OptimizedNodeSeparator();
        var four = new OptimizedNode("Four");
        var five = new OptimizedNode("Five");
        var six = new OptimizedNode("Six");
        var seven = new OptimizedNode("Seven");
        var eight = new OptimizedNode("Eight");
        var nine = new OptimizedNode("Nine");
        var ten = new OptimizedNode("Ten");
        var eleven = new OptimizedNode("Eleven");

        three.AddChild(four);
        two.AddChild(three);
        two.AddChild(five);

        six.AddChild(seven);
        six.AddChild(eight);
        six.AddChild(nine);

        root.AddChild(one);
        root.AddChild(two);
        root.AddChild(six);
        root.AddChild(ten);
        root.AddChild(eleven);

        three.InsertChild(0, xxx);

        six.SetExpanded(false);

        List<IOptimizedNode> expectedFalse = new List<IOptimizedNode>()
        {
            one, two, three, xxx, four, five, six, ten, eleven
        };

        List<IOptimizedNode> expectedTrue = new List<IOptimizedNode>()
        {
            one, two, three, xxx, four, five, six, seven, eight, nine, ten, eleven
        };

        assertEqual(expectedFalse, root.GetAllChildren(false));
        assertEqual(expectedTrue, root.GetAllChildren(true));
    }
}
