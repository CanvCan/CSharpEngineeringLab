using System.Diagnostics;

namespace Day03.ThreadsAndSync;

public static class Ex04_InterlockedAndBenchmark
{
    private static int _counter = 0;
    private static readonly object _lockObj = new();

    public static void Run()
    {
        const int iterations = 2_000_000;
        const int expected = iterations * 2;

        PrintSection("1. Benchmark: Unsafe vs. lock vs. Interlocked");

        // TODO 1: Measure 3 approaches with 2 threads each doing 'iterations' increments:
        //   A) Unsafe: _counter++ (fast, but WRONG result!)
        //   B) lock: lock (_lockObj) { _counter++; }
        //   C) Interlocked: Interlocked.Increment(ref _counter);
        // Use Stopwatch for each. Reset _counter = 0 before each test.
        // Print a clean comparison table showing Method, Result, Expected, and Elapsed ms.

        Console.WriteLine($"{"Method",-15} | {"Actual",12} | {"Expected",12} | {"Lost",10} | {"Elapsed (ms)",12}");
        Console.WriteLine(new string('-', 73));

        _counter = 0;
        var sw = new Stopwatch();

        sw.Restart();

        Thread thread1 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++) { _counter++; }
        }
        );

        Thread thread2 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++) { _counter++; }
        }
        );

        thread1.Start();
        thread2.Start();

        thread1.Join();
        thread2.Join();

        sw.Stop();

        int lostUpdates = expected - _counter;
        Console.WriteLine($"{"Unsafe",-15} | {_counter,12:N0} | {expected,12:N0} | {lostUpdates,10:N0} | {sw.Elapsed.TotalMilliseconds,12:F2}");

        _counter = 0;
        sw.Restart();

        Thread thread3 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++) { lock (_lockObj) { _counter++; } }
        }
        );

        Thread thread4 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++) { lock (_lockObj) { _counter++; } }
        }
        );

        thread3.Start();
        thread4.Start();

        thread3.Join();
        thread4.Join();

        sw.Stop();

        lostUpdates = expected - _counter;
        Console.WriteLine($"{"Lock",-15} | {_counter,12:N0} | {expected,12:N0} | {lostUpdates,10:N0} | {sw.Elapsed.TotalMilliseconds,12:F2}");

        _counter = 0;
        sw.Restart();

        Thread thread5 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++) { Interlocked.Increment(ref _counter); }
        }
        );

        Thread thread6 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++) { Interlocked.Increment(ref _counter); }
        }
        );

        thread5.Start();
        thread6.Start();

        thread5.Join();
        thread6.Join();

        sw.Stop();

        lostUpdates = expected - _counter;
        Console.WriteLine($"{"Interlocked",-15} | {_counter,12:N0} | {expected,12:N0} | {lostUpdates,10:N0} | {sw.Elapsed.TotalMilliseconds,12:F2}");

        PrintSection("2. Interlocked.Add & Interlocked.Exchange");

        int totalScore = 0;

        // TODO 2: Start 4 threads. Each thread adds 100 to 'totalScore' 10,000 times
        // using Interlocked.Add(ref totalScore, 100). Join all 4 threads.
        // Print totalScore (expect: 4,000,000).

        Thread t1 = new Thread(() =>
        {
            for (int i = 0; i < 10_000; i++) { Interlocked.Add(ref totalScore, 100); }
        });

        Thread t2 = new Thread(() =>
        {
            for (int i = 0; i < 10_000; i++) { Interlocked.Add(ref totalScore, 100); }
        });

        Thread t3 = new Thread(() =>
        {
            for (int i = 0; i < 10_000; i++) { Interlocked.Add(ref totalScore, 100); }
        });

        Thread t4 = new Thread(() =>
        {
            for (int i = 0; i < 10_000; i++) { Interlocked.Add(ref totalScore, 100); }
        });

        t1.Start();
        t2.Start();
        t3.Start();
        t4.Start();

        t1.Join();
        t2.Join();
        t3.Join();
        t4.Join();

        lostUpdates = 4_000_000 - totalScore;
        Console.WriteLine($"Expected = {expected:N0} | Actual = {totalScore:N0} | Lost = {lostUpdates:N0}\n");

        // TODO 3: Use Interlocked.Exchange(ref totalScore, 0) to atomically reset totalScore to 0
        // and capture the previous value in 'int previousScore'.
        // Print both previousScore and the new totalScore.

        int previousScore = Interlocked.Exchange(ref totalScore, 0);
        Console.WriteLine($"Previous score: {previousScore} | Current score: {totalScore}");

        PrintSection("3. Compare-And-Swap (Interlocked.CompareExchange)");

        int configVersion = 1;

        // TODO 4: Try to update configVersion from 1 -> 2 using:
        // int original1 = Interlocked.CompareExchange(ref configVersion, 2, 1);
        // Print original1 and current configVersion (should succeed: 1 -> 2).
        //
        // Now try again with a WRONG expected value (expecting 1, trying to set 99):
        // int original2 = Interlocked.CompareExchange(ref configVersion, 99, 1);
        // Print original2 and current configVersion (should FAIL and stay 2!).

        int original1 = Interlocked.CompareExchange(ref configVersion, 2, 1);
        Console.WriteLine($"original1: {original1} | Current configVersion: {configVersion}");

        int original2 = Interlocked.CompareExchange(ref configVersion, 99, 1);
        Console.WriteLine($"original2: {original2} | Current configVersion: {configVersion}");

        Console.WriteLine("\n[Done] Ex04 finished.");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}