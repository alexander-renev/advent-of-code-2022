using System.Collections.Immutable;
using LanguageExt.ClassInstances.Const;

namespace AdventOfCode2022.Days;

[Day(21)]
public class Day21 : IDay
{
    private record Monkey(string Id);

    private record ConstantMonkey(string Id, int Value) : Monkey(Id);

    private record ExpressionMonkey(string Id, string Monkey1, string Monkey2, string OperatorValue, Func<long, long, long> Operator) : Monkey(Id)
    {
        public static ExpressionMonkey Create(string id, string monkey1, string monkey2, string operation)
        {
            Func<long, long, long> op = operation switch
            {
                "+" => (a, b) => a + b,
                "-" => (a, b) => a - b,
                "/" => (a, b) => a / b,
                "*" => (a, b) => a * b,
                _ => throw new InvalidOperationException($"Unknown operation {operation}")
            };

            return new ExpressionMonkey(id, monkey1, monkey2,  operation, op);
        }
    }
    
    public void CalculateTaskOne(string source)
    {
        var monkeys = ParseInput(source).ToList();
        var values = new Dictionary<string, long>();
        foreach (var monkey in monkeys.OfType<ConstantMonkey>())
        {
            values[monkey.Id] = monkey.Value;
        }

        var monkeyMap = monkeys.OfType<ExpressionMonkey>().ToDictionary(m => m.Id);

        while (!values.ContainsKey("root"))
        {
            var toRemove = new List<string>();
            foreach (var m in monkeyMap.Where(m =>
                         values.ContainsKey(m.Value.Monkey1) && values.ContainsKey(m.Value.Monkey2)))
            {
                var value = m.Value.Operator(values[m.Value.Monkey1], values[m.Value.Monkey2]);
                values[m.Key] = value;
                toRemove.Add(m.Key);
            }
            
            toRemove.ForEach(k => monkeyMap.Remove(k));
        }
        
        Console.WriteLine(values["root"]);
    }

    public void CalculateTaskTwo(string source)
    {
        var monkeys = ParseInput(source).ToList();
        var rootMonkey = (ExpressionMonkey)monkeys.First(m => m.Id == "root");
        var monkey1 = rootMonkey.Monkey1;
        var monkey2 = rootMonkey.Monkey2;


        var values = ImmutableDictionary<string, long>.Empty;
        foreach (var monkey in monkeys.OfType<ConstantMonkey>().Where(m => m.Id != "humn"))
        {
            values = values.Add(monkey.Id, monkey.Value);
        }

        var monkeyMap = ImmutableDictionary<string, ExpressionMonkey>.Empty;
        foreach (var monkey in monkeys.OfType<ExpressionMonkey>().Where(m => m.Id != "root"))
        {
            monkeyMap = monkeyMap.Add(monkey.Id, monkey);
        }
        
        while (true)
        {
            var toRemove = new List<string>();
            foreach (var m in monkeyMap.Where(m =>
                         values.ContainsKey(m.Value.Monkey1) && values.ContainsKey(m.Value.Monkey2)))
            {
                var value = m.Value.Operator(values[m.Value.Monkey1], values[m.Value.Monkey2]);
                values = values.Add(m.Key, value);
                toRemove.Add(m.Key);
            }
            
            toRemove.ForEach(k => monkeyMap = monkeyMap.Remove(k));
            if (!toRemove.Any())
            {
                break;
            }
        }

        if (!values.ContainsKey(monkey1) && !values.ContainsKey(monkey2))
        {
            // Apparently sample data is generated so one of the two monkeys will be precalculated
            throw new InvalidOperationException("Monkey 1 or monkey 2 must be precalculated for this solution to work");
        }

        // Swap so monkey1 is always calculated
        if (!values.ContainsKey(monkey1))
        {
            (monkey1, monkey2) = (monkey2, monkey1);
        }

        // We need monkeys to be equal
        values = values.Add(monkey2, values[monkey1]);
        
        // Now move backwards
        var currentMonkey = monkey2;
        while (true)
        {
            if (currentMonkey.Equals("humn"))
            {
                break;
            }
            var monkeyObject = monkeyMap[currentMonkey];
            var currentValue = values[currentMonkey];
            if (!values.ContainsKey(monkeyObject.Monkey1) && !values.ContainsKey(monkeyObject.Monkey2))
            {
                break;
            }

            if (values.ContainsKey(monkeyObject.Monkey1) && values.ContainsKey(monkeyObject.Monkey2))
            {
                throw new InvalidOperationException("Something went wrong");
            }

            // Reverse operation
            if (values.ContainsKey(monkeyObject.Monkey1))
            {
                var monkey1Value = values[monkeyObject.Monkey1];
                var monkey2Value = monkeyObject.OperatorValue switch
                {
                    "+" => currentValue - monkey1Value,
                    "-" => monkey1Value - currentValue,
                    "*" => currentValue / monkey1Value,
                    "/" => monkey1Value / currentValue
                };
                values = values.Add(monkeyObject.Monkey2, monkey2Value);
                currentMonkey = monkeyObject.Monkey2;
            }
            else
            {
                var monkey2Value = values[monkeyObject.Monkey2];
                var monkey1Value = monkeyObject.OperatorValue switch
                {
                    "+" => currentValue - monkey2Value,
                    "-" => monkey2Value + currentValue,
                    "*" => currentValue / monkey2Value,
                    "/" => monkey2Value * currentValue
                };
                values = values.Add(monkeyObject.Monkey1, monkey1Value);
                currentMonkey = monkeyObject.Monkey1;
            }
        }
        
        // Test data is made so that now humn value is precalculated.
        if (values.TryGetValue("humn", out var humnValue))
        {
            Console.WriteLine(humnValue);
            return;
        }

        throw new InvalidOperationException("Should never get here");
    }

    private static Monkey[] ParseInput(string input)
    {
        var monkeyIdParser = Pipe(
            AsciiLower,
            AsciiLower,
            AsciiLower,
            AsciiLower,
            (ch1, ch2, ch3, ch4) => new string(new[] {ch1, ch2, ch3, ch4})
        );

        var monkeyParser = Pipe(
            monkeyIdParser.AndL(StringP(": ")),
            Choice(
                Int.Map(i => (Monkey) new ConstantMonkey(string.Empty, i)),
                Pipe(
                    monkeyIdParser.AndL(WS),
                    Choice(StringP("+", "+"), StringP("-", "-"), StringP("*", "*"), StringP("/", "/")).AndL(WS),
                    monkeyIdParser,
                    (m1, op, m2) => (Monkey) ExpressionMonkey.Create(string.Empty, m1, m2, op)
                )),
            (id, monkey) => monkey with {Id = id});

        var monkeysParser = Many1(monkeyParser, sep: Newline, canEndWithSep: true);
        return monkeysParser.Run(input).GetResult().ToArray();
    }
}
