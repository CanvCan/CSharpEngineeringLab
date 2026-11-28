using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Day06.SocketsAndSerialization;

public readonly record struct UdpTelemetryPacket(uint Seq, double Temperature, double Battery)
{
    public const int PacketSize = 20; // 4 (uint) + 8 (double) + 8 (double)

    // TODO 1a: Implement public byte[] ToBytes()
    //   - Allocate a byte[PacketSize] buffer and write Seq (0..4), Temperature (4..12), Battery (12..20)
    //     in Big-Endian using BinaryPrimitives.

    public byte[] ToBytes()
    {
        byte[] buffer = new byte[PacketSize];
        Span<byte> span = buffer;

        BinaryPrimitives.WriteUInt32BigEndian(span.Slice(0, 4), Seq);
        BinaryPrimitives.WriteDoubleBigEndian(span.Slice(4, 8), Temperature);
        BinaryPrimitives.WriteDoubleBigEndian(span.Slice(12, 8), Battery);

        return buffer;
    }

    // TODO 1b: Implement public static bool TryParse(ReadOnlySpan<byte> data, out UdpTelemetryPacket packet)
    //   - Validate data.Length == PacketSize; if not, set packet = default and return false.
    //   - Otherwise read the 3 fields in Big-Endian, assign packet, and return true.

    public static bool TryParse(ReadOnlySpan<byte> data, out UdpTelemetryPacket packet)
    {
        if (data.Length != PacketSize)
        {
            packet = default;
            return false;
        }

        uint seq = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0, 4));
        double temperature = BinaryPrimitives.ReadDoubleBigEndian(data.Slice(4, 8));
        double battery = BinaryPrimitives.ReadDoubleBigEndian(data.Slice(12, 8));

        packet = new UdpTelemetryPacket(seq, temperature, battery);
        return true;
    }
}

public static class Ex06_UdpTelemetry
{
    private const int Port = 9004;

    public static async Task RunAsync()
    {
        PrintSection("1. UDP Binary Telemetry & Packet Loss Detection (Seq 1..20)");

        // TODO 2: Create a CancellationTokenSource (cts) and start a Receiver task:
        //   - Bind a UdpClient to Port (using var receiver = new UdpClient(Port)).
        //   - Track: int received = 0, int lost = 0, uint? lastSeq = null.
        //   - Inside try / catch (OperationCanceledException):
        //       * Loop while (!ct.IsCancellationRequested) and await receiver.ReceiveAsync(cts.Token).
        //       * Parse result.Buffer with UdpTelemetryPacket.TryParse.
        //       * If invalid, print a warning and continue.
        //       * Increment received.
        //       * Gap detection: if lastSeq has a value and packet.Seq > lastSeq.Value + 1:
        //           calculate missing = (int)(packet.Seq - lastSeq.Value - 1), add to lost,
        //           and print a Red warning showing which sequence numbers were lost.
        //       * Update lastSeq only if it is null or packet.Seq > lastSeq.Value.
        //       * Print the received packet details.
        //   - Return (Received: received, Lost: lost) from the receiver task.

        using var cts = new CancellationTokenSource();

        var receiverTask = Task.Run(async () =>
        {
            using var receiver = new UdpClient(Port);

            int received = 0;
            int lost = 0;
            uint? lastSeq = null;
            // uint holds only non-negative integers,
            // doubling the positive range (~4.3B) for packet sequence numbers.

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    UdpReceiveResult result = await receiver.ReceiveAsync(cts.Token);

                    if (!UdpTelemetryPacket.TryParse(result.Buffer, out UdpTelemetryPacket packet))
                    {
                        Console.WriteLine("[Receiver] Invalid packet received, skipping.");
                        continue;
                    }

                    received++;

                    // HasValue returns true if lastSeq holds a number, or false if it is still null (first packet).
                    // We must check this before reading .Value to avoid crashing on null and skip gap checks on the very first packet.
                    if (lastSeq.HasValue && packet.Seq > lastSeq.Value + 1)
                    {
                        int missing = (int)(packet.Seq - lastSeq.Value - 1);
                        lost += missing;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            $"[Receiver] WARNING: Lost {missing} packet(s) between #{lastSeq.Value} and #{packet.Seq}!");
                        Console.ResetColor();
                    }

                    if (!lastSeq.HasValue || packet.Seq > lastSeq.Value)
                    {
                        lastSeq = packet.Seq;
                    }

                    Console.WriteLine($"[Receiver] Received: {packet}");
                }
            }
            catch (OperationCanceledException)
            {
            }

            return (Received: received, Lost: lost);
        });


        // TODO 3: Wait briefly (100ms), then run the Sender:
        //   - Create a UdpClient (using var sender = new UdpClient()).
        //   - Loop uint seq from 1 to 20:
        //       * Simulate UDP packet loss: if (seq % 5 == 0) continue; (skips 5, 10, 15, 20).
        //       * Build a UdpTelemetryPacket and send its bytes to "127.0.0.1", Port via SendAsync.
        //       * Add a small delay (e.g., 20ms) between packets.
        //   - After sending finishes, wait briefly (150ms) so the receiver processes the last datagram,
        //     then call cts.Cancel() and await the receiver task.
        //   - Print the summary: Received (expect 16) and Detected Lost (expect 3).
        //     (Observe why packet #20 is not counted in Detected Lost: no packet arrives after it to reveal the gap!)

        await Task.Delay(100);

        using (var sender = new UdpClient())
        {
            for (uint seq = 1; seq <= 20; seq++)
            {
                if (seq % 5 == 0) continue; // Simulates packet loss (skips 5, 10, 15, 20)

                var packet = new UdpTelemetryPacket(seq, 20.0 + seq * 0.5, 100.0 - seq * 1.5);
                byte[] bytes = packet.ToBytes();

                await sender.SendAsync(bytes, "127.0.0.1", Port);
                await Task.Delay(20);
            }
        }

        await Task.Delay(150);
        cts.Cancel();

        var (receivedCount, lostCount) = await receiverTask;
        Console.WriteLine($"\n[Summary] Sent: 16 (skipped 4) | Received: {receivedCount} | Detected Lost: {lostCount}");

        PrintSection("2. UDP Fire-and-Forget to a Closed Port (Contrast with Ex03 TCP)");

        // TODO 4: Now that the receiver is closed, create a UdpClient and send 1 packet to "127.0.0.1", Port.
        //   - Observe that SendAsync completes without throwing SocketException (unlike TCP in Ex03).
        //   - Print a short confirmation message noting that the packet was silently dropped.

        using var udpClient = new UdpClient();

        var udpPacket = new UdpTelemetryPacket(100, 25.0, 80.0);
        byte[] udpBytes = udpPacket.ToBytes();

        await udpClient.SendAsync(udpBytes, "127.0.0.1", Port);
        Console.WriteLine("[Sender] Sent packet #100 to closed port — no exception thrown (silently dropped by OS).");


        Console.WriteLine("\n[Done] Ex06 finished. Day 06 Complete!");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}