using System.Diagnostics;

namespace Day04.TasksAndAsync;

public static class Ex01_TaskBasics
{
    public static async Task RunAsync()
    {
        PrintSection("1. Task.Run, ThreadPool & Background Property");

        // TODO 1: Print Main thread's ManagedThreadId.
        // Start a Task<int> using Task.Run(() => { ... }):
        //   - Print its ManagedThreadId, IsThreadPoolThread, and IsBackground.
        //   - Sleep 500ms and return 42.
        // Capture the result using t.Result and print it.
        // NOTE: .Result blocks Main until the task finishes (we will replace this with 'await' in Ex02).

        Console.WriteLine($"Main Thread ID: {Thread.CurrentThread.ManagedThreadId}");

        Task<int> task = Task.Run(() =>
            {
                Console.WriteLine(
                    $"Worker Thread ID: {Thread.CurrentThread.ManagedThreadId} " +
                    $"| ThreadPool: {Thread.CurrentThread.IsThreadPoolThread} " +
                    $"| Background: {Thread.CurrentThread.IsBackground}");
                Thread.Sleep(500);
                return 42;
            }
        );

        int result = task.Result;
        Console.WriteLine($"Result: {result}");


        PrintSection("2. Task Lifecycle & Status");

        // TODO 2: Create a Task WITHOUT starting it yet:
        // var manualTask = new Task(() => Thread.Sleep(400));
        // Print manualTask.Status BEFORE Start() (expect: Created).
        // Call manualTask.Start(), sleep Main 50ms, print Status
        // (expect: Running, or WaitingToRun if the ThreadPool hasn't picked it up yet).
        // Call manualTask.Wait(), then print Status and IsCompleted (expect: RanToCompletion, True).

        var manualTask = new Task(() => { Thread.Sleep(400); });

        Console.WriteLine($"Task Status [Before Start] {manualTask.Status}");

        manualTask.Start(); // new Task() requires .Start() to run.
        Thread.Sleep(50); // Give ThreadPool time to pick up and start the task.

        Console.WriteLine($"Task Status [After Start] {manualTask.Status}");

        manualTask.Wait();
        Console.WriteLine($"Task Status [After Wait] {manualTask.Status} | Completed: {manualTask.IsCompleted}");


        PrintSection("3. Benchmark: 500 OS Threads vs. 500 Tasks (Delay)");

        const int count = 500;

        // TODO 3 (OS Threads):
        // Record int before = Process.GetCurrentProcess().Threads.Count;
        // Start Stopwatch.
        // Create 'count' new Thread(() => Thread.Sleep(1000)) instances and start all of them.
        // Call Thread.Sleep(200) so OS threads actually spawn, then capture int peak = Process.GetCurrentProcess().Threads.Count;
        // Join all threads and stop Stopwatch.
        // Print: Elapsed ms, Threads Before, Peak Threads, and Delta (peak - before).

        int before = Process.GetCurrentProcess().Threads.Count;
        Stopwatch sw = Stopwatch.StartNew();

        List<Thread> listThread = new();

        for (int i = 0; i < count; i++)
        {
            Thread thread = new Thread(() => Thread.Sleep(1000));
            listThread.Add(thread);
            thread.Start();
        }

        // Give the OS time to allocate and start all 500 threads so we capture an accurate peak count.
        Thread.Sleep(200);
        int peak = Process.GetCurrentProcess().Threads.Count;

        foreach (var item in listThread)
        {
            item.Join();
        }

        sw.Stop();
        double listMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine(
            $"Elapsed ms: {listMs} " +
            $"| Threads [Before]: {before} " +
            $"| Threads [Peak]: {peak} " +
            $"| Delta: {peak - before}");


        // TODO 4 (Tasks - Non-Blocking Timer):
        // Record int before = Process.GetCurrentProcess().Threads.Count;
        // Start Stopwatch.
        // Create 'count' tasks using: Enumerable.Range(0, count).Select(_ => Task.Delay(1000)).ToArray();
        // Capture int peak = Process.GetCurrentProcess().Threads.Count;
        // Wait for all using: await Task.WhenAll(tasks);
        // Stop Stopwatch.
        // Print: Elapsed ms, Threads Before, Peak Threads, and Delta (peak - before).
        // NOTE: Task.Delay uses a timer and does NOT block a thread, so peak ≈ before.

        before = Process.GetCurrentProcess().Threads.Count;
        sw.Restart();

        var tasks = Enumerable.Range(0, count).Select(_ => Task.Delay(1000)).ToArray();

        peak = Process.GetCurrentProcess().Threads.Count;
        await Task.WhenAll(tasks);

        sw.Stop();
        listMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine(
            $"Elapsed ms: {listMs} " +
            $"| Threads [Before]: {before} " +
            $"| Threads [Peak]: {peak} " +
            $"| Delta: {peak - before}");


        PrintSection("4. (Optional) ThreadPool Starvation Preview");

        // TODO 5 (Optional): Run 100 (or 500) x Task.Run(() => Thread.Sleep(1000)) and await Task.WhenAll(...).
        // Measure elapsed time with Stopwatch.
        // Observe: Elapsed time is MUCH longer than 1 second because ThreadPool injects new threads slowly.
        // This is why blocking (Thread.Sleep / .Result) inside Task.Run is bad!

        sw.Restart();
        before = Process.GetCurrentProcess().Threads.Count;


        List<Task> starvationTasks = new();

        /*
         * Alternative LINQ Approach (One-liner equivalent to the for-loop):
         * Why Task.Run() wrapper?
         * 1. Select() requires a return value, whereas Thread.Sleep() returns 'void'. Wrapping it in Task.Run() returns a Task object.
         * 2. Calling Thread.Sleep() directly would execute synchronously (one by one) instead of scheduling parallel tasks on the ThreadPool.
         * 3. .ToList() is mandatory to trigger immediate execution (eager evaluation).
         *
         * starvationTasks = Enumerable.Range(0, 100)
         *     .Select(_ => Task.Run(() => Thread.Sleep(1000)))
         *     .ToList();
         */

        for (int i = 0; i < 100; i++)
        {
            starvationTasks.Add(Task.Run(() => Thread.Sleep(1000)));
        }

        await Task.WhenAll(starvationTasks);

        peak = Process.GetCurrentProcess().Threads.Count;

        sw.Stop();
        listMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine(
            $"Elapsed ms: {listMs} " +
            $"| Threads [Before]: {before} " +
            $"| Threads [Peak]: {peak} " +
            $"| Delta: {peak - before}");


        Console.WriteLine("\n[Done] Ex01 finished.");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}