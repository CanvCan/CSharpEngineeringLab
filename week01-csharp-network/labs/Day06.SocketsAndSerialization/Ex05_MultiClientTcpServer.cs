using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Day06.SocketsAndSerialization;

public static class Ex05_MultiClientTcpServer
{
    private const int Port = 9003;
    private static readonly Lock ConsoleLock = new();
    private static int _activeClients;

    public static async Task RunAsync()
    {
        PrintSection("Multi-Client TCP Echo Server (1 Server, 3 Concurrent Clients)");

        // TODO 1: Create a CancellationTokenSource (cts) to control the server's lifetime.
        //   - Start RunServerAsync(cts.Token) as a background Task.
        //   - Wait briefly (e.g., 100ms) so the listener is bound and ready to accept connections.

        using var cts = new CancellationTokenSource();
        Task serverTask = RunServerAsync(cts.Token);

        await Task.Delay(100);


        // TODO 2: Launch 3 clients concurrently using Enumerable.Range(1, 3) and Task.WhenAll.
        //   - Each client calls RunClientAsync(clientId).
        //   - Await all 3 client tasks to complete.

        var clientTasks = Enumerable.Range(1, 3)
            .Select(clientId => RunClientAsync(clientId));

        await Task.WhenAll(clientTasks);

        // TODO 3: Shut down the server cleanly after all clients finish:
        //   - Call cts.Cancel() to break out of AcceptTcpClientAsync.
        //   - Await the server task so listener.Stop() executes cleanly.
        //   - Print the final _activeClients count (should be 0).

        cts.Cancel();
        await serverTask;
        PrintColored($"[Main] Final active clients count: {_activeClients}", ConsoleColor.Cyan);


        Console.WriteLine("\n[Done] Ex05 finished.");
    }

    // TODO 4: Implement RunServerAsync(CancellationToken ct):
    //   - Create a TcpListener on IPAddress.Loopback and Port, then call Start().
    //   - Wrap the accept loop in try / catch (OperationCanceledException) / finally (listener.Stop()).
    //   - Inside while (!ct.IsCancellationRequested), await listener.AcceptTcpClientAsync(ct).
    //   - IMPORTANT: Do NOT await HandleClientAsync here. Launch it fire-and-forget:
    //       _ = HandleClientAsync(client, ct);
    //     so the loop immediately returns to accept the next incoming client.

    private static async Task RunServerAsync(CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Loopback, Port);
        listener.Start();
        PrintColored($"[Server] Listening on {IPAddress.Loopback}:{Port}...", ConsoleColor.Cyan);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(ct);
                _ = HandleClientAsync(client, ct);

                // CRITICAL: Fire-and-forget intentionally without 'await'.
                // If we awaited HandleClientAsync here, the accept loop would block until this client finishes
                // all its messages, preventing the server from accepting other concurrent clients.
                // The discard '_ =' tells the compiler we deliberately started this task in the background
                // and will immediately loop back to accept the next incoming connection.
            }
        }
        catch (OperationCanceledException)
        {
            PrintColored("[Server] Shutdown requested. Stopping listener...", ConsoleColor.DarkGray);
        }
        finally
        {
            listener.Stop();
            PrintColored("[Server] Listener stopped.", ConsoleColor.DarkGray);
        }
    }


    // TODO 5: Implement HandleClientAsync(TcpClient client, CancellationToken ct):
    //   - Wrap the client in a using (client) block to guarantee socket disposal on any exit path.
    //   - Increment _activeClients safely with Interlocked.Increment and log the connection + active count.
    //   - Inside try / catch (OperationCanceledException) / catch (Exception) / finally:
    //       * In try: get the NetworkStream, loop with ReadAsync > 0, log the received message,
    //         and echo back only the received bytes (buffer.AsMemory(0, bytesRead)).
    //       * In finally: decrement _activeClients safely with Interlocked.Decrement
    //         and log the disconnection + remaining active count.

    private static async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
            int current = Interlocked.Increment(ref _activeClients);
            var remoteEndpoint = client.Client.RemoteEndPoint;
            PrintColored($"[Server] Client connected ({remoteEndpoint}). Active clients: {current}", ConsoleColor.Gray);

            try
            {
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, ct)) > 0)
                {
                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    PrintColored($"[Server <- {remoteEndpoint}] Received: {received}", ConsoleColor.White);

                    await stream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Server shutting down
            }
            catch (Exception ex)
            {
                PrintColored($"[Server] Error with {remoteEndpoint}: {ex.Message}", ConsoleColor.Red);
            }
            finally
            {
                int remaining = Interlocked.Decrement(ref _activeClients);
                PrintColored($"[Server] Client disconnected ({remoteEndpoint}). Active clients: {remaining}",
                    ConsoleColor.Gray);
            }
        }
    }

    // TODO 6: Implement RunClientAsync(int clientId):
    //   - Create a TcpClient and connect to IPAddress.Loopback and Port using ConnectAsync.
    //   - Send 3 messages (e.g., $"Client-{clientId} Msg-{i}"), read the echoed reply after each one,
    //     and print it using PrintColored(..., GetClientColor(clientId)).
    //   - Add a short random delay between messages (e.g., Random.Shared.Next(50, 150) ms)
    //     so you can clearly see all 3 clients interleaved on the server concurrently.

    private static async Task RunClientAsync(int clientId)
    {
        using (TcpClient tcpClient = new TcpClient())
        {
            await tcpClient.ConnectAsync(IPAddress.Loopback, Port);
            using NetworkStream stream = tcpClient.GetStream();

            byte[] buffer = new byte[1024];

            for (int i = 1; i <= 3; i++)
            {
                string message = $"Client-{clientId} Msg-{i}";
                byte[] outBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(outBytes);

                int bytesRead = await stream.ReadAsync(buffer);
                string reply = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                PrintColored($"[Client-{clientId}] Received: {reply}", GetClientColor(clientId));

                await Task.Delay(Random.Shared.Next(50, 150));
            }
        }
    }


    private static ConsoleColor GetClientColor(int id) => id switch
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