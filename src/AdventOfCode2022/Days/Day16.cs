using System.Collections.Concurrent;
using System.Collections.Immutable;
using AdventOfCode2022.Helpers;

namespace AdventOfCode2022.Days;

[Day(16)]
public class Day16 : IDay
{
    private record Valve(string Id, long FlowRate, string[] Valves);

    private record Step(string Position, HashSet<string> Opened, long Pressure);

    private record StepWithElefant(string Position, string ElefantPosition, ImmutableHashSet<string> Opened,
        long TotalPressure)
    {
        public string SortValue { get; } =
            $"{string.Join(";", Opened.Order())}_{(StringComparer.Ordinal.Compare(Position, ElefantPosition) > 0 ? Position + ElefantPosition : ElefantPosition + Position)}_{TotalPressure}";
    }

    public void CalculateTaskOne(string source)
    {
        var valves = ParseInput(source).ToDictionary(v => v.Id);

        var first = new[] {new Step("AA", new HashSet<string>(), 0)};
        var nonZero = valves.Where(v => v.Value.FlowRate > 0).Select(v => v.Key).Count();
        var allFlowRate = valves.Values.Sum(v => v.FlowRate);
        var current = first.ToList();

        var progress = ProgressHelper.Create(30);
        foreach (var i in Enumerable.Range(0, 30))
        {
            var result = new ConcurrentBag<Step>();

            Parallel.ForEach(current,
                new ParallelOptions { MaxDegreeOfParallelism = 16},
                (item, state) =>
                {
                    if (item.Opened.Count == nonZero)
                    {
                        result.Add(item with {Pressure = item.Pressure + allFlowRate});
                        return;
                    }

                    var pipe = valves[item.Position];
                    var newPressure = item.Pressure + item.Opened.Select(s => valves[s].FlowRate).Sum();
                    if (!item.Opened.Contains(item.Position) && pipe.FlowRate > 0)
                    {
                        // We can open
                        var newOpened = new HashSet<string>(item.Opened) {item.Position};
                        result.Add(item with {Opened = newOpened, Pressure = newPressure});
                    }

                    foreach (var connected in pipe.Valves)
                    {
                        result.Add(new (connected, item.Opened, newPressure));
                    }
                });

            var count = result.Count;
            var optimized = result
                .OrderByDescending(r => r.Pressure)
                .Take(Math.Max(count / 4, 1000))
                .AsParallel()
                .WithDegreeOfParallelism(16)
                .DistinctBy(s => (string.Join(";", s.Opened.Order()), s.Position, s.Pressure))
                .ToList();
            current = optimized;
            progress.AddProgress();
        }

        var maxPressure = current.Max(c => c.Pressure);
        
        Console.WriteLine(maxPressure);
    }

    public void CalculateTaskTwo(string source)
    {
        var valves = ParseInput(source).ToDictionary(v => v.Id);

        var first = new[] {new StepWithElefant("AA", "AA", ImmutableHashSet<string>.Empty, 0)};
        var nonZero = valves.Where(v => v.Value.FlowRate > 0).Select(v => v.Key).Count();
        var allFlowRate = valves.Values.Sum(v => v.FlowRate);
        var current = first.ToList();

        var progress = ProgressHelper.Create(26);
        foreach (var i in Enumerable.Range(0, 26))
        {
            var result = new ConcurrentBag<StepWithElefant>();

            Parallel.ForEach(current,
                new ParallelOptions { MaxDegreeOfParallelism = 32},
                (item, state) =>
                {
                    if (item.Opened.Count == nonZero)
                    {
                        result.Add(item with {TotalPressure = item.TotalPressure + allFlowRate});
                        return;
                    }

                    var manPipe = valves[item.Position];
                    var elefantPipe = valves[item.ElefantPosition];
                    var deltaPressure = item.Opened.Select(s => valves[s].FlowRate).Sum();
                    var newPressure = item.TotalPressure + deltaPressure;
                    var manCanAct = !item.Opened.Contains(item.Position) && manPipe.FlowRate > 0;
                    var elefantCanAct = !item.Opened.Contains(item.ElefantPosition) && elefantPipe.FlowRate > 0;

                    var manActedPossibilities = manCanAct ? new[] {true, false} : new[] {false};

                    foreach (var manActed in manActedPossibilities)
                    {
                        var manPossibleMoves = manActed ? new[] {item.Position} : manPipe.Valves;
                        var elefantActedPossibilites =
                            elefantCanAct && (!manActed || item.Position != item.ElefantPosition)
                                ? new[] {true, false}
                                : new[] {false};

                        foreach (var elefantActed in elefantActedPossibilites)
                        {
                            var elefantPossibleMoves = elefantActed
                                ? new[] {item.ElefantPosition}
                                : elefantPipe.Valves;

                            var newOpened = item.Opened;
                            if (elefantActed || manActed)
                            {
                                if (manActed)
                                {
                                    newOpened = newOpened.Add(item.Position);
                                }

                                if (elefantActed)
                                {
                                    newOpened = newOpened.Add(item.ElefantPosition);
                                }
                            }
                            
                            foreach (var manMove in manPossibleMoves)
                            {
                                foreach (var elefantMove in elefantPossibleMoves)
                                {
                                    result.Add(new (manMove, elefantMove, newOpened, newPressure));
                                }
                            }
                        }
                    }
                });

            var count = result.Count;
            var optimized = result
                .OrderByDescending(r => r.TotalPressure)
                .Take(Math.Max(count / 4, 1000))
                .AsParallel()
                .WithDegreeOfParallelism(16)
                .DistinctBy(s => s.SortValue)
                .ToList();
            
            current = optimized;
            
            progress.AddProgress();
        }

        var maxPressure = current.MaxBy(c => c.TotalPressure);
        
        Console.WriteLine(maxPressure.TotalPressure);
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
