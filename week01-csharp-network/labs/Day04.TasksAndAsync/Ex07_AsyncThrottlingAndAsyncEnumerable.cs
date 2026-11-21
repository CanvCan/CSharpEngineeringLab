using System.Diagnostics;

namespace Day04.TasksAndAsync;

public static class Ex07_AsyncThrottlingAndAsyncEnumerable
{
    public static async Task RunAsync()
    {
        PrintSection("1. Why lock + await Fails (CS1996)");

        // TODO 1: Uncomment the 3 lines below briefly to see the exact compiler error (CS1996),
        // read the error message, then comment it back out and note WHY it is forbidden.
        //
        // object lockObj = new();
        // lock (lockObj) { await Task.Delay(100); }
        Console.WriteLine("CS1996 verified: Cannot await inside a lock block due to thread affinity.");


        PrintSection("2. Async Throttling with SemaphoreSlim.WaitAsync (20 Jobs, Max 3 Concurrent)");

        // TODO 2: Create: using var sem = new SemaphoreSlim(initialCount: 3, maxCount: 3);
        // Start a Stopwatch.
        // Write a local async function (or helper method): async Task ProcessJobAsync(int id)
        //   - await sem.WaitAsync();
        //   - try
        //     {
        //         Print $"[{DateTime.Now:HH:mm:ss.fff}] Job-{id:D2} STARTED (Free slots: {sem.CurrentCount})"
        //         await Task.Delay(1000);
        //         Print $"[{DateTime.Now:HH:mm:ss.fff}] Job-{id:D2} FINISHED"
        //     }
        //     finally { sem.Release(); }
        //
        // Launch 20 jobs concurrently:
        //   var tasks = Enumerable.Range(1, 20).Select(ProcessJobAsync);
        //   await Task.WhenAll(tasks);
        // Stop Stopwatch and print total elapsed time.
        // Observe: 20 jobs / 3 concurrent * 1s ≈ 7 seconds (ceil(20/3) = 7 waves).

        using var sem = new SemaphoreSlim(initialCount: 3, maxCount: 3);

        Stopwatch sw = Stopwatch.StartNew();

        async Task ProcessJobAsync(int id)
        {
            await sem.WaitAsync();
            try
            {
                PrintColored(
                    $"[{DateTime.Now:HH:mm:ss.fff}] Job-{id:D2} " +
                    $"STARTED (Free slots: {sem.CurrentCount})", ConsoleColor.Green);

                await Task.Delay(1000);

                PrintColored($"[{DateTime.Now:HH:mm:ss.fff}] Job-{id:D2} FINISHED", ConsoleColor.Red);
            }
            finally
            {
                sem.Release();
            }
        }

        var tasks = Enumerable.Range(1, 20).Select(ProcessJobAsync);
        await Task.WhenAll(tasks);

        sw.Stop();
        double elapsedMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine($"\nElapsed: {elapsedMs:F2} ms");


        PrintSection("3. Streaming Data with IAsyncEnumerable<T> and await foreach");

        // TODO 3: Implement the GenerateNumbersAsync(int count) method below.
        // Consume it here using:
        //   await foreach (int num in GenerateNumbersAsync(5))
        //   {
        //       Console.WriteLine($"  [Consumer] Received {num} at {DateTime.Now:HH:mm:ss.fff}");
        //   }
        // Observe: Items arrive one by one every 500ms instead of waiting 2.5s for a full List<int>.

        await foreach (int num in GenerateNumbersAsync(5))
        {
            Console.WriteLine($"  [Consumer] Received {num} at {DateTime.Now:HH:mm:ss.fff}");
        }

        Console.WriteLine("\n[Done] Ex07 finished. Day 04 Complete!");
    }

    // TODO 3 (Helper): Implement this async iterator:
    // private static async IAsyncEnumerable<int> GenerateNumbersAsync(int count)
    // {
    //     for (int i = 1; i <= count; i++)
    //     {
    //         await Task.Delay(500);
    //         Console.WriteLine($"[Producer] Yielding {i} at {DateTime.Now:HH:mm:ss.fff}");
    //         yield return i;
    //     }
    // }

    private static async IAsyncEnumerable<int> GenerateNumbersAsync(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            await Task.Delay(500);
            Console.WriteLine($"[Producer] Yielding {i} at {DateTime.Now:HH:mm:ss.fff}");
            yield return i;
        }
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }

    private static readonly Lock ConsoleLock = new();

    private static void PrintColored(string message, ConsoleColor color = ConsoleColor.White)
    {
        lock (ConsoleLock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}