using System.Collections.Immutable;
using System.Text.Json;
using LanguageExt;

namespace AdventOfCode2022.Days;

[Day(16)]
public class Day16 : IDay
{
    private record Valve(string Id, long FlowRate, string[] Valves);

    private record Step(string Position, ImmutableHashSet<string> Opened, long Pressure, int Elapsed);
    
    private record StepWithElephant(string Position, string ElephantPosition, ImmutableDictionary<string, int> Opened, int Elapsed, int ElephantElapsed);

    private record Pair(string V1, string V2);

    public void CalculateTaskOne(string source)
    {
        var valvesList = ParseInput(source);
        var valvesMap = valvesList.ToDictionary(v => v.Id);
        var nonZeroValves = valvesList.Where(v => v.FlowRate > 0).Select(v => v.Id).ToHashSet();
        var distances = CalculateDistances(valvesList);

        var step = new Step("AA", ImmutableHashSet<string>.Empty, 0, 0);

        var maxPressure = 0L;
        var time = 30;

        void Process(Step step)
        {
            var currentSpeed = step.Opened.Select(op => valvesMap[op].FlowRate).Sum();
            
            // Do nothing
            var newPressure = step.Pressure + currentSpeed * (time - step.Elapsed);
            maxPressure = Math.Max(maxPressure, newPressure);
            
            foreach (var newValve in nonZeroValves.Where(v => !step.Opened.Contains(v)))
            {
                var distance = distances[(step.Position, newValve)] + 1;
                if (step.Elapsed + distance >= time)
                {
                    continue;
                }

                newPressure = step.Pressure + currentSpeed * distance;
                var newElapsed = step.Elapsed + distance;
                var newOpened = step.Opened.Add(newValve);
                
                Process(new Step(newValve, newOpened, newPressure, newElapsed));
            }
        }
        
        Process(step);
        
        Console.WriteLine(maxPressure);
    }

    public void CalculateTaskTwo(string source)
    {
        var valvesList = ParseInput(source);
        var valvesMap = valvesList.ToDictionary(v => v.Id);
        var nonZeroValves = valvesList.Where(v => v.FlowRate > 0).Select(v => v.Id).ToHashSet();
        var distances = CalculateDistances(valvesList);

        var step = new Step("AA", ImmutableHashSet<string>.Empty, 0, 0);

        var maxPressure = 0L;
        var time = 26;
        var solutions = new List<(long pressure, ImmutableHashSet<string> opened)>();

        void Process(Step step)
        {
            var currentSpeed = step.Opened.Select(op => valvesMap[op].FlowRate).Sum();
            
            // Do nothing
            var newPressure = step.Pressure + currentSpeed * (time - step.Elapsed);
            maxPressure = Math.Max(maxPressure, newPressure);
            
            foreach (var newValve in nonZeroValves.Where(v => !step.Opened.Contains(v)))
            {
                var distance = distances[(step.Position, newValve)] + 1;
                if (step.Elapsed + distance >= time)
                {
                    continue;
                }

                var newStepPressure = step.Pressure + currentSpeed * distance;
                var newElapsed = step.Elapsed + distance;
                var newOpened = step.Opened.Add(newValve);
                
                Process(new Step(newValve, newOpened, newStepPressure, newElapsed));
            }

            solutions.Add((newPressure, step.Opened));
        }
        
        Process(step);

        long max = 0;
        foreach (var s1 in solutions)
        {
            foreach (var s2 in solutions)
            {
                if (s1.pressure + s2.pressure > max)
                {
                    if (s1.opened.All(v => !s2.opened.Contains(v)))
                    {
                        max = s1.pressure + s2.pressure;
                    }
                    
                }
            }
        }
        
        Console.WriteLine(max);
    }

    private static Dictionary<(string From, string To), int> CalculateDistances(Valve[] valves)
    {
        var valvesToCalculate = valves
            .Select(v => v.Id).ToImmutableHashSet();
        var valvesMap = valves
            .ToDictionary(v => v.Id);

        var enumerated = valvesToCalculate.Order().ToArray();
        var enumeratedMap = enumerated
            .Select((id, index) => new {id, index})
            .ToImmutableDictionary(v => v.id, v => v.index);

        var existingPaths = valves.SelectMany(v1 =>
            v1.Valves.Select(v2 => new Pair(v1.Id, v2)))
            .GroupBy(p => p.V1)
            .ToDictionary(p => p.Key, p => p.Select(p1 => p1.V2).ToArray());

        var result = new Option<int>[enumerated.Length, enumerated.Length];
        for (var i = 0; i < enumerated.Length; i++)
        {
            for (var j = 0; j < enumerated.Length; j++)
            {
                result[i,j] = Option<int>.None;
            }
        }

        foreach (var valve in valvesToCalculate)
        {
            foreach (var nested in valvesMap[valve].Valves)
            {
                var i = enumeratedMap[valve];
                var j = enumeratedMap[nested];

                result[i, j] = 1;
                result[j, i] = 1;
            }
        }

        foreach (var pathLength in Enumerable.Range(0, enumerated.Length - 1))
        {
            for (var i = 0; i < enumerated.Length; i++)
            {
                for (var j = 0; j < enumerated.Length; j++)
                {
                    if (j >= i)
                    {
                        continue;
                    }

                    var path =
                        from p1 in result[i, pathLength]
                        from p2 in result[j, pathLength]
                        select p1 + p2;

                    var newPath = path.Match(
                        v => result[i,j].Match(
                            r => Math.Min(r, v),
                            () => v
                            ),
                        () => result[i, j]
                    );

                    result[i, j] = newPath;
                    result[j, i] = newPath;
                }
            }
        }

        var calculated = from i in Enumerable.Range(0, enumerated.Length)
            from j in Enumerable.Range(0, enumerated.Length)
            where j != i
            let fr = enumerated[i]
            let to = enumerated[j]
            let path = result[i, j].Match(x => x,
                () => throw new InvalidOperationException($"Did not calculate length between {fr} and {to}"))
            select new {fr, to, path};

        return calculated.ToDictionary(r => (r.fr, r.to), r => r.path);
    }

    private static Valve[] ParseInput(string input)
    {
        var tunnel = Pipe(
            AsciiUpper,
            AsciiUpper,
            (ch1, ch2) => new string(new[] {ch1, ch2})
        );

        var tunnels = Many1(tunnel, sep: StringP(", "));
        var line = Pipe(
            StringP("Valve").And(WS),
            tunnel.AndL(WS).AndL(StringP("has flow rate=")),
            Int.AndL(Choice(StringP("; tunnels lead to valves"), StringP("; tunnel leads to valve"))).AndL(WS),
            tunnels,
            (_, id, rate, valves) => new Valve(id, rate, valves.ToArray())
        );

        var parser = Many1(line, sep: Newline, canEndWithSep: true);
        var result = parser.Run(input).GetResult().ToArray();
        
        return result;
    }
}
