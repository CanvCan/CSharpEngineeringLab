using System.Diagnostics;

namespace Day04.TasksAndAsync;

public static class Ex04_WhenAllAndWhenAny
{
    public static async Task RunAsync()
    {
        PrintSection("1. Sequential await vs. Parallel Task.WhenAll");

        // TODO 1 (Sequential - ~3 seconds):
        // Start a Stopwatch.
        // Call DoWorkAsync(1, 1000), DoWorkAsync(2, 1000), DoWorkAsync(3, 1000) ONE BY ONE with 'await'.
        // Stop Stopwatch and print the 3 results and total elapsed ms (expect ~3000 ms).

        Stopwatch sw = Stopwatch.StartNew();

        // WITH 'await' = Waits for completion and unwraps 'Task<int>' directly into 'int':

        int r1 = await DoWorkAsync(1, 1000);
        int r2 = await DoWorkAsync(2, 1000);
        int r3 = await DoWorkAsync(3, 1000);

        sw.Stop();
        double elapsedMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine($"\nResults: {r1}, {r2}, {r3} | Elapsed: {elapsedMs:F2} ms\n");


        // TODO 2 (Parallel with Task.WhenAll - ~1 second):
        // Restart Stopwatch.
        // Start all 3 tasks FIRST without awaiting them:
        //   Task<int> t1 = DoWorkAsync(1, 1000);
        //   Task<int> t2 = DoWorkAsync(2, 1000);
        //   Task<int> t3 = DoWorkAsync(3, 1000);
        // Then await all of them together: int[] results = await Task.WhenAll(t1, t2, t3);
        // Stop Stopwatch and print string.Join(", ", results) and total elapsed ms (expect ~1000 ms).

        sw.Restart();

        // NO 'await' = Type stays 'Task<int>' (starts immediately in background, does NOT wait):

        Task<int> t1 = DoWorkAsync(1, 1000);
        Task<int> t2 = DoWorkAsync(2, 1000);
        Task<int> t3 = DoWorkAsync(3, 1000);

        // WITHOUT 'await', the type is 'Task<int[]>' (an uncompleted wrapper/box):
        // Task<int[]> pendingBox = Task.WhenAll(t1, t2, t3);

        // WITH 'await', the box is unwrapped once all tasks finish, returning the actual array.
        // (After this line, you can also safely use .Result like t1.Result without blocking.)
        int[] results = await Task.WhenAll(t1, t2, t3); // results = [10, 20, 30]

        sw.Stop();
        elapsedMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine($"\nResult: {string.Join(", ", results)} | Elapsed: {elapsedMs:F2} ms");


        PrintSection("2. Task.WhenAny (Fastest Response Race)");

        // TODO 3: Simulate 3 servers racing with random delays between 1000ms and 3000ms
        // (Use Random.Shared.Next(1000, 3001) for each server's delay).
        // Start 3 tasks calling FetchFromServerAsync("Istanbul", ...), ("Frankfurt", ...), ("London", ...).
        // Use: Task<string> winner = await Task.WhenAny(s1, s2, s3);
        // Print the winner's result using: await winner
        // (Note: The slower tasks still keep running in the background unless cancelled!)

        Task<string> s1 = FetchFromServerAsync("Istanbul", Random.Shared.Next(1000, 3001));
        Task<string> s2 = FetchFromServerAsync("Frankfurt", Random.Shared.Next(1000, 3001));
        Task<string> s3 = FetchFromServerAsync("London", Random.Shared.Next(1000, 3001));

        Task<string> winner = await Task.WhenAny(s1, s2, s3);
        string result = await winner;

        Console.WriteLine($"\nWinner is {result}");

        PrintSection("3. Timeout Pattern (Task.WhenAny & .WaitAsync)");

        // TODO 4A (Classic Task.WhenAny Timeout):
        // Start a slow task: Task<string> slowTask = FetchFromServerAsync("SlowDB", 5000);
        // Create a timeout task: Task timeoutTask = Task.Delay(2000);
        // Race them: Task finishedFirst = await Task.WhenAny(slowTask, timeoutTask);
        // If (finishedFirst == slowTask), print success + await slowTask.
        // Else, print in Red: "TIMEOUT! SlowDB took longer than 2000ms."

        Task<string> slowTask = FetchFromServerAsync("SlowDB", 5000);
        Task timeoutTask = Task.Delay(2000);

        Task finishedFirst = await Task.WhenAny(slowTask, timeoutTask);

        if (finishedFirst == slowTask)
        {
            Console.WriteLine($"Success: {await slowTask}\n");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("TIMEOUT! SlowDB took longer than 2000ms.\n");
            Console.ResetColor();
        }

        // TODO 4B (.NET 6+ Shortcut - WaitAsync):
        // Call FetchFromServerAsync("SlowAPI", 5000).WaitAsync(TimeSpan.FromSeconds(2)) inside a try/catch.
        // Catch TimeoutException and print: "WaitAsync caught TimeoutException!"

        try
        {
            await FetchFromServerAsync("SlowAPI", 5000).WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch (TimeoutException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("WaitAsync caught TimeoutException!");
            Console.ResetColor();
        }


        Console.WriteLine("\n[Done] Ex04 finished.");
    }

    private static async Task<int> DoWorkAsync(int id, int delayMs)
    {
        Console.WriteLine($"  [Work-{id}] Started ({delayMs}ms)...");
        await Task.Delay(delayMs);
        Console.WriteLine($"  [Work-{id}] Completed.");
        return id * 10;
    }

    private static async Task<string> FetchFromServerAsync(string serverName, int delayMs)
    {
        Console.WriteLine($"  [{serverName}] Request sent (will take {delayMs}ms)...");
        await Task.Delay(delayMs);
        return $"{serverName} responded in {delayMs}ms";
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}