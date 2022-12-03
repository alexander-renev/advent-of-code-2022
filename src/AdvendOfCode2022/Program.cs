using System.Diagnostics;
using AdvendOfCode2022;
using AdvendOfCode2022.Helpers;
using Autofac;

var container = ContainerHelper.CreateContainer();

using var scope = container.BeginLifetimeScope();
var day = Environment.GetEnvironmentVariable("AOC_DAY");
var solver = scope.ResolveNamed<IDay>(day);
var source = scope.ResolveNamed<IInput>(day);

var sw = Stopwatch.StartNew();

solver.CalculateTaskOne(source);
solver.CalculateTaskTwo(source);

sw.Stop();
Console.WriteLine($"Day {day} completed in {sw.Elapsed}");

Console.ReadLine();