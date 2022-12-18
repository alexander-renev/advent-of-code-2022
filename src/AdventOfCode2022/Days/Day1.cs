namespace AdventOfCode2022.Days;

[Day(1)]
public sealed class Day1 : IDay
{
    public void CalculateTaskOne(string source)
    {
        var data = ParseSource(source);
        var maxValue = data.Select(d => d.Sum()).Max();
        Console.WriteLine(maxValue);
    }

    public void CalculateTaskTwo(string source)
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
        var longLines = Many(Long, sep: Newline.AndTry(NotFollowedByNewline));
        var longLineGroups = Many(longLines, sep: Newline.And(Newline));
        var result = longLineGroups.Run(source);
        return result.GetResult().Select(r => r.ToArray()).ToArray();
    }
}
