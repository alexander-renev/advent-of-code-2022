using System.Collections.Concurrent;
using System.Diagnostics;

namespace AdventOfCode2022.Days;

[Day(15)]
public class Day15 : IDay
{
    [DebuggerDisplay("({X},{Y})")]
    private readonly record struct Point(int X, int Y)
    {
        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public int DistanceTo(in Point pt)
        {
            return Math.Abs(X - pt.X) + Math.Abs(Y - pt.Y);
        }
    }

    [DebuggerDisplay("{Position} -> {Closest}")]
    private readonly record struct Sensor(Point Position, Point Closest)
    {
        public int Distance { get; } = Position.DistanceTo(Closest);
        
        public override string ToString()
        {
            return $"{Position} -> {Closest}";
        }
    }

    private readonly record struct Pair(int From, int To);
    
    public void CalculateTaskOne(string source)
    {
        var (y, _,  sensors) = ParseInput(source);
        var beacons = sensors.Select(s => s.Closest).ToHashSet();
        var result = new ConcurrentBag<Point>();

        var maxCoords = new[]
        {
            sensors.Max(s => s.Position.X),
            sensors.Max(s => s.Position.Y),
            sensors.Max(s => s.Closest.X),
            sensors.Max(s => s.Closest.Y),
        }.Max() * 10;
        
        Parallel.For(
            0,
            maxCoords,
            new ParallelOptions { MaxDegreeOfParallelism = 32},
            i =>
            {
                Span<Point> pts = stackalloc Point[2];
                pts[0] = new Point(i, y);
                pts[1] = new Point(-i, y);

                foreach (ref var pt in pts)
                {
                    if (!beacons.Contains(pt) && IsCloseToAny(pt, sensors))
                    {
                        result.Add(pt);
                    }    
                }
            });
        
        Console.WriteLine(result.Count);
    }

    public void CalculateTaskTwo(string source)
    {
        var (y, limit, sensors) = ParseInput(source);
        var beacons = sensors.Select(s => s.Closest).ToHashSet();
        Point? result = null;

        Parallel.For(0,
            limit + 1,
            new ParallelOptions { MaxDegreeOfParallelism = 16},
            (y, state) =>
            {
                var ranges = sensors.Select(sensor =>
                {
                    var heightDifference = Math.Abs(sensor.Position.Y - y);
                    if (heightDifference > sensor.Distance)
                    {
                        return (Pair?) null;
                    }

                    var horizontalDifference = sensor.Distance - heightDifference;
                    return new Pair(Math.Max(sensor.Position.X - horizontalDifference, 0), Math.Min(sensor.Position.X + horizontalDifference, limit));
                }).Where(s => s.HasValue).Select(s => s.Value).OrderBy(p => p.From);

                var rangesList = new LinkedList<Pair>(ranges);

                var x = 0;
                var found = false;
                while (x <= limit)
                {
                    if (rangesList.Count == 0)
                    {
                        found = true;
                        break;
                    }

                    var range = rangesList.First.Value;
                    if (x >= range.From && x <= range.To)
                    {
                        x = range.To + 1;
                        continue;
                    }
                    rangesList.RemoveFirst();
                }

                if (found)
                {
                    result = new Point(x, y);
                    state.Break();
                }
            });

        var value = (long) result.Value.X * 4000000 + result.Value.Y;
        Console.WriteLine($"{result.Value} ({value})");
    }

    private static (int y, int limit, Sensor[] Sensors) ParseInput(string input)
    {
        var firstLine = StringP("y=").AndR(Int);
        var secondLine = StringP("limit=").AndR(Int);
        var point = Pipe(
            StringP("x="),
            Int.AndL(StringP(",")),
            WS.AndR(StringP("y=")),
            Int,
            (_, x, _, y) => new Point(x, y)
        );
        var sensor = Pipe(
            StringP("Sensor at").AndR(WS),
            point.AndL(StringP(":")),
            WS.AndR(StringP("closest beacon is at")).AndR(WS),
            point,
            (_, pt1, _, pt2) => new Sensor(pt1, pt2)
        );

        var resultParser = Pipe(
            firstLine.AndL(Newline),
            secondLine.AndL(Newline),
            Many1(sensor, sep: Many1(Newline), canEndWithSep: true),
            (y, limit, sensors) => (y, limit, sensors.ToArray())
        );

        var result = resultParser.Run(input).GetResult();
        return result;
    }

    private static bool IsCloseToAny(Point pt, Sensor[] sensors)
    {
        return sensors.Any(s => s.Position.DistanceTo(pt) <= s.Distance);
    }
    
    private static bool IsFarFromAny(Point pt, Sensor[] sensors)
    {
        return sensors.All(s => s.Position.DistanceTo(pt) > s.Distance);
    }
}
