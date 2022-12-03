namespace AdvendOfCode2022;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class InputAttribute : Attribute
{
    public int Number { get; }

    public InputAttribute(int number)
    {
        Number = number;
    }
}