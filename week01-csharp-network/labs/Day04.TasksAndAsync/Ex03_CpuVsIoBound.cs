using System.Diagnostics;

namespace Day04.TasksAndAsync;

public static class Ex03_CpuVsIoBound
{
    public static async Task RunAsync()
    {
        const int loopLimit = 500_000_000;

        PrintSection("1. CPU-Bound Directly on Calling Thread (Synchronous)");

        // TODO 1: Print current ManagedThreadId BEFORE calling HeavyCalculation(loopLimit).
        // Start a Stopwatch, call long sum1 = HeavyCalculation(loopLimit); directly.
        // Stop Stopwatch and print: sum1, elapsed ms, and ManagedThreadId AFTER.
        // Observe: The calling thread did all the work itself (ID stayed the same, thread was busy).

        Console.WriteLine($"[Before Calling Function] Current Thread ID: {Environment.CurrentManagedThreadId}");

        Stopwatch sw = Stopwatch.StartNew();

        long sum1 = HeavyCalculation(loopLimit);

        sw.Stop();
        double elapsedMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine(
            $"[After Calling Function] Current Thread ID: {Environment.CurrentManagedThreadId} " +
            $"| Sum: {sum1} " +
            $"| Elapsed: {elapsedMs:F2} ms");


        PrintSection("2. CPU-Bound Offloaded with await Task.Run(...)");

        // TODO 2: Print current ManagedThreadId BEFORE await Task.Run.
        // Start a Stopwatch and offload the work:
        // long sum2 = await Task.Run(() => HeavyCalculation(loopLimit));
        // Stop Stopwatch and print: sum2, elapsed ms, and ManagedThreadId AFTER.
        // Observe: HeavyCalculation ran on a ThreadPool worker thread, freeing the calling thread!

        Console.WriteLine($"[Before await Task.Run()] Current Thread ID: {Environment.CurrentManagedThreadId}");

        sw.Restart();

        long sum2 = await Task.Run(() => HeavyCalculation(loopLimit));

        sw.Stop();

        elapsedMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine(
            $"[After await Task.Run()] Current Thread ID: {Environment.CurrentManagedThreadId} " +
            $"| Sum: {sum2} " +
            $"| Elapsed: {elapsedMs:F2} ms");


        PrintSection("3. Anti-Pattern: Wrapping Blocking Wait in Task.Run vs. Pure Async I/O");

        // TODO 3 (BAD - Anti-Pattern):
        // Simulate a fake async I/O by doing:
        // await Task.Run(() =>
        // {
        //     Console.WriteLine($"[Bad] Blocking ThreadPool Thread {Environment.CurrentManagedThreadId}...");
        //     Thread.Sleep(1000);
        // });
        // Add a comment explaining WHY this is bad:
        // (Wastes a ThreadPool thread sleeping for 1s; under load causes ThreadPool starvation like Ex01 TODO 5.)

        await Task.Run(() =>
        {
            Console.WriteLine($"[Bad] Blocking ThreadPool Thread {Environment.CurrentManagedThreadId}...\n");
            Thread.Sleep(1000);
        });


        // TODO 4 (GOOD - Pure I/O-Bound):
        // Print current ManagedThreadId, call await Task.Delay(1000), print ManagedThreadId again.
        // Add a comment with the golden rule: "I/O-bound -> pure await | CPU-bound -> await Task.Run".

        Console.WriteLine($"[Before pure await] Current Thread ID: {Environment.CurrentManagedThreadId}");

        await Task.Delay(1000);

        Console.WriteLine($"[After pure await] Current Thread ID: {Environment.CurrentManagedThreadId}");


        Console.WriteLine("\n[Done] Ex03 finished.");
    }

    private static long HeavyCalculation(int limit)
    {
        Console.WriteLine($"  [HeavyCalculation] Running on Thread {Environment.CurrentManagedThreadId}...");
        long total = 0;
        for (int i = 0; i < limit; i++)
        {
            total += i;
        }

        return total;
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}