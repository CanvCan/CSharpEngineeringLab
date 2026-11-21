namespace Day04.TasksAndAsync;

public static class Ex05_Cancellation
{
    public static async Task RunAsync()
    {
        PrintSection("1. Cancelling an Async I/O Operation (Task.Delay + CancelAfter)");

        // TODO 1: Create: using var cts = new CancellationTokenSource();
        // Schedule automatic cancellation after 2 seconds: cts.CancelAfter(2000);
        // Inside a try/catch:
        //   - Print "Starting 10-second work (will be cancelled at 2s)..."
        //   - Call: await Task.Delay(10_000, cts.Token);
        //   - Print "Completed!" (this line should NOT execute)
        // Catch OperationCanceledException (catches both OperationCanceledException and TaskCanceledException):
        //   - Print in Yellow: $"Cancelled! Exception type: {ex.GetType().Name}"

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(2000);

        try
        {
            Console.WriteLine("Starting 10-second work (will be cancelled at 2s)...");
            await Task.Delay(10_000, cts.Token);
            Console.WriteLine("Completed!");
        }
        catch (OperationCanceledException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Cancelled! Exception type: {ex.GetType().Name}");
            Console.ResetColor();
        }


        PrintSection("2. Cancelling a CPU-Bound Loop (ThrowIfCancellationRequested & Task.Status)");

        // TODO 2: Create a new CTS: using var cts2 = new CancellationTokenSource();
        // Schedule cancellation after 500ms: cts2.CancelAfter(500);
        // Start a CPU-bound Task and store it in a variable (pass cts2.Token to Task.Run as 2nd arg!):
        //
        // Task cpuTask = Task.Run(() =>
        // {
        //     for (int i = 0; i < 1_000_000_000; i++)
        //     {
        //         cts2.Token.ThrowIfCancellationRequested();
        //         // (Experiment: Comment out the line above once and see that the loop IGNORES CancelAfter!)
        //     }
        // }, cts2.Token);
        //
        // Await cpuTask inside try/catch (OperationCanceledException).
        // After the try/catch, print:
        //   $"cpuTask.Status = {cpuTask.Status} | IsCanceled = {cpuTask.IsCanceled} | IsFaulted = {cpuTask.IsFaulted}"
        // Observe: Status is 'Canceled', NOT 'Faulted'!

        using var cts2 = new CancellationTokenSource();
        cts2.CancelAfter(500);

        Task cpuTask = Task.Run(() =>
        {
            for (int i = 0; i < 1_000_000_000; i++)
            {
                cts2.Token.ThrowIfCancellationRequested();
            }
        }, cts2.Token);

        try
        {
            await cpuTask;
        }
        catch (OperationCanceledException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Cancelled! Exception type: {ex.GetType().Name}");
            Console.ResetColor();
        }

        Console.WriteLine(
            $"cpuTask.Status = {cpuTask.Status} " +
            $"| IsCanceled = {cpuTask.IsCanceled} " +
            $"| IsFaulted = {cpuTask.IsFaulted}");


        PrintSection("3. Graceful Exit without Exception (IsCancellationRequested)");

        // TODO 3: Create: using var cts3 = new CancellationTokenSource();
        // Call cts3.CancelAfter(1200);
        // Run a while (!cts3.Token.IsCancellationRequested) loop:
        //   - Print $"Working... step {step++}"
        //   - Use Thread.Sleep(300) (or await Task.Delay(300) WITHOUT passing the token so it finishes the current step)
        // After the loop ends naturally, print: "Loop exited gracefully without throwing an exception!"


        using var cts3 = new CancellationTokenSource();
        cts3.CancelAfter(2000);

        int step = 0;

        while (!cts3.Token.IsCancellationRequested)
        {
            Console.WriteLine($"Working... step {step++}");
            Thread.Sleep(250);
        }

        Console.WriteLine("\nLoop exited gracefully without throwing an exception!");


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