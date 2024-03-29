﻿using System.Globalization;
using System.Reflection;
using Autofac;

namespace AdventOfCode2022.Helpers;

internal static class ContainerHelper
{
    internal static IContainer CreateContainer()
    {
        var builder = new ContainerBuilder();
        foreach (var type in typeof(IDay).Assembly.GetTypes().Where(t => typeof(IDay).IsAssignableFrom(t) && !t.IsAbstract))
        {
            var dayAttribute = type.GetCustomAttribute<DayAttribute>();
            if (dayAttribute is null)
            {
                continue;
            }

            builder.RegisterType(type).Named<IDay>(dayAttribute.Number.ToString(CultureInfo.InvariantCulture));
        }
        
        var container = builder.Build();

        return container;
    }
}