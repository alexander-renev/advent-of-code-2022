using System.ComponentModel;

namespace AdventOfCode2022.Days;

[Day(9)]
public class Day9 : IDay
{
    private enum Direction
    {
        Up, Down, Left, Right
    }

    private record struct Position(int Row, int Col);

    public void CalculateTaskOne(string source)
    {
        var head = new Position(0, 0);
        var tail = new Position(0, 0);
        var visited = new HashSet<Position> { tail };

        foreach (var command in ParseInput(source))
        {
            head = MoveHead(command, head);
            tail = MoveTail(head, tail);
            visited.Add(tail);
        }

        Console.WriteLine(visited.Count);
    }

    public void CalculateTaskTwo(string source)
    {
        var items = Enumerable.Range(0, 10).Select(_ => new Position(0, 0)).ToArray();
        var visited = new HashSet<Position> { items.Last() };

        foreach (var command in ParseInput(source))
        {
            items[0] = MoveHead(command, items[0]);
            foreach (var i in Enumerable.Range(1, 9))
            {
                items[i] = MoveTail(items[i-1], items[i]);
            }

            visited.Add(items.Last());
        }

        Console.WriteLine(visited.Count);
    }

    private static Position MoveTail(Position head, Position tail)
    {
        if (head.Row == tail.Row && Math.Abs(head.Col - tail.Col) > 1)
        {
            tail = tail with { Col = tail.Col > head.Col ? head.Col + 1 : head.Col - 1 };
        }
        else if (head.Col == tail.Col && Math.Abs(head.Row - tail.Row) > 1)
        {
            tail = tail with { Row = tail.Row > head.Row ? head.Row + 1 : head.Row - 1 };
        }
        else if (Math.Abs(head.Col - tail.Col) > 1 || Math.Abs(head.Row - tail.Row) > 1)
        {
            tail = new Position(Row: tail.Row > head.Row ? tail.Row - 1 : tail.Row + 1,
                Col: tail.Col > head.Col ? tail.Col - 1 : tail.Col + 1);
        }

        return tail;
    }

    private static Position MoveHead(Direction command, Position head)
    {
        var newHead = command switch
        {
            Direction.Down => head with { Row = head.Row - 1 },
            Direction.Up => head with { Row = head.Row + 1 },
            Direction.Right => head with { Col = head.Col + 1 },
            Direction.Left => head with { Col = head.Col - 1 },
            _ => throw new InvalidEnumArgumentException($"Invalid direction {command:G}")
        };
        return newHead;
    }

    private static IEnumerable<Direction> ParseInput(string input)
    {
        return input.Split(Environment.NewLine)
            .SelectMany(line =>
            {
                var direction = line[0] switch
                {
                    'U' => Direction.Up,
                    'D' => Direction.Down,
                    'L' => Direction.Left,
                    'R' => Direction.Right
                };

                var count = int.Parse(line.Substring(2));
                return Enumerable.Repeat(direction, count);
            });
    }
}