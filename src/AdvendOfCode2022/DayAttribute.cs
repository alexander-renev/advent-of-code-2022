namespace AdvendOfCode2022;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class DayAttribute : Attribute
{
    public int Number { get; }

    public DayAttribute(int number)
    {
        Number = number;
    }
}