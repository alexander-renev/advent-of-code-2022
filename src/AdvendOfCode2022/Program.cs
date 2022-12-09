using System.Diagnostics;
using AdvendOfCode2022;
using AdvendOfCode2022.Helpers;
using AdvendOfCode2022.Inputs;
using Autofac;

var container = ContainerHelper.CreateContainer();

using var scope = container.BeginLifetimeScope();
var day = Environment.GetEnvironmentVariable("AOC_DAY");
var solver = scope.ResolveNamed<IDay>(day);
var dayNumber = int.Parse(day);

Console.WriteLine("Using test data");
var testData = InputHelper.GetResource(dayNumber, "test");
solver.CalculateTaskOne(testData);
solver.CalculateTaskTwo(testData);

Console.WriteLine("Using real data");
var realData = InputHelper.GetResource(dayNumber, "real");
var sw = Stopwatch.StartNew();

solver.CalculateTaskOne(realData);
solver.CalculateTaskTwo(realData);

sw.Stop();
Console.WriteLine($"Day {day} completed in {sw.Elapsed}");

Console.ReadLine();