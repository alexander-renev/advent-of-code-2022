using AdvendOfCode2022.Inputs;

namespace AdvendOfCode2022.Days;

[Day(1)]
public sealed class Day1 : DayBase
{
    protected override void CalculateTaskOne(string source)
    {
        var data = ParseSource(source);
        var maxValue = data.Select(d => d.Sum()).Max();
        Console.WriteLine(maxValue);
    }

    protected override void CalculateTaskTwo(string source)
    {
        var data = ParseSource(source);
        var max3Value = data.Select(d => d.Sum())
            .OrderDescending()
            .Take(3)
            .Sum();
        Console.WriteLine(max3Value);
    }

    private static long[][] ParseSource(string source)
    {
        var elves = source.Split(Environment.NewLine + Environment.NewLine);
        return elves
            .Select(e => e.Split(Environment.NewLine).Select(long.Parse).ToArray())
            .ToArray();
    }
}
