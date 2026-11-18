namespace Day01.MemoryManagement;

struct MyStruct
{
    public int Value;
}

class MyClass
{
    public int Value;
}

public class Ex01_ValueVsReference
{
    public static void Run()
    {
        Console.WriteLine("=== EXPERIMENT 1: VARIABLE ASSIGNMENT (var b = a) ===\n");

        MyStruct structA = new MyStruct { Value = 10 };
        MyStruct structB = structA;
        structB.Value = 20;

        Console.WriteLine($"structA.Value = {structA.Value}"); // Output: 10
        Console.WriteLine($"structB.Value = {structB.Value}"); // Output: 20

        MyClass classA = new MyClass { Value = 10 };
        MyClass classB = classA;
        classB.Value = 20;

        Console.WriteLine();
        Console.WriteLine($"classA.Value = {classA.Value}"); // Output: 20
        Console.WriteLine($"classB.Value = {classB.Value}"); // Output: 20

        Console.WriteLine("\n=== EXPERIMENT 2: PASSING AS METHOD PARAMETER ===\n");

        structA.Value = 10;
        classA.Value = 10;

        ModifyStruct(structA); // Pass structA by value
        ModifyClass(classA); // Pass classA by reference

        Console.WriteLine($"structA.Value = {structA.Value}"); // Output: 10
        Console.WriteLine($"classA.Value = {classA.Value}"); // Output: 30

        Console.WriteLine("\n=== EXPERIMENT 3: PASSING STRUCT BY REFERENCE (ref) ===\n");

        structA.Value = 10;
        ModifyStructRef(ref structA); // Pass structA by reference
        Console.WriteLine($"structA.Value = {structA.Value}"); // Output: 40

        Console.WriteLine("\n=== EXPERIMENT 4: UPDATING STRUCT VIA RETURN VALUE ===\n");

        structA.Value = 10;
        Console.WriteLine($"structA.Value = {structA.Value}"); // Output: 10
        structA.Value = ModifyStructReturn(structA);
        Console.WriteLine($"structA.Value = {structA.Value}"); // Output: 50

    }

    static void ModifyStruct(MyStruct s)
    {
        s.Value = 30;
    }

    static void ModifyClass(MyClass c)
    {
        c.Value = 30;
    }

    static void ModifyStructRef(ref MyStruct s)
    {
        s.Value = 40;
    }

    static int ModifyStructReturn(MyStruct s)
    {
        s.Value = 50;
        return s.Value;
    }
}