namespace Day03.ThreadsAndSync;

public static class Ex01_ThreadBasicsAndLifecycle
{
    public static void Run()
    {
        PrintSection("1. Thread Creation, Lifecycle (IsAlive & ThreadState) & Join()");

        // TODO 1: Print the Main thread's ManagedThreadId using Environment.CurrentManagedThreadId.

        Console.WriteLine("Current thread ID: " + Environment.CurrentManagedThreadId);
        Console.WriteLine();

        // TODO 2: Create a Thread named "Worker-1" that prints its Name and ManagedThreadId,
        // sleeps for 500ms (Thread.Sleep), then prints that it finished.
        // BEFORE calling .Start(), print t.IsAlive and t.ThreadState (expect: False, Unstarted).

        Thread thread = new Thread(() =>
        {
            Console.WriteLine($"Running Thread -> Name: {Thread.CurrentThread.Name}, ID: {Environment.CurrentManagedThreadId}");
            Thread.Sleep(500);
            Console.WriteLine($"{Thread.CurrentThread.Name} finished.");
        }
        );

        thread.Name = "Worker-1";

        Console.WriteLine($"Before Start -> IsAlive: {thread.IsAlive}, ThreadState: {thread.ThreadState}");
        // Console.WriteLine($"Name: {thread.Name} | ID: {thread.ManagedThreadId}");

        thread.Start();

        thread.Join();   // Wait for Worker-1 to finish;
                         // otherwise, the main thread will keep running
                         // and mix up the console output.

        Console.WriteLine();

        // TODO 3: Start the thread. Immediately sleep Main for 100ms, then print t.IsAlive and t.ThreadState
        // (expect: True, WaitSleepJoin).
        // Finally, call t.Join(), and AFTER .Join() print t.IsAlive and t.ThreadState (expect: False, Stopped).

        Thread thread2 = new Thread(() =>
        {
            Console.WriteLine($"Running Thread -> Name: {Thread.CurrentThread.Name}, ID: {Environment.CurrentManagedThreadId}");
            Thread.Sleep(500);
            Console.WriteLine($"{Thread.CurrentThread.Name} finished.");
        }
        );

        thread2.Name = "Worker-2";

        Console.WriteLine($"Before Start -> IsAlive: {thread2.IsAlive}, ThreadState: {thread2.ThreadState}");

        thread2.Start();
        Thread.Sleep(100); // Sleep main thread for 100ms
                           // to ensure Worker-2 enters its Thread.Sleep(500) state

        Console.WriteLine($"After Start -> IsAlive: {thread2.IsAlive}, ThreadState: {thread2.ThreadState}");

        thread2.Join();

        Console.WriteLine($"After Join -> IsAlive: {thread2.IsAlive}, ThreadState: {thread2.ThreadState}");

        PrintSection("2. Concurrent Execution & Interleaving");

        // TODO 4: Create two threads: threadA and threadB.
        // threadA loops 50 times and writes "A" (Console.Write).
        // threadB loops 50 times and writes "B" (Console.Write).
        // Start both, then Join both. Observe the interleaved output.

        Thread threadA = new Thread(() =>
        {
            for (int i = 0; i < 50; i++) { Console.Write("A"); }
        }
        );

        Thread threadB = new Thread(() =>
        {
            for (int i = 0; i < 50; i++) { Console.Write("B"); }
        }
       );

        threadA.Start();
        threadB.Start();
        threadA.Join();
        threadB.Join();

        Console.WriteLine();

        PrintSection("3. Lambda Closure Trap vs. Fixed Copy");

        // TODO 5 (The Trap): Write a for loop from i = 0 to 4.
        // Inside the loop, create and start a Thread that prints $"[Trap] i = {i}".
        // Store threads in a List<Thread> and Join all of them after the loop.

        List<Thread> list = new List<Thread>();

        for (int i = 0; i < 5; i++)
        {
            Thread threadTrap = new Thread(() => { Console.WriteLine($"[Trap] i = {i}"); });
            threadTrap.Start();
            list.Add(threadTrap);
        }

        foreach (var item in list) { item.Join(); }

        // TODO 6 (The Fix): Repeat the loop from i = 0 to 4,
        // capture a local copy: int localCopy = i; inside the loop.
        // Print $"[Fixed] copy = {localCopy}" and Join all threads.

        List<Thread> list2 = new List<Thread>();

        for (int i = 0; i < 5; i++)
        {
            int localCopy = i;
            Thread threadTrap = new Thread(() => { Console.WriteLine($"[Fixed] copy = {localCopy}"); });
            threadTrap.Start();
            list2.Add(threadTrap);
        }

        foreach (var item in list2) { item.Join(); }

        PrintSection("4. Foreground vs. Background Thread");

        // TODO 7: Create a thread 'bgThread' that loops 10 times, printing $"[BG] tick {i}"
        // with Thread.Sleep(200) on each iteration.
        // Set bgThread.IsBackground = true; and start it WITHOUT calling .Join().
        // Let Main sleep 500ms and exit Run().
        // (Experiment: Change IsBackground = false and run again to see Main wait for all 10 ticks!)

        Thread bgThread = new Thread(() =>
        {
            for (int i = 0; i < 10; i++) { Console.WriteLine($"[BG] tick {i}"); Thread.Sleep(200);}
        }
        );

        bgThread.IsBackground = true;
        bgThread.Start();
        Thread.Sleep(500);

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