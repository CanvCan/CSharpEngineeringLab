using System.Diagnostics;

namespace Day02.CollectionsAndLinq;

public static class Ex05_LookupBenchmark
{
    public static void Run()
    {
        const int itemCount = 100_000;
        const int lookupIterations = 1_000;
        const int target = 99_999; // Worst-case element at the very end

        PrintSection("1. Lookup Speed Benchmark: List<T> vs. HashSet<T> (100,000 items)");

        // TODO 1: Create a List<int> and a HashSet<int> containing numbers from 0 to 99,999.
        // Hint: Enumerable.Range(0, itemCount).ToList() and .ToHashSet()
        List<int> numberList = Enumerable.Range(0, itemCount).ToList();
        HashSet<int> numberSet = Enumerable.Range(0, itemCount).ToHashSet();

        // Measure List<int>.Contains (O(N) linear scan)
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < lookupIterations; i++)
        {
            numberList.Contains(target);

        }
        sw.Stop();
        double listMs = sw.Elapsed.TotalMilliseconds;

        // Measure HashSet<int>.Contains (O(1) hash lookup)
        sw.Restart();
        for (int i = 0; i < lookupIterations; i++)
        {
            numberSet.Contains(target);

        }
        sw.Stop();
        double setMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine($"  {"List<int>.Contains x1000",-28} -> {listMs:F4} ms");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  {"HashSet<int>.Contains x1000",-28} -> {setMs:F4} ms");
        Console.ResetColor();

        PrintSection("2. Word Frequency Counter with Dictionary<string, int>");

        string sentence = "csharp is fast and linq is clean and csharp is powerful";
        string[] words = sentence.Split(' ');

        // TODO 4: Create a Dictionary<string, int> named 'wordCounts' and count each word in 'words'.
        // Use TryGetValue(word, out int count) to increment existing counts cleanly!
        Dictionary<string, int> wordCounts = new();

        foreach (string word in words)
        {
            wordCounts.TryGetValue(word, out int currentCount);
            wordCounts[word] = currentCount + 1;

        }

        foreach (var kvp in wordCounts)
        {
            Console.WriteLine($"  {kvp.Key,-15} -> {kvp.Value}");
        }
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────");
        Console.ResetColor();
    }
}