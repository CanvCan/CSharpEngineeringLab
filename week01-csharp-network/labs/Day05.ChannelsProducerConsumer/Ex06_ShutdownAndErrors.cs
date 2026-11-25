using System.Threading.Channels;

namespace Day05.ChannelsProducerConsumer;

public static class Ex06_ShutdownAndErrors
{
    private static readonly Lock ConsoleLock = new();

    public static async Task RunAsync()
    {
        PrintSection("1. Graceful Shutdown (Ctrl+C / CancelAfter) -> Drain & Exit");

        // TODO 1: Create a bounded channel (capacity 10) and a CancellationTokenSource:
        //   var channel = Channel.CreateBounded<int>(10);
        //   using var cts = new CancellationTokenSource();
        //
        // Hook Ctrl+C so pressing it triggers cooperative cancellation instead of killing the process:
        //   ConsoleCancelEventHandler handler = (_, e) =>
        //   {
        //       e.Cancel = true;
        //       PrintColored("\n[Ctrl+C] Cancellation requested! Stopping producer, draining queue...", ConsoleColor.Yellow);
        //       cts.Cancel();
        //   };
        //   Console.CancelKeyPress += handler;
        //
        // (Optional convenience: also call cts.CancelAfter(2500) OR test by pressing Ctrl+C manually within 3 seconds!)
        //
        // Producer task (receives cts.Token):
        //   - try
        //     {
        //         int i = 1;
        //         while (true)
        //         {
        //             await channel.Writer.WriteAsync(i, cts.Token);
        //             PrintColored($"[Producer] Wrote {i:D2}", ConsoleColor.DarkGray);
        //             i++;
        //             await Task.Delay(150, cts.Token);
        //         }
        //     }
        //     catch (OperationCanceledException)
        //     {
        //         PrintColored("[Producer] Stopped producing due to cancellation.", ConsoleColor.Yellow);
        //     }
        //     finally
        //     {
        //         channel.Writer.Complete(); // Crucial: allows consumer to drain remaining items and exit!
        //     }
        //
        // Consumer task (NO cts.Token passed to ReadAllAsync -> Drain & Exit strategy!):
        //   - await foreach (int item in channel.Reader.ReadAllAsync())
        //     {
        //         PrintColored($"  [Consumer] Processing {item:D2}...", ConsoleColor.Cyan);
        //         await Task.Delay(350); // Slower than producer so some items stay buffered when cancelled
        //     }
        //   - PrintColored("  [Consumer] Drained all remaining items and exited cleanly.", ConsoleColor.Green);
        //
        // Await both tasks, then unsubscribe the event handler in a finally block:
        //   Console.CancelKeyPress -= handler;

        var channel = Channel.CreateBounded<int>(10);
        using var cts = new CancellationTokenSource();

        ConsoleCancelEventHandler handler = (_, e) =>
        {
            e.Cancel = true;
            PrintColored("\n[Ctrl+C] Cancellation requested! Stopping producer, draining queue...",
                ConsoleColor.Yellow);
            cts.Cancel();
        };

        Console.CancelKeyPress += handler;

        try
        {
            var producer = Task.Run(async () =>
            {
                try
                {
                    int i = 1;
                    while (true)
                    {
                        await channel.Writer.WriteAsync(i, cts.Token);
                        PrintColored($"[Producer] Wrote {i:D2}", ConsoleColor.DarkGray);
                        i++;
                        await Task.Delay(150, cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    PrintColored("\n[Producer] Stopped producing due to cancellation.\n", ConsoleColor.Yellow);
                }
                finally
                {
                    channel.Writer.Complete();
                }
            });

            var consumer = Task.Run(async () =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync())
                {
                    PrintColored($"  [Consumer] Processing {item:D2}...", ConsoleColor.Cyan);
                    await Task.Delay(350);
                }

                PrintColored("  [Consumer] Drained all remaining items and exited cleanly.", ConsoleColor.Green);
            });

            await Task.WhenAll(producer, consumer);
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }


        PrintSection("2. Propagating Producer Exceptions via Writer.Complete(ex)");

        // TODO 2: Create an unbounded channel: var errChannel = Channel.CreateUnbounded<int>();
        //
        // Producer task:
        //   - try
        //     {
        //         for (int i = 1; i <= 6; i++)
        //         {
        //             if (i == 4)
        //                 throw new InvalidOperationException("Sensor disconnected at packet #4!");
        //
        //             await errChannel.Writer.WriteAsync(i);
        //             PrintColored($"[Producer] Wrote {i}", ConsoleColor.DarkGray);
        //             await Task.Delay(100);
        //         }
        //         errChannel.Writer.Complete();
        //     }
        //     catch (Exception ex)
        //     {
        //         PrintColored($"[Producer] Faulted! Passing exception to channel: {ex.Message}", ConsoleColor.Red);
        //         errChannel.Writer.Complete(ex); // Propagates 'ex' to the consumer!
        //     }
        //
        // Consumer task:
        //   - try
        //     {
        //         await foreach (int item in errChannel.Reader.ReadAllAsync())
        //         {
        //             PrintColored($"  [Consumer] Processed healthy item {item}", ConsoleColor.Green);
        //             await Task.Delay(250);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         PrintColored($"  [Consumer] Caught propagated exception: {ex.GetType().Name} - {ex.Message}", ConsoleColor.Red);
        //     }
        //
        // Await both tasks.
        // OBSERVE: Consumer finishes processing healthy items 1, 2, 3 FIRST, and ONLY THEN receives the exception!

        var errChannel = Channel.CreateUnbounded<int>();

        var producerA = Task.Run(async () =>
        {
            try
            {
                for (int i = 1; i <= 6; i++)
                {
                    if (i == 4)
                        throw new InvalidOperationException("Sensor disconnected at packet #4!");

                    await errChannel.Writer.WriteAsync(i);
                    PrintColored($"[Producer] Wrote {i}", ConsoleColor.DarkGray);
                    await Task.Delay(100);
                }

                errChannel.Writer.Complete();
            }
            catch (Exception ex)
            {
                PrintColored($"[Producer] Faulted! Passing exception to channel: {ex.Message}", ConsoleColor.Red);
                errChannel.Writer.Complete(ex); // Propagates 'ex' to the consumer!
            }
        });

        var consumerA = Task.Run(async () =>
        {
            try
            {
                await foreach (int item in errChannel.Reader.ReadAllAsync())
                {
                    PrintColored($"  [Consumer] Processed healthy item {item}", ConsoleColor.Green);
                    await Task.Delay(250);
                }
            }
            catch (Exception ex)
            {
                PrintColored($"  [Consumer] Caught propagated exception: {ex.GetType().Name} - {ex.Message}",
                    ConsoleColor.Red);
            }
        });

        await Task.WhenAll(producerA, consumerA);

        Console.WriteLine("\n[Done] Ex06 finished.");
    }

    private static void PrintColored(string message = "", ConsoleColor color = ConsoleColor.White)
    {
        lock (ConsoleLock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
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