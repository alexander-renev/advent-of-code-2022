namespace AdventOfCode2022.Helpers;

public sealed class ProgressHelper
{
    private readonly int _total;
    private int _current;

    private ProgressHelper(int total)
    {
        _total = total;
        _current = 0;
        Console.Write(new string(Enumerable.Repeat('_', _total).ToArray()));
        Console.Write("\r");
    }

    public static ProgressHelper Create(int total)
    {
        return new ProgressHelper(total);
    }

    public void AddProgress()
    {
        Console.Write("*");
        _current++;
        if (_current == _total)
        {
            Console.WriteLine();
        }
    }
}
