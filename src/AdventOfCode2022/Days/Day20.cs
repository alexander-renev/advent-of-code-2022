namespace AdventOfCode2022.Days;

[Day(20)]
public class Day20 : IDay
{
    public void CalculateTaskOne(string source)
    {
        var numbers = ParseInput(source).ToList();
        var list = new LinkedList<int>();
        var nodes = new List<LinkedListNode<int>>();

        foreach (var number in numbers)
        {
            nodes.Add(list.AddLast(number));
        }
        
        foreach (var node in nodes)
        {
            var value = node.Value;
            if (value == 0)
            {
                continue;
            }

            if (value > 0)
            {
                var previous = node.Previous;
                if (previous == null)
                {
                    previous = list.Last;
                }
                list.Remove(node);
                foreach (var i in Enumerable.Range(0, value))
                {
                    previous = previous.Next ?? list.First;
                }

                list.AddAfter(previous, node);
            }
            else
            {
                var next = node.Next;
                if (next == null)
                {
                    next = list.First;
                }
                list.Remove(node);
                foreach (var i in Enumerable.Range(0, -value))
                {
                    next = next.Previous ?? list.Last;
                }

                list.AddBefore(next, node);
            }
        }

        var zero = list.Find(0);
        var current = zero;
        var values = new List<int>(3);
        foreach (var i in Enumerable.Range(1, 3000))
        {
            current = current.Next ?? list.First;
            if (i % 1000 == 0)
            {
                values.Add(current.Value);
            }
        }
        
        Console.WriteLine(values.Sum());
    }

    public void CalculateTaskTwo(string source)
    {
        var numbers = ParseInput(source).Select(v => v * 811589153L).ToList();
        var list = new LinkedList<long>();
        var nodes = new List<LinkedListNode<long>>();
        var divisor = numbers.Count - 1;

        foreach (var number in numbers)
        {
            nodes.Add(list.AddLast(number));
        }

        foreach (var time in Enumerable.Range(0, 10))
        {
            foreach (var node in nodes)
            {
                var value = node.Value;
                if (value == 0)
                {
                    continue;
                }

                if (value > 0)
                {
                    var previous = node.Previous;
                    if (previous == null)
                    {
                        previous = list.Last;
                    }
                    list.Remove(node);
                    foreach (var i in Enumerable.Range(0, (int)(value % divisor)))
                    {
                        previous = previous.Next ?? list.First;
                    }

                    list.AddAfter(previous, node);
                }
                else
                {
                    var next = node.Next;
                    if (next == null)
                    {
                        next = list.First;
                    }
                    list.Remove(node);
                    foreach (var i in Enumerable.Range(0, -(int)(value % divisor)))
                    {
                        next = next.Previous ?? list.Last;
                    }

                    list.AddBefore(next, node);
                }
            }
        }

        var zero = list.Find(0);
        var current = zero;
        var values = new List<long>(3);
        foreach (var i in Enumerable.Range(1, 3000))
        {
            current = current.Next ?? list.First;
            if (i % 1000 == 0)
            {
                values.Add(current.Value);
            }
        }
        
        Console.WriteLine(values.Sum());
    }

    private static int[] ParseInput(string input)
    {
        return input.Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
    }
}
