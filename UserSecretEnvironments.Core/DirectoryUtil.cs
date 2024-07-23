namespace UserSecretEnvironments.Core;

public static class DirectoryUtil
{
    public static string GetLocalProjectFilePath()
    {
        string binDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.FullName;
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;

        if (Directory.GetFiles(Environment.CurrentDirectory, "*.csproj").Length != 0)
        {
            return Directory.GetFiles(Environment.CurrentDirectory, "*.csproj").First();
        }

        if (Directory.GetFiles(binDirectory, "*.csproj").Length != 0)
        {
            return Directory.GetFiles(binDirectory, "*.csproj").First();
        }

        return Directory.GetFiles(projectDirectory, "*.csproj").First();
    }

    public static string GetLocalProjectFilePath(string startingPath)
    {
        var listOfDirectories = GetFlattenedListOfDirectories(startingPath);

        return listOfDirectories.SelectMany(path => Directory.GetFiles(path, "*.csproj")).First();
    }

    private static List<string> GetFlattenedListOfDirectories(string path)
    {
        var flattenedListOfdirectories = new List<string>() { path };

        foreach (var nestedDirectory in Directory.EnumerateDirectories(path))
        {
            var flattenedList = GetFlattenedListOfDirectories(nestedDirectory);

            flattenedListOfdirectories.AddRange(flattenedList);
            flattenedListOfdirectories.Add(nestedDirectory);
        }

        return flattenedListOfdirectories;
    }

}
