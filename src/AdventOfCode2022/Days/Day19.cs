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
    
    private static ImmutableList<ResourceType> ResourceTypes = ImmutableList<ResourceType>.Empty.AddRange(new[]
    {
        ResourceType.Geode, ResourceType.Clay, ResourceType.Obsidian, ResourceType.Ore
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
        int Elapsed,
        ImmutableList<ResourceType> CanBuildLastStep);

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
        var queue = new Queue<Step>();
        queue.Enqueue(new Step(InitialResource, InitialRobots, 0, ImmutableList<ResourceType>.Empty));
        var maxCracked = 0;

        var maxPrice = new[] {ResourceType.Ore, ResourceType.Clay, ResourceType.Obsidian}
            .ToDictionary(res => res,
                res => bp.Robots.Select(r => r.Price.Where(p => p.Resource == res).Select(p => p.Value).FirstOrDefault()).Max());
        maxPrice[ResourceType.Geode] = int.MaxValue;

        while (true)
        {
            if (!queue.TryDequeue(out var element))
            {
                break;
            }

            if (element.Elapsed == time)
            {
                maxCracked = Math.Max(maxCracked, element.Resources[ResourceType.Geode]);
                continue;
            }

            // Calculate resources
            var newResources = element.Resources;
            foreach (var type in ResourceTypes)
            {
                if (element.Robots[type] > 0)
                {
                    newResources = newResources.SetItem(type, newResources[type] + element.Robots[type]);
                }
            }

            var timeRemaining = time - element.Elapsed;
            var canBuildThisStep = element.CanBuildLastStep; 
            
            foreach (var resource in ResourceTypes.Where(r => !element.CanBuildLastStep.Contains(r)))
            {
                // can we build this robot
                var price = bp.RobotsByType[resource];
                if (!price.Price.All(p => p.Value <= element.Resources[p.Resource]))
                {
                    continue;
                }

                if (resource != ResourceType.Geode)
                {
                    // can we use all resources of specified type
                    var currentResourceValue = newResources[resource];
                    var currentRobotsCount = element.Robots[resource];
                    if (currentResourceValue + currentRobotsCount * timeRemaining >= maxPrice[resource] * timeRemaining)
                    {
                        continue;
                    }
                }

                var resourcesAfterBuild = newResources;
                foreach (var item in price.Price)
                {
                    resourcesAfterBuild = resourcesAfterBuild.SetItem(item.Resource, resourcesAfterBuild[item.Resource] - item.Value);
                }

                var robotsAfterBuild = element.Robots.SetItem(resource, element.Robots[resource] + 1);
                
                queue.Enqueue(new Step(resourcesAfterBuild, robotsAfterBuild, element.Elapsed + 1, ImmutableList<ResourceType>.Empty));
                canBuildThisStep = canBuildThisStep.Add(resource);

                if (resource == ResourceType.Geode)
                {
                    break;
                }
            }

            queue.Enqueue(new Step(newResources, element.Robots, element.Elapsed + 1, canBuildThisStep));
        }
        
        Console.WriteLine($"BP {bp.Number} max {maxCracked}");
        
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
