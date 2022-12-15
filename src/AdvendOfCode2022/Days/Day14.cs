namespace AdvendOfCode2022.Days;

[Day(14)]
public class Day14 : IDay
{
    private readonly record struct Point(int X, int Y)
    {
        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
    
    public void CalculateTaskOne(string source)
    {
        var rocks = ParseInput(source);
        var occupied = rocks.ToHashSet();

        var maxY = rocks.Select(r => r.Y).Max();
        var units = 0;
        var abyss = false;

        while (!abyss)
        {
            var position = new Point(500, 0);
            
            while (true)
            {
                if (position.Y > maxY)
                {
                    abyss = true;
                    break;
                }
                
                var nextBelow = new Point(position.X, position.Y + 1);
                if (!occupied.Contains(nextBelow))
                {
                    position = nextBelow;
                    continue;
                }

                var nextBelowLeft = new Point(position.X - 1, position.Y + 1);
                if (!occupied.Contains(nextBelowLeft))
                {
                    position = nextBelowLeft;
                    continue;
                }
                
                var nextBelowRight = new Point(position.X + 1, position.Y + 1);
                if (!occupied.Contains(nextBelowRight))
                {
                    position = nextBelowRight;
                    continue;
                }

                occupied.Add(position);
                units++;
                break;
            }
        }
        
        Console.WriteLine(units);
    }

    public void CalculateTaskTwo(string source)
    {
        var rocks = ParseInput(source);
        var occupied = rocks.ToHashSet();

        var floor = rocks.Select(r => r.Y).Max() + 2;
        var units = 0;
        var startPoint = new Point(500, 0);

        while (!occupied.Contains(startPoint))
        {
            var position = startPoint;
            
            while (true)
            {
                if (position.Y == floor - 1)
                {
                    occupied.Add(position);
                    units++;
                    break;
                }
                
                var nextBelow = new Point(position.X, position.Y + 1);
                if (!occupied.Contains(nextBelow))
                {
                    position = nextBelow;
                    continue;
                }

                var nextBelowLeft = new Point(position.X - 1, position.Y + 1);
                if (!occupied.Contains(nextBelowLeft))
                {
                    position = nextBelowLeft;
                    continue;
                }
                
                var nextBelowRight = new Point(position.X + 1, position.Y + 1);
                if (!occupied.Contains(nextBelowRight))
                {
                    position = nextBelowRight;
                    continue;
                }

                occupied.Add(position);
                units++;
                break;
            }
        }
        
        Console.WriteLine(units);
    }

    private static Point[] ParseInput(string input)
    {
        var item = Pipe(
            Int.AndL(StringP(",")),
            Int,
            (x, y) => new Point(x, y)
        );
        var line = Many1(item, sep: StringP(" -> "));
        var lines = Many1(line, sep: Many1(Newline), canEndWithSep: true);

        var result = lines.Run(input).GetResult().Select(lst => lst.ToArray()).ToArray();
        return result.SelectMany(line =>
        {
            return line.Zip(line.Skip(1),
                (startPoint, finishPoint) =>
                {
                    if (startPoint.X == finishPoint.X)
                    {
                        var direction = startPoint.Y < finishPoint.Y ? 1 : -1;
                        return Enumerable.Range(0, Math.Abs(startPoint.Y - finishPoint.Y))
                            .Select(i => new Point(startPoint.X, startPoint.Y + i * direction));
                    }
                    else
                    {
                        var direction = startPoint.X < finishPoint.X ? 1 : -1;
                        return Enumerable.Range(0, Math.Abs(startPoint.X - finishPoint.X))
                            .Select(i => new Point(startPoint.X + i * direction, startPoint.Y));
                    }

                }).SelectMany(x => x).Concat(new [] { line.Last() });
        }).ToArray();
    }
}
