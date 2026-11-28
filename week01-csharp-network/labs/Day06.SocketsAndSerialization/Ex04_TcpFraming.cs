using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Day06.SocketsAndSerialization;

public static class Ex04_TcpFraming
{
    private const int Port = 9002;

    public static async Task RunAsync()
    {
        PrintSection("Length-Prefixed Framing: 10 Rapid Messages Without Sticking");

        // TODO 3: Start a Server task on IPAddress.Loopback and Port:
        //   - Start TcpListener, accept 1 client inside try/finally (Stop in finally).
        //   - Read frames in a loop using your ReadFrameAsync helper until it returns null (EOF).
        //   - Decode each frame as UTF-8, count how many messages arrived, and print each one.
        //   - Print the total frame count when the client disconnects.

        var serverTask = Task.Run(async () =>
        {
            var listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();

            try
            {
                using TcpClient client = await listener.AcceptTcpClientAsync();
                using NetworkStream stream = client.GetStream();

                byte[]? frame;
                int messageCount = 0;

                while ((frame = await ReadFrameAsync(stream)) is not null)
                {
                    messageCount++;
                    string message = Encoding.UTF8.GetString(frame);
                    Console.WriteLine($"[Server] Frame #{messageCount}: {message}");
                }

                Console.WriteLine($"[Server] Client disconnected. Total frames received: {messageCount}");
            }
            finally
            {
                listener.Stop();
            }
        });

        // TODO 4: After a brief delay (100ms), connect a Client to IPAddress.Loopback and Port:
        //   - Inside a scoped using block (so the client closes before awaiting the server):
        //       * Send 10 messages back-to-back in a tight loop with NO delay between writes
        //         (e.g., $"Message #{i:D2} - payload data") using your SendFrameAsync helper.
        //   - Await the server task and verify that all 10 messages arrived cleanly separated
        //     (no sticky/merged messages, no partial reads).

        await Task.Delay(100);

        using (TcpClient tcpClient = new TcpClient())
        {
            await tcpClient.ConnectAsync(IPAddress.Loopback, Port);
            using NetworkStream clientStream = tcpClient.GetStream();

            for (int i = 1; i <= 10; i++)
            {
                string text = $"Message #{i:D2} - payload data";
                Console.WriteLine($"[Client] Sending: {text}");
                byte[] payload = Encoding.UTF8.GetBytes(text);
                await SendFrameAsync(clientStream, payload);
            }
        }

        await serverTask;

        Console.WriteLine("\n[Done] Ex04 finished.");
    }

    // TODO 1: Implement SendFrameAsync(NetworkStream stream, byte[] payload, CancellationToken ct = default)
    //   - Create a 4-byte header and write payload.Length in Big-Endian (BinaryPrimitives.WriteInt32BigEndian).
    //   - Write the 4-byte header to the stream, then write the payload bytes.

    private static async Task SendFrameAsync(NetworkStream stream, byte[] payload, CancellationToken ct = default)
    {
        byte[] header = new byte[4];
        Span<byte> span = header;

        BinaryPrimitives.WriteInt32BigEndian(span.Slice(0, 4), payload.Length);

        await stream.WriteAsync(header, ct);
        await stream.WriteAsync(payload, ct);
    }


    // TODO 2: Implement ReadFrameAsync(NetworkStream stream, CancellationToken ct = default) -> Task<byte[]?>
    //   - Create a 4-byte header buffer and read it with stream.ReadExactlyAsync(header, ct).
    //   - Catch EndOfStreamException and return null (client closed the connection cleanly).
    //   - Parse the payload length using BinaryPrimitives.ReadInt32BigEndian(header).
    //   - Allocate a byte[length] buffer, read the exact payload with ReadExactlyAsync, and return it.


    private static async Task<byte[]?> ReadFrameAsync(NetworkStream stream, CancellationToken ct = default)
    {
        byte[] header = new byte[4];

        try
        {
            await stream.ReadExactlyAsync(header, ct);
            int payloadLength = BinaryPrimitives.ReadInt32BigEndian(header);

            if (payloadLength < 0 || payloadLength > 10 * 1024 * 1024)
                throw new InvalidDataException($"Invalid frame length: {payloadLength}");

            byte[] payload = new byte[payloadLength];
            await stream.ReadExactlyAsync(payload, ct);

            return payload;
        }
        catch (EndOfStreamException ex)
        {
            // Client closed the connection cleanly between frames
            Console.WriteLine($"EOF Error: {ex.Message}");
            return null;
        }
    }

    // Note: In production systems, only the header read should be wrapped in try/catch (EndOfStreamException).
    // An EOF during header read means the client closed cleanly between frames (return null),
    // whereas an EOF during payload read indicates a truncated/corrupted frame and should bubble up as an exception.


    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}