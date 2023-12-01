using System.ComponentModel;

namespace AdventOfCode2022.Days;

[Day(17)]
public class Day17 : IDay
{
    private enum Direction
    {
        Left,
        Right
    }

    private static readonly Point[][] Figures = {
        new[] {new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(0, 3)},
        new[] {new Point(1, 0), new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(1, 2)},
        new [] { new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(1, 2), new Point(2, 2) },
        new [] { new Point(0,0), new Point(1, 0), new Point(2, 0), new Point(3, 0)},
        new[] { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1)}
    };

    private readonly record struct Point(long Row, long Column)
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
        Calculate(source, 1000000000000);
    }

    private void Calculate(string source, long steps)
    {
        var directions = ParseInput(source);
        var data = Take(GetFigures(), steps);
        using var directionsEnumerator = directions.GetEnumerator();
        long bottom = 0;
        var listLength = 30;
        var staticPoints = Enumerable.Range(0, 7).ToDictionary(i => (long)i, _ => new LinkedList<long>());
        bool CheckCondition(Point pt) => pt.Column is >= 0 and < 7 && pt.Row > 0 && !staticPoints[pt.Column].Contains(pt.Row);
        var history = new Dictionary<string, List<(long height, long steps)>>();
        var step = 0L;
        foreach (var figure in data)
        {
            step++;
            var currentFigure = Move(figure, bottom + 4, 2, CheckCondition).result;
            while (true)
            {
                directionsEnumerator.MoveNext();
                var direction = directionsEnumerator.Current;
                currentFigure = direction switch
                {
                    Direction.Left => Move(currentFigure, 0, -1, CheckCondition).result,
                    Direction.Right => Move(currentFigure, 0, 1, CheckCondition).result,
                    _ => throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(Direction)),
                };

                var (downResult, downPosition) = Move(currentFigure, -1, 0, CheckCondition);
                if (!downResult)
                {
                    foreach (var item in currentFigure)
                    {
                        var col = staticPoints[item.Column];
                        if (!col.Contains(item.Row))
                        {
                            col.AddLast((long) item.Row);
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
            var description = Describe(staticPoints, 16);
            history.TryAdd(description, new ());
            history[description].Add((bottom, step));
            if (step > 100000)
            {
                break;
            }
        }

        if (step == steps)
        {
            Console.WriteLine($"Height is {bottom}");
            return;
        }
        

        var historyItem = history.Values.First(v => v.Count > 10 && v[^1].height != v[^2].height);
        var period = historyItem[^1].steps - historyItem[^2].steps;
        var remainder = steps % period;

        var foundItem = history.Values.Where(v => v.Count > 10).First(v => v.Last().steps % period == remainder);
        
        var difference = foundItem[^1].height - foundItem[^2].height;
        var height = foundItem[^1].height + (steps - foundItem[^1].steps) / period * difference;
            
        Console.WriteLine($"Height is {height}");
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

    private static (bool success, Point[] result) Move(Point[] figure, long deltaRow, long deltaColumn,
        Func<Day17.Point, bool> checkCondition)
    {
        var result = System.Array.ConvertAll(figure, pt => pt.Move(deltaRow, deltaColumn));
        if (result.All(checkCondition))
        {
            return (true, result);
        }

        return (false, figure);
    }

    private static string Describe(Dictionary<long, LinkedList<long>> points, int height)
    {
        var maxHeight = points.Values.SelectMany(v => v).Max();
        var strings = Enumerable.Range(0, height)
            .Select(i => maxHeight - i)
            .Select(y => new string(Enumerable.Range(0, 7).Select(x => points[x].Contains(y) ? 'x' : ' ').ToArray()))
            .ToArray();
        return string.Join(Environment.NewLine, strings);
    }
}
