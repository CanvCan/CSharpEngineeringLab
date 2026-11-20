namespace Day03.ThreadsAndSync;

public static class Ex02_MemoryAndRaceCondition
{
    // Shared state on the Heap (accessible by all threads)
    private static int _sharedCounter = 0;

    public static void Run()
    {
        PrintSection("1. Stack Isolation (Thread-Safe Local Variables)");

        // TODO 1: Write a local method or lambda 'CountLocally' that declares:
        // int localCounter = 0;
        // Loops 1_000_000 times incrementing localCounter++, then prints
        // $"[Thread {Environment.CurrentManagedThreadId}] localCounter = {localCounter}".
        // Run this on 2 threads concurrently, Start and Join both.
        // Observe: Both always reach exactly 1,000,000 because each thread has its own Stack.

        static void CountLocally()
        {
            int localCounter = 0;
            for (int i = 0; i < 1_000_000; i++)
            {
                localCounter++;
            }

            Console.WriteLine($"[Thread {Environment.CurrentManagedThreadId}] localCounter = {localCounter}");
        }

        Thread thread1 = new Thread(() => { CountLocally(); }); // Also, "new Thread(CountLocally)" is correct.
        Thread thread2 = new Thread(() => { CountLocally(); });

        thread1.Start();
        thread2.Start();

        thread1.Join();
        thread2.Join();

        PrintSection("2. Heap Sharing & Race Condition (Lost Updates)");

        const int iterations = 1_000_000;
        const int expected = iterations * 2;

        // TODO 2: Run the experiment 5 times in a loop (run = 1 to 5):
        // - Reset _sharedCounter = 0;
        // - Create 2 threads (t1, t2), each incrementing _sharedCounter++ 'iterations' times.
        // - Start both, Join both.
        // - Calculate lostUpdates = expected - _sharedCounter.
        // - Print: $"Run {run}: Expected = {expected:N0} | Actual = {_sharedCounter:N0} | Lost = {lostUpdates:N0}"
        // (Tip: If your CPU is too fast and it hits 2,000,000, bump iterations to 10_000_000!)

        for (int run = 1; run <= 5; run++)
        {
            _sharedCounter = 0;

            Thread t1 = new Thread(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    _sharedCounter++;
                }
            });

            Thread t2 = new Thread(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    _sharedCounter++;
                }
            });

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            int lostUpdates = expected - _sharedCounter;
            Console.WriteLine($"Run {run}: Expected = {expected:N0} | Actual = {_sharedCounter:N0} | Lost = {lostUpdates:N0}");
        }

        PrintSection("3. Shared Instance Object Race Condition (Bank Account)");

        // TODO 3: Create a BankAccount with initial balance = 1000.
        // Start 2 threads, each trying to withdraw 1000 at the same time using account.WithdrawUnsafe(1000).
        // Join both threads and print the final Balance.
        // Observe how both threads pass the "if (Balance >= amount)" check and drive the balance negative!

        BankAccount account = new BankAccount(1000);

        Thread threadA = new Thread(() => { account.WithdrawUnsafe(1000); });
        Thread threadB = new Thread(() => { account.WithdrawUnsafe(1000); });

        threadA.Start();
        Thread.Sleep(5); // Simulating small processing delay to make the race condition obvious
        threadB.Start();

        threadA.Join();
        threadB.Join();

        Console.WriteLine();
        Console.WriteLine("Balance: " + account.Balance);

        Console.WriteLine("\n[Done] Ex02 finished.");
    }

    private class BankAccount(int initialBalance)
    {
        public int Balance { get; private set; } = initialBalance;

        public void WithdrawUnsafe(int amount)
        {
            // Check-then-act race condition!
            if (Balance >= amount)
            {
                // Simulating small processing delay to make the race condition obvious
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

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}