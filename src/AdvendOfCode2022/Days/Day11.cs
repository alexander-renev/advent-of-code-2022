using System.Numerics;

namespace AdvendOfCode2022.Days;

[Day(11)]
public class Day11 : IDay
{
    private sealed class MonkeyDescription
    {
        public Func<BigInteger , BigInteger> Operation { get; }
        public Func<BigInteger, bool> Test { get; }
        public int TrueMonkeyId { get; }
        public int FalseMonkeyId { get; }

        public MonkeyDescription(Func<BigInteger, BigInteger> operation, Func<BigInteger, bool> test, int trueMonkeyId, int falseMonkeyId)
        {
            Operation = operation;
            Test = test;
            TrueMonkeyId = trueMonkeyId;
            FalseMonkeyId = falseMonkeyId;
        }
    }

    private class Monkey
    {
        public int Index { get; }
        public MonkeyDescription Description { get; }

        public long Inspected { get; private set; } = 0;

        public Monkey(int index, MonkeyDescription description, BigInteger[] items)
        {
            Index = index;
            Description = description;
            Items = new (items);
        }

        public LinkedList<BigInteger> Items { get; }

        public void Inspect() => Inspected += 1;
    }
    
    public void CalculateTaskOne(string source)
    {
        CalculateTask(source, 20, level => level / 3);
    }

    public void CalculateTaskTwo(string source)
    {
        CalculateTask(source, 10000);
    }

    private void CalculateTask(string source, int rounds, Func<BigInteger, BigInteger> worryAdjustment = null)
    {
        var divisors = new HashSet<BigInteger>();
        var monkeys = ParseInput(source, divisors).ToDictionary(m => m.Index);
        var keys = monkeys.Keys.Order().ToArray();
        // Least common multiple for all divisors, we can use it to adjust worry levels so they won't get ridiculously high
        var lcm = divisors.Aggregate((a, b) => a * b / BigInteger.GreatestCommonDivisor(a, b));

        foreach (var roundNumber in Enumerable.Range(1, rounds))
        {
            foreach (var monkeyIndex in keys)
            {
                var monkey = monkeys[monkeyIndex];
                while (monkey.Items.Any())
                {
                    var item = monkey.Items.First();
                    monkey.Items.RemoveFirst();
                    monkey.Inspect();

                    var newLevel = monkey.Description.Operation(item);
                    var adjustedLevel = worryAdjustment?.Invoke(newLevel) ?? newLevel % lcm;
                    var testResult = monkey.Description.Test(adjustedLevel);
                    var targetMonkey = testResult ? monkey.Description.TrueMonkeyId : monkey.Description.FalseMonkeyId;
                    monkeys[targetMonkey].Items.AddLast(adjustedLevel);
                }
            }
        }

        var business = monkeys.Select(m => m.Value.Inspected).OrderDescending().Take(2).Aggregate((x, y) => x * y);
        Console.WriteLine(business);
    }

    private static Monkey[] ParseInput(string input, HashSet<BigInteger> divisors)
    {
        return input.Split(Environment.NewLine + Environment.NewLine).Select(line => ParseMonkey(line, divisors)).ToArray();
    }

    // TODO use FParsec
    private static Monkey ParseMonkey(string monkey, HashSet<BigInteger> divisors)
    {
        var lines = monkey.Split(Environment.NewLine);
        var index = int.Parse(lines[0].Substring("Monkey ".Length).Trim().TrimEnd(':'));
        var startingItems = lines[1].Substring("  Starting items: ".Length).Split(", ").Select(BigInteger.Parse).ToArray();
        var operation = ParseOperation(lines[2].Substring("  Operation: new = ".Length));
        var testDivisor = int.Parse(lines[3].Substring("  Test: divisible by ".Length));
        divisors.Add(testDivisor);
        Func<BigInteger, bool> testOperation = x => x % testDivisor == 0;
        var trueMonkey = int.Parse(lines[4].Substring("    If true: throw to monkey ".Length));
        var falseMonkey = int.Parse(lines[5].Substring("    If false: throw to monkey ".Length));

        return new Monkey(index,
            new MonkeyDescription(operation, testOperation, trueMonkey, falseMonkey),
            startingItems);
    }

    private static Func<BigInteger, BigInteger> ParseOperation(string operation)
    {
        var items = operation.Split(' ');
        Func<BigInteger, BigInteger, BigInteger> performer = items[1] switch
        {
            "+" => (a, b) => a + b,
            "-" => (a, b) => a - b,
            "*" => (a, b) => a * b,
            "/" => (a, b) => a / b,
        };

        static Func<BigInteger, BigInteger> ParseOperand(string operand)
        {
            if (operand is "old")
            {
                return x => x;
            }

            var value = int.Parse(operand);
            return _ => value;
        }

        var operand1 = ParseOperand(items[0]);
        var operand2 = ParseOperand(items[2]);

        return x => performer(operand1(x), operand2(x));
    }
}
