using System.Text;
using System.Text.Json;

namespace Day06.SocketsAndSerialization;

public record TelemetryPacket(int DeviceId, double Temperature, double Pressure, DateTime Timestamp);

public static class Ex01_JsonSerialization
{
    // TODO 1: Define two static readonly JsonSerializerOptions instances:
    //   - IndentedOptions: CamelCase, CaseInsensitive, WriteIndented = true (for readable console output)
    //   - CompactOptions:  CamelCase, CaseInsensitive, WriteIndented = false (for compact network bytes)

    private static readonly JsonSerializerOptions IndentedOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };


    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };


    public static Task RunAsync()
    {
        PrintSection("1. Object -> JSON String -> Object");

        // TODO 2: Create a TelemetryPacket (e.g., DeviceId: 101, Temperature: 24.7, Pressure: 1013.25, DateTime.UtcNow).
        // - Serialize it to a string using JsonSerializer.Serialize with IndentedOptions and print it.
        // - Deserialize it back to TelemetryPacket? using JsonSerializer.Deserialize with IndentedOptions.
        // - Verify record value equality (original == restored) and print the result.

        TelemetryPacket original =
            new TelemetryPacket(DeviceId: 101, Temperature: 24.7, Pressure: 1013.25, DateTime.UtcNow);

        string json = JsonSerializer.Serialize(original, IndentedOptions);

        Console.WriteLine($"Telemetry packet: {json}\n");

        TelemetryPacket? restored = JsonSerializer.Deserialize<TelemetryPacket>(json, IndentedOptions);

        bool equality = original == restored;

        // 'record' (record class) is a reference type stored on the heap,
        // whereas 'record struct' is a value type stored on the stack.

        // Unlike standard classes, records compare their internal values instead of memory addresses,
        // so original == restored returns true.

        Console.WriteLine($"[original == restored]: {equality}");

        PrintSection("2. Direct UTF-8 Byte Serialization (Network-Ready)");

        // TODO 3: Serialize the same packet directly to byte[] using JsonSerializer.SerializeToUtf8Bytes with CompactOptions.
        // - Print the byte array length (utf8Bytes.Length) and compare its size to the indented string.
        // - Print a string preview of the bytes using Encoding.UTF8.GetString(utf8Bytes).
        // - Deserialize directly from utf8Bytes using JsonSerializer.Deserialize<TelemetryPacket> and print the restored object.

        byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(original, CompactOptions);

        Console.WriteLine($"Byte array length: {utf8Bytes.Length} bytes " +
                          $"(Indented string: {Encoding.UTF8.GetByteCount(json)} bytes)\n");

        Console.WriteLine($"Preview string: {Encoding.UTF8.GetString(utf8Bytes)}\n");

        TelemetryPacket? restored2 = JsonSerializer.Deserialize<TelemetryPacket>(utf8Bytes, CompactOptions);

        Console.WriteLine($"Deserialized object: {restored2}");


        PrintSection("3. Handling Corrupted JSON Safely");

        // TODO 4: Define a malformed JSON string (e.g., """{ "deviceId": "not-a-number", "temperature": 25.0 }""").
        // - Inside a try/catch (JsonException ex) block, attempt to deserialize it into TelemetryPacket.
        // - In the catch block, print the exception message in Red so the program continues cleanly.

        // Use a raw string literal (""") to avoid escaping double quotes inside JSON
        string malformedJson = """{ "deviceId": "not-a-number", "temperature": 25.0 }""";

        try
        {
            TelemetryPacket? packet = JsonSerializer.Deserialize<TelemetryPacket>(malformedJson, CompactOptions);
        }
        catch (JsonException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"JSON Error: {ex.Message}");
            Console.ResetColor();
        }


        Console.WriteLine("\n[Done] Ex01 finished.");

        // Required because RunAsync returns a Task without the 'async' keyword (no 'await' calls inside).
        // Returns a pre-completed Task without allocating extra memory to satisfy the method signature.
        return Task.CompletedTask;
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────\n");
        Console.ResetColor();
    }
}