using System.Threading.Channels;

namespace Day05.ChannelsProducerConsumer;

public static class Ex02_MultiConsumer
{
    private static readonly Lock ConsoleLock = new();

    public static async Task RunAsync()
    {
        PrintSection("1. Competing Consumers: 1 Producer, 3 Consumers (30 Items)");

        // TODO 1: Create an unbounded channel: var channel = Channel.CreateUnbounded<int>();
        //
        // Start 1 Producer task:
        //   - Loop i from 1 to 30:
        //       await channel.Writer.WriteAsync(i);
        //       await Task.Delay(50);
        //   - Call channel.Writer.Complete();
        //
        // Start 3 Consumer tasks using Enumerable.Range(1, 3).Select(consumerId => Task.Run(async () => ...)).ToArray():
        //   - Note: 'consumerId' is a lambda parameter here, so there is NO closure trap!
        //   - Keep a local 'int processedCount = 0;' inside each consumer.
        //   - Consume with: await foreach (int item in channel.Reader.ReadAllAsync())
        //     {
        //         processedCount++;
        //         PrintColored($"  [Consumer-{consumerId}] Processing item {item:D2} (Thread {Environment.CurrentManagedThreadId})", GetConsumerColor(consumerId));
        //         // Simulate variable processing time so work distribution becomes visible:
        //         await Task.Delay(Random.Shared.Next(100, 250));
        //     }
        //   - Return processedCount from each consumer Task (so consumers array is Task<int>[]).
        //
        // Await producer and all consumers:
        //   await producer;
        //   int[] counts = await Task.WhenAll(consumers);
        //
        // TODO 2: Print summary of how many items each consumer processed AND the total sum (counts.Sum()).
        // Observe:
        //   1) Did two consumers EVER process the same number? (No -> work distribution, not broadcast)
        //   2) Does counts.Sum() equal 30?
        //   3) Is the distribution roughly balanced based on who finished earlier?

        var channel = Channel.CreateUnbounded<int>();

        var producer = Task.Run(async () =>
        {
            for (int i = 1; i < 31; i++)
            {
                await channel.Writer.WriteAsync(i);
                await Task.Delay(50); // Removed: producer adds all items at once, so faster consumers grab more items.
            }

            channel.Writer.Complete();
        });

        var consumers = Enumerable.Range(1, 3).Select(consumerId => Task.Run(async () =>
        {
            int processedCount = 0;
            await foreach (var item in channel.Reader.ReadAllAsync())
            {
                processedCount++;
                PrintColored(
                    $"  [Consumer-{consumerId}] Processing item {item:D2} (Thread {Environment.CurrentManagedThreadId})",
                    GetConsumerColor(consumerId));
                await Task.Delay(Random.Shared.Next(100, 300));
            }

            return (Id: consumerId, Count: processedCount);
        })).ToArray();

        await producer;
        var results = await Task.WhenAll(consumers);

        Console.WriteLine();
        foreach (var res in results)
        {
            PrintColored($"  Consumer-{res.Id} processed {res.Count} items.", GetConsumerColor(res.Id));
        }

        int total = results.Sum(r => r.Count);
        Console.WriteLine($"\n  Total processed: {total}");

        // WHY DID EACH CONSUMER PROCESS ~10 ITEMS? (Pacing / Timing Math)
        // - Producer pace: 1 item every 50ms (~4 items per 200ms).
        // - 3 Consumers pace: ~200ms per item each (~3 items processed per 200ms).
        // - Since production and consumption rates match, the queue never builds up.
        // - Consumers pick up items in a natural Round-Robin rhythm (C1 -> C2 -> C3), resulting in an even 10-10-10 split.


        Console.WriteLine("\n[Done] Ex02 finished.");
    }

    private static ConsoleColor GetConsumerColor(int id) => id switch
    {
        1 => ConsoleColor.Green,
        2 => ConsoleColor.Yellow,
        3 => ConsoleColor.Magenta,
        _ => ConsoleColor.White
    };

    private static void PrintColored(string message, ConsoleColor color = ConsoleColor.White)
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