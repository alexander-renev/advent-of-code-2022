using System.Text;

namespace AdventOfCode2022.Inputs;

internal static class InputHelper
{
    internal static string GetResource(int day, string type)
    {
        return GetEmbeddedResourceAsString(typeof(InputHelper), $"day{day}-{type}.txt");
    }

    private static byte[] GetEmbeddedResource(Type type, string name)
    {
        var result = type.Assembly.GetManifestResourceStream(type, name);

        if (result is null)
        {
            string[] streams = type.Assembly.GetManifestResourceNames();

            throw new InvalidOperationException(
                $"Resource {name} not found in {type.Namespace}. Existing resources: {string.Join(", ", streams)}");
        }

        using var buffer = new MemoryStream();
        result.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static string GetEmbeddedResourceAsString(Type type, string name)
    {
        var result = type.Assembly.GetManifestResourceStream(type, name);

        if (result is null)
        {
            string[] streams = type.Assembly.GetManifestResourceNames();

            throw new InvalidOperationException(
                $"Resource {name} not found in {type.Namespace}. Existing resources: {string.Join(", ", streams)}");
        }

        using var reader = new StreamReader(result);
        return reader.ReadToEnd();
    }
}