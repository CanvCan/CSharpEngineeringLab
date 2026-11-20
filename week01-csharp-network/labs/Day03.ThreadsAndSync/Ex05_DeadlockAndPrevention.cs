namespace Day03.ThreadsAndSync;

public static class Ex05_DeadlockAndPrevention
{
    private static readonly object _lockA = new();
    private static readonly object _lockB = new();

    public static void Run()
    {
        PrintSection("1. Producing a Deadlock (Opposite Lock Order)");

        // TODO 1: Create two threads (t1, t2) with IsBackground = true (so the app can still exit later):
        // - t1: lock (_lockA) -> print "[T1] Locked A" -> Thread.Sleep(100) -> lock (_lockB) -> print "[T1] Locked B"
        // - t2: lock (_lockB) -> print "[T2] Locked B" -> Thread.Sleep(100) -> lock (_lockA) -> print "[T2] Locked A"
        // Start both threads.

        Thread t1 = new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            lock (_lockA)
            {
                Console.WriteLine("[T1] Locked A");
                Thread.Sleep(100);
                lock (_lockB)
                {
                    Console.WriteLine("[T1] Locked B");
                }
            }
        }
        );

        Thread t2 = new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            lock (_lockB)
            {
                Console.WriteLine("[T2] Locked B");
                Thread.Sleep(100);
                lock (_lockA)
                {
                    Console.WriteLine("[T2] Locked A");
                }
            }
        }
        );

        t1.Start();
        t2.Start();

        // TODO 2: Wait for both threads using a timeout:
        // bool t1Done = t1.Join(1500);
        // bool t2Done = t2.Join(1500);
        // If (!t1Done || !t2Done), print in Red: "DEADLOCK DETECTED! Threads are stuck waiting for each other."
        // (Experiment: Try plain t1.Join() without timeout once to watch the console freeze forever!)

        bool t1Done = t1.Join(1500);
        bool t2Done = t2.Join(1500);

        if (!t1Done || !t2Done)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nDEADLOCK DETECTED! Threads are stuck waiting for each other.");
            Console.ResetColor();
        }

        PrintSection("2. Preventing Deadlock (Consistent Lock Ordering)");

        object lockX = new();
        object lockY = new();

        // TODO 3: Create two threads (safeT1, safeT2) using lockX and lockY.
        // BOTH threads must acquire lockX FIRST, sleep 100ms, then acquire lockY SECOND.
        // Start both, Join both (no timeout needed), and print that both completed safely.

        Thread safeT1 = new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            lock (lockX)
            {
                Console.WriteLine("[safeT1] Locked X");
                Thread.Sleep(100);
                lock (lockY)
                {
                    Console.WriteLine("[safeT1] Locked Y");
                }
            }
        }
);

        Thread safeT2 = new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            lock (lockX)
            {
                Console.WriteLine("[safeT2] Locked X");
                Thread.Sleep(100);
                lock (lockY)
                {
                    Console.WriteLine("[safeT2] Locked Y");
                }
            }
        }
        );

        safeT1.Start();
        safeT2.Start();

        safeT1.Join();
        safeT2.Join();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nBoth threads completed safely without deadlock!");
        Console.ResetColor();

        Console.WriteLine("\n[Done] Ex05 finished.");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}