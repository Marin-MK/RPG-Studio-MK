using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

    public static (int, List<AssertionException>) Run(Type type)
    {
        if (type.BaseType != typeof(TestSuite)) throw new Exception("Cannot run tests on a class that does not inherit from the test suite.");
        object obj = Activator.CreateInstance(type);
        int Count = 0;
        List<AssertionException> Exceptions = new List<AssertionException>();
        foreach (MethodInfo method in obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            TestMethod? attrib = method.GetCustomAttribute<TestMethod>();
            if (attrib != null)
            {
                Action test = (Action) Delegate.CreateDelegate(typeof(Action), obj, method.Name);
                if (attrib.Throw)
                {
                    test();
                    continue;
                }
                try
                {
                    test();
                }
                catch (AssertionException ex)
                {
                    throw;
                    string[] lines = ex.StackTrace.Split('\n');
                    int? line = null;
                    Match m = Regex.Match(lines[0], @":line (\d+)");
                    if (m.Success)
                    {
                        line = Convert.ToInt32(m.Groups[1].Value);
                    }
                    string linetext = line != null ? $" line {line}" : "";
                    Logger.Error($"Assertion failed ({method.Name}(){linetext}): {ex.Message}");
                    Exceptions.Add(ex);
                }
                Count++;
            }
        }
        return (Count, Exceptions);
    }

    public static (int, List<AssertionException>) Run<T>() where T : TestSuite
    {
        return Run(typeof(T));
    }

    public static void RunAll(bool ThrowIfAnyErrors = true)
    {
        Stopwatch s = Stopwatch.StartNew();
        int Count = 0;
        List<AssertionException> Exceptions = new List<AssertionException>();
        Logger.WriteLine("Running tests...");
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.BaseType == typeof(TestSuite))
            {
                if (type.GetCustomAttribute<DisableTestClass>() != null) continue;
                (int, List<AssertionException>) result = Run(type);
                Count += result.Item1;
                Exceptions.AddRange(result.Item2);
            }
        }
        if (Count > 0)
        {
            float perc = (int) Math.Round((float) (Count - Exceptions.Count) / Count * 1000) / 10f;
            if (Exceptions.Count > 0) Logger.WriteLine();
            Logger.WriteLine($"Ran {Count} tests, {Count - Exceptions.Count}/{Count} ({perc}%) passed ({s.ElapsedMilliseconds}ms).");
            Logger.WriteLine();
            if (ThrowIfAnyErrors && Exceptions.Count > 0)
            {
                bool Plural = Exceptions.Count > 1;
                throw new AssertionException($"There {(Plural ? "were" : "was")} {Exceptions.Count}/{Count} failing test{(Plural ? "s" : "")}.");
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class TestMethod : Attribute
{
    public bool Throw { get; protected set; }

    public TestMethod(bool Throw = false)
    {
        this.Throw = Throw;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class DisableTestClass : Attribute
{

}

public class AssertionException : Exception
{
    public AssertionException(string Message) : base(Message) { }
}

public class AssertEqualsException : AssertionException
{
    public AssertEqualsException(object expected, object actual) : base($"Expected result to be '{expected}', but got '{actual}'.") { }
}

public class AssertEqualsListException<T> : AssertionException
{
    public AssertEqualsListException(List<T> expected, List<T> actual)
        : base($"Expected result to be:\n{ToStr(expected)}\nBut got:\n{ToStr(actual)}") { }

    public AssertEqualsListException(List<T> expected, List<T> actual, int index, T ex, T ac)
        : base($"Expected result to be:\n{ToStr(expected)}\nBut got:\n{ToStr(actual)}\n\nElement {index} was '{ex}' but was expected to be '{ac}'.") { }

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

public class AssertNullException : AssertionException
{
    public AssertNullException(object result) : base($"Expected result to be null, but got '{result}'.") { }
}

public class AssertNotNullException : AssertionException
{
    public AssertNotNullException() : base($"Expected result to not be null, but the result was null.") { }
}

public class AssertTrueException : AssertionException
{
    public AssertTrueException(object result) : base($"Expected result to be true, but got '{result}'.") { }
}

public class AssertFalseException : AssertionException
{
    public AssertFalseException(object result) : base($"Expected result to be false, but got '{result}'.") { }
}