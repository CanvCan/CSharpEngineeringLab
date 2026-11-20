using System.Collections.Concurrent;

namespace Day03.ThreadsAndSync;

public static class Ex08_ThreadPoolAndConcurrentCollections
{
    private static volatile bool _producerDone;
    public static void Run()
    {
        PrintSection("1. ThreadPool.QueueUserWorkItem (Reusable Background Threads)");

        const int jobCount = 8;
        using var countdown = new CountdownEvent(jobCount);

        // TODO 1: Queue 'jobCount' (8) jobs using ThreadPool.QueueUserWorkItem(_ => { ... }).
        // Remember Lambda Closure Trap (capture jobId = i)!
        // Inside each job:
        //   try
        //   {
        //       Print $"[Job-{jobId}] ThreadId={Environment.CurrentManagedThreadId}, IsPool={Thread.CurrentThread.IsThreadPoolThread}, IsBg={Thread.CurrentThread.IsBackground}"
        //       Thread.Sleep(200);
        //   }
        //   finally { countdown.Signal(); }
        // After the loop, call countdown.Wait() so Main doesn't exit early.
        // Observe: Notice how the same ThreadIds get REUSED across different jobs!

        for (int i = 0; i < jobCount; i++)
        {
            int jobId = i;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    Console.WriteLine($"[Job-{jobId}] ThreadId={Environment.CurrentManagedThreadId}, IsPool={Thread.CurrentThread.IsThreadPoolThread}, IsBg={Thread.CurrentThread.IsBackground}");
                    Thread.Sleep(200);
                }
                finally
                {
                    countdown.Signal();
                }
            });
        }

        countdown.Wait();

        PrintSection("2. Standard Dictionary (Unsafe) vs. ConcurrentDictionary (Safe)");

        const int threadsCount = 4;
        const int itemsPerThread = 50_000;
        const int expectedCount = threadsCount * itemsPerThread;

        // TODO 2 (The Crash / Corruption):
        // Create a standard Dictionary<int, int> unsafeDict = new();
        // Start 4 threads; each tries to add keys from (t * itemsPerThread) to ((t + 1) * itemsPerThread).
        // Wrap the loop inside try/catch (Exception ex) and print if an exception occurs!
        // Join all 4 threads and print unsafeDict.Count vs expected (200,000).

        Dictionary<int, int> unsafeDict = new();
        var unsafeThreads = new List<Thread>();

        for (int t = 0; t < threadsCount; t++)
        {
            int threadIndex = t; // Lambda closure trap
            var thread = new Thread(() =>
            {
                try
                {
                    int start = threadIndex * itemsPerThread;
                    int end = (threadIndex + 1) * itemsPerThread;

                    for (int key = start; key < end; key++)
                    {
                        unsafeDict[key] = key; // or unsafeDict.Add(key, key);
                    }
                }
                catch (Exception ex)
                {
                    PrintColored($"[Unsafe Thread-{threadIndex}] CRASHED! {ex.GetType().Name}: {ex.Message}", ConsoleColor.Red);
                }
            });

            unsafeThreads.Add(thread);
            thread.Start();
        }

        foreach (var thread in unsafeThreads)
        {
            thread.Join();
        }

        PrintColored($"Unsafe Dictionary Count: {unsafeDict.Count:N0} | Expected: {expectedCount:N0}");

        // TODO 3 (The Fix):
        // Create a ConcurrentDictionary<int, int> safeDict = new();
        // Start 4 threads doing the exact same inserts using safeDict.TryAdd(key, key).
        // Join all 4 threads and print safeDict.Count (must be exactly 200,000, zero exceptions).

        ConcurrentDictionary<int, int> safeDict = new();
        var safeThreads = new List<Thread>();

        for (int t = 0; t < threadsCount; t++)
        {
            int threadIndex = t;
            var thread = new Thread(() =>
            {
                int start = threadIndex * itemsPerThread;
                int end = (threadIndex + 1) * itemsPerThread;

                for (int key = start; key < end; key++)
                {
                    safeDict.TryAdd(key, key);
                }
            });

            safeThreads.Add(thread);
            thread.Start();
        }

        foreach (var thread in safeThreads)
        {
            thread.Join();
        }

        PrintColored($"ConcurrentDictionary Count: {safeDict.Count:N0} | Expected: {expectedCount:N0}", ConsoleColor.Green);

        PrintSection("3. ConcurrentQueue<T> (Mini Producer-Consumer)");

        var queue = new ConcurrentQueue<int>();

        // TODO 4: Create a 'producer' thread that enqueues numbers 1..20 with Thread.Sleep(30) between each.
        // Use a 'volatile bool producerDone = false;' flag (set true when producer finishes).
        // Create 2 'consumer' threads that loop: while (!producerDone || !queue.IsEmpty)
        //   if (queue.TryDequeue(out int item)) -> print $"[Consumer-{id}] Processed {item}"
        //   else -> Thread.Sleep(10); // Notice this awkward polling! (Day 5 Channel<T> fixes this!)
        // Start producer and both consumers, Join all 3.

        _producerDone = false;

        Thread producer = new Thread(() =>
        {
            for (int i = 1; i <= 20; i++)
            {
                queue.Enqueue(i);
                Console.WriteLine($"[Producer] Enqueued {i}");
                Thread.Sleep(30);
            }
            _producerDone = true;
            Console.WriteLine("[Producer] Finished!");
        });

        var consumers = new List<Thread>();
        for (int c = 1; c <= 2; c++)
        {
            int id = c;
            var consumer = new Thread(() =>
            {
                while (!_producerDone || !queue.IsEmpty)
                {
                    if (queue.TryDequeue(out int item))
                    {
                        Console.WriteLine($"  [Consumer-{id}] Processed {item}");
                    }
                    else
                    {
                        Thread.Sleep(10); // Brief sleep when the queue is empty to avoid burning 100% CPU (polling)
                    }
                }
            });
            consumers.Add(consumer);
        }

        producer.Start();
        foreach (var c in consumers) c.Start();

        producer.Join();
        foreach (var c in consumers) c.Join();

        Console.WriteLine("\n[Done] Ex08 finished. Day 03 Complete!");
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