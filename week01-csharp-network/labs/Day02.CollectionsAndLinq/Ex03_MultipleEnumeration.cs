namespace Day02.CollectionsAndLinq;

public static class Ex03_MultipleEnumeration
{
    public static void Run()
    {
        List<int> numbers = new() { 1, 2, 3 };

        PrintSection("1. Multiple Enumeration Trap (Without .ToList())");

        // TODO 1: Define a LINQ Select query on 'numbers' that logs a message each time an item is processed,
        // and returns x * 2.
        // IMPORTANT: Do NOT call .ToList() here!
        IEnumerable<int> query = numbers.Select(x =>
        {
            Console.WriteLine($"    [Processing] {x}");
            return x * 2;
        });

        // TODO 2: Consume the query the FIRST time by calling .Count() and printing the count.
        Console.WriteLine($"  {"First Consumption (Count)",-28} -> {query.Count()}");

        // TODO 3: Consume the exact same query a SECOND time using string.Join(", ", query).
        // Observe: How many times did "[Processing]" print in total? (Expected: 6 times for 3 items!)
        Console.WriteLine($"  {"Second Consumption (Items)",-28} -> [{string.Join(", ", query)}]");

        PrintSection("2. Safe Materialization (With .ToList())");

        // TODO 4: Define the exact same .Select(...) query, but append .ToList() at the end.
        List<int> query2 = numbers.Select(x =>
        {
            Console.WriteLine($"    [Processing] {x}");
            return x * 2;
        }).ToList();

        // TODO 5: First get .Count, then print the items using string.Join(", ", materializedList).
        // Observe: How many times did "[Processing]" print now? (Expected: only 3 times!)
        Console.WriteLine($"  {"First Consumption (Count)",-28} -> {query2.Count}");
        Console.WriteLine($"  {"Second Consumption (Items)",-28} -> [{string.Join(", ", query2)}]");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────");
        Console.ResetColor();
    }
}