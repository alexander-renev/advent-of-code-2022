namespace AdventOfCode2022.Days;

[Day(7)]
public class Day7 : IDay
{
    private interface ISourceItem
    {
    }

    private record FileItem(string Name, long Size) : ISourceItem;

    private record DirectoryItem(string Name) : ISourceItem;

    private record CommandItem(string Command) : ISourceItem;

    private record DirectoryInfo(string Name, DirectoryInfo Parent, List<DirectoryInfo> Directories,
        List<FileInfo> Files) : DirectoryItem(Name)
    {
        public DirectoryInfo(string name, DirectoryInfo parent = null) : this(name, parent, new(), new())
        {
            totalSizeValue = new Lazy<long>(CalculateSize, LazyThreadSafetyMode.None);
        }

        private readonly Lazy<long> totalSizeValue;

        public long TotalSize => totalSizeValue.Value;

        private long CalculateSize()
        {
            return Files.Sum(f => f.Size) + Directories.Sum(d => d.TotalSize);
        }
    }

    private record FileInfo(string Name, long Size, DirectoryInfo Parent) : FileItem(Name, Size);

    public void CalculateTaskOne(string source)
    {
        var root = PrepareRoot(source);

        var sum = IterateDirectories(root).Where(d => d.TotalSize <= 100000).Sum(d => d.TotalSize);
        Console.WriteLine(sum);
    }

    public void CalculateTaskTwo(string source)
    {
        var root = PrepareRoot(source);

        var sizeToDelete = root.TotalSize - 40000000;

        var dirToDelete = IterateDirectories(root)
            .Where(d => d.TotalSize >= sizeToDelete)
            .OrderBy(d => d.TotalSize)
            .First();

        Console.WriteLine(dirToDelete.TotalSize);
    }

    private static DirectoryInfo PrepareRoot(String source)
    {
        var inputData = ParseInput(source).Skip(1);

        var root = new DirectoryInfo("/");
        var current = root;

        foreach (var item in inputData)
        {
            switch (item)
            {
                case CommandItem cmd:
                    if (cmd.Command.StartsWith("cd "))
                    {
                        var newDir = cmd.Command.Substring(3);
                        if (newDir == "..")
                        {
                            current = current.Parent;
                        }
                        else
                        {
                            var found = current.Directories.FirstOrDefault(d => d.Name == newDir);
                            if (found != null)
                            {
                                current = found;
                            }
                            else
                            {
                                found = new DirectoryInfo(newDir, current);
                                current.Directories.Add(found);
                                current = found;
                            }
                        }
                    }
                    else if (cmd.Command != "ls")
                    {
                        throw new InvalidOperationException($"Unknown command {cmd.Command}");
                    }

                    break;
                case DirectoryItem dirInfo:
                    var existingDir = current.Directories.FirstOrDefault(dir => dir.Name == dirInfo.Name);
                    if (existingDir is null)
                    {
                        current.Directories.Add(new(dirInfo.Name, current));
                    }

                    break;
                case FileItem fileInfo:
                    var existingFile = current.Files.FirstOrDefault(f => f.Name == fileInfo.Name);
                    if (existingFile is null)
                    {
                        current.Files.Add(new(fileInfo.Name, fileInfo.Size, current));
                    }

                    break;
                default:
                    throw new InvalidOperationException("Unsupported command");
            }
        }

        return root;
    }

    private static ISourceItem[] ParseInput(string input)
    {
        return input.Split(Environment.NewLine)
            .Select<String, ISourceItem>(line =>
            {
                if (line.StartsWith('$'))
                {
                    return new CommandItem(line.Substring(2));
                }
                else if (line.StartsWith("dir "))
                {
                    return new DirectoryItem(line.Substring(4));
                }

                var parts = line.Split(' ');
                return new FileItem(parts[1], long.Parse(parts[0]));
            }).ToArray();
    }

    private static IEnumerable<DirectoryInfo> IterateDirectories(DirectoryInfo root)
    {
        yield return root;

        foreach (var d in root.Directories.SelectMany(IterateDirectories))
        {
            yield return d;
        }
    }
}