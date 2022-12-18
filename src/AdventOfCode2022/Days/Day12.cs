namespace AdventOfCode2022.Days;

[Day(12)]
public class Day12 : IDay
{
    private record struct Position(int Row, int Col);

    private record struct Input(int[,] Heights, int Rows, int Cols, Position Start, Position Finish);

    private record Step(int Number, HashSet<Position> Finishes, LinkedList<Position>[] CurrentRoutes, HashSet<Position> VisitedPositions);
    
    public void CalculateTaskOne(string source)
    {
        var input = ParseInput(source);
        var firstStep = new Step(
            0,
            new HashSet<Position>(new[] { input.Finish }),
            new[] { new LinkedList<Position>(new[] { input.Finish }) },
            new HashSet<Position>(new[] { input.Finish }));

        var steps = new List<Step> { firstStep };
        var step = firstStep;

        while (true)
        {
            var newStep = NextStep(step, input);
            if (newStep.Finishes.Contains(input.Start))
            {
                Console.WriteLine($"Distance is {newStep.Number}");
                break;
            }

            step = newStep;
        }
    }

    public void CalculateTaskTwo(string source)
    {
        var input = ParseInput(source);
        var firstStep = new Step(
            0,
            new HashSet<Position>(new[] { input.Finish }),
            new[] { new LinkedList<Position>(new[] { input.Finish }) },
            new HashSet<Position>(new[] { input.Finish }));

        var steps = new List<Step> { firstStep };
        var step = firstStep;

        while (true)
        {
            var newStep = NextStep(step, input);
            if (newStep.Finishes.Select(f => input.Heights[f.Row, f.Col]).Any(h => h == 1))
            {
                Console.WriteLine($"Distance is {newStep.Number}");
                break;
            }

            step = newStep;
        }
    }

    private static Step NextStep(Step step, Input input)
    {
        var lastVisited = step.VisitedPositions;
        var currentVisited = new HashSet<Position>();
        var newRoutes = new List<LinkedList<Position>>();

        foreach (var route in step.CurrentRoutes)
        {
            var position = route.Last.Value;
            foreach (var nextPosition in GetNextPositions(position, input.Heights, input.Rows, input.Cols))
            {
                if (lastVisited.Contains(nextPosition) || currentVisited.Contains(nextPosition))
                {
                    continue;
                }

                currentVisited.Add(nextPosition);
                var clone = new LinkedList<Position>(route);
                clone.AddLast(nextPosition);
                newRoutes.Add(clone);
            }
        }

        var newVisited = new HashSet<Position>(currentVisited);
        currentVisited.ToList().ForEach(p => newVisited.Add(p));
        var finishes = newRoutes.Select(r => r.Last.Value).ToHashSet();

        return new Step(step.Number + 1, finishes, newRoutes.ToArray(), newVisited);
    }

    private static IEnumerable<Position> GetNextPositions(Position current, int[,] heights, int rows, int cols)
    {
        var height = heights[current.Row, current.Col];
        return new[]
        {
            new Position(current.Row, current.Col + 1),
            new Position(current.Row, current.Col - 1),
            new Position(current.Row + 1, current.Col),
            new Position(current.Row - 1, current.Col),
        }.Where(pos =>
            pos.Col >= 0 && pos.Col < cols &&
            pos.Row >= 0 && pos.Row < rows &&
            height - heights[pos.Row, pos.Col] <= 1);
    }

    private static Input ParseInput(string input)
    {
        var lines = input.Split(Environment.NewLine).Where(s => !string.IsNullOrEmpty(s)).ToArray();
        var rows = lines.Length;
        var cols = lines[0].Length;

        var heights = new int[rows, cols];
        Position start = new Position(-1, -1);
        Position finish = new Position(-1, -1);

        foreach (var row in Enumerable.Range(0, rows))
        {
            foreach (var col in Enumerable.Range(0, cols))
            {
                var ch = lines[row][col];
                if (ch == 'S')
                {
                    start = new Position(row, col);
                    ch = 'a';
                }
                else if (ch == 'E')
                {
                    finish = new Position(row, col);
                    ch = 'z';
                }

                heights[row, col] = (int) ch - (int) 'a' + 1;
            }
        }

        if (start is {Row: -1, Col: -1} || finish is {Row: -1, Col: -1})
        {
            throw new InvalidOperationException("Failed to find start ir finish");
        }

        return new Input(heights, rows, cols, start, finish);
    }
}
