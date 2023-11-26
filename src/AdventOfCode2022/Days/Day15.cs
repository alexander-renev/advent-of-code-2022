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

        var ranges = sensors.Select(sensor =>
        {
            var distance = sensor.Distance;
            var yDelta = Math.Abs(y - sensor.Position.Y);
            if (yDelta > distance)
            {
                return (Pair?) null;
            }

            var xDelta = distance - yDelta;
            var x = sensor.Position.X;
            return new Pair(x - xDelta, x + xDelta);
        }).ToArray();

        var total = ranges.Aggregate((r1, r2) =>
        {
            if (r1 is null)
            {
                return r2;
            }

            if (r2 is null)
            {
                return r1;
            }

            return new Pair(Math.Min(r1.Value.From, r2.Value.From),
                Math.Max(r1.Value.To, r2.Value.To));
        });

        if (total is null)
        {
            Console.WriteLine("0");
            return;
        }
        
        Console.WriteLine(total.Value.To - total.Value.From);
    }

    public void CalculateTaskTwo(string source)
    {
        var (_, limit, sensors) = ParseInput(source);
        Point? result = null;

        var point = new Point(0, 0);
        while (true)
        {
            var failingSensor = sensors.Where(s => s.Position.DistanceTo(point) <= s.Distance)
                .Select(s => (Sensor?)s)
                .FirstOrDefault();
            
            if (failingSensor is null)
            {
                result = point;
                break;
            }

            var sensor = failingSensor.Value;
            
            // move right so this sensor won't see us
            point = point with {X = sensor.Position.X + (sensor.Distance - Math.Abs(sensor.Position.Y - point.Y) + 1)};
            if (point.X > limit)
            {
                point = new Point(0, point.Y + 1);
            }

            if (point.Y > limit)
            {
                break;
            }
        }
        
        if (result is not null)
        {
            Console.WriteLine(result.Value.X * 4000000L + result.Value.Y);
        }
        else
        {
            Console.WriteLine("Not found");
        }
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
