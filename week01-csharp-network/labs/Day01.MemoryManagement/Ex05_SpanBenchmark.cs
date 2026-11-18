using BenchmarkDotNet.Attributes;

namespace Day01.MemoryManagement;

/*
 * =========================================================================================
 * SPAN<T> VS. SUBSTRING OBSERVATION NOTES:
 * =========================================================================================
 * 1. Substring() Allocates on the Heap:
 *    - Strings in C# are immutable. Every call to Substring() allocates a brand-new string
 *      object on the Managed Heap and copies the characters into it, creating GC pressure.
 *
 * 2. AsSpan().Slice() is Zero-Allocation:
 *    - ReadOnlySpan<char> is a 'ref struct' that lives strictly on the Stack.
 *    - Slice() does NOT copy any characters; it simply creates a lightweight memory window
 *      (start pointer + length) pointing directly into the existing string's memory.
 * =========================================================================================
 */

[MemoryDiagnoser]
public class Ex05_SpanBenchmark
{
    private string _longText = null!;
    private const int Iterations = 1_000;

    [GlobalSetup] // Runs once before benchmarks start to prepare the test data
    public void Setup()
    {
        // Create a 10,000-character string
        _longText = new string('A', 10_000);
    }

    [Benchmark(Baseline = true)]
    public int SliceWithSubstring()
    {
        int totalLength = 0;

        for (int i = 0; i < Iterations; i++)
        {
            // Allocates a new 50-char string on the Heap in EVERY iteration!
            string sub = _longText.Substring(i, 50);
            totalLength += sub.Length;
        }

        return totalLength;
    }

    [Benchmark]
    public int SliceWithSpan()
    {
        int totalLength = 0;
        ReadOnlySpan<char> span = _longText.AsSpan();

        for (int i = 0; i < Iterations; i++)
        {
            // Zero Heap allocation! Just shifts the start pointer and length on the Stack.
            ReadOnlySpan<char> slice = span.Slice(i, 50);
            totalLength += slice.Length;
        }

        return totalLength;
    }
}