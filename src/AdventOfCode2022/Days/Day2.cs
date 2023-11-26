using System.ComponentModel;

namespace AdventOfCode2022.Days;

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
                Result.Lose => WinnersOf[opponent],
                _ => throw new InvalidEnumArgumentException(nameof(result), (int)result, typeof(Result)),
            };
            var points = (int)me + (int)result;
            return points;
        }).Sum();

        Console.WriteLine(score);
    }

    private (Figure opponent, Figure me)[] ParseInputForOne(string input)
    {
        var parseFigure = Choice(
            StringP("A", Figure.Rock),
            StringP("B", Figure.Paper),
            StringP("C", Figure.Scissors),
            StringP("X", Figure.Rock),
            StringP("Y", Figure.Paper),
            StringP("Z", Figure.Scissors)
        );

        var parseLine = Pipe(
            parseFigure,
            WS,
            parseFigure, 
            (c1, _, c2) => (c1, c2));

        var parser = Many(parseLine, sep: Newline, canEndWithSep: true);

        return parser.Run(input).GetResult().ToArray();
    }

    private (Figure opponent, Result result)[] ParseInputForTwo(string input)
    {
        var parseFigure = Choice(
            StringP("A", Figure.Rock),
            StringP("B", Figure.Paper),
            StringP("C", Figure.Scissors)
        );
        
        var parseResult = Choice(
            StringP("X", Result.Lose),
            StringP("Y", Result.Draw),
            StringP("Z", Result.Win)
        );

        var parseLine = Pipe(
            parseFigure,
            WS,
            parseResult, 
            (c1, _, c2) => (c1, c2));

        var parser = Many(parseLine, sep: Newline, canEndWithSep: true);

        return parser.Run(input).GetResult().ToArray();
    }
}
