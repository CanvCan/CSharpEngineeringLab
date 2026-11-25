using System.Threading.Channels;

namespace Day05.ChannelsProducerConsumer;

public record Measurement(int Id, double RawTemp, DateTime Timestamp);

public record CleanMeasurement(int Id, double Celsius, bool IsAnomaly, DateTime Timestamp);

public static class Ex07_Pipeline
{
    private static readonly Lock ConsoleLock = new();

    public static async Task RunAsync()
    {
        PrintSection("3-Stage Pipeline: Producer -> Filter/Transform -> Consumer");

        // TODO 1: Create 2 bounded channels (capacity = 4 each) with SingleWriter = true, SingleReader = true:
        //   var rawChannel = Channel.CreateBounded<Measurement>(new BoundedChannelOptions(4)
        //   {
        //       SingleWriter = true,
        //       SingleReader = true,
        //       FullMode = BoundedChannelFullMode.Wait
        //   });
        //   var cleanChannel = Channel.CreateBounded<CleanMeasurement>(new BoundedChannelOptions(4)
        //   {
        //       SingleWriter = true,
        //       SingleReader = true,
        //       FullMode = BoundedChannelFullMode.Wait
        //   });

        var rawChannel = Channel.CreateBounded<Measurement>(new BoundedChannelOptions(4)
        {
            SingleWriter = true,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });

        var cleanChannel = Channel.CreateBounded<CleanMeasurement>(new BoundedChannelOptions(4)
        {
            SingleWriter = true,
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });


        // TODO 2: Stage 1 — Producer (writes 20 Measurements into rawChannel.Writer):
        //   - Generate random RawTemp values between -10.0 and 120.0 (e.g. Random.Shared.Next(-10, 121)).
        //     (Values < 0 or > 100 will be treated as corrupted sensor readings and filtered out in Stage 2!)
        //   - Write to rawChannel.Writer.WriteAsync(...) with a short delay (e.g. 50ms) and log in DarkGray.
        //   - Call rawChannel.Writer.Complete() in a finally block.

        var producer = Task.Run(async () =>
        {
            try
            {
                for (int i = 1; i <= 20; i++)
                {
                    var m = new Measurement(Id: i, RawTemp: Random.Shared.Next(-10, 121), Timestamp: DateTime.Now);
                    await rawChannel.Writer.WriteAsync(m);
                    PrintColored($"[Stage1-Producer] Wrote #{m.Id:D2} ({m.RawTemp}°C)", ConsoleColor.DarkGray);
                    await Task.Delay(50);
                }
            }
            finally
            {
                rawChannel.Writer.Complete();
            }
        });

        // TODO 3: Stage 2 — Filter & Transform (reads from rawChannel.Reader, writes to cleanChannel.Writer):
        //   - Track int filteredOutCount = 0;
        //   - Inside try:
        //       await foreach (var raw in rawChannel.Reader.ReadAllAsync())
        //       {
        //           if (raw.RawTemp < 0 || raw.RawTemp > 100)
        //           {
        //               filteredOutCount++;
        //               PrintColored($"  [Stage2-Filter] DROPPED invalid reading #{raw.Id:D2} ({raw.RawTemp}°C)", ConsoleColor.Red);
        //               continue;
        //           }
        //           bool isAnomaly = raw.RawTemp > 80;
        //           var clean = new CleanMeasurement(raw.Id, raw.RawTemp, isAnomaly, raw.Timestamp);
        //           await cleanChannel.Writer.WriteAsync(clean);
        //           PrintColored($"  [Stage2-Transform] Forwarded #{clean.Id:D2} ({clean.Celsius}°C, Anomaly={clean.IsAnomaly})", ConsoleColor.Yellow);
        //       }
        //   - Inside finally:
        //       cleanChannel.Writer.Complete(); // Cascading completion!
        //   - Return filteredOutCount.

        var filter = Task.Run(async () =>
        {
            int filteredOutCount = 0;
            try
            {
                await foreach (var raw in rawChannel.Reader.ReadAllAsync())
                {
                    if (raw.RawTemp < 0 || raw.RawTemp > 100)
                    {
                        filteredOutCount++;
                        PrintColored($"  [Stage2-Filter] DROPPED invalid reading #{raw.Id:D2} ({raw.RawTemp}°C)",
                            ConsoleColor.Red);
                        continue;
                    }

                    bool isAnomaly = raw.RawTemp > 80;
                    var clean = new CleanMeasurement(raw.Id, raw.RawTemp, isAnomaly, raw.Timestamp);
                    await cleanChannel.Writer.WriteAsync(clean);
                    PrintColored(
                        $"  [Stage2-Transform] Forwarded #{clean.Id:D2} ({clean.Celsius}°C, Anomaly={clean.IsAnomaly})",
                        ConsoleColor.Yellow);
                }
            }
            finally
            {
                cleanChannel.Writer.Complete(); // Cascading completion!
            }

            return filteredOutCount;
        });

        // TODO 4: Stage 3 — Slow Consumer (reads from cleanChannel.Reader):
        //   - Track int processedCount = 0; and int anomalyCount = 0;
        //   - await foreach (var item in cleanChannel.Reader.ReadAllAsync())
        //     {
        //         processedCount++;
        //         if (item.IsAnomaly) anomalyCount++;
        //         PrintColored($"    [Stage3-Consumer] Saved #{item.Id:D2} | {item.Celsius}°C {(item.IsAnomaly ? "[ANOMALY!]" : "")}",
        //             item.IsAnomaly ? ConsoleColor.Magenta : ConsoleColor.Green);
        //         await Task.Delay(300); // Slow consumer -> triggers backpressure all the way back to Stage 1!
        //     }
        //   - Return (Processed: processedCount, Anomalies: anomalyCount).

        var consumer = Task.Run(async () =>
        {
            int processedCount = 0;
            int anomalyCount = 0;

            await foreach (var item in cleanChannel.Reader.ReadAllAsync())
            {
                processedCount++;
                if (item.IsAnomaly) anomalyCount++;

                PrintColored(
                    $"    [Stage3-Consumer] Saved #{item.Id:D2} | {item.Celsius}°C {(item.IsAnomaly ? "[ANOMALY!]" : "")}",
                    item.IsAnomaly ? ConsoleColor.Magenta : ConsoleColor.Green);

                await Task.Delay(300);
                // Slow consumer -> triggers backpressure all the way back to Stage 1!
            }

            return (Processed: processedCount, Anomalies: anomalyCount);
        });

        // TODO 5: Await all 3 stages, then print a final pipeline summary:
        //   - Total Produced: 20
        //   - Filtered Out (Invalid): {filteredOut}
        //   - Saved (Valid): {consumerResult.Processed} (Anomalies: {consumerResult.Anomalies})
        //   - Integrity Check (Filtered + Saved == 20): {filteredOut + consumerResult.Processed}

        // Task.WhenAll awaits all tasks concurrently for unified exception handling.
        // Subsequent 'await' calls do not wait again;
        // they safely extract the results without blocking (.Result).

        await Task.WhenAll(producer, filter, consumer);
        var filteredOut = await filter;
        var consumerResult = await consumer;

        PrintColored();
        PrintColored("Total Produced: 20", ConsoleColor.Cyan);
        PrintColored($"Filtered Out (Invalid): {filteredOut}", ConsoleColor.Red);
        PrintColored($"Saved [Valid]: {consumerResult.Processed} [Anomalies]: {consumerResult.Anomalies}",
            ConsoleColor.Green);
        PrintColored($"Integrity Check (Filtered + Saved == 20): {filteredOut + consumerResult.Processed}",
            ConsoleColor.Yellow);

        Console.WriteLine("\n[Done] Ex07 finished. Day 05 Complete!");
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