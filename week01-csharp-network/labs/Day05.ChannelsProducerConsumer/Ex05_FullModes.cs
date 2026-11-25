using System.Diagnostics;
using System.Threading.Channels;

namespace Day05.ChannelsProducerConsumer;

public static class Ex05_FullModes
{
    private static readonly Lock ConsoleLock = new();

    public static async Task RunAsync()
    {
        PrintSection("BoundedChannelFullMode Comparison (100 Items, Capacity = 5)");

        // TODO 1: Run all 4 modes sequentially using your helper method below:
        //   await RunModeTestAsync(BoundedChannelFullMode.Wait);
        //   await RunModeTestAsync(BoundedChannelFullMode.DropOldest);
        //   await RunModeTestAsync(BoundedChannelFullMode.DropNewest);
        //   await RunModeTestAsync(BoundedChannelFullMode.DropWrite);
        //
        // OBSERVE across all 4 outputs:
        //   1) Does (readItems.Count + droppedCount == 100) ALWAYS hold?
        //   2) In Wait mode: dropped == 0, slowest elapsed time (producer forced to slow down).
        //   3) In DropOldest: newest numbers survive (e.g., ends with ...98, 99, 100) -> ideal for live telemetry.
        //   4) In DropWrite: oldest numbers survive (new writes get rejected while full).

        await RunModeTestAsync(BoundedChannelFullMode.Wait);
        await RunModeTestAsync(BoundedChannelFullMode.DropOldest);
        await RunModeTestAsync(BoundedChannelFullMode.DropNewest);
        await RunModeTestAsync(BoundedChannelFullMode.DropWrite);

        Console.WriteLine("\n[Done] Ex05 finished.");
    }

    private static async Task RunModeTestAsync(BoundedChannelFullMode mode)
    {
        const int totalItems = 100;
        const int capacity = 5;
        int droppedCount = 0;


        // TODO 2: Track dropped items safely with: long droppedCount = 0;
        // Create BoundedChannelOptions(capacity) with FullMode = mode.
        // Create the channel with the itemDropped callback:
        //   var channel = Channel.CreateBounded<int>(
        //       options,
        //       itemDropped: _ => Interlocked.Increment(ref droppedCount));
        //
        // Start a Stopwatch.
        //
        // Producer task:
        //   - Loop i from 1 to totalItems:
        //       await channel.Writer.WriteAsync(i);
        //       await Task.Delay(2); // Fast producer (~2ms per item)
        //   - Call channel.Writer.Complete();
        //
        // Consumer task:
        //   - Create a local List<int> received = new();
        //   - Read with await foreach (int item in channel.Reader.ReadAllAsync()):
        //       received.Add(item);
        //       await Task.Delay(15); // Slower consumer (15ms per item)
        //   - Return received.
        //
        // Await producer and consumer:
        //   await producer;
        //   List<int> readItems = await consumer;
        // Stop Stopwatch.

        var options = new BoundedChannelOptions(capacity) { FullMode = mode };

        var channel = Channel.CreateBounded<int>(options, itemDropped: _ => Interlocked.Increment(ref droppedCount));

        Stopwatch sw = Stopwatch.StartNew();

        var producer = Task.Run(async () =>
        {
            for (int i = 1; i <= totalItems; i++)
            {
                await channel.Writer.WriteAsync(i);
                await Task.Delay(2); // Fast producer (~2ms per item)
            }

            channel.Writer.Complete();
        });

        var consumer = Task.Run(async () =>
        {
            List<int> received = new();
            await foreach (int item in channel.Reader.ReadAllAsync())
            {
                received.Add(item);
                await Task.Delay(32); // Slower consumer (32ms per item)
            }

            return received;
        });

        await producer;
        List<int> readItems = await consumer;

        sw.Stop();
        double msElapsed = sw.ElapsedMilliseconds;

        // TODO 3: Print a clean summary block for this mode:
        //   - Mode name & elapsed ms
        //   - Read: {readItems.Count} | Dropped: {droppedCount} | Check (Read + Dropped): {readItems.Count + droppedCount}
        //   - First 5 received: string.Join(", ", readItems.Take(5))
        //   - Last  5 received: string.Join(", ", readItems.TakeLast(5))

        PrintColored($"\n=== Mode: {mode} " +
                     $"| Elapsed: {msElapsed} ms ===", ConsoleColor.Yellow);

        PrintColored($"Read: {readItems.Count} " +
                     $"| Dropped: {droppedCount} " +
                     $"| Check (Read + Dropped): {readItems.Count + droppedCount}");

        PrintColored($"First 10 received: {string.Join(", ", readItems.Take(10))}", ConsoleColor.Green);
        PrintColored($"Last  10 received: {string.Join(", ", readItems.TakeLast(10))}", ConsoleColor.Green);

        /*
         *   Note: Windows Timer Resolution & Task.Delay Rounding
         *
         * - Reason: Windows has a default system timer resolution of ~15.6 ms.
         * - Impact: Even if you write `await Task.Delay(2)`, Windows rounds it up to the minimum
         *   tick (~15.6 ms). Since the consumer's `Task.Delay(15)` also takes ~15.6 ms, both run
         *   at practically the same speed, so the channel rarely overflows and mode differences aren't obvious.
         * - Solution / Tip: To clearly observe real overflow (backpressure / drop behavior), remove
         *   `Task.Delay(2)` from the producer (or increase the consumer delay to 50–100 ms).
         */
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