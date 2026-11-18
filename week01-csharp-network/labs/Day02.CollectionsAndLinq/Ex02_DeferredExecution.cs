namespace Day02.CollectionsAndLinq;

public static class Ex02_DeferredExecution
{
    public static void Run()
    {
        PrintSection("1. Deferred Execution (Lazy Evaluation)");
        List<int> numbers = new() { 1, 2, 3 };

        // TODO 1: Define a LINQ query that filters numbers greater than 1 (use .Where(...)).
        // IMPORTANT: Do NOT call .ToList() yet! Keep it as IEnumerable<int> (var query = ...).

        IEnumerable<int> query = numbers.Where(x => x > 1);

        // TODO 2: Add 10 to the 'numbers' list AFTER defining the query.

        numbers.Add(10);

        // TODO 3: Print the query result (e.g., using string.Join(", ", query)).
        // Observe: Is 10 included in the output? Why?

        Console.WriteLine($"  {"Deferred Query Result",-26} -> [{string.Join(", ", query)}]");

        PrintSection("2. Immediate Execution (Materialized with .ToList())");
        List<int> numbers2 = new() { 1, 2, 3 };

        // TODO 4: Define the exact same .Where(...) query, but append .ToList() at the end.

        IEnumerable<int> query2 = numbers2.Where(x => x > 1).ToList();

        // TODO 5: Add 10 to 'numbers2'.

        numbers2.Add(10);

        // TODO 6: Print the materialized list and compare it with Section 1.

        Console.WriteLine($"  {"Immediate Query Result",-26} -> [{string.Join(", ", query2)}]");

    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────");
        Console.ResetColor();
    }
}