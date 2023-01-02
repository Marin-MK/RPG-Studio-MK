using RPGStudioMK.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace RPGStudioMK;

public class TestSuite
{
    public TestSuite()
    {

    }

    protected void assertEqual(int expected, int actual)
    {
        if (expected != actual) throw new Exception($"AssertionException: Expected value '{expected}' is not equivalent to actual value '{actual}'.");
    }

    protected void assertEqual(OptimizedNode? expected, OptimizedNode? actual)
    {
        if (expected != actual) throw new Exception($"AssertionException: Expected node '{expected?.Text}' is not equivalent to actual node '{actual?.Text}'");
    }

    protected void assertNull(OptimizedNode? node)
    {
        if (node != null) throw new Exception($"AssertionException: Expected node '{node?.Text}' to be null.");
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
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.BaseType == typeof(TestSuite))
            {
                Run(type);
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class TestMethod : Attribute
{

}