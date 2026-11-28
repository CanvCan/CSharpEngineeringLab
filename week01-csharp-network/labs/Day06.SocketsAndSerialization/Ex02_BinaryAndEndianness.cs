using System.Buffers.Binary;

namespace Day06.SocketsAndSerialization;

public readonly record struct BinaryTelemetry(int DeviceId, double Temperature, double Pressure)
{
    public const int PacketSize = 20; // 4 (int) + 8 (double) + 8 (double)

    // TODO 2a: Write a method: public byte[] ToBytes()
    //   - Allocate a byte[PacketSize] buffer and create a Span<byte> over it.
    //   - Write DeviceId (offset 0, len 4), Temperature (offset 4, len 8), Pressure (offset 12, len 8)
    //     using BinaryPrimitives Big-Endian methods.
    //   - Return the buffer.

    public byte[] ToBytes()
    {
        byte[] buffer = new byte[PacketSize];
        Span<byte> span = buffer;

        BinaryPrimitives.WriteInt32BigEndian(span.Slice(0, 4), DeviceId);
        BinaryPrimitives.WriteDoubleBigEndian(span.Slice(4, 8), Temperature);
        BinaryPrimitives.WriteDoubleBigEndian(span.Slice(12, 8), Pressure);

        return buffer;
    }

    // TODO 2b: Write a static method: public static bool TryParse(ReadOnlySpan<byte> data, out BinaryTelemetry packet)
    //   - If data.Length != PacketSize, set packet = default and return false.
    //   - Otherwise read the 3 fields back using BinaryPrimitives Big-Endian methods + data.Slice(...),
    //     assign packet, and return true.

    public static bool TryParse(ReadOnlySpan<byte> data, out BinaryTelemetry packet)
    {
        if (data.Length != PacketSize)
        {
            packet = default;
            return false;
        }

        int deviceId = BinaryPrimitives.ReadInt32BigEndian(data.Slice(0, 4));
        double temperature = BinaryPrimitives.ReadDoubleBigEndian(data.Slice(4, 8));
        double pressure = BinaryPrimitives.ReadDoubleBigEndian(data.Slice(12, 8));

        packet = new BinaryTelemetry(deviceId, temperature, pressure);
        return true;
    }
}

public static class Ex02_BinaryAndEndianness
{
    public static Task RunAsync()
    {
        PrintSection("1. Big-Endian vs. Little-Endian (0x01020304)");

        // TODO 1: Take int value = 0x01020304;
        // - Write it into a 4-byte array using BinaryPrimitives.WriteInt32BigEndian.
        // - Write it into another 4-byte array using BinaryPrimitives.WriteInt32LittleEndian.
        // - Print both arrays using Convert.ToHexString(...) and observe the byte order difference.

        int value = 0x01020304;

        byte[] bigEndianBytes = new byte[4];
        byte[] littleEndianBytes = new byte[4];

        BinaryPrimitives.WriteInt32BigEndian(bigEndianBytes, value);
        BinaryPrimitives.WriteInt32LittleEndian(littleEndianBytes, value);

        Console.WriteLine($"Big-Endian    : {Convert.ToHexString(bigEndianBytes)}");
        Console.WriteLine($"Little-Endian : {Convert.ToHexString(littleEndianBytes)}");

        PrintSection("2. Binary Packet Round-Trip (20 Bytes vs. JSON)");

        // TODO 2c: Create a BinaryTelemetry instance (e.g., DeviceId: 101, Temperature: 24.7, Pressure: 1013.25).
        // - Call ToBytes() and print the byte length and Convert.ToHexString(bytes).
        // - Call BinaryTelemetry.TryParse(bytes, out var parsed).
        // - Print whether TryParse succeeded, the parsed struct, and verify (original == parsed).

        BinaryTelemetry packet = new BinaryTelemetry(DeviceId: 101, Temperature: 24.7, Pressure: 1013.25);
        Console.WriteLine($"Original struct     : {packet}");

        byte[] bytes = packet.ToBytes();
        Console.WriteLine($"Packet byte length: {bytes.Length} | Hex: {Convert.ToHexString(bytes)}\n");

        bool success = BinaryTelemetry.TryParse(bytes, out var parsed);
        Console.WriteLine($"TryParse succeeded: {success}");
        Console.WriteLine($"Parsed struct     : {parsed}");
        Console.WriteLine($"[packet == parsed]: {packet == parsed}");


        PrintSection("3. Safety Check: Truncated / Corrupted Packet");

        // TODO 3: Create an invalid byte array with wrong length (e.g., new byte[10]).
        // - Call BinaryTelemetry.TryParse(badBytes, out var invalidPacket).
        // - Print the result (should be False, no exception thrown).

        byte[] badBytes = new byte[10];
        bool invalidResult = BinaryTelemetry.TryParse(badBytes, out var invalidPacket);

        Console.WriteLine($"TryParse on 10-byte packet : {invalidResult}");
        Console.WriteLine($"Default struct returned    : {invalidPacket}");

        Console.WriteLine("\n[Done] Ex02 finished.");
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