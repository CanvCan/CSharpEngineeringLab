using System.Threading.Channels;

namespace Day05.ChannelsProducerConsumer;

public static class Ex01_BasicChannel
{
    public static async Task RunAsync()
    {
        PrintSection("1. Basic Unbounded Channel (WriteAsync, Complete, ReadAllAsync)");

        // TODO 1: Create an unbounded channel: var channel = Channel.CreateUnbounded<int>();
        //
        // Start a Producer task (Task.Run(async () => ...)):
        //   - Loop i from 1 to 10:
        //       await channel.Writer.WriteAsync(i);
        //       Print $"[Producer] Wrote {i}"
        //       await Task.Delay(100);
        //   - Call channel.Writer.Complete();
        //     (Experiment once: comment out Complete() and observe that the consumer hangs forever!)
        //
        // Start a Consumer task (Task.Run(async () => ...)):
        //   - Use: await foreach (int item in channel.Reader.ReadAllAsync())
        //       Print $"  [Consumer] Read {item}"
        //   - After the loop, print "  [Consumer] Channel completed. Exiting cleanly."
        //
        // Await both tasks with: await Task.WhenAll(producer, consumer);

        var channel = Channel.CreateUnbounded<int>();

        var producer = Task.Run(async () =>
        {
            for (int i = 1; i < 6; i++)
            {
                await channel.Writer.WriteAsync(i);
                Console.WriteLine($"[Producer] Wrote {i}");
                await Task.Delay(100);
            }

            channel.Writer.Complete();
        });

        var consumer = Task.Run(async () =>
        {
            await foreach (var item in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"  [Consumer] Read {item}");
            }

            Console.WriteLine();
            Console.WriteLine("  [Consumer] Channel completed. Exiting cleanly.");
        });

        await Task.WhenAll(producer, consumer);

        PrintSection("2. Writing After Complete() -> ChannelClosedException");

        // TODO 2: Using the already completed 'channel' from TODO 1:
        //   - Call: bool accepted = channel.Writer.TryWrite(99);
        //     Print $"TryWrite(99) on completed channel returned: {accepted}" (expect: False)
        //   - Inside try/catch (ChannelClosedException ex):
        //       await channel.Writer.WriteAsync(99);
        //     Print in Red: $"WriteAsync threw {ex.GetType().Name}: {ex.Message}"

        bool accepted = channel.Writer.TryWrite(99);

        Console.WriteLine($"TryWrite(99) on completed channel returned: {accepted}");
        Console.WriteLine();

        try
        {
            await channel.Writer.WriteAsync(99);
        }
        catch (ChannelClosedException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"WriteAsync threw {ex.GetType().Name}: {ex.Message}");
            Console.ResetColor();
        }


        PrintSection("3. Under the Hood: Manual Reading (WaitToReadAsync + TryRead)");

        // TODO 3: Create a second unbounded channel: var manualChannel = Channel.CreateUnbounded<int>();
        //
        // Producer task:
        //   - Write numbers 1 to 5 (with 80ms delay between each), then call manualChannel.Writer.Complete();
        //
        // Consumer task:
        //   - Instead of ReadAllAsync(), use the manual two-loop pattern:
        //       while (await manualChannel.Reader.WaitToReadAsync())
        //       {
        //           while (manualChannel.Reader.TryRead(out int x))
        //           {
        //               Console.WriteLine($"  [Manual Consumer] Pulled {x}");
        //           }
        //       }
        //   - Print "  [Manual Consumer] WaitToReadAsync returned false. Done."
        //
        // Await both tasks with Task.WhenAll.

        var manualChannel = Channel.CreateUnbounded<int>();

        var manuelProducer = Task.Run(async () =>
        {
            for (int i = 1; i < 6; i++)
            {
                await manualChannel.Writer.WriteAsync(i);
                Console.WriteLine($"[Producer] Wrote {i}");
                await Task.Delay(80);
            }

            manualChannel.Writer.Complete();
        });

        var manuelConsumer = Task.Run(async () =>
        {
            while (await manualChannel.Reader.WaitToReadAsync())
            {
                while (manualChannel.Reader.TryRead(out int item))
                {
                    Console.WriteLine($"  [Manual Consumer] Pulled {item}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("  [Manual Consumer] WaitToReadAsync returned false. Done.");
        });

        await Task.WhenAll(manuelProducer, manuelConsumer);


        Console.WriteLine("\n[Done] Ex01 finished.");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}