namespace AdventOfCode2022.Days;

[Day(8)]
public class Day8 : IDay
{
    private record struct Position(int Row, int Col);

    public void CalculateTaskOne(string source)
    {
        var positions = ParseInput(source);
        var visibilities = new Dictionary<Position, bool>();

        foreach (var positionKey in positions.Keys)
        {
            visibilities[positionKey] = false;
        }

        var rows = positions.Keys.Max(k => k.Row) + 1;
        var cols = positions.Keys.Max(k => k.Col) + 1;

        foreach (var row in Enumerable.Range(0, rows))
        {
            var maxHeight = -1;
            foreach (var col in Enumerable.Range(0, cols))
            {
                var key = new Position(row, col);
                if (positions[key] > maxHeight)
                {
                    visibilities[key] = true;
                }
                maxHeight = Math.Max(maxHeight, positions[key]);
            }

            maxHeight = -1;
            foreach (var col in Enumerable.Range(0, cols).Select(x => cols - x - 1))
            {
                var key = new Position(row, col);
                if (positions[key] > maxHeight)
                {
                    visibilities[key] = true;
                }
                maxHeight = Math.Max(maxHeight, positions[key]);
            }
        }

        foreach (var col in Enumerable.Range(0, cols))
        {
            var maxHeight = -1;
            foreach (var row in Enumerable.Range(0, rows))
            {
                var key = new Position(row, col);
                if (positions[key] > maxHeight)
                {
                    visibilities[key] = true;
                }
                maxHeight = Math.Max(maxHeight, positions[key]);
            }

            maxHeight = -1;
            foreach (var row in Enumerable.Range(0, rows).Select(x => rows - x - 1))
            {
                var key = new Position(row, col);
                if (positions[key] > maxHeight)
                {
                    visibilities[key] = true;
                }
                maxHeight = Math.Max(maxHeight, positions[key]);
            }
        }

        Console.WriteLine(visibilities.Values.Count(x => x));
    }

    public void CalculateTaskTwo(string source)
    {
        var positions = ParseInput(source);
        var rows = positions.Keys.Max(k => k.Row) + 1;
        var cols = positions.Keys.Max(k => k.Col) + 1;

        var scores = from row in Enumerable.Range(0, rows)
            from col in Enumerable.Range(0, cols)
            select GetScore(row, col, rows, cols, positions);

        Console.WriteLine(scores.Max());
    }

    private static Dictionary<Position, int> ParseInput(string input)
    {
        var positions = new Dictionary<Position, int>();

        foreach (var item in input.Split(Environment.NewLine).Select((line, index) => new { line, index }))
        {
            var row = item.index;
            foreach (var rowItem in item.line.Select((ch, index) => new { ch, index }))
            {
                positions[new Position(row, rowItem.index)] = int.Parse(rowItem.ch.ToString());
            }
        }

        return positions;
    }

    private static int GetScore(int row, int col, int rows, int cols, Dictionary<Position, int> positions)
    {
        var height = positions[new Position(row, col)];

        var score = MakeRanges(row, col, rows, cols)
            .Select(dirs =>
            {
                if (!dirs.Any())
                {
                    return 0;
                }

                var trees = dirs.Select(dir => positions[dir]).TakeWhile(h => h < height).Count()
                    +
                    dirs.Select(dir => positions[dir]).SkipWhile(h => h < height).Take(1).Count();
                return trees;
            })
            .Aggregate((x, y) => x * y);

        return score;
    }

    private static IEnumerable<Position[]> MakeRanges(int row, int col, int rows, int cols)
    {
        if (row > 0)
        {
            yield return Enumerable.Range(1, rows)
                .Select(r => row - r)
                .TakeWhile(r => r >= 0)
                .Select(r => new Position(r, col)).ToArray();
        }
        else
        {
            yield return System.Array.Empty<Position>();
        }

        if (col > 0)
        {
            yield return Enumerable.Range(1, cols)
                .Select(c => col - c)
                .TakeWhile(c => c >= 0)
                .Select(c => new Position(row, c)).ToArray();
        }
        else
        {
            yield return System.Array.Empty<Position>();
        }

        if (row < rows - 1)
        {
            yield return Enumerable.Range(1, rows)
                .Select(r => row + r)
                .TakeWhile(r => r < rows)
                .Select(r => new Position(r, col)).ToArray();
        }
        else
        {
            yield return System.Array.Empty<Position>();
        }

        if (col < cols - 1)
        {
            yield return Enumerable.Range(1, cols)
                .Select(c => col + c)
                .TakeWhile(c => c < cols)
                .Select(c => new Position(row, c)).ToArray();
        }
        else
        {
            yield return System.Array.Empty<Position>();
        }
    }
}
