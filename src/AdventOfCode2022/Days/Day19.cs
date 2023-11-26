namespace AdventOfCode2022.Days;

[Day(19)]
public class Day19 : IDay
{
    private enum ResourceType
    {
        Unknown = 0,
        Ore,
        Clay,
        Obsidian,
        Geode
    }

    private record struct PriceValue(ResourceType Resource, int Value);

    private record struct Robot(ResourceType Type, PriceValue[] Price);

    private readonly record struct Blueprint(int Number, Robot[] Robots)
    {
        public Dictionary<ResourceType, Robot> RobotsByType { get; } = Robots.ToDictionary(r => r.Type);
    }

    private readonly record struct Step(
        Dictionary<ResourceType, int> Resources,
        Dictionary<ResourceType, int> Robots,
        int Elapsed)
    {
        public override string ToString()
        {
            var res = string.Join(",", Resources.OrderBy(r => (int)r.Key).Select(r => r.Value));
            var rob = string.Join(",", Robots.OrderBy(r => (int)r.Key).Select(r => r.Value));
            return $"Res={res};rob={rob};time={Elapsed}";
        }
    }
    

    public void CalculateTaskOne(string source)
    {
        var blueprints = ParseInput(source);
        var total = blueprints.Select(GetQuality).Sum();
        
        Console.WriteLine($"Total = {total}");
    }

    public void CalculateTaskTwo(string source)
    {
    }

    private static int GetQuality(Blueprint bp)
    {
        var resourceTypes = new[] {ResourceType.Geode, ResourceType.Obsidian, ResourceType.Clay, ResourceType.Ore};
        // We cannot build multiple robots simultaneously so max resource production required for all resources except geode
        // is defined by maximum price of robot in this resource
        var maxRobots = new[] {ResourceType.Ore, ResourceType.Clay, ResourceType.Obsidian}
            .ToDictionary(t => t, t => bp.Robots.SelectMany(r => r.Price).Where(p => p.Resource == t).Max(p => p.Value) + 5);
        maxRobots[ResourceType.Geode] = int.MaxValue;

        var robots = MakeEmpty();
        robots[ResourceType.Ore] = 1;
        var resources = MakeEmpty();

        var stateGraph = new LinkedList<Step>();
        stateGraph.AddLast(new Step(resources, robots, 0));
        var visited = new HashSet<string>();
        
        const int maxTime = 24;
        var maxCracked = 0;

        while (stateGraph.Any())
        {
            var state = stateGraph.First.Value;
            stateGraph.RemoveFirst();

            var expectedCracked = state.Resources[ResourceType.Geode] + (maxTime - state.Elapsed) * state.Robots[ResourceType.Geode];
            maxCracked = Math.Max(expectedCracked, maxCracked);

            // Trying to build another robot
            foreach (var resourceType in resourceTypes)
            {
                var count = state.Robots[resourceType];
                if (count == maxRobots[resourceType])
                {
                    continue;
                }

                /*if (state.Resources[resourceType] > 15 && resourceType is not ResourceType.Ore)
                {
                    continue;
                }*/
                
                var price = bp.RobotsByType[resourceType].Price;
                var canBuild = price.All(p => state.Robots[p.Resource] > 0);
                if (!canBuild)
                {
                    continue;
                }

                var requiredTime = price
                    .Select(p => CalculateTime(
                        state.Resources[p.Resource],
                        p.Value,
                        state.Robots[p.Resource]))
                    .Max() + 1;

                if (state.Elapsed + requiredTime > maxTime)
                {
                    continue;
                }
                // Need time to get resources + 1 minute to build robot.
                var newResources = state.Resources
                    .ToDictionary(r => r.Key, r => r.Value + state.Robots[r.Key] * requiredTime);
                foreach (var item in price)
                {
                    newResources[item.Resource] -= item.Value;
                }

                var newRobots = state.Robots.ToDictionary(r => r.Key, r => r.Value);
                newRobots[resourceType] += 1;

                var newStep = new Step(newResources, newRobots, state.Elapsed + requiredTime);
                if (visited.Add(newStep.ToString()))
                {
                    stateGraph.AddLast(newStep);
                }
            }
        }

        Console.WriteLine($"Blueprint {bp.Number} result {maxCracked}");
        return maxCracked * bp.Number;

        static Dictionary<ResourceType, int> MakeEmpty()
        {
            return new Dictionary<ResourceType, int>
            {
                {ResourceType.Clay, 0},
                {ResourceType.Geode, 0},
                {ResourceType.Obsidian, 0},
                {ResourceType.Ore, 0}
            };
        }

        static int CalculateTime(int startResource, int requiredResource, int income)
        {
            var result = 0;
            while (startResource < requiredResource)
            {
                result++;
                startResource += income;
            }

            return result;
        }
    }

    private static Blueprint[] ParseInput(string source)
    {
        var resourceType = Choice(
            StringP("ore").Map(_ => ResourceType.Ore),
            StringP("clay").Map(_ => ResourceType.Clay),
            StringP("obsidian").Map(_ => ResourceType.Obsidian),
            StringP("geode").Map(_ => ResourceType.Geode)
        );

        var priceElement = Pipe(
            Int.AndL(WS),
            resourceType,
            (value, type) => new PriceValue(type, value)
        );

        var prices = Many1(priceElement, sep: StringP(" and "));
        
        var robot = Pipe(
            StringP("Each").AndL(WS),
            resourceType.AndL(WS),
            StringP("robot costs").AndL(WS),
            prices.AndL(StringP(".")),
            (_, type, _, prices) => new Robot(type, prices.ToArray())
        );
        
        var line = Pipe(
            StringP("Blueprint").AndR(WS).AndR(Int).AndL(StringP(":")).AndL(WS),
            Many1(robot, sep: StringP(" ")), 
            (number, robots) => new Blueprint(number, robots.ToArray())
        );

        var parser = Many1(line, sep: Newline, canEndWithSep: true);

        var result = parser.Run(source).GetResult().ToArray();
        return result;
    }
}
