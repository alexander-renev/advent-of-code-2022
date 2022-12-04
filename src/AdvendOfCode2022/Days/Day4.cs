namespace AdvendOfCode2022.Days;

[Day(4)]
public class Day4 : DayBase
{
    private record struct ValueRange(int From, int To);

    protected override void CalculateTaskOne(String input)
    {
        var data = ParseInput(input);
        var count = data.Where(item =>
        {
            var (first, second) = item;
            return (
                       IsBetween(first.From, second.From, second.To)
                       && IsBetween(first.To, second.From, second.To)
                   )
                   ||
                   (
                       IsBetween(second.From, first.From, first.To)
                       && IsBetween(second.To, first.From, first.To)
                   );
        }).Count();

        Console.WriteLine(count);
    }

    protected override void CalculateTaskTwo(String input)
    {
        var data = ParseInput(input);
        var count = data.Where(item =>
        {
            var (first, second) = item;
            return (
                       IsBetween(first.From, second.From, second.To)
                       || IsBetween(first.To, second.From, second.To)
                   )
                   ||
                   (
                       IsBetween(second.From, first.From, first.To)
                       || IsBetween(second.To, first.From, first.To)
                   );
        }).Count();

        Console.WriteLine(count);
    }

    private static (ValueRange first, ValueRange second)[] ParseInput(string input)
    {
        return input.Split(Environment.NewLine)
            .Select(line =>
            {
                var parts = line.Split(',');
                return (ParseRange(parts[0]), ParseRange(parts[1]));
            }).ToArray();
    }

    private static ValueRange ParseRange(string input)
    {
        var parts = input.Split('-');
        return new ValueRange(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static bool IsBetween(int value, int from, int to)
    {
        return value >= from && value <= to;
    }
}