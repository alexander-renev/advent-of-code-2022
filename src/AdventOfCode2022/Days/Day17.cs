using System.ComponentModel;

namespace AdventOfCode2022.Days;

[Day(17)]
public class Day17 : IDay
{
    public enum Direction
    {
        Left,
        Right
    }

    private static readonly Point[][] Figures = new[]
    {
        new[] {new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(0, 3)},
        new[] {new Point(1, 0), new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(1, 2)},
        new [] { new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(1, 2), new Point(2, 2) },
        new [] { new Point(0,0), new Point(1, 0), new Point(2, 0), new Point(3, 0)},
        new[] { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1)}
    };

    public readonly record struct Point(long Row, long Column)
    {
        public Point Move(long deltaRow, long deltaColumn)
        {
            return new Point(Row + deltaRow, Column + deltaColumn);
        }
    }

    public void CalculateTaskOne(string source)
    {
        Calculate(source, 2022);
    }

    public void CalculateTaskTwo(string source)
    {
        //Calculate(source, 1000000000000);
    }

    private void Calculate(string source, long steps)
    {
        var directions = ParseInput(source);
        var data = Take(GetFigures(), steps);
        var directionsEnumerator = directions.GetEnumerator();
        long bottom = 0;
        var listLength = 30;
        var staticPoints = Enumerable.Range(0, 7).ToDictionary(i => (long)i, _ => new LinkedList<long>());
        bool CheckCondition(Point pt) => pt.Column is >= 0 and < 7 && pt.Row > 0 && !staticPoints[pt.Column].Contains(pt.Row);
        foreach (var figure in data)
        {
            var currentFigure = figure.Move(bottom + 4, 2, CheckCondition).result;
            while (true)
            {
                directionsEnumerator.MoveNext();
                var direction = directionsEnumerator.Current;
                currentFigure = direction switch
                {
                    Direction.Left => currentFigure.Move(0, -1, CheckCondition).result,
                    Direction.Right => currentFigure.Move(0, 1, CheckCondition).result,
                    _ => throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(Direction)),
                };

                var (downResult, downPosition) = currentFigure.Move(-1, 0, CheckCondition);
                if (!downResult)
                {
                    foreach (var item in currentFigure)
                    {
                        var col = staticPoints[item.Column];
                        if (!col.Contains(item.Row))
                        {
                            col.AddLast(item.Row);
                        }
                    }

                    foreach (var lst in staticPoints.Values)
                    {
                        while (lst.Count > listLength)
                        {
                            lst.RemoveFirst();
                        }
                    }

                    break;
                }

                currentFigure = downPosition;
            }

            bottom = staticPoints.Values.SelectMany(v => v).Max();
        }
        
        Console.WriteLine($"Height is {bottom}");
    }

    private static IEnumerable<Direction> ParseInput(string input)
    {
        input = input.Trim();
        var sample = input.Select(ch => ch == '<' ? Direction.Left : Direction.Right).ToArray();

        while (true)
        {
            foreach (var ch in sample)
            {
                yield return ch;
            }
        }
    }

    private static IEnumerable<Point[]> GetFigures()
    {
        while (true)
        {
            foreach (var f in Figures)
            {
                yield return f;
            }
        }
    }

    private static IEnumerable<T> Take<T>(IEnumerable<T> source, long count)
    {
        long index = 0;
        foreach (var item in source)
        {
            yield return item;
            index++;
            if (index == count)
            {
                yield break;
            }
        }
    }
}

file static class FigureExtensions
{
    public static (bool success, Day17.Point[] result) Move(this Day17.Point[] figure, long deltaRow, long deltaColumn,
        Func<Day17.Point, bool> checkCondition)
    {
        var result = System.Array.ConvertAll(figure, pt => pt.Move(deltaRow, deltaColumn));
        if (result.All(checkCondition))
        {
            return (true, result);
        }

        return (false, figure);
    }
}
