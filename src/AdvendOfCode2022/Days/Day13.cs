using System.Globalization;
using Microsoft.FSharp.Core;
using static FParsec.Primitives;

namespace AdvendOfCode2022.Days;

[Day(13)]
public class Day13 : IDay
{
    private interface IElement
    {
    }

    private sealed record NumberElement(int Value) : IElement
    {
        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    private sealed record ListElement(IElement[] Elements) : IElement
    {
        public override string ToString()
        {
            return "[" + string.Join(",", Elements.Select(s => s.ToString())) + "]";
        }
    }
    
    public void CalculateTaskOne(string source)
    {
        var data = ParseInput(source);
        var rightPairs = new List<int>();
        foreach (var item in data.Select((it, index) => new { it, index }))
        {
            var index = item.index + 1;
            var (item1, item2) = item.it;
            if (IsLowerThan(item1, item2) ?? true)
            {
                rightPairs.Add(index);
            }
        }
        
        Console.WriteLine(rightPairs.Sum());
    }

    public void CalculateTaskTwo(string source)
    {
        var data = ParseInput(source).SelectMany(s => new[] {s.el1, s.el2}).ToList();
        var div1 = new ListElement(new[] {new ListElement(new[] {new NumberElement(2)})});
        var div2 = new ListElement(new[] {new ListElement(new[] {new NumberElement(6)})});
        data.Add(div1);
        data.Add(div2);

        data.Sort((el1, el2) => IsLowerThan(el1, el2) switch
        {
            null => 0,
            true => -1,
            false => 1
        });

        var index1 = data.IndexOf(div1) + 1;
        var index2 = data.IndexOf(div2) + 1;
        
        Console.WriteLine(index1 * index2);
    }

    private static bool? IsLowerThan(IElement first, IElement second)
    {
        return (first, second) switch
        {
            (NumberElement num1, NumberElement num2)
                when num1.Value < num2.Value => true,
            (NumberElement num1, NumberElement num2)
                when num1.Value == num2.Value => null,
            (NumberElement num1, NumberElement num2)
                when num1.Value > num2.Value => false,
            
            (NumberElement num1, ListElement lst1)
                => IsLowerThan(new ListElement(new[] {num1}), lst1),
            (ListElement lst1, NumberElement num1)
                => IsLowerThan(lst1, new ListElement(new[] {num1})),
            
            (ListElement { Elements: []}, ListElement{ Elements: []}) => null,
            (ListElement { Elements: [..]}, ListElement{ Elements: []}) => false,
            (ListElement { Elements: []}, ListElement{ Elements: [..]}) => true,
            
            (
                ListElement { Elements: [var el1, .. var rest1 ]},
                ListElement { Elements: [var el2, .. var rest2] }
            ) => IsLowerThan(el1, el2) ?? IsLowerThan(new ListElement(rest1), new ListElement(rest2)) 
        };
    }

    private static (IElement el1, IElement el2)[] ParseInput(string input)
    {
        var (element, elementRef) = createParserForwardedToRef<IElement, Unit>();
        var listParser = StringP("[").AndR(Many(element, sep: StringP(","))).AndL(StringP("]"));
        elementRef.Value = Choice(
            Int.Map(i => new NumberElement(i) as IElement),
            listParser.Map(lst => new ListElement(lst.ToArray()) as IElement)
        );
        var pairParser = Pipe(
            element.AndL(Newline.AndTry(NotFollowedByNewline)),
            element,
            (el1, el2) => (el1, el2)
        );

        var inputParser = Many1(pairParser, Many1(Newline), canEndWithSep: true);

        var result = inputParser.Run(input).GetResult().ToArray();
        return result;
    }
}
