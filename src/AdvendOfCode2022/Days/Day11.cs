using System.Numerics;

namespace AdvendOfCode2022.Days;

[Day(11)]
public class Day11 : IDay
{
    private interface IOperand
    {
    }

    private sealed class OldValue : IOperand
    {
    }

    private sealed class ConstantValue : IOperand
    {
        public int Value { get; }

        public ConstantValue(int value)
        {
            Value = value;
        }
    }

    private enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

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

    private sealed class Monkey
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
        var intList = Many(Int, sep: StringP(", "));
        var titleLine = StringP("Monkey").AndR(WS).AndR(Int).AndL(StringP(":"));
        var itemsLine = WS.AndR(StringP("Starting items:")).AndR(WS).AndR(intList);
        var operandParser = Choice(
            StringP("old", (IOperand)new OldValue()),
            Int.Map(r => (IOperand)(new ConstantValue(r)))
        );
        var operationParser = Choice(
            StringP("+", Operation.Add),
            StringP("-", Operation.Subtract),
            StringP("*", Operation.Multiply),
            StringP("/", Operation.Divide)
        );
        var operationLine = Pipe(
            WS.AndR(StringP("Operation: new =")).AndR(WS),
            operandParser,
            WS.AndR(operationParser),
            WS.AndR(operandParser),
            (_, op1, operation, op2) => (op1, operation, op2)
        );
        var testLine = WS.AndR(StringP("Test: divisible by")).AndR(WS).AndR(Int.Map(i => (BigInteger)i));
        var monkeyTrueLine = WS.AndR(StringP("If true: throw to monkey")).AndR(WS).AndR(Int);
        var monkeyFalseLine = WS.AndR(StringP("If false: throw to monkey")).AndR(WS).AndR(Int);
        var monkeyLines = Pipe(
            monkeyTrueLine.AndL(Newline),
            monkeyFalseLine,
            (monkeyTrue, monkeyFalse) => (monkeyTrue, monkeyFalse)
        );
        var monkeyParser = Pipe(
            titleLine.AndL(Newline),
            itemsLine.AndL(Newline),
            operationLine.AndL(Newline),
            testLine.AndL(Newline),
            monkeyLines,
            (id, items, operation, divisor, monkeys) =>
                (id, items, operation, divisor, monkeys)
        );

        var monkeyListParser = Many1(monkeyParser, sep: Many1(Newline), canEndWithSep: true);
        var result = monkeyListParser.Run(input).GetResult();

        return result.Select(item =>
        {
            var operation = GetOperation(item.operation.operation);
            var operand1 = GetOperand(item.operation.op1);
            var operand2 = GetOperand(item.operation.op2);

            var monkeyDescription = new MonkeyDescription(
                value => operation(operand1(value), operand2(value)),
                x => x % item.divisor == 0,
                item.monkeys.monkeyTrue,
                item.monkeys.monkeyFalse
            );

            divisors.Add(item.divisor);

            return new Monkey(item.id, monkeyDescription, item.items.Select(i => (BigInteger) i).ToArray());
        }).ToArray();
    }

    private static Func<BigInteger, BigInteger> GetOperand(IOperand operand)
    {
        return operand switch
        {
            OldValue _ => x => x,
            ConstantValue constantValue => _ => constantValue.Value
        };
    }
    
    private static Func<BigInteger, BigInteger, BigInteger> GetOperation(Operation operation)
    {
        return operation switch
        {
            Operation.Add => (a, b) => a + b,
            Operation.Subtract=> (a, b) => a - b,
            Operation.Multiply => (a, b) => a * b,
            Operation.Divide => (a, b) => a / b,
        };
    }
}
