using Microsoft.Build.Construction;
using System.Diagnostics;

namespace UserSecretEnvironments.Core;

public class UserSecretEnvironmentManager
{
    public static void Initialize()
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
        {
            throw new Exception();
        }

        var userSecretParentId = userSecretId.Split('_').First();

        CreateNewUserSecretsEnvironment(userSecretParentId, "Dev");
        CreateNewUserSecretsEnvironment(userSecretParentId, "Qa");
        CreateNewUserSecretsEnvironment(userSecretParentId, "Prod");
    }

    public static void Initialize(string[] environmentNames)
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
        {
            throw new Exception();
        }

        var userSecretParentId = userSecretId.Split('_').First();

        foreach (var environmentName in environmentNames)
        {
            CreateNewUserSecretsEnvironment(userSecretParentId, environmentName);
        }
    }

    public static void UseEnvironment(string environmentName)
    {
        var projectFile = DirectoryUtil.GetLocalProjectFilePath();

        var projectRootElement = ProjectRootElement.Open(projectFile);

        var userSecretId = projectRootElement.Properties
            .Where(x => x.Name == "UserSecretsId")
            .Select(x => x.Value)
            .FirstOrDefault();

        if (userSecretId == null)
        {
            throw new Exception();
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var selectedUserSecretValue = $"{userSecretId}_{environmentName}";
        var userSecretsPath = appDataPath + $"/Microsoft/UserSecrets/{selectedUserSecretValue}/secrets.json";

        if (!File.Exists(userSecretsPath))
        {
            throw new Exception();
        }

        projectRootElement.AddProperty("UserSecretsId", selectedUserSecretValue);
        projectRootElement.Save();
    }

    public static List<string> GetEnvironments()
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
        {
            throw new Exception();
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsDirectoryPath = appDataPath + $"/Microsoft/UserSecrets";

        if (!Directory.Exists(userSecretsDirectoryPath))
        {
            throw new Exception();
        }

        var userSecretFiles = Directory.GetDirectories(userSecretsDirectoryPath)
            .Where(dir => dir.Contains(userSecretId + "_"))
            .ToList();

        return userSecretFiles;
    }

    public static void EditEnvironment(string environmentName)
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
        {
            throw new Exception();
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsDirectoryName = $"{userSecretId}_{environmentName.ToUpper()}";
        var userSecretsDirectoryPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretsDirectoryName}";

        if (!Directory.Exists(userSecretsDirectoryPath))
        {
            throw new Exception();
        }

        Process.Start("notepad.exe", userSecretsDirectoryPath + "/secrets.json");
    }

    private static string? GetUserSecretId()
    {
        var projectFile = DirectoryUtil.GetLocalProjectFilePath();

        var projectRootElement = ProjectRootElement.Open(projectFile);

        return projectRootElement.Properties
            .Where(x => x.Name == "UserSecretsId")
            .Select(x => x.Value)
            .FirstOrDefault();
    }

    private static void CreateNewUserSecretsEnvironment(string userSecretsId, string environmentName)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretsId}/secrets.json";

        if (!File.Exists(userSecretsPath))
        {
            throw new Exception();
        }

        var newDirectoryPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretsId}_{environmentName.ToUpper()}";
         
        if (!Directory.Exists(newDirectoryPath))
        {
            Directory.CreateDirectory(newDirectoryPath);
            File.Copy(userSecretsPath, newDirectoryPath + "/secrets.json");
        }

    }
}
