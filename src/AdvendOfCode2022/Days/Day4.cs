namespace AdvendOfCode2022.Days;

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
        return input.Split(Environment.NewLine)
            .Select(line =>
            {
                var parts = line.Split(',');
                return (ParseRange(parts[0]), ParseRange(parts[1]));
            }).ToArray();
    }

    private static Range ParseRange(string input)
    {
        var parts = input.Split('-');
        return int.Parse(parts[0]) .. int.Parse(parts[1]);
    }
}

file static class RangeExtensions
{
    public static bool Contains(this Range range, int value)
    {
        return range.Start.Value <= value && value <= range.End.Value;
    }
}