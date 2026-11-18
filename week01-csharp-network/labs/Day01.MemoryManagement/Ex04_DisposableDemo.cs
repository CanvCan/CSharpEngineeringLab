namespace Day01.MemoryManagement;

public class FakeResource : IDisposable
{
    private readonly string _name;
    private bool _disposed = false;

    public FakeResource(string name)
    {
        _name = name;
        Console.WriteLine($"[{_name}] Opened");
    }

    public void DoWork()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(_name);
        }

        Console.WriteLine($"[{_name}] Working...");

    }
    public void Dispose()
    {
        if (_disposed) return;

        Console.WriteLine($"[{_name}] Closed");
        _disposed = true;
    }
}

public class Ex04_DisposableDemo
{
    public static void Run()
    {
        Console.WriteLine("=== SCENARIO 1: WITHOUT CALLING Dispose() ===\n");
        var res1 = new FakeResource("Resource-1");
        res1.DoWork();
        // Notice: "Closed" is NEVER printed here!
        // The resource leaks until GC/Finalizer (if any) runs.

        Console.WriteLine("\n=== SCENARIO 2: MANUALLY CALLING Dispose() ===\n");
        var res2 = new FakeResource("Resource-2");
        res2.DoWork();
        res2.Dispose(); // Works, but error-prone if an exception occurs before this line!

        Console.WriteLine("\n=== SCENARIO 3: USING BLOCK (NORMAL FLOW) ===\n");
        using (var res3 = new FakeResource("Resource-3"))
        {
            res3.DoWork();
        } // res3.Dispose() is automatically invoked right at this closing brace!

        Console.WriteLine("\n=== SCENARIO 4: USING BLOCK WITH AN EXCEPTION ===\n");
        try
        {
            // C# 8+ using declaration: without explicit {},
            // the enclosing block's closing '}' acts as the disposal boundary.
            using var res4 = new FakeResource("Resource-4");
            res4.DoWork();

            Console.WriteLine($"[{nameof(FakeResource)}] Throwing simulated exception...");
            throw new InvalidOperationException("Something went wrong during work!");
            // Despite the exception, res4.Dispose() STILL runs before entering 'catch'!
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught Exception: {ex.Message}");
        }
    }
}
