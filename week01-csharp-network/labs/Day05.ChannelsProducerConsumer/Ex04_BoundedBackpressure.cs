using System.Threading.Channels;

namespace Day05.ChannelsProducerConsumer;

public static class Ex04_BoundedBackpressure
{
    private static readonly Lock ConsoleLock = new();

    public static async Task RunAsync()
    {
        PrintSection("1. Bounded Channel (Capacity = 5) & Backpressure in Action");

        // TODO 1: Create a bounded channel with capacity 5 (default FullMode is Wait):
        //   var channel = Channel.CreateBounded<int>(5);
        //
        // Start a FAST Producer task (no delay between writes, writing 1 to 12):
        //   - For i = 1 to 12:
        //       PrintColored($"[{DateTime.Now:HH:mm:ss.fff}] [Producer] Writing {i:D2}...", ConsoleColor.Yellow);
        //       await channel.Writer.WriteAsync(i);
        //       PrintColored($"[{DateTime.Now:HH:mm:ss.fff}] [Producer] Wrote   {i:D2} ✓", ConsoleColor.Green);
        //   - Call channel.Writer.Complete();
        //
        // Start a SLOW Consumer task:
        //   - Give the producer a tiny head start (e.g., await Task.Delay(100)) so the buffer fills up first.
        //   - Read with await foreach (var item in channel.Reader.ReadAllAsync()):
        //       PrintColored($"[{DateTime.Now:HH:mm:ss.fff}]   [Consumer] Pulled {item:D2} (processing 500ms...)", ConsoleColor.Cyan);
        //       await Task.Delay(500);
        //
        // Await both tasks with Task.WhenAll(producer, consumer).
        //
        // OBSERVE the timestamps:
        //   - Items 1..5 are written almost instantaneously (buffer fills from 0 to 5).
        //   - Starting at item 6, "Writing 06..." prints immediately, then "Wrote 06 ✓" WAITS ~500ms
        //     until the consumer frees a slot! (Non-blocking backpressure via await).

        var channel = Channel.CreateBounded<int>(5);

        var fastProducer = Task.Run(async () =>
        {
            for (int i = 0; i < 12; i++)
            {
                PrintColored($"[{DateTime.Now:HH:mm:ss.fff}] [Producer] Writing {i:D2}...", ConsoleColor.Yellow);
                await channel.Writer.WriteAsync(i);
                PrintColored($"[{DateTime.Now:HH:mm:ss.fff}] [Producer] Wrote   {i:D2}   ", ConsoleColor.Green);
            }

            channel.Writer.Complete();
        });

        var slowConsumer = Task.Run(async () =>
        {
            await Task.Delay(100);
            await foreach (var item in channel.Reader.ReadAllAsync())
            {
                PrintColored($"[{DateTime.Now:HH:mm:ss.fff}]   [Consumer] Pulled {item:D2} (processing 500ms...)",
                    ConsoleColor.Cyan);
                await Task.Delay(500);
            }
        });

        await Task.WhenAll(fastProducer, slowConsumer);

        PrintSection("2. Non-Blocking Write Attempt: TryWrite on a Full Bounded Channel");

        // TODO 2: Create a small bounded channel with capacity 3:
        //   var smallChannel = Channel.CreateBounded<int>(3);
        //   (No consumer reading yet!)
        //
        // Loop i from 1 to 5:
        //   - Call: bool ok = smallChannel.Writer.TryWrite(i);
        //   - Print $"TryWrite({i}) -> {ok}" (Green if true, Red if false).
        //
        // Observe: First 3 return True, 4th and 5th return False immediately without waiting!
        // Then drain remaining items using TryRead in a while loop and print them.

        var smallChannel = Channel.CreateBounded<int>(3);

        for (int i = 1; i < 6; i++)
        {
            bool ok = smallChannel.Writer.TryWrite(i);
            PrintColored($"  TryWrite({i}) -> {ok}", ok ? ConsoleColor.Green : ConsoleColor.Red);
        }

        PrintColored();
        while (smallChannel.Reader.TryRead(out int item))
        {
            PrintColored($"  TryRead -> {item}", ConsoleColor.Cyan);
        }

        Console.WriteLine("\n[Done] Ex04 finished.");
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