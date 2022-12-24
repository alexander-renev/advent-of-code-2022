namespace AdventOfCode2022.Days;

[Day(18)]
public class Day18 : IDay
{
    private readonly record struct Point(int X, int Y, int Z);
    
    public void CalculateTaskOne(string source)
    {
        var points = ParseInput(source).ToHashSet();
        var result = points.Select(pt => GetAdjacent(pt).Count(pt => !points.Contains(pt))).Sum();
        
        Console.WriteLine(result);
    }

    public void CalculateTaskTwo(string source)
    {
        var points = ParseInput(source).ToHashSet();
        var connectedPoints = new HashSet<Point>();
        var maxCoord = points.Select(pt => Math.Max(pt.X, Math.Max(pt.Y, pt.Z))).Max();
        var minCoord = points.Select(pt => Math.Min(pt.X, Math.Min(pt.Y, pt.Z))).Min();
        var result = points.Select(pt => GetAdjacent(pt).Count(pt => !points.Contains(pt) && HasWayOut(pt))).Sum();
        
        Console.WriteLine(result);

        bool HasWayOut(Point pt)
        {
            if (connectedPoints.Contains(pt))
            {
                return true;
            }

            var route = new LinkedList<Point>();
            var visited = new HashSet<Point>();
            route.AddLast(pt);
            var current = pt;

            while (route.Count > 0)
            {
                var next = GetAdjacent(current)
                    .Where(pt => !points.Contains(pt) && !visited.Contains(pt))
                    .Select(pt => (Point?) pt)
                    .FirstOrDefault();

                if (next is null)
                {
                    current = route.Last.Value;
                    route.RemoveLast();
                    continue;
                }

                current = next.Value;
                route.AddLast(current);
                visited.Add(current);
                
                if (
                     current.X >  maxCoord || current.X < minCoord
                     ||
                     current.Y > maxCoord || current.Y < minCoord
                     ||
                     current.Z > maxCoord || current.Z < minCoord
                )
                {
                    foreach (var visitedPoint in visited)
                    {
                        connectedPoints.Add(visitedPoint);
                    }

                    return true;
                }
            }

            return false;
        }
    }

    private static Point[] ParseInput(string input)
    {
        return input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(line =>
            {
                var coords = line.Split(",").Select(int.Parse).ToArray();
                return new Point(coords[0], coords[1], coords[2]);
            }).ToArray();
    }

    private static IEnumerable<Point> GetAdjacent(Point point)
    {
        yield return point with {X = point.X + 1};
        yield return point with {X = point.X - 1};
        yield return point with {Y = point.Y + 1};
        yield return point with {Y = point.Y - 1};
        yield return point with {Z = point.Z + 1};
        yield return point with {Z = point.Z - 1};
    }
}
