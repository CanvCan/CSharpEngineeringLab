using System.Diagnostics;

namespace Day01.MemoryManagement;

public class Ex02_BoxingBenchmark
{
    private const int Count = 10_000_000;

    public static void Run()
    {
        Console.WriteLine("=== EXPERIMENT: BOXING COST WITH STOPWATCH (10M ITEMS) ===\n");

        // Warm-up to eliminate initial JIT compilation overhead
        var warmList = new List<int> { 1 };
        var warmObjList = new List<object> { 1 };

        // 1. TEST: List<int> (No Boxing)
        Stopwatch sw = Stopwatch.StartNew();
        var intList = new List<int>();
        for (int i = 0; i < Count; i++)
        {
            intList.Add(i); // Value is added directly to the internal int[] array
        }
        sw.Stop();
        Console.WriteLine($"List<int>    (No Boxing) : {sw.ElapsedMilliseconds} ms");

        // 2. TEST: List<object> (Boxing!)
        sw.Restart();
        var objectList = new List<object>();
        for (int i = 0; i < Count; i++)
        {
            objectList.Add(i); // Each 'i' is boxed into a new object on the Heap
        }
        sw.Stop();
        Console.WriteLine($"List<object> (Boxing)    : {sw.ElapsedMilliseconds} ms");
    }
}