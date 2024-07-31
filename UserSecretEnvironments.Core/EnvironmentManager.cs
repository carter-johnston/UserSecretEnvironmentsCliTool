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
        var getUserSecretIdResult = GetUserSecretId();

        var failedToGetUserSecret = getUserSecretIdResult.OperationStatus != OperationStatus.Success
            || string.IsNullOrEmpty(getUserSecretIdResult.UserSecretsId);
        
        if (failedToGetUserSecret)
        {
            return new InitializeResult(getUserSecretIdResult.OperationStatus);
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsFilePath = appDataPath + $"/Microsoft/UserSecrets/{getUserSecretIdResult.UserSecretsId}/secrets.json";

        if (!File.Exists(userSecretsFilePath))
        {
            return new InitializeResult(OperationStatus.UnableToFindUserSecretsDirectory);
        }

        var userSecretEnvironments = environmentNames
            .Select(environmentName =>
            {
                var filePath = appDataPath + $"/Microsoft/UserSecrets/{getUserSecretIdResult.UserSecretsId}/{environmentName.ToUpper()}.json";
                return new UserSecretEnvironment(environmentName, filePath, false);
            })
            .ToList();

        foreach (var userSecretEnvironment in userSecretEnvironments)
        {
            var newFilePath = appDataPath + $"/Microsoft/UserSecrets/{getUserSecretIdResult.UserSecretsId}/{userSecretEnvironment.Name.ToUpper()}.json";

            if (!Directory.Exists(newFilePath))
            {
                File.Copy(userSecretsFilePath, newFilePath, true);
            }
        }

        return new InitializeResult(OperationStatus.Success)
        {
            UserSecretEnvironments = userSecretEnvironments,
        };
    }

    public static EnvironmentManagerResult UseEnvironment(string? environmentName)
    {
        var getUserSecretIdResult = GetUserSecretId();

        var failedToGetUserSecret = getUserSecretIdResult.OperationStatus != OperationStatus.Success
            || string.IsNullOrEmpty(getUserSecretIdResult.UserSecretsId);

        if (failedToGetUserSecret)
        {
            return new EnvironmentManagerResult(getUserSecretIdResult.OperationStatus);
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsFilePath = $"{appDataPath}/Microsoft/UserSecrets/{getUserSecretIdResult.UserSecretsId}/secrets.json";
        var environmentFilePath = $"{appDataPath}/Microsoft/UserSecrets/{getUserSecretIdResult.UserSecretsId}/{environmentName?.ToUpper()}.json";

        if (!File.Exists(environmentFilePath))
        {
            return new EnvironmentManagerResult(OperationStatus.UnableToFindUserSecretsDirectory);
        }

        File.Copy(environmentFilePath, userSecretsFilePath, true);

        return new EnvironmentManagerResult(OperationStatus.Success);
    }

    public static GetEnvironmentsResult GetEnvironments()
    {
        var getUserSecretIdResult = GetUserSecretId();

        var failedToGetUserSecret = getUserSecretIdResult.OperationStatus != OperationStatus.Success
            || string.IsNullOrEmpty(getUserSecretIdResult.UserSecretsId);

        if (failedToGetUserSecret)
        {
            return new GetEnvironmentsResult(getUserSecretIdResult.OperationStatus);
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsDirectoryPath = appDataPath + $"/Microsoft/UserSecrets/{getUserSecretIdResult.UserSecretsId}";

        if (!Directory.Exists(userSecretsDirectoryPath))
        {
            return new GetEnvironmentsResult(OperationStatus.UnableToFindUserSecretsDirectory);
        }

        var userSecretEnvironments = Directory.GetFiles(userSecretsDirectoryPath)
            .Where(filePath => !filePath.EndsWith("secrets.json"))
            .Select(filePath => 
            {
                var environmentName = Path.GetFileNameWithoutExtension(filePath);
                return new UserSecretEnvironment(environmentName, filePath, false);
            })
            .ToList();

        return new GetEnvironmentsResult(OperationStatus.Success)
        {
            UserSecretEnvironments = userSecretEnvironments,
        };
    }

    public static OperationStatus EditEnvironment(string environmentName)
    {
        var getUserSecretIdResult = GetUserSecretId();

        var failedToGetUserSecret = getUserSecretIdResult.OperationStatus != OperationStatus.Success
            || string.IsNullOrEmpty(getUserSecretIdResult.UserSecretsId);

        if (failedToGetUserSecret)
        {
            return getUserSecretIdResult.OperationStatus;
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsEnvironmentPath = appDataPath + $"/Microsoft/UserSecrets/{getUserSecretIdResult.UserSecretsId}/{environmentName}.json";

        if (!Directory.Exists(userSecretsEnvironmentPath))
        {
            return OperationStatus.UnableToFindUserSecretsDirectory;
        }

        Process.Start("notepad.exe", userSecretsEnvironmentPath);
        
        return OperationStatus.Success;
    }

    public record GetEnvironmentsResult(OperationStatus OperationResult)
    : EnvironmentManagerResult(OperationResult)
    {
        public List<UserSecretEnvironment> UserSecretEnvironments { get; init; } = [];
    }

    public record InitializeResult(OperationStatus OperationResult)
        : EnvironmentManagerResult(OperationResult)
    {
        public List<UserSecretEnvironment> UserSecretEnvironments { get; set; } = [];
    }

    public record UserSecretEnvironment(string Name, string FilePath, bool IsActive);

    private static GetUserSecretIdResult GetUserSecretId()
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

        if (!localProjectPaths.Any()) { 
            return new GetUserSecretIdResult(OperationStatus.UnableToFindProjectFile); 
        }

        var projectRootElement = ProjectRootElement.Open(localProjectPaths.First());

        var userSecretId = projectRootElement.Properties
            .Where(x => x.Name == "UserSecretsId")
            .Select(x => x.Value)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(userSecretId)
            ? new GetUserSecretIdResult(OperationStatus.UnableToFindUserSecrets)
            : new GetUserSecretIdResult(OperationStatus.Success) 
            { 
                UserSecretsId = userSecretId 
            };
    }

    public record GetUserSecretIdResult(OperationStatus OperationStatus)
    {
        public string? UserSecretsId { get; init; }
    }
}
