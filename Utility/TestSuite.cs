using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace RPGStudioMK;

public class TestSuite
{
    public TestSuite()
    {
        
    }

    protected void assertEqual(int expected, int actual)
    {
        if (expected != actual) throw new AssertEqualsException(expected, actual);
    }

    protected void assertEqual(object? expected, object? actual)
    {
        if (!expected.Equals(actual)) throw new AssertEqualsException(expected, actual);
    }

    protected void assertEqual<T>(List<T> expected, List<T> actual)
    {
        if (expected.Count != actual.Count) throw new AssertEqualsListException<T>(expected, actual);
        for (int i = 0; i < expected.Count; i++)
        {
            T ex = expected[i];
            T ac = actual[i];
            if (!ex.Equals(ac)) throw new AssertEqualsListException<T>(expected, actual, i, ex, ac);
        }
    }

    protected void assertNull(object? node)
    {
        if (node != null) throw new AssertNullException(node);
    }

    protected void assertNotNull(object? node)
    {
        if (node == null) throw new AssertNotNullException();
    }

    protected void assertTrue(object? obj)
    {
        if (obj is not bool || (bool) obj != true) throw new AssertTrueException(obj);
    }

    protected void assertFalse(object? obj)
    {
        if (obj is not bool || (bool) obj != false) throw new AssertFalseException(obj);
    }

    public static void Run(Type type)
    {
        if (type.BaseType != typeof(TestSuite)) throw new Exception("Cannot run tests on a class that does not inherit from the test suite.");
        object obj = Activator.CreateInstance(type);
        foreach (MethodInfo method in obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            TestMethod? attrib = method.GetCustomAttribute<TestMethod>();
            if (attrib != null)
            {
                Console.WriteLine($"Running {type.Name}.{method.Name}...");
                method.Invoke(obj, null);
            }
        }
    }

    public static void Run<T>() where T : TestSuite
    {
        Run(typeof(T));
    }

    public static void RunAll()
    {
        bool RanAny = false;
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.BaseType == typeof(TestSuite))
            {
                if (type.GetCustomAttribute<DisableTestClass>() != null) continue;
                Run(type);
                RanAny = true;
            }
        }
        if (RanAny) Console.WriteLine();
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class TestMethod : Attribute
{

}

[AttributeUsage(AttributeTargets.Class)]
public class DisableTestClass : Attribute
{

}

public class AssertEqualsException : Exception
{
    public AssertEqualsException(object expected, object actual) : base($"Expected result to be '{expected}', but got '{actual}'.") { }
}

public class AssertEqualsListException<T> : Exception
{
    public AssertEqualsListException(List<T> expected, List<T> actual)
        : base($"Expected result to be:\n{ToStr(expected)}\n\nBut got:\n{ToStr(actual)}") { }

    public AssertEqualsListException(List<T> expected, List<T> actual, int index, T ex, T ac)
        : base($"Expected result to be:\n{ToStr(expected)}\n\nBut got:\n{ToStr(actual)}\n\nElement {index} was '{ex}' but was expected to be '{ac}'.") { }

    static string ToStr(List<T> list)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < list.Count; i++)
        {
            sb.Append(list[i].ToString());
            if (i < list.Count - 1) sb.Append(", ");
        }
        sb.Append("]");
        return sb.ToString();
    }
}

public class AssertNullException : Exception
{
    public AssertNullException(object result) : base($"Expected result to be null, but got '{result}'.") { }
}

public class AssertNotNullException : Exception
{
    public AssertNotNullException() : base($"Expected result to not be null, but the result was null.") { }
}

public class AssertTrueException : Exception
{
    public AssertTrueException(object result) : base($"Expected result to be true, but got '{result}'.") { }
}

public class AssertFalseException : Exception
{
    public AssertFalseException(object result) : base($"Expected result to be false, but got '{result}'.") { }
}