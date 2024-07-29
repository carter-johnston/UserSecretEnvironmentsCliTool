using Microsoft.Build.Construction;
using System.Diagnostics;

namespace UserSecretEnvironments.Core;

public class EnvironmentManager
{
    public static InitializeResult InitializeDefaultEnvironments()
    {
        var defaultEnvironmentNames = new string[] { "Dev", "Qa", "Prod" };

        return InitializeEnvironments(defaultEnvironmentNames);
    }

    public static InitializeResult InitializeEnvironments(string[] environmentNames)
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
        {
            return new InitializeResult(OperationType.UnableToFindUserSecrets);
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsFilePath = appDataPath + $"/Microsoft/UserSecrets/{userSecretId}/secrets.json";

        if (!File.Exists(userSecretsFilePath))
        {
            return new InitializeResult(OperationType.UnableToFindUserSecretsDirectory);
        }

        foreach (var environmentName in environmentNames)
        {
            var newFilePath = appDataPath + $"/Microsoft/UserSecrets/{userSecretId}/{environmentName.ToUpper()}.json";

            if (!Directory.Exists(newFilePath))
            {
                File.Copy(userSecretsFilePath, newFilePath);
            }
        }

        return new InitializeResult(OperationType.Success);
    }

    public static EnvironmentManagerResult UseEnvironment(string? environmentName)
    {
        var userSecretsId = GetUserSecretId();

        if (userSecretsId == null)
        {
            return new EnvironmentManagerResult(OperationType.UnableToFindUserSecrets);
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsFilePath = $"{appDataPath}/Microsoft/UserSecrets/{userSecretsId}/secrets.json";
        var environmentFilePath = $"{appDataPath}/Microsoft/UserSecrets/{userSecretsId}/{environmentName?.ToUpper()}.json";

        if (!File.Exists(environmentFilePath))
        {
            return new EnvironmentManagerResult(OperationType.UnableToFindUserSecretsDirectory);
        }

        File.Copy(environmentFilePath, userSecretsFilePath);

        return new EnvironmentManagerResult(OperationType.Success);
    }

    public static GetEnvironmentsResult GetEnvironments()
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
        {
            return new GetEnvironmentsResult(OperationType.UnableToFindUserSecrets);
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsDirectoryPath = appDataPath + $"/Microsoft/UserSecrets";

        if (!Directory.Exists(userSecretsDirectoryPath))
        {
            return new GetEnvironmentsResult(OperationType.UnableToFindUserSecretsDirectory);
        }

        var userSecretParentId = userSecretId.Split('_').First();

        //Get list of files in a directory
        var environments = Directory
            .GetDirectories(userSecretsDirectoryPath)
            .Where(dir => dir.Contains(userSecretParentId + "_"))
            .ToList();

        return new GetEnvironmentsResult(OperationType.UnableToFindUserSecretsDirectory)
        {
            UserSecretEnvironments = new 
        };
    }

    public static OperationType EditEnvironment(string environmentName)
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
            return OperationType.UnableToFindUserSecrets;

        var userSecretParentId = userSecretId.Split('_').First();
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsDirectoryName = $"{userSecretParentId}_{environmentName.ToUpper()}";
        var userSecretsDirectoryPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretsDirectoryName}";

        if (!Directory.Exists(userSecretsDirectoryPath))
            return OperationType.UnableToFindUserSecretsDirectory;

        Process.Start("notepad.exe", userSecretsDirectoryPath + "/secrets.json");

        return OperationType.Success;
    }

    public record GetEnvironmentsResult(OperationType OperationResult)
    : EnvironmentManagerResult(OperationResult)
    {
        public List<UserSecretEnvironment> UserSecretEnvironments { get; init; } = [];
    }

    public record InitializeResult(OperationType OperationResult)
        : EnvironmentManagerResult(OperationResult)
    {
        public List<string> EnvironmentDirectories { get; set; } = [];
    }

    public record UserSecretEnvironment(string Name, string FilePath, bool IsActive);

    private static string? GetUserSecretId()
    {
        string binDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.FullName;
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;

        string[] localProjectPaths;

        if (Directory.GetFiles(Environment.CurrentDirectory, "*.csproj").Length != 0)
        {
            localProjectPaths = Directory.GetFiles(Environment.CurrentDirectory, "*.csproj");
        }
        else if (Directory.GetFiles(binDirectory, "*.csproj").Length != 0)
        {
            localProjectPaths = Directory.GetFiles(binDirectory, "*.csproj");
        }
        else
        {
            localProjectPaths = Directory.GetFiles(projectDirectory, "*.csproj");
        }

        if (!localProjectPaths.Any()) return null;

        var projectRootElement = ProjectRootElement.Open(localProjectPaths.First());

        return projectRootElement.Properties
            .Where(x => x.Name == "UserSecretsId")
            .Select(x => x.Value)
            .FirstOrDefault();
    }

    private static string? GetLocalProjectFilePath()
    {
        string binDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.FullName;
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;

        string[] localProjectPaths;

        if (Directory.GetFiles(Environment.CurrentDirectory, "*.csproj").Length != 0)
        {
            localProjectPaths = Directory.GetFiles(Environment.CurrentDirectory, "*.csproj");
        }
        else if (Directory.GetFiles(binDirectory, "*.csproj").Length != 0)
        {
            localProjectPaths = Directory.GetFiles(binDirectory, "*.csproj");
        }
        else
        {
            localProjectPaths = Directory.GetFiles(projectDirectory, "*.csproj");
        }

        return localProjectPaths.FirstOrDefault();
    }

    private static OperationType CreateNewSecretsFile(
        string userSecretsId, 
        string environmentName)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretsId}/secrets.json";
        
        if (!File.Exists(userSecretsPath))
        {
            return OperationType.UnableToFindUserSecretsDirectory;
        }

        var newDirectoryPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretsId}_{environmentName.ToUpper()}";
         
        if (!Directory.Exists(newDirectoryPath))
        {
            Directory.CreateDirectory(newDirectoryPath);
            File.Copy(userSecretsPath, newDirectoryPath + "/secrets.json");
        }

        return OperationType.Success;
    }
}
