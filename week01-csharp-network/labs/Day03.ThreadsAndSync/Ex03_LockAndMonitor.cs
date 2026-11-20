namespace Day03.ThreadsAndSync;

public static class Ex03_LockAndMonitor
{
    private static int _counter = 0;
    private static readonly object _counterLock = new();

    public static void Run()
    {
        const int iterations = 1_000_000;
        const int expected = iterations * 2;

        PrintSection("1. Fixing Race Condition with lock");

        // TODO 1: Reset _counter = 0.
        // Create 2 threads (t1, t2), each looping 'iterations' times and incrementing _counter
        // INSIDE lock (_counterLock) { _counter++; }.
        // Start and Join both, then print Expected vs Actual (must be exactly 2,000,000).

        _counter = 0;

        Thread t1 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                lock (_counterLock)
                {
                    _counter++;
                }
            }
        });

        Thread t2 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                lock (_counterLock)
                {
                    _counter++;
                }
            }
        });

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        int lostUpdates = expected - _counter;
        Console.WriteLine($"Expected = {expected:N0} | Actual = {_counter:N0} | Lost = {lostUpdates:N0}");

        PrintSection("2. Under the Hood: Manual Monitor.Enter / Monitor.Exit");

        // TODO 2: Reset _counter = 0.
        // Do the exact same 2-thread increment, but instead of 'lock', manually use:
        // bool lockTaken = false;
        // try { Monitor.Enter(_counterLock, ref lockTaken); _counter++; }
        // finally { if (lockTaken) Monitor.Exit(_counterLock); }
        // Start, Join, and verify _counter == 2,000,000.

        _counter = 0;

        Thread t3 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(_counterLock, ref lockTaken);
                    _counter++;
                }
                finally
                {
                    if (lockTaken) Monitor.Exit(_counterLock);
                }
            }
        });

        Thread t4 = new Thread(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(_counterLock, ref lockTaken);
                    _counter++;
                }
                finally
                {
                    if (lockTaken) Monitor.Exit(_counterLock);
                }
            }
        });

        t3.Start();
        t4.Start();

        t3.Join();
        t4.Join();

        lostUpdates = expected - _counter;
        Console.WriteLine($"Expected = {expected:N0} | Actual = {_counter:N0} | Lost = {lostUpdates:N0}");

        PrintSection("3. Timeout Control with Monitor.TryEnter");

        object resourceLock = new();

        // TODO 3: Create 'holderThread' that locks 'resourceLock', prints "[Holder] Acquired lock, working 1500ms...",
        // sleeps 1500ms, then prints "[Holder] Releasing lock."
        // Create 'waiterThread' that tries to acquire 'resourceLock' with Monitor.TryEnter(resourceLock, TimeSpan.FromMilliseconds(500)).
        // - If true: print "[Waiter] Acquired lock!" and release it in a finally block (Monitor.Exit).
        // - If false: print "[Waiter] Timeout! Could not acquire lock within 500ms, moving on."
        // Start holderThread, sleep Main 50ms (so holder grabs the lock first), then start waiterThread.
        // Join both.

        Thread holderThread = new Thread(() =>
        {
            lock (resourceLock)
            {
                Console.WriteLine("[Holder] Acquired lock, working 1500ms...");
                Thread.Sleep(1500);
                Console.WriteLine("[Holder] Releasing lock.");
            }
        });

        Thread waiterThread = new Thread(() =>
        {
            bool lockTaken = false;
            try
            {
                lockTaken = Monitor.TryEnter(resourceLock, TimeSpan.FromMilliseconds(500));
                if (lockTaken)
                {
                    Console.WriteLine("[Waiter] Acquired lock!");
                }
                else
                {
                    Console.WriteLine("[Waiter] Timeout! Could not acquire lock within 500ms, moving on.");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(resourceLock);
                }
            }
        });

        holderThread.Start();
        Thread.Sleep(50);
        waiterThread.Start();

        holderThread.Join();
        waiterThread.Join();

        PrintSection("4. Thread-Safe BankAccount");

        // TODO 4: Create a SafeBankAccount with balance = 1000.
        // Start 2 threads, both calling account.WithdrawSafe(1000).
        // Join both and print the final Balance (must be 0, NEVER -1000!).

        SafeBankAccount account = new SafeBankAccount(1000);

        Thread threadA = new Thread(() => { account.WithdrawSafe(1000); });
        Thread threadB = new Thread(() => { account.WithdrawSafe(1000); });

        threadA.Start();
        Thread.Sleep(5); // Simulating small processing delay to make the race condition obvious
        threadB.Start();

        threadA.Join();
        threadB.Join();

        Console.WriteLine();
        Console.WriteLine("Balance: " + account.Balance);

        Console.WriteLine("\n[Done] Ex03 finished.");
    }

    private class SafeBankAccount(int initialBalance)
    {
        private readonly object _balanceLock = new();
        public int Balance { get; private set; } = initialBalance;

        public void WithdrawSafe(int amount)
        {
            // TODO 4b: Protect the check-then-act block using lock (_balanceLock)
            lock (_balanceLock)
            {
                if (Balance >= amount)
                {
                    Thread.Sleep(50);
                    Balance -= amount;
                    Console.WriteLine($"[Thread {Environment.CurrentManagedThreadId}] Withdrew {amount}. Remaining: {Balance}");
                }
                else
                {
                    Console.WriteLine($"[Thread {Environment.CurrentManagedThreadId}] Insufficient funds!");
                }
            }
        }
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}