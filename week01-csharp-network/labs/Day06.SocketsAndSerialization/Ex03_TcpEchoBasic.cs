using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Day06.SocketsAndSerialization;

public static class Ex03_TcpEchoBasic
{
    private const int Port = 9001;

    public static async Task RunAsync()
    {
        PrintSection("1. Basic TCP Echo (1 Server, 1 Client on 127.0.0.1:9001)");

        // TODO 1: Start a TCP Echo Server task (Task.Run(async () => ...)):
        //   - Create a TcpListener on IPAddress.Loopback and Port, then call Start().
        //   - Inside try/finally (calling listener.Stop() in finally):
        //       * Accept a single client with AcceptTcpClientAsync().
        //       * Get its NetworkStream.
        //       * Loop with stream.ReadAsync(buffer) until it returns 0 (EOF / client closed).
        //       * Print what the server received (UTF-8) and echo ONLY the received bytes back
        //         using buffer.AsMemory(0, bytesRead).
        //       * Print when the client disconnects cleanly.

        var TcpEchoServerTask = Task.Run(async () =>
        {
            var listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();

            try
            {
                using TcpClient client = await listener.AcceptTcpClientAsync();
                using NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[Server] Received: {receivedMessage}");

                    await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
                }

                Console.WriteLine("[Server] Client disconnected cleanly.");
            }
            finally
            {
                listener.Stop();
            }
        });


        // TODO 2: Give the server a brief moment (e.g. await Task.Delay(100)) so it is listening,
        // then run the Client:
        //   - Create a TcpClient and connect to IPAddress.Loopback, Port using ConnectAsync.
        //   - Get its NetworkStream.
        //   - Send 3 messages (e.g., "Hello TCP", "Packet #2", "Goodbye"):
        //       * Convert to UTF-8 bytes and write to stream.
        //       * Read the echoed response back from the stream and print it.
        //   - Dispose the client (using var) so the server receives 0 (EOF) and exits its loop.
        //
        // Await the serverTask so both sides finish cleanly before Section 2.

        await Task.Delay(100);

        using (TcpClient tcpClient = new TcpClient())
        {
            await tcpClient.ConnectAsync(IPAddress.Loopback, Port);
            using NetworkStream clientStream = tcpClient.GetStream();

            string[] messages = ["Hello TCP", "Packet #2", "Goodbye"];
            byte[] buffer = new byte[1024];

            foreach (string msg in messages)
            {
                // Encode message to UTF-8 bytes and send it over the network stream
                byte[] outBytes = Encoding.UTF8.GetBytes(msg);
                await clientStream.WriteAsync(outBytes);
                Console.WriteLine($"[Client] Sent    : {msg}");

                // Read the echoed bytes back from the server and decode to string
                int bytesRead = await clientStream.ReadAsync(buffer);
                string echo = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"[Client] Received: {echo}");
            }
        } // Dispose tcpClient here so the server receives EOF (0 bytes) and exits its read loop cleanly

        await TcpEchoServerTask;


        PrintSection("2. Connecting to a Closed Port -> SocketException");

        // TODO 3: Now that the server is stopped, try to ConnectAsync with a new TcpClient
        // to the same IPAddress.Loopback and Port inside a try/catch (SocketException ex).
        // Print the exception's SocketErrorCode and Message in Red.

        using (TcpClient tcpClient2 = new TcpClient())
        {
            try
            {
                await tcpClient2.ConnectAsync(IPAddress.Loopback, Port);
            }
            catch (SocketException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Socket Error ({ex.SocketErrorCode}): {ex.Message}");
                Console.ResetColor();
            }
        }


        Console.WriteLine("\n[Done] Ex03 finished.");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}