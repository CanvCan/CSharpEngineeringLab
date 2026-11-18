namespace Day02.CollectionsAndLinq;

public static class Ex01_CollectionsDemo
{
    public static void Run()
    {
        PrintSection("1. IEnumerable<int> Contract");
        List<int> numberList = new() { 10, 20, 30 };
        int[] numberArray = { 40, 50, 60 };

        PrintEnumerable("PrintEnumerable (List)", numberList);
        PrintEnumerable("PrintEnumerable (Array)", numberArray);

        PrintWithEnumerator("Enumerator (List)", numberList);
        PrintWithEnumerator("Enumerator (Array)", numberArray);

        PrintSection("2. ICollection<int> Contract");
        PrintAndModifyCollection(numberList);

        // Note: Passing numberArray compiles, but throws NotSupportedException at runtime because arrays have a fixed size.
        // PrintAndModifyCollection(numberArray);

        PrintSection("3. IList<int> Contract");
        AccessByIndex("Index[0] Update (List)", numberList);
        AccessByIndex("Index[0] Update (Array)", numberArray);
    }

    // Iteration contract => IEnumerable<T>
    private static void PrintEnumerable(string label, IEnumerable<int> numbers)
    {
        Console.WriteLine($"  {label,-26} -> [{string.Join(", ", numbers)}]");
    }

    // Low-level iteration contract => IEnumerator<T>
    private static void PrintWithEnumerator(string label, IEnumerable<int> numbers)
    {
        // C# 8+ using declaration: without explicit {},
        // the enclosing block's closing '}' acts as the disposal boundary.
        using IEnumerator<int> enumerator = numbers.GetEnumerator();

        List<int> collected = new();
        while (enumerator.MoveNext())
        {
            collected.Add(enumerator.Current);
        }

        Console.WriteLine($"  {label,-26} -> [{string.Join(", ", collected)}]");
    }

    // Count and Add/Remove contract => ICollection<T>
    private static void PrintAndModifyCollection(ICollection<int> numbers)
    {
        Console.WriteLine($"  {"Before Add(99)",-26} -> Count: {numbers.Count} | Items: [{string.Join(", ", numbers)}]");

        numbers.Add(99);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  {"After  Add(99)",-26} -> Count: {numbers.Count} | Items: [{string.Join(", ", numbers)}]");
        Console.ResetColor();
    }

    // Index-based access contract => IList<T>
    private static void AccessByIndex(string label, IList<int> numbers)
    {
        int before = numbers[0];
        numbers[0] = 999;
        int after = numbers[0];

        Console.WriteLine($"  {label,-26} -> Before: {before} | After: {after} | Items: [{string.Join(", ", numbers)}]");
    }

    private static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"┌─ {title} ───────────────────────────────────────────");
        Console.ResetColor();
    }
}