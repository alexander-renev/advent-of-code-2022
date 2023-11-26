namespace AdventOfCode2022.Days;

[Day(10)]
public class Day10 : IDay
{
    private interface ICommand
    {
    }

    private record Noop : ICommand
    {
    }

    private record Add(int Value) : ICommand
    {
    }

    public void CalculateTaskOne(string source)
    {
        var history = MakeHistory(source);

        var sum = new[] { 20, 60, 100, 140, 180, 220 }.Select(x => x * history[x]).Sum();
        Console.WriteLine(sum);
    }

    public void CalculateTaskTwo(string source)
    {
        var history = MakeHistory(source);
        var board = new int[6, 40];

        foreach (var row in Enumerable.Range(0, 6))
        {
            var start = 40 * row;
            foreach (var cycle in Enumerable.Range(0, 40))
            {
                var value = history[cycle + start + 1];
                var coords = new[] { value - 1, value, value + 1 }.Where(c => c is >= 0 and < 40).ToArray();
                if (coords.Contains(cycle))
                {
                    board[row, cycle] = 1;
                }
                else
                {
                    board[row, cycle] = 0;
                }
            }
        }

        var lines = Enumerable.Range(0, 6)
            .Select(row =>
                new String(Enumerable.Range(0, 40).Select(col => board[row, col] == 1 ? '#' : '.').ToArray()));
        Console.WriteLine(string.Join(Environment.NewLine, lines));
    }

    private static Dictionary<Int32, Int32> MakeHistory(String source)
    {
        var commands = ParseInput(source);
        var history = new Dictionary<int, int>();
        var cycle = 1;
        var register = 1;

        foreach (var command in commands)
        {
            if (command is Noop)
            {
                history[cycle] = register;
                cycle++;
            }
            else if (command is Add addCommand)
            {
                history[cycle] = register;
                cycle++;
                history[cycle] = register;
                cycle++;
                register += addCommand.Value;
            }
            else
            {
                throw new InvalidOperationException("Unsupported command");
            }
        }

        return history;
    }

    private static ICommand[] ParseInput(string input)
    {
        return input.Split(Environment.NewLine)
            .Select<string, ICommand>(line =>
            {
                if (line == "noop")
                {
                    return new Noop();
                }

                if (line.StartsWith("addx"))
                {
                    return new Add(int.Parse(line.Substring(5)));
                }

                throw new InvalidOperationException($"Invalid command: {line}");
            }).ToArray();
    }
}
