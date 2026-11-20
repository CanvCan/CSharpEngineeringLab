namespace Day03.ThreadsAndSync;

public static class Ex06_SemaphoreSlimAndMutex
{
    public static void Run()
    {
        PrintSection("1. Throttling with SemaphoreSlim (Max 3 Concurrent Workers)");

        // TODO 1: Create a SemaphoreSlim allowing at most 3 concurrent threads:
        // using var semaphore = new SemaphoreSlim(initialCount: 3, maxCount: 3);

        using var semaphore = new SemaphoreSlim(initialCount: 3, maxCount: 3);

        // TODO 2: Create and start 10 threads (Worker-1 to Worker-10).
        // Remember the Lambda Closure Trap (capture localCopy)!
        // Inside each thread:
        //   - Print $"[Worker-{id}] Waiting... (Free slots: {semaphore.CurrentCount})"
        //   - Call semaphore.Wait();
        //   - Inside try { ... } finally { semaphore.Release(); }:
        //       * Print in Green (or plain): $"  --> [Worker-{id}] ENTERED! Working 1000ms... (Free slots: {semaphore.CurrentCount})"
        //       * Thread.Sleep(1000);
        //       * Print $"  <-- [Worker-{id}] EXITING."
        // Store all 10 threads in a List<Thread> and Join all of them.
        // Observe: At most 3 workers are inside at any given second!

        var threads = new List<Thread>();

        for (int i = 1; i <= 10; i++)
        {
            int id = i; // Lambda closure trap
            var t = new Thread(() =>
            {
                PrintColored($"[Worker-{id}] Waiting... (Free slots: {semaphore.CurrentCount})");
                semaphore.Wait();

                try
                {
                    PrintColored($"  --> [Worker-{id}] ENTERED! Working 1000ms... (Free slots: {semaphore.CurrentCount})", ConsoleColor.Green);
                    Thread.Sleep(1000);
                    PrintColored($"  <-- [Worker-{id}] EXITING.", ConsoleColor.Red);
                }
                finally { semaphore.Release(); }

            });

            threads.Add(t);
            t.Start();
        }

        foreach (var t in threads)
        {
            t.Join();
        }

        PrintSection("2. Named Mutex (Cross-Process / Single-Instance Concept)");

        // TODO 3: Create a named Mutex:
        // const string mutexName = "Day03_Unique_Demo_Mutex";
        // using var mutex = new Mutex(initiallyOwned: false, name: mutexName);
        //
        // Start 2 threads (p1, p2) simulating two workers competing for the OS-level Mutex:
        //   - Call bool acquired = mutex.WaitOne(TimeSpan.FromMilliseconds(2000));
        //   - If acquired:
        //       try { print entered, Thread.Sleep(500), print releasing }
        //       finally { mutex.ReleaseMutex(); }
        //   - Else:
        //       print timeout!
        // Join both threads.

        const string mutexName = "Day03_Unique_Demo_Mutex";
        using var mutex = new Mutex(initiallyOwned: false, name: mutexName);

        Thread p1 = new Thread(() =>
        {
            bool acquired = mutex.WaitOne(TimeSpan.FromMilliseconds(2000));
            if (acquired)
            {
                try
                {
                    PrintColored("[P1] Entered", ConsoleColor.Green);
                    Thread.Sleep(2500); // 500 ms
                    PrintColored("[P1] Releasing ...");
                }
                finally { mutex.ReleaseMutex(); }
            }
            else { PrintColored("[P1] TIMEOUT !", ConsoleColor.Red); }
        }
        );

        Thread p2 = new Thread(() =>
        {
            bool acquired = mutex.WaitOne(TimeSpan.FromMilliseconds(2000));
            if (acquired)
            {
                try
                {
                    PrintColored("[P2] Entered", ConsoleColor.Green);
                    Thread.Sleep(500);
                    PrintColored("[P2] Releasing ...");
                }
                finally { mutex.ReleaseMutex(); }
            }
            else { PrintColored("[P2] TIMEOUT !", ConsoleColor.Red); }
        }
        );

        p1.Start();
        p2.Start();

        p1.Join();
        p2.Join();

        Console.WriteLine("\n[Done] Ex06 finished.");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }

    private static readonly object _consoleLock = new();

    private static void PrintColored(string message, ConsoleColor color = ConsoleColor.White)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}