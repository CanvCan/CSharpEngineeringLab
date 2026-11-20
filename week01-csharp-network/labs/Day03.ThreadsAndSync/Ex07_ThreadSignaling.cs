namespace Day03.ThreadsAndSync;

public static class Ex07_ThreadSignaling
{
    public static void Run()
    {
        PrintSection("1. ManualResetEventSlim (Open the Gate for All)");

        // TODO 1: Create: using var gate = new ManualResetEventSlim(initialState: false);
        // Start 3 runner threads (Runner-1 to Runner-3). Each prints "Waiting at the gate...",
        // calls gate.Wait(), then prints "GO! Running!".
        // In Main: sleep 1000ms, print "[Main] Opening the gate!", call gate.Set().
        // Join all 3 threads. Observe all 3 wake up simultaneously.

        using var gate = new ManualResetEventSlim(initialState: false);

        Thread t1 = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Runner-1";
            PrintColored($"{Thread.CurrentThread.Name} waiting at the gate...", ConsoleColor.White);
            gate.Wait();
            PrintColored($"{Thread.CurrentThread.Name} GO! Running!", ConsoleColor.Green);
        }
        );

        Thread t2 = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Runner-2";
            PrintColored($"{Thread.CurrentThread.Name} waiting at the gate...", ConsoleColor.White);
            gate.Wait();
            PrintColored($"{Thread.CurrentThread.Name} GO! Running!", ConsoleColor.Green);
        }
        );

        Thread t3 = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Runner-3";
            PrintColored($"{Thread.CurrentThread.Name} waiting at the gate...", ConsoleColor.White);
            gate.Wait();
            PrintColored($"{Thread.CurrentThread.Name} GO! Running!", ConsoleColor.Green);
        }
        );

        t1.Start();
        t2.Start();
        t3.Start();

        Thread.Sleep(1000);
        PrintColored("[Main] Opening the gate!", ConsoleColor.White);
        gate.Set();

        t1.Join();
        t2.Join();
        t3.Join();

        PrintSection("2. AutoResetEvent (Turnstile: One at a Time)");

        // TODO 2: Create: using var turnstile = new AutoResetEvent(initialState: false);
        // Start 3 worker threads (Worker-1 to Worker-3). Each prints "Waiting at turnstile...",
        // calls turnstile.WaitOne(), then prints "Passed through turnstile!".
        // In Main: loop 3 times -> sleep 700ms -> print "[Main] Letting 1 worker in..." -> turnstile.Set().
        // Join all 3 threads. Observe they pass ONE BY ONE, not all at once.

        var turnstile = new AutoResetEvent(initialState: false);

        Thread t4 = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Worker-1";
            PrintColored($"{Thread.CurrentThread.Name} waiting at the turnstile...", ConsoleColor.White);
            turnstile.WaitOne();
            PrintColored($"{Thread.CurrentThread.Name} Passed through turnstile!", ConsoleColor.Green);
        }
        );

        Thread t5 = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Worker-2";
            PrintColored($"{Thread.CurrentThread.Name} waiting at the turnstile...", ConsoleColor.White);
            turnstile.WaitOne();
            PrintColored($"{Thread.CurrentThread.Name} Passed through turnstile!", ConsoleColor.Green);
        }
        );

        Thread t6 = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Worker-3";
            PrintColored($"{Thread.CurrentThread.Name} waiting at the turnstile...", ConsoleColor.White);
            turnstile.WaitOne();
            PrintColored($"{Thread.CurrentThread.Name} Passed through turnstile!", ConsoleColor.Green);
        }
        );

        t4.Start();
        t5.Start();
        t6.Start();

        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(700);
            PrintColored("[Main] Letting 1 worker in...", ConsoleColor.White);
            turnstile.Set();
        }

        t4.Join();
        t5.Join();
        t6.Join();

        PrintSection("3. CountdownEvent (Wait for N Workers to Finish)");

        // TODO 3: Create: using var countdown = new CountdownEvent(3);
        // Start 3 service-loader threads (DB, Cache, Config) with different sleep durations (400ms, 800ms, 1200ms).
        // Inside each thread, use try { sleep & print loaded } finally { countdown.Signal(); }.
        // In Main: print "[Main] Waiting for all 3 services to initialize...", call countdown.Wait(),
        // then print "[Main] All services ready! App started."

        using var countdown = new CountdownEvent(3);

        Thread DB = new Thread(() =>
        {
            Thread.CurrentThread.Name = "DB";

            try
            {
                Thread.Sleep(600);
                PrintColored($"{Thread.CurrentThread.Name} loaded.", ConsoleColor.Green);
            }
            finally { countdown.Signal(); }
        }
        );
        Thread Cache = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Cache";

            try
            {
                Thread.Sleep(1000);
                PrintColored($"{Thread.CurrentThread.Name} loaded.", ConsoleColor.Green);
            }
            finally { countdown.Signal(); }
        }
        );

        Thread Config = new Thread(() =>
        {
            Thread.CurrentThread.Name = "Config";

            try
            {
                Thread.Sleep(1500);
                PrintColored($"{Thread.CurrentThread.Name} loaded.", ConsoleColor.Green);
            }
            finally { countdown.Signal(); }
        }
        );

        DB.Start();
        Cache.Start();
        Config.Start();

        PrintColored("[Main] Waiting for all 3 services to initialize...", ConsoleColor.White);
        countdown.Wait();
        PrintColored("[Main] All services ready! App started.", ConsoleColor.White);

        // NOTE: countdown.Wait() already blocks Main until all 3 threads call Signal(),
        // making Join() optional here (kept only to ensure full OS thread cleanup).

        // DB.Join();
        // Cache.Join();
        // Config.Join();

        Console.WriteLine("\n[Done] Ex07 finished.");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }

    private static readonly object _consoleLock = new();

    private static void PrintColored(string message, ConsoleColor color = ConsoleColor.White)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}