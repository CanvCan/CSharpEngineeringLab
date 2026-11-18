namespace Day02.CollectionsAndLinq;

public static class Ex04_YieldReturn
{
    public static void Run()
    {
        List<int> numbers = new() { 1, 2, 3, 4, 5, 6 };

        PrintSection("1. Interleaved Execution (Production vs. Consumption)");

        foreach (int even in GetEvenNumbers(numbers))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    [Consuming] Received {even}");
            Console.ResetColor();
        }

        PrintSection("2. Custom LINQ Extensions (MyWhere & MySelect)");

        // TODO 1: Chain your custom extension methods on 'numbers':
        // Filter numbers > 2 using .MyWhere(...), then multiply by 10 using .MySelect(...)
        IEnumerable<int> customResult = numbers
            .MyWhere(x => x > 2)
            .MySelect(x => x * 10);

        Console.WriteLine($"  {"Custom LINQ Pipeline",-26} -> [{string.Join(", ", customResult)}]");
    }

    private static IEnumerable<int> GetEvenNumbers(IEnumerable<int> source)
    {
        foreach (int item in source)
        {
            Console.WriteLine($"  [Producing] Checking {item}");

            // TODO 2: If 'item' is even (item % 2 == 0), yield return it.
            if (item % 2 == 0)
            {
                yield return item;
            }

        }
    }

    // Extension Method 1: Your own implementation of LINQ .Where()
    private static IEnumerable<T> MyWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        // TODO 3: Loop through 'source' with foreach.
        // If predicate(item) returns true, use 'yield return item;'

        foreach (var item in source)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    // Extension Method 2: Your own implementation of LINQ .Select()
    private static IEnumerable<TResult> MySelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        // TODO 4: Loop through 'source' with foreach.
        // Transform each item using selector(item) and yield return the result.
        
        foreach(var item in source)
        {
            yield return selector(item);
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