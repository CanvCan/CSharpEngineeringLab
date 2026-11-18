namespace Day01.MemoryManagement;

/*
 * =========================================================================================
 * GC OBSERVATION & BENCHMARKING NOTES (KEY MECHANISMS & TRICKS USED IN THIS CODE):
 * =========================================================================================
 * 1. Short-Lived vs. Long-Lived (Generational Hypothesis):
 *    - Unreferenced objects in Experiment 1 lose their pointer on the next iteration and die
 *      quickly in Gen 0 (fast sweeps, flat RAM, zero promotion to Gen 2).
 *    - Referenced objects stored in 'keptObjects' survive Gen 0 sweeps and get promoted to
 *      Gen 1 & Gen 2, causing noticeable Heap memory growth and heavier GC cycles.
 *
 * 2. Clean-Slate Reset Trick (The 3-Line Full GC Pattern):
 *    - 'GC.Collect() -> WaitForPendingFinalizers() -> GC.Collect()' forces an immediate,
 *      blocking cleanup across all generations so Experiment 1 leftovers don't skew Experiment 2.
 *
 * 3. Pre-Sizing the List Capacity -> new List<SmallObject>(ObjectCount):
 *    - Prevents the internal array from dynamically resizing/copying multiple times during the
 *      loop, ensuring we only measure the actual SmallObject allocations, not List growth noise.
 *
 * 4. Raw Memory Inspection -> GC.GetTotalMemory(forceFullCollection: false):
 *    - Passing 'false' reads the exact current Heap footprint without triggering a forced cleanup
 *      that would artificially wipe out the numbers before we print them.
 *
 * 5. Anti-JIT Optimization Trick -> Reading 'keptObjects.Count' at the very end:
 *    - In Release mode, the JIT compiler aggressively tracks variable lifetimes. If 'keptObjects'
 *      is never read after the loop, GC can legally collect it BEFORE PrintGcStats runs!
 *      Accessing '.Count' at the end forces the runtime to keep the list alive during measurement.
 * =========================================================================================
 */

class SmallObject // Small dummy class (~24-32 bytes on 64-bit Heap)
{
    public int Id;
    public long Timestamp;
}

public class Ex03_GcObservation
{
    private const int ObjectCount = 1_000_000;

    public static void Run()
    {
        Console.WriteLine("=== EXPERIMENT 1: SHORT-LIVED OBJECTS (NO REFERENCES KEPT) ===\n");
        PrintGcStats("Before Loop");

        for (int i = 0; i < ObjectCount; i++)
        {
            // Created on Gen 0 and immediately becomes unreachable in the next iteration
            var temp = new SmallObject { Id = i, Timestamp = i * 100L };
        }

        PrintGcStats("After Loop ");

        // -------------------------------------------------------------------------

        Console.WriteLine("\n=== EXPERIMENT 2: LONG-LIVED OBJECTS (KEPT IN A LIST) ===\n");

        // Clean up any leftover garbage before starting Experiment 2
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        PrintGcStats("Before Loop (List)");

        var keptObjects = new List<SmallObject>(ObjectCount);
        for (int i = 0; i < ObjectCount; i++)
        {
            // Reference is stored in 'keptObjects', so GC CANNOT collect these objects!
            keptObjects.Add(new SmallObject { Id = i, Timestamp = i * 100L });
        }

        PrintGcStats("After Loop  (List)");

        // Keep 'keptObjects' alive until stats are printed (prevents JIT from optimizing it away)
        Console.WriteLine($"\nTotal kept objects in memory: {keptObjects.Count:N0}");
    }

    private static void PrintGcStats(string label)
    {
        long totalMemoryBytes = GC.GetTotalMemory(forceFullCollection: false);
        double totalMemoryMb = totalMemoryBytes / (1024.0 * 1024.0);

        int gen0 = GC.CollectionCount(0);
        int gen1 = GC.CollectionCount(1);
        int gen2 = GC.CollectionCount(2);

        Console.WriteLine($"{label} -> Memory: {totalMemoryMb,6:F2} MB | Gen 0: {gen0} | Gen 1: {gen1} | Gen 2: {gen2}");
    }
}
