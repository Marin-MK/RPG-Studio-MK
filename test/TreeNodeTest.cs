using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGStudioMK.Widgets;

namespace RPGStudioMK.Tests;

//[DisableTestClass]
public class TreeNodeTest : TestSuite
{
    [TestMethod]
    void TestNested()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //     - four
        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
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
        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");

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

        var root = new TreeNode("ROOT");
        var na = new TreeNode("A");
        var nb = new TreeNode("B");
        var nc = new TreeNode("C");
        var nd = new TreeNode("D");

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

        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        var eight = new TreeNode("Eight");
        var nine = new TreeNode("Nine");
        var ten = new TreeNode("Ten");
        var eleven = new TreeNode("Eleven");
        var twelve = new TreeNode("Twelve");
        var thirteen = new TreeNode("Thirteen");
        var fourteen = new TreeNode("Fourteen");

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

        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        var eight = new TreeNode("Eight");
        var nine = new TreeNode("Nine");
        var ten = new TreeNode("Ten");
        var eleven = new TreeNode("Eleven");
        var twelve = new TreeNode("Twelve");
        var thirteen = new TreeNode("Thirteen");
        var fourteen = new TreeNode("Fourteen");

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

        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        var eight = new TreeNode("Eight");
        var nine = new TreeNode("Nine");
        var ten = new TreeNode("Ten");
        var eleven = new TreeNode("Eleven");
        var twelve = new TreeNode("Twelve");
        var thirteen = new TreeNode("Thirteen");
        var fourteen = new TreeNode("Fourteen");

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
        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var sep = new TreeNodeSeparator();
        var three = new TreeNode("Three");

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
        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var sep = new TreeNodeSeparator();

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
        var one = new TreeNode("One");
        var sep = new TreeNodeSeparator();
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");

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

        var root = new TreeNode("ROOT");
        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var xxx = new TreeNodeSeparator();
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        var eight = new TreeNode("Eight");
        var nine = new TreeNode("Nine");
        var ten = new TreeNode("Ten");
        var eleven = new TreeNode("Eleven");

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
    void TestDeepTreeNextPrev()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //       - four
        //   - five
        //     - six
        //       - seven
        //         - eight
        //     - nine
        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        var eight = new TreeNode("Eight");
        var nine = new TreeNode("Nine");

        three.AddChild(four);
        two.AddChild(three);
        one.AddChild(two);

        seven.AddChild(eight);
        six.AddChild(seven);
        five.AddChild(six);
        five.AddChild(nine);
        one.AddChild(five);

        assertEqual(nine, one.GetLastNode());
        assertEqual(four, two.GetLastNode());
        assertEqual(four, three.GetLastNode());
        assertEqual(four, four.GetLastNode());
        assertEqual(nine, five.GetLastNode());
        assertEqual(eight, six.GetLastNode());
        assertEqual(eight, seven.GetLastNode());
        assertEqual(eight, eight.GetLastNode());
        assertEqual(nine, nine.GetLastNode());

        assertEqual(two, one.GetNextNode());
        assertNull(one.GetNextSibling());
        assertNull(one.GetPreviousNode());
        assertNull(one.GetPreviousSibling());

        assertEqual(three, two.GetNextNode());
        assertEqual(five, two.GetNextSibling());
        assertEqual(one, two.GetPreviousNode());
        assertNull(two.GetPreviousSibling());

        assertEqual(four, three.GetNextNode());
        assertNull(three.GetNextSibling());
        assertEqual(two, three.GetPreviousNode());
        assertNull(three.GetPreviousSibling());

        assertEqual(five, four.GetNextNode());
        assertNull(four.GetNextSibling());
        assertEqual(three, four.GetPreviousNode());
        assertNull(four.GetPreviousSibling());

        assertEqual(six, five.GetNextNode());
        assertNull(five.GetNextSibling());
        assertEqual(four, five.GetPreviousNode());
        assertEqual(two, five.GetPreviousSibling());

        assertEqual(seven, six.GetNextNode());
        assertEqual(nine, six.GetNextSibling());
        assertEqual(five, six.GetPreviousNode());
        assertNull(six.GetPreviousSibling());

        assertEqual(eight, seven.GetNextNode());
        assertNull(seven.GetNextSibling());
        assertEqual(six, seven.GetPreviousNode());
        assertNull(seven.GetPreviousSibling());

        assertEqual(nine, eight.GetNextNode());
        assertNull(eight.GetNextSibling());
        assertEqual(seven, eight.GetPreviousNode());
        assertNull(eight.GetPreviousSibling());

        assertNull(nine.GetNextNode());
        assertNull(nine.GetNextSibling());
        assertEqual(eight, nine.GetPreviousNode());
        assertEqual(six, nine.GetPreviousSibling());

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(3, four.GlobalIndex);
        assertEqual(4, five.GlobalIndex);
        assertEqual(5, six.GlobalIndex);
        assertEqual(6, seven.GlobalIndex);
        assertEqual(7, eight.GlobalIndex);
        assertEqual(8, nine.GlobalIndex);

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

        var root = new TreeNode("ROOT");
        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var xxx = new TreeNodeSeparator();
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        var eight = new TreeNode("Eight");
        var nine = new TreeNode("Nine");
        var ten = new TreeNode("Ten");
        var eleven = new TreeNode("Eleven");

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

        List<ITreeNode> expectedFalse = new List<ITreeNode>()
        {
            one, two, three, xxx, four, five, six, ten, eleven
        };

        List<ITreeNode> expectedTrue = new List<ITreeNode>()
        {
            one, two, three, xxx, four, five, six, seven, eight, nine, ten, eleven
        };

        assertEqual(expectedFalse, root.GetAllChildren(false));
        assertEqual(expectedTrue, root.GetAllChildren(true));
    }

    [TestMethod]
    void TestDelete()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //       - four
        //       - five
        //   - six
        //   - seven
        //   - eight
        //     - nine

        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        var eight = new TreeNode("Eight");
        var nine = new TreeNode("Nine");

        three.AddChild(four);
        three.AddChild(five);

        two.AddChild(three);

        eight.AddChild(nine);

        one.AddChild(two);
        one.AddChild(six);
        one.AddChild(seven);
        one.AddChild(eight);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(3, four.GlobalIndex);
        assertEqual(4, five.GlobalIndex);
        assertEqual(5, six.GlobalIndex);
        assertEqual(6, seven.GlobalIndex);
        assertEqual(7, eight.GlobalIndex);
        assertEqual(8, nine.GlobalIndex);

        // final tree:
        // - one
        //   - two
        //     - three
        //       - four
        //   - six
        //   - seven
        //   - eight
        //     - nine

        five.Delete(true);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(3, four.GlobalIndex);
        assertEqual(4, six.GlobalIndex);
        assertEqual(5, seven.GlobalIndex);
        assertEqual(6, eight.GlobalIndex);
        assertEqual(7, nine.GlobalIndex);

        assertNull(four.GetNextSibling());
        assertEqual(six, four.GetNextNode());
        assertEqual(two, six.GetPreviousSibling());
        assertEqual(four, six.GetPreviousNode());

        // final tree:
        // - one
        //   - two
        //   - six
        //   - seven
        //   - eight
        //     - nine

        three.Delete(true);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, six.GlobalIndex);
        assertEqual(3, seven.GlobalIndex);
        assertEqual(4, eight.GlobalIndex);
        assertEqual(5, nine.GlobalIndex);

        assertEqual(two, six.GetPreviousNode());
        assertEqual(two, six.GetPreviousSibling());
        assertEqual(six, two.GetNextNode());
        assertEqual(six, two.GetNextSibling());
    }

    [TestMethod]
    void TestDeleteFlatten()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //       - four
        //       - five
        //   - six
        //   - seven
        //   - eight
        //     - nine

        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        var eight = new TreeNode("Eight");
        var nine = new TreeNode("Nine");

        three.AddChild(four);
        three.AddChild(five);

        two.AddChild(three);

        eight.AddChild(nine);

        one.AddChild(two);
        one.AddChild(six);
        one.AddChild(seven);
        one.AddChild(eight);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(3, four.GlobalIndex);
        assertEqual(4, five.GlobalIndex);
        assertEqual(5, six.GlobalIndex);
        assertEqual(6, seven.GlobalIndex);
        assertEqual(7, eight.GlobalIndex);
        assertEqual(8, nine.GlobalIndex);

        // final tree:
        // - one
        //   - two
        //     - four
        //     - five
        //   - six
        //   - seven
        //   - eight
        //     - nine

        three.Delete(false);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, four.GlobalIndex);
        assertEqual(3, five.GlobalIndex);
        assertEqual(4, six.GlobalIndex);
        assertEqual(5, seven.GlobalIndex);
        assertEqual(6, eight.GlobalIndex);
        assertEqual(7, nine.GlobalIndex);

        assertEqual(two, four.Parent);
        assertEqual(two, five.Parent);
        assertEqual(six, two.GetNextSibling());
        assertEqual(four, two.GetNextNode());
        assertEqual(five, four.GetNextNode());
        assertEqual(six, five.GetNextNode());
        assertEqual(four, five.GetPreviousSibling());
        assertEqual(five, four.GetNextSibling());

        // final tree:
        // - one
        //   - six
        //   - seven
        //   - eight
        //     - nine

        two.Delete(true);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, six.GlobalIndex);
        assertEqual(2, seven.GlobalIndex);
        assertEqual(3, eight.GlobalIndex);
        assertEqual(4, nine.GlobalIndex);

        assertEqual(one, six.GetPreviousNode());
        assertNull(six.GetPreviousSibling());
        assertEqual(seven, six.GetNextNode());
        assertEqual(seven, six.GetNextSibling());
        assertEqual(six, one.GetNextNode());

        // final tree:
        // - one
        //   - six
        //   - seven
        //   - nine

        eight.Delete(false);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, six.GlobalIndex);
        assertEqual(2, seven.GlobalIndex);
        assertEqual(3, nine.GlobalIndex);

        assertEqual(one, nine.Parent);
        assertEqual(seven, nine.GetPreviousNode());
        assertEqual(seven, nine.GetPreviousSibling());
        assertEqual(nine, seven.GetNextNode());
        assertEqual(nine, seven.GetNextSibling());
    }

    [TestMethod]
    void TestBigFlatten()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //       - four
        //     - five
        //   - six
        //     - seven

        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        
        three.AddChild(four);

        two.AddChild(three);
        two.AddChild(five);

        six.AddChild(seven);

        one.AddChild(two);
        one.AddChild(six);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(3, four.GlobalIndex);
        assertEqual(4, five.GlobalIndex);
        assertEqual(5, six.GlobalIndex);
        assertEqual(6, seven.GlobalIndex);

        // final tree:
        // - one
        //   - three
        //     - four
        //   - five
        //   - six
        //     - seven

        two.Delete(false);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, three.GlobalIndex);
        assertEqual(2, four.GlobalIndex);
        assertEqual(3, five.GlobalIndex);
        assertEqual(4, six.GlobalIndex);
        assertEqual(5, seven.GlobalIndex);

        assertEqual(one, three.Parent);
        assertEqual(three, four.Parent);
        assertEqual(one, five.Parent);
        assertEqual(one, six.Parent);

        assertEqual(three, one.GetNextNode());
        assertEqual(one, three.GetPreviousNode());
        assertEqual(four, three.GetNextNode());
        assertEqual(three, four.GetPreviousNode());
        assertEqual(five, four.GetNextNode());
        assertEqual(four, five.GetPreviousNode());
        assertEqual(six, five.GetNextNode());
        assertEqual(five, six.GetPreviousNode());
        assertEqual(seven, six.GetNextNode());
        assertEqual(six, seven.GetPreviousNode());

        assertEqual(five, three.GetNextSibling());
        assertNull(three.GetPreviousSibling());
        assertEqual(six, five.GetNextSibling());
        assertEqual(three, five.GetPreviousSibling());
        assertNull(six.GetNextSibling());
        assertEqual(five, six.GetPreviousSibling());
    }

    [TestMethod]
    void TestBigDelete()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //       - four
        //     - five
        //   - six
        //     - seven

        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");

        three.AddChild(four);

        two.AddChild(three);
        two.AddChild(five);

        six.AddChild(seven);

        one.AddChild(two);
        one.AddChild(six);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, two.GlobalIndex);
        assertEqual(2, three.GlobalIndex);
        assertEqual(3, four.GlobalIndex);
        assertEqual(4, five.GlobalIndex);
        assertEqual(5, six.GlobalIndex);
        assertEqual(6, seven.GlobalIndex);

        // final tree:
        // - one
        //   - six
        //     - seven

        two.Delete(true);

        assertEqual(0, one.GlobalIndex);
        assertEqual(1, six.GlobalIndex);
        assertEqual(2, seven.GlobalIndex);

        assertEqual(one, six.Parent);
        assertEqual(six, seven.Parent);

        assertEqual(six, one.GetNextNode());
        assertEqual(seven, six.GetNextNode());
        assertNull(seven.GetNextNode());

        assertNull(one.GetPreviousNode());
        assertEqual(one, six.GetPreviousNode());
        assertEqual(six, seven.GetPreviousNode());

        assertNull(one.GetNextSibling());
        assertNull(one.GetPreviousSibling());
        assertNull(six.GetNextSibling());
        assertNull(six.GetPreviousSibling());
        assertNull(seven.GetNextSibling());
        assertNull(seven.GetPreviousSibling());
    }

    [TestMethod]
    void TestSeveralDeletes()
    {
        // final tree:
        // - root
        //   - one
        //     - two
        //       - three
        //       - four
        //         - five
        //       - six
        //     - seven
        //       - eight
        //       - nine
        //         - ten
        //         - eleven
        //       - twelve
        //     - thirteen
        //       - fourteen
        //         - abc
        //         - def
        //           - ghi
        //             - jkl
        //           - mno
        //     - pqr
        //   - XXX8
        //   - fifteen
        //   - XXX32
        //   - sixteen

        var root = new TreeNode("Root");
        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");
        var eight = new TreeNode("Eight");
        var nine = new TreeNode("Nine");
        var ten = new TreeNode("Ten");
        var eleven = new TreeNode("Eleven");
        var twelve = new TreeNode("Twelve");
        var thirteen = new TreeNode("Thirteen");
        var fourteen = new TreeNode("Fourteen");
        var fifteen = new TreeNode("Fifteen");
        var sixteen = new TreeNode("Sixteen");
        var seventeen = new TreeNode("Seventeen");
        var abc = new TreeNode("abc");
        var def = new TreeNode("def");
        var ghi = new TreeNode("ghi");

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
        fourteen.AddChild(abc);
        fourteen.AddChild(def);
        def.AddChild(ghi);
        ghi.AddChild(new TreeNode("jkl"));
        def.AddChild(new TreeNode("mno"));

        one.AddChild(two);
        one.AddChild(seven);
        one.AddChild(thirteen);
        one.AddChild(new TreeNode("pqr"));

        sixteen.AddChild(seventeen);

        root.AddChild(one);
        root.AddChild(new TreeNodeSeparator(8));
        root.AddChild(fifteen);
        root.AddChild(new TreeNodeSeparator(32));
        root.AddChild(sixteen);

        assertEqual(1, one.GlobalIndex);
        assertEqual(2, two.GlobalIndex);
        assertEqual(3, three.GlobalIndex);
        assertEqual(4, four.GlobalIndex);
        assertEqual(5, five.GlobalIndex);
        assertEqual(6, six.GlobalIndex);
        assertEqual(7, seven.GlobalIndex);
        assertEqual(8, eight.GlobalIndex);

        assertEqual(one, two.Parent);
        assertEqual(two, three.Parent);
        assertEqual(two, four.Parent);
        assertEqual(four, five.Parent);
        assertEqual(two, six.Parent);
        assertEqual(one, seven.Parent);
        assertEqual(seven, eight.Parent);

        // final tree:
        // - root
        //   - one
        //     - two
        //       - three
        //       - four
        //         - five <<< DELETE
        //       - six
        //     - seven
        //       - eight

        five.Delete(true);

        assertEqual(1, one.GlobalIndex);
        assertEqual(2, two.GlobalIndex);
        assertEqual(3, three.GlobalIndex);
        assertEqual(4, four.GlobalIndex);
        assertEqual(5, six.GlobalIndex);
        assertEqual(6, seven.GlobalIndex);
        assertEqual(7, eight.GlobalIndex);

        assertEqual(one, two.Parent);
        assertEqual(two, three.Parent);
        assertEqual(two, four.Parent);
        assertEqual(two, six.Parent);
        assertEqual(one, seven.Parent);
        assertEqual(seven, eight.Parent);
        assertEqual(six, four.GetNextNode());
        assertEqual(four, six.GetPreviousNode());
        assertEqual(six, four.GetNextSibling());
        assertEqual(four, six.GetPreviousSibling());
    }

    [TestMethod]
    void TestAncestors()
    {
        // final tree:
        // - one
        //   - two
        //     - three
        //       - four
        //     - five
        //   - six
        //     - seven

        var one = new TreeNode("One");
        var two = new TreeNode("Two");
        var three = new TreeNode("Three");
        var four = new TreeNode("Four");
        var five = new TreeNode("Five");
        var six = new TreeNode("Six");
        var seven = new TreeNode("Seven");

        three.AddChild(four);

        two.AddChild(three);
        two.AddChild(five);

        six.AddChild(seven);

        one.AddChild(two);
        one.AddChild(six);

        assertEqual(new List<TreeNode>() { one, two, three }, four.GetAncestors());
        assertEqual(new List<TreeNode>() { one, two }, three.GetAncestors());
        assertEqual(new List<TreeNode>() { one }, two.GetAncestors());
        assertEqual(new List<TreeNode>() { }, one.GetAncestors());

        assertEqual(new List<TreeNode>() { one, two }, five.GetAncestors());
        assertEqual(new List<TreeNode>() { one, six }, seven.GetAncestors());
        assertEqual(new List<TreeNode>() { one }, six.GetAncestors());
    }
}
