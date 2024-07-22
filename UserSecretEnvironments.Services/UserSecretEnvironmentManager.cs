using Microsoft.Build.Construction;
using System.IO;

namespace UserSecretEnvironments.Services;

public static class UserSecretEnvironmentManager
{
    public static void Initialize()
    {
        var projectFile = GetLocalProjectFile(Environment.CurrentDirectory);

        var projectRootElement = ProjectRootElement.Open(projectFile);

        var userSecretId = projectRootElement.Properties
            .Where(x => x.Name == "UserSecretsId")
            .Select(x => x.Value)
            .FirstOrDefault();

        if (userSecretId == null)
        {
            throw new Exception("");
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretId}/secrets.json";

        if (!File.Exists(userSecretsPath))
        {
            throw new Exception("");
        }

        File.Copy(userSecretsPath, appDataPath + $"/Microsoft/UserSecrets/{userSecretId}_Dev/secrets.json");
        File.Copy(userSecretsPath, appDataPath + $"/Microsoft/UserSecrets/{userSecretId}_Qa/secrets.json");
        File.Copy(userSecretsPath, appDataPath + $"/Microsoft/UserSecrets/{userSecretId}_Prod/secrets.json");
    }

    private static string GetLocalProjectFile(string startingPath)
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
