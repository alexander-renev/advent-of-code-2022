namespace AdventOfCode2022.Days;

[Day(4)]
public class Day4 : IDay
{
    public void CalculateTaskOne(String input)
    {
        var data = ParseInput(input);
        var count = data.Where(item =>
        {
            var (first, second) = item;
            return (
                       second.Contains(first.Start.Value)
                       &&
                       second.Contains(first.End.Value)
                   )
                   ||
                   (
                       first.Contains(second.Start.Value)
                       &&
                       first.Contains(second.End.Value)
                   );
        }).Count();

        Console.WriteLine(count);
    }

    public void CalculateTaskTwo(String input)
    {
        var data = ParseInput(input);
        var count = data.Where(item =>
        {
            var (first, second) = item;
            return (
                       second.Contains(first.Start.Value)
                       ||
                       second.Contains(first.End.Value)
                   )
                   ||
                   (
                       first.Contains(second.Start.Value)
                       ||
                       first.Contains(second.End.Value)
                   );
        }).Count();

        Console.WriteLine(count);
    }

    private static (Range first, Range second)[] ParseInput(string input)
    {
        // 2-4,6-8
        var rangeParser = Pipe(
            Int,
            StringP("-"),
            Int,
            (a, _, b) => a..b
        );

        var lineParser = Pipe(
            rangeParser,
            StringP(","),
            rangeParser,
            (r1, _, r2) => (r1, r2)
        );

        var parser = Many1(lineParser, sep: Newline, canEndWithSep: true);

        return parser.Run(input).GetResult().ToArray();
    }
}

file static class RangeExtensions
{
    public static bool Contains(this Range range, int value)
    {
        return range.Start.Value <= value && value <= range.End.Value;
    }
}
