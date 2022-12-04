using System.Diagnostics;
using AdvendOfCode2022;
using AdvendOfCode2022.Helpers;
using Autofac;

var container = ContainerHelper.CreateContainer();

using var scope = container.BeginLifetimeScope();
var day = Environment.GetEnvironmentVariable("AOC_DAY");
var solver = scope.ResolveNamed<IDay>(day);
var source = scope.ResolveNamed<IInput>(day);

Console.WriteLine("Using test data");
solver.CalculateTaskOne(source.TestData);
solver.CalculateTaskTwo(source.TestData);

Console.WriteLine("Using real data");
var sw = Stopwatch.StartNew();

solver.CalculateTaskOne(source.Data);
solver.CalculateTaskTwo(source.Data);

sw.Stop();
Console.WriteLine($"Day {day} completed in {sw.Elapsed}");

Console.ReadLine();