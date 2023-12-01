using System.Collections.Immutable;

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
    
    private static  readonly ImmutableList<ResourceType> ResourceTypes = ImmutableList<ResourceType>.Empty.AddRange(new[]
    {
        ResourceType.Geode, ResourceType.Obsidian, ResourceType.Clay, ResourceType.Ore
    });

    private record struct PriceValue(ResourceType Resource, int Value);

    private record struct Robot(ResourceType Type, PriceValue[] Price);

    private readonly record struct Blueprint(int Number, Robot[] Robots)
    {
        public Dictionary<ResourceType, Robot> RobotsByType { get; } = Robots.ToDictionary(r => r.Type);
    }

    private readonly record struct Step(
        ImmutableDictionary<ResourceType, int> Resources,
        ImmutableDictionary<ResourceType, int> Robots,
        int Elapsed);

    private static readonly ImmutableDictionary<ResourceType, int> InitialResource =
        ImmutableDictionary<ResourceType, int>.Empty.AddRange(ResourceTypes.ToDictionary(t => t, _ => 0));
    
    private static readonly ImmutableDictionary<ResourceType, int> InitialRobots =
        ImmutableDictionary<ResourceType, int>.Empty
            .AddRange(ResourceTypes.ToDictionary(t => t, _ => 0))
            .SetItem(ResourceType.Ore, 1);
    

    public void CalculateTaskOne(string source)
    {
        var blueprints = ParseInput(source);
        var total = blueprints.Select(bp => GetMaxGeodes(bp, 24) * bp.Number).Sum();
        
        Console.WriteLine($"Total = {total}");
    }

    public void CalculateTaskTwo(string source)
    {
        var blueprints = ParseInput(source);
        var product = blueprints.Take(3).Select(bp => GetMaxGeodes(bp, 32)).Aggregate(1L, (a, b) => a * b);
        Console.WriteLine($"Total = {product}");
    }

    private static int GetMaxGeodes(Blueprint bp, int time)
    {
        var maxCracked = 0;
        var maxPrice = new[] {ResourceType.Ore, ResourceType.Clay, ResourceType.Obsidian}
            .ToDictionary(res => res,
                res => bp.Robots.Select(r => r.Price.Where(p => p.Resource == res).Select(p => p.Value).FirstOrDefault()).Max());
        maxPrice[ResourceType.Geode] = int.MaxValue;

        var s = new Step(InitialResource, InitialRobots, 0);

        void GetMaxCracked(Step step)
        {
            maxCracked  = Math.Max(maxCracked, step.Resources[ResourceType.Geode] +
                                        step.Robots[ResourceType.Geode] * (time - step.Elapsed));

            // Is we build Geode every next step how much can we produce
            var limitCracked = step.Resources[ResourceType.Geode] +
                               step.Robots[ResourceType.Geode] * (time - step.Elapsed) +
                               Enumerable.Range(0, time - step.Elapsed).Sum();
            if (limitCracked <= maxCracked)
            {
                return;
            }

            if (step.Elapsed == time - 1)
            {
                return;
            }


            foreach (var resourceType in ResourceTypes)
            {
                if (resourceType != ResourceType.Geode)
                {
                    // can we use all resources of specified type
                    var currentResourceValue = step.Resources[resourceType];
                    var currentRobotsCount = step.Robots[resourceType];
                    if (currentResourceValue + currentRobotsCount * (time - step.Elapsed) >= maxPrice[resourceType] * (time - step.Elapsed))
                    {
                        continue;
                    }
                }
                
                
                // Can we build this robot now or in future
                var price = bp.RobotsByType[resourceType];
                if (!price.Price.All(p => step.Robots[p.Resource] > 0))
                {
                    continue;
                }

                // Calculate required time 
                var maxTime = price.Price
                    .Select(p =>
                    {
                        if (step.Resources[p.Resource] >= p.Value)
                        {
                            return 0;
                        }

                        decimal difference = p.Value - step.Resources[p.Resource];
                        return (int) Math.Ceiling(difference / step.Robots[p.Resource]);
                    })
                    .Max() + 1;

                if (maxTime + step.Elapsed > time - 1)
                {
                    continue;
                }

                var newResources = step.Resources;
                foreach (var r in ResourceTypes)
                {
                    if (step.Robots[r] == 0)
                    {
                        continue;
                    }

                    newResources = newResources.SetItem(r, newResources[r] + step.Robots[r] * maxTime);
                }

                foreach (var priceItem in price.Price)
                {
                    newResources = newResources.SetItem(priceItem.Resource,
                        newResources[priceItem.Resource] - priceItem.Value);
                }

                var newRobots = step.Robots.SetItem(resourceType, step.Robots[resourceType] + 1);
                var newElapsed = step.Elapsed + maxTime;

                maxCracked = Math.Max(maxCracked, newResources[ResourceType.Geode]);
                
                GetMaxCracked(new Step(newResources, newRobots, newElapsed));
            }
        }

        GetMaxCracked(s);
        
        return maxCracked;
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
            (_, type, _, p) => new Robot(type, p.ToArray())
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
