namespace AdvendOfCode2022.Days;

public abstract class DayBase : IDay
{
    public void CalculateTaskOne(IInput source)
    {
        CalculateTaskOne(GetInput(source));
    }

    public void CalculateTaskTwo(IInput source)
    {
        CalculateTaskTwo(GetInput(source));
    }

    protected abstract void CalculateTaskOne(string input);
    
    protected abstract void CalculateTaskTwo(string input);

    private string GetInput(IInput source)
    {
        if (Environment.GetEnvironmentVariable("AOC_TEST").Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return source.TestData;
        }

        return source.Data;
    }
}