namespace AdvendOfCode2022.Days;

[Day(5)]
public class Day5 : IDay
{
    private record Command(int Count, int From, int To);

    private record InputData(Dictionary<int, LinkedList<char>> Input, Command[] Commands);

    public void CalculateTaskOne(string source)
    {
        var input = ParseInput(source);

        var containers = input.Input;
        foreach (var command in input.Commands)
        {
            var (count, from, to) = command;
            var fromContainer = containers[from];
            var toContainer = containers[to];

            foreach (var i in Enumerable.Range(0, count))
            {
                var item = fromContainer.Last.Value;
                fromContainer.RemoveLast();
                toContainer.AddLast(new LinkedListNode<char>(item));
            }
        }

        var tops = containers
            .OrderBy(c => c.Key)
            .Select(c => c.Value.Last.Value)
            .ToArray();

        Console.WriteLine(new string(tops));
    }

    public void CalculateTaskTwo(string source)
    {
        var input = ParseInput(source);

        var containers = input.Input;
        var temp = new Stack<char>();
        foreach (var command in input.Commands)
        {
            var (count, from, to) = command;
            var fromContainer = containers[from];
            var toContainer = containers[to];
            

            foreach (var i in Enumerable.Range(0, count))
            {
                var item = fromContainer.Last.Value;
                fromContainer.RemoveLast();
                temp.Push(item);
            }

            while (temp.Count > 0)
            {
                toContainer.AddLast(new LinkedListNode<Char>(temp.Pop()));
            }
        }

        var tops = containers
            .OrderBy(c => c.Key)
            .Select(c => c.Value.Last.Value)
            .ToArray();

        Console.WriteLine(new string(tops));
    }

    private InputData ParseInput(string source)
    {
        var parts = source.Split(Environment.NewLine + Environment.NewLine);
        var result = new Dictionary<int, LinkedList<char>>();

        var input = parts[0];
        var commands = parts[1];

        var inputLines = input.Split(Environment.NewLine);
        var inputData = inputLines.Take(inputLines.Length - 1).Reverse().ToArray();

        foreach (var line in inputData)
        {
            Span<char> chars = line.ToCharArray();
            var index = 1;
            while (true)
            {
                var symbol = chars.Slice(0, 3);
                if (symbol.Length == 3)
                {
                    if (symbol[0] == '[' && symbol[2] == ']')
                    {
                        result[index] = result.GetValueOrDefault(index) ?? new LinkedList<char>();
                        result[index].AddLast(new LinkedListNode<Char>(symbol[1]));
                    }
                    if (chars.Length < 4)
                    {
                        break;
                    }
                    chars = chars.Slice(4);

                    index++;
                }
                else
                {
                    break;
                }
            }
        }

        var commandsData = commands.Split(Environment.NewLine).Select(cmd =>
        {
            var cmdParts = cmd.Split(' ');
            return new Command(int.Parse(cmdParts[1]), int.Parse(cmdParts[3]), int.Parse(cmdParts[5]));
        }).ToArray();

        return new InputData(result, commandsData);
    }
}
