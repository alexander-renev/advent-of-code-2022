namespace AdvendOfCode2022.Days;

[Day(6)]
public class Day6 : IDay
{
    public void CalculateTaskOne(string source)
    {
        var chars = source.ToCharArray();
        var window = new LinkedList<char>();
        for (var i = 0; i < 3; i++)
        {
            window.AddLast(new LinkedListNode<Char>(chars[i]));
        }

        var index = 0;
        foreach (var ch in chars.Skip(3))
        {
            window.AddLast(new LinkedListNode<Char>(ch));
            if (window.Distinct().Count() == 4)
            {
                break;
            }

            index++;
            window.RemoveFirst();
        }

        Console.WriteLine(index + 4);
    }

    public void CalculateTaskTwo(string source)
    {
        var chars = source.ToCharArray();
        var window = new LinkedList<char>();
        for (var i = 0; i < 13; i++)
        {
            window.AddLast(new LinkedListNode<Char>(chars[i]));
        }

        var index = 0;
        foreach (var ch in chars.Skip(13))
        {
            window.AddLast(new LinkedListNode<Char>(ch));
            if (window.Distinct().Count() == 14)
            {
                break;
            }

            index++;
            window.RemoveFirst();
        }

        Console.WriteLine(index + 14);
    }
}