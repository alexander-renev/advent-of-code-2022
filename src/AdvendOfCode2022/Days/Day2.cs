namespace AdvendOfCode2022.Days;

[Day(2)]
public sealed class Day2 : IDay
{
    private enum Figure
    {
        Rock = 1, Paper = 2, Scissors = 3
    }

    private enum Result
    {
        Win = 6, Draw = 3, Lose = 0
    }

    private static readonly Dictionary<Figure, Figure> WinnersOf = new()
    {
        { Figure.Rock, Figure.Scissors },
        { Figure.Paper, Figure.Rock },
        { Figure.Scissors, Figure.Paper }
    };

    private static readonly Dictionary<Figure, Figure> LosersOf = WinnersOf
        .ToDictionary(p => p.Value, p => p.Key);

    public void CalculateTaskOne(string input)
    {
        var data = ParseInputForOne(input);

        var score = data.Select(item =>
        {
            var (opponent, me) = item;
            var value = (int)me;
            if (opponent == me)
            {
                value += 3;
            }
            else if (WinnersOf[me] == opponent)
            {
                value += 6;
            }

            return value;
        }).Sum();

        Console.WriteLine(score);
    }

    public void CalculateTaskTwo(string input)
    {
        var data = ParseInputForTwo(input);

        var score = data.Select(item =>
        {
            var (opponent, result) = item;
            var me = result switch
            {
                Result.Draw => opponent,
                Result.Win => LosersOf[opponent],
                Result.Lose => WinnersOf[opponent]
            };
            var points = (int)me + (int)result;
            return points;
        }).Sum();

        Console.WriteLine(score);
    }

    private (Figure opponent, Figure me)[] ParseInputForOne(string input)
    {
        return input.Split(Environment.NewLine)
            .Select(line =>
            {
                var items = line.Split(' ');
                var opponent = items[0][0] switch
                {
                    'A' => Figure.Rock,
                    'B' => Figure.Paper,
                    'C' => Figure.Scissors
                };
                var me = items[1][0] switch
                {
                    'X' => Figure.Rock,
                    'Y' => Figure.Paper,
                    'Z' => Figure.Scissors
                };

                return (opponent, me);
            }).ToArray();
    }

    private (Figure opponent, Result result)[] ParseInputForTwo(string input)
    {
        return input.Split(Environment.NewLine)
            .Select(line =>
            {
                var items = line.Split(' ');
                var opponent = items[0][0] switch
                {
                    'A' => Figure.Rock,
                    'B' => Figure.Paper,
                    'C' => Figure.Scissors
                };
                var result = items[1][0] switch
                {
                    'X' => Result.Lose,
                    'Y' => Result.Draw,
                    'Z' => Result.Win
                };

                return (opponent, result);
            }).ToArray();
    }
}