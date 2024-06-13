using Cocona;
using Microsoft.Build.Construction;

CoconaApp.Run(([Argument] SecretEnvTypes envType) =>
{
    var relatedProjectFile = GetRelatedProjectFile(Environment.CurrentDirectory);
    var projectRootElement = ProjectRootElement.Open(relatedProjectFile);

    var previousUserSecretId = projectRootElement.Properties
        .Where(x => x.Name == "UserSecretsId")
        .Select(x => x.Value)
        .FirstOrDefault() 
            ?? "<NO USER SECRETS SET>";

    projectRootElement.AddProperty("UserSecretsId", envType.ToString());
    projectRootElement.Save();

    Console.WriteLine($"Setting UserSecrets from {previousUserSecretId} to {envType}");
});

string GetRelatedProjectFile(string startingPath)
{
    var listOfDirectories = GetFlattenedListOfDirectories(startingPath);

    return listOfDirectories.SelectMany(path => Directory.GetFiles(path, "*.csproj"))
        .FirstOrDefault() 
            ?? throw new Exception("Unable to find Project file.");
}

List<string> GetFlattenedListOfDirectories(string path)
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

public enum SecretEnvTypes
{
    Dev,
    Qa,
    Prod
}