namespace Day04.TasksAndAsync;

public static class Ex06_ExceptionsAndAsyncVoid
{
    public static async Task RunAsync()
    {
        PrintSection("1. Task.WhenAll with Multiple Exceptions (InnerExceptions)");

        // TODO 1: Start 3 tasks via Task.WhenAll and store the combined Task in a variable BEFORE awaiting:
        //   Task allTask = Task.WhenAll(
        //       FailAfterAsync("Service-A", 100),
        //       SucceedAfterAsync("Service-B", 150),
        //       FailAfterAsync("Service-C", 200)
        //   );
        //
        // Inside try { await allTask; } catch (Exception ex):
        //   - Print what 'await' caught: $"Caught by await: {ex.Message}" (Notice: ONLY the first exception!)
        //   - Loop through allTask.Exception!.InnerExceptions and print ALL exceptions + total count.

        Task allTask = Task.WhenAll(
            FailAfterAsync("Service-A", 100),
            SucceedAfterAsync("Service-B", 150),
            FailAfterAsync("Service-C", 200)
        );

        try
        {
            await allTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught by await: {ex.Message}");
            Console.WriteLine();
        }

        foreach (var item in allTask.Exception!.InnerExceptions)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Exception: {item}");
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine($"Total Exception Count: {allTask.Exception!.InnerExceptions.Count}");


        PrintSection("2. Fire-and-Forget Trap (Missing await -> Compiler Warning CS4014)");

        // TODO 2: Call FailAfterAsync("ForgottenTask", 100); WITHOUT 'await' inside a try/catch block.
        // Then wait 300ms (await Task.Delay(300)) so the task has time to throw in the background.
        // Observe:
        //   1) Visual Studio shows warning CS4014 on the unawaited call.
        //   2) The catch block is NEVER hit! The exception is silently lost (unobserved).
        // Print a line confirming execution continued without catching the error.

        try
        {
            FailAfterAsync("ForgottenTask", 100);
            await Task.Delay(300);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }


        PrintSection("3. The async void Trap (Uncatchable -> Crashes the Process)");

        // TODO 3: Keep 'triggerCrash = false' while testing TODO 1 and TODO 2.
        // Once they work, set 'triggerCrash = true' ONCE and run the app!
        // Observe: Even though FailVoidAsync() is inside try/catch, the catch block CANNOT catch it
        // and the entire console application crashes with an unhandled exception.
        // After observing the crash, set it back to false.

        bool triggerCrash = false;

        if (triggerCrash)
        {
            try
            {
                Console.WriteLine("Calling async void method inside try/catch...");
                FailVoidAsync();
                await Task.Delay(500); // Give it time to throw and crash the process
            }
            catch (Exception ex)
            {
                // This will NEVER execute!
                Console.WriteLine($"Caught async void? {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Skipped async void crash demo (set triggerCrash = true to observe process crash).");
        }

        Console.WriteLine("\n[Done] Ex06 finished.");
    }

    private static async Task SucceedAfterAsync(string name, int delayMs)
    {
        await Task.Delay(delayMs);
        Console.WriteLine($"  [{name}] Succeeded.");
    }

    private static async Task FailAfterAsync(string name, int delayMs)
    {
        await Task.Delay(delayMs);
        throw new InvalidOperationException($"{name} failed!");
    }

    private static async void FailVoidAsync()
    {
        await Task.Delay(100);
        throw new InvalidOperationException("BOOM from async void!");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}