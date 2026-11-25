using System.Threading.Channels;

namespace Day05.ChannelsProducerConsumer;

public static class Ex03_MultiProducerComplete
{
    private static readonly Lock ConsoleLock = new();

    public static async Task RunAsync()
    {
        PrintSection("1. Correct Pattern: 3 Producers, 2 Consumers (Complete After Task.WhenAll)");

        // TODO 1: Create an unbounded channel: var channel = Channel.CreateUnbounded<string>();
        //
        // Start 2 Consumer tasks:
        //   - Each reads via ReadAllAsync() and prints $"  [Consumer-{id}] Processed: {msg}"
        //   - Return the count of processed messages per consumer (or just Task).
        //
        // Start 3 Producer tasks (use the helper ProduceAsync(id, count, delayMs, channel.Writer) below):
        //   - Producer 1: 5 items, 40ms delay
        //   - Producer 2: 5 items, 80ms delay
        //   - Producer 3: 5 items, 120ms delay
        //   - IMPORTANT: Producers must NOT call Complete() inside themselves!
        //
        // Wait for ALL producers to finish FIRST:
        //   await Task.WhenAll(producers);
        //   PrintColored("[Coordinator] All producers finished. Calling Writer.Complete()...", ConsoleColor.Yellow);
        //   channel.Writer.Complete();
        //
        // Wait for the channel to be completely drained using Reader.Completion:
        //   await channel.Reader.Completion;
        //   PrintColored("[Coordinator] channel.Reader.Completion finished (queue fully drained).", ConsoleColor.Green);
        //   await Task.WhenAll(consumers);


        // 1. Create the shared unbounded channel
        var channel = Channel.CreateUnbounded<string>();

        // 2. Start 2 Consumer tasks FIRST so they are ready to listen immediately
        var consumers = Enumerable.Range(1, 2).Select(id => Task.Run(async () =>
        {
            int count = 0;
            await foreach (var msg in channel.Reader.ReadAllAsync())
            {
                count++;
                PrintColored($"  [Consumer-{id}] Processed: {msg}", ConsoleColor.Cyan);
            }

            return (Id: id, Count: count);
        })).ToArray();

        // 3. Start 3 Producer tasks (none of them call Complete() inside!)
        Task[] producers =
        [
            ProduceAsync(1, 5, 40, channel.Writer),
            ProduceAsync(2, 5, 80, channel.Writer),
            ProduceAsync(3, 5, 120, channel.Writer)
        ];

        // 4. Wait for ALL producers to finish writing before closing the channel
        await Task.WhenAll(producers);
        PrintColored("\n[Coordinator] All producers finished. Calling Writer.Complete()...", ConsoleColor.Yellow);
        channel.Writer.Complete();

        // 5. Wait until the channel is closed AND every item in the queue has been read
        await channel.Reader.Completion;
        PrintColored("[Coordinator] channel.Reader.Completion finished (queue fully drained).", ConsoleColor.Green);

        // 6. Wait for all consumers to exit and collect their processed item counts
        var results = await Task.WhenAll(consumers);

        PrintColored();
        foreach (var res in results)
        {
            PrintColored($"  Consumer-{res.Id} processed {res.Count} items.", ConsoleColor.Cyan);
        }

        int total = results.Sum(r => r.Count);
        PrintColored($"\n  Total processed: {total}");

        /*
         * 1. Reader.Completion vs Task.WhenAll(consumers):
         * a. await channel.Reader.Completion ->
         * Finishes as soon as the queue is empty (last item is pulled).
         * b. await Task.WhenAll(consumers) ->
         * Finishes only AFTER consumers finish processing that last item and exit the loop.
         *
         * await foreach (var msg in channel.Reader.ReadAllAsync())
         * {
         *    // 1. As soon as the last item is pulled here, the queue is empty -> Reader.Completion finishes!
         *
         *    await Task.Delay(500); // 2. But the consumer keeps working on that last item for 500ms more...
         * }
         * // 3. Only when it exits here does the Consumer Task finish -> Task.WhenAll(consumers) completes!
         *
         * 2. No extra Task.Run needed for async I/O methods:
         *    ProduceAsync already returns a Task, so putting ProduceAsync(...) directly into Task[]
         *    is cleaner and avoids unnecessary thread pool overhead.
         *
         *    Pure Async Rule:
         *    - I/O-bound (already async): Call directly without Task.Run to avoid wasting ThreadPool threads.
         *    - CPU-bound (heavy sync work): Wrap in Task.Run to offload to a background thread.
         */


        PrintSection("2. Anti-Pattern: First Producer Calls Complete() Early -> ChannelClosedException");

        // TODO 2: Create a second channel: var brokenChannel = Channel.CreateUnbounded<string>();
        //
        // Start 1 Consumer reading from brokenChannel.Reader.ReadAllAsync().
        //
        // Start 2 Producers on brokenChannel:
        //   - FastProducer (30ms delay, 3 items): calls brokenChannel.Writer.Complete() right when it finishes!
        //   - SlowProducer (100ms delay, 5 items): wraps WriteAsync in try/catch (ChannelClosedException ex)
        //     and prints in Red: $"[SlowProducer] CRASHED! {ex.GetType().Name}: {ex.Message}"
        //
        // Await both producers and the consumer.
        // Observe: FastProducer closes the door while SlowProducer is still trying to write.

        var brokenChannel = Channel.CreateUnbounded<string>();

        var consumer = Task.Run(async () =>
        {
            await foreach (var item in brokenChannel.Reader.ReadAllAsync())
            {
                await Task.Delay(10); // Gives the producer time to print its log first
                PrintColored($"  [Consumer] Processed: {item}", ConsoleColor.Cyan);
            }
        });

        var fastProducer = Task.Run(async () =>
        {
            for (int i = 1; i <= 3; i++)
            {
                string msg = $"Fast-Msg{i}";
                await brokenChannel.Writer.WriteAsync(msg);
                PrintColored($"[FastProducer] Wrote {msg}", ConsoleColor.DarkGray);
                await Task.Delay(30);
            }

            PrintColored("[FastProducer] Finished! Calling Complete() too early...", ConsoleColor.Yellow);
            brokenChannel.Writer.Complete(); // MISTAKE: Closes the door while SlowProducer is still running!
        });

        var slowProducer = Task.Run(async () =>
        {
            try
            {
                for (int i = 1; i <= 5; i++)
                {
                    string msg = $"Slow-Msg{i}";
                    await brokenChannel.Writer.WriteAsync(msg);
                    PrintColored($"[SlowProducer] Wrote {msg}", ConsoleColor.DarkGray);
                    await Task.Delay(100);
                }
            }
            catch (ChannelClosedException ex)
            {
                PrintColored($"[SlowProducer] CRASHED! {ex.GetType().Name}: {ex.Message}", ConsoleColor.Red);
            }
        });

        await Task.WhenAll(fastProducer, slowProducer);
        await consumer;


        Console.WriteLine("\n[Done] Ex03 finished.");
    }

    // Helper for TODO 1 (Notice: accepts ChannelWriter<string>, NOT the whole Channel!)

    // Reusable producer helper: takes only ChannelWriter<T> (cannot read, only write).
    // Does NOT call writer.Complete() inside because other producers might still be writing!
    private static async Task ProduceAsync(int producerId, int count, int delayMs, ChannelWriter<string> writer)
    {
        for (int i = 1; i <= count; i++)
        {
            string msg = $"P{producerId}-Msg{i}"; // Tag message with producer ID
            await writer.WriteAsync(msg); // Push to channel
            PrintColored($"[Producer-{producerId}] Wrote {msg}", ConsoleColor.DarkGray);
            await Task.Delay(delayMs); // Control producer speed
        }
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