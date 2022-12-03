namespace AdvendOfCode2022.Days;

[Day(3)]
public class Day3 : DayBase
{
    protected override void CalculateTaskOne(string input)
    {
        var data = ParseInputForOne(input);

        var score = data.Select(item =>
        {
            var (first, second) = item;
            var common = first.ToCharArray().Intersect(second.ToCharArray()).First();
            return GetPriority(common);
        }).Sum();

        Console.WriteLine(score);
    }

    protected override void CalculateTaskTwo(string input)
    {
        var data = ParseInputForTwo(input);

        var score = data.Select(item =>
        {
            var intersection = item.Aggregate((ch1, ch2) => ch1.Intersect(ch2).ToArray())[0];
            return GetPriority(intersection);
        }).Sum();
        
        Console.WriteLine(score);
    }

    private static (string first, string second)[] ParseInputForOne(string input)
    {
        return input.Split(Environment.NewLine)
            .Select(line =>
            {
                var length = line.Length / 2;
                return (line.Substring(0, length), line.Substring(length));
            }).ToArray();
    }

    private static char[][][] ParseInputForTwo(string input)
    {
        return input.Split(Environment.NewLine)
            .Select(s => s.ToCharArray())
            .Chunk(3)
            .ToArray();
    }

    private static int GetPriority(char ch)
    {
        return ch switch
        {
            >= 'a' and <= 'z' => (int)ch - (int)'a' + 1,
            >= 'A' and <= 'Z' => (int)ch - (int)'A' + 27,
            _ => throw new ArgumentOutOfRangeException($"Invalid char \"{ch}\"")
        };
    }
}