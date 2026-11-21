namespace Day04.TasksAndAsync;

public static class Ex02_AsyncAwaitModel
{
    public static async Task RunAsync()
    {
        PrintSection("1. Non-Blocking Wait: await Task.Delay (Thread May Change)");

        // TODO 1: Print the current ManagedThreadId BEFORE await.
        // Call: await Task.Delay(1000);
        // Print the current ManagedThreadId AFTER await.
        // Observe: Did the Thread ID change? Why?
        // (In a console app, there is no SynchronizationContext, so after the delay
        // finishes, any free ThreadPool thread can resume the rest of the method.)

        // SLOWER: Accesses/allocates the full Thread object first, then reads its ID.
        // Use only if you also need other properties (Name, Priority, IsBackground, etc.).
        // Console.WriteLine($"[Before Await] Current Thread ID: {Thread.CurrentThread.ManagedThreadId}");

        // FASTER (Recommended): Zero-allocation. Reads the ID directly from the runtime.
        // Always prefer this when you only need the Thread ID (e.g., logging/debugging).
        Console.WriteLine($"[Before Await] Current Thread ID: {Environment.CurrentManagedThreadId}");

        await Task.Delay(1000);

        Console.WriteLine($"[After Await] Current Thread ID: {Environment.CurrentManagedThreadId}");


        PrintSection("2. Blocking Wait: Thread.Sleep (Same Thread Stuck Waiting)");

        // TODO 2: Print the current ManagedThreadId BEFORE Thread.Sleep(1000).
        // Call: Thread.Sleep(1000);
        // Print the current ManagedThreadId AFTER Thread.Sleep(1000).
        // Add a short comment explaining the difference from TODO 1:
        // (Same thread stayed blocked doing nothing for 1 full second.)

        Console.WriteLine($"[Before Thread.Sleep()] Current Thread ID: {Environment.CurrentManagedThreadId}");

        Thread.Sleep(1000);

        Console.WriteLine($"[After Thread.Sleep()] Current Thread ID: {Environment.CurrentManagedThreadId}");


        PrintSection("3. Returning a Value with async Task<T>");

        // TODO 3: Complete the helper method 'DoWorkAsync(int id)' below.
        // Call it here using: int result = await DoWorkAsync(5);
        // Print the returned result and the ManagedThreadId after receiving it.

        int result = await DoWorkAsync(5);

        Console.WriteLine();
        Console.WriteLine($"Result: {result} | Current Thread ID: {Environment.CurrentManagedThreadId}");


        Console.WriteLine("\n[Done] Ex02 finished.");
    }

    // TODO 3 (Helper): Implement this method:
    // - Signature: private static async Task<int> DoWorkAsync(int id)
    // - Print $"[Work-{id}] Started on Thread {Environment.CurrentManagedThreadId}"
    // - Wait asynchronously for 1000ms using await Task.Delay(1000)
    // - Print $"[Work-{id}] Finished on Thread {Environment.CurrentManagedThreadId}"
    // - Return id * 10 (or just id)

    private static async Task<int> DoWorkAsync(int id)
    {
        Console.WriteLine($"[Work-{id}] Started on Thread {Environment.CurrentManagedThreadId}");
        await Task.Delay(1000);
        Console.WriteLine($"[Work-{id}] Finished on Thread {Environment.CurrentManagedThreadId}");
        return id;
    }


    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}