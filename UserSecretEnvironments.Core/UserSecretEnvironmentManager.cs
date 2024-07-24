using Microsoft.Build.Construction;
using System.Diagnostics;

namespace UserSecretEnvironments.Core;

public class UserSecretEnvironmentManager
{
    public static InitializeEnvironmentsResult InitializeDefaultEnvironments()
    {
        var defaultEnvironmentNames = new string[] { "Dev", "Qa", "Prod" };

        return InitializeEnvironments(defaultEnvironmentNames);
    }

    public class InitializeEnvironmentsResult
    {
        public List<string> InitializedEnvironmentNames { get; set; } = [];

        public required UserSecretEnvironmentOperationResult OperationResult { get; set; }
    }

    public enum UserSecretEnvironmentOperationResult
    {
        Success,
        Failure,
        PartialInitialization,
        UnableToFindEnvironment,
        UnableToFindUserSecrets,
        UnableToFindUserSecretsDirectory
    }

    public static InitializeEnvironmentsResult InitializeEnvironments(string[] environmentNames)
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
        {
            return new InitializeEnvironmentsResult
            {
                OperationResult = UserSecretEnvironmentOperationResult.UnableToFindUserSecrets,
            };
        }

        var userSecretParentId = userSecretId.Split('_').First();

        var creationResults = new List<UserSecretEnvironmentOperationResult>();

        foreach (var environmentName in environmentNames)
        {
            var creationResult = CreateNewUserSecretsEnvironment(userSecretParentId, environmentName);

            creationResults.Add(creationResult);
        }

        var initializationResult = new InitializeEnvironmentsResult
        {
            OperationResult = UserSecretEnvironmentOperationResult.Success,
        };

        var successfulResults = creationResults
            .Where(x => x.Equals(UserSecretEnvironmentOperationResult.Success))
            .ToList();

        if (successfulResults.Count == 0)
        {
            initializationResult.OperationResult = UserSecretEnvironmentOperationResult.Failure;
        }
        else if (successfulResults.Count != creationResults.Count)
        {
            initializationResult.OperationResult = UserSecretEnvironmentOperationResult.PartialInitialization;
        }

        return initializationResult;
    }

    public class UseEnvironmentResult
    {
        public required UserSecretEnvironmentOperationResult OperationResult { get; set; }

        public string? PreviousEnvironmentDirectory { get; set; }

        public string? CurrentEnvironmentDirectory { get; set; }
    }

    public static UseEnvironmentResult UseEnvironment(string? environmentName)
    {
        var projectFile = DirectoryUtil.GetLocalProjectFilePath();

        var projectRootElement = ProjectRootElement.Open(projectFile);

        var userSecretId = projectRootElement.Properties
            .Where(x => x.Name == "UserSecretsId")
            .Select(x => x.Value)
            .FirstOrDefault();

        if (userSecretId == null)
        {
            return new UseEnvironmentResult
            {
                OperationResult = UserSecretEnvironmentOperationResult.UnableToFindUserSecrets,
            };
        }

        var userSecretParentId = userSecretId.Split('_').First();

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        var selectedUserSecretValue = userSecretParentId;
        
        if (string.IsNullOrEmpty(environmentName))
        {
            selectedUserSecretValue += "_" + environmentName;
        }

        var userSecretsPath = appDataPath + $"/Microsoft/UserSecrets/{selectedUserSecretValue}/secrets.json";

        if (!File.Exists(userSecretsPath))
        {
            return new UseEnvironmentResult
            {
                OperationResult = UserSecretEnvironmentOperationResult.UnableToFindUserSecretsDirectory,
            };
        }

        projectRootElement.AddProperty("UserSecretsId", selectedUserSecretValue);
        projectRootElement.Save();

        return new UseEnvironmentResult
        {
            OperationResult = UserSecretEnvironmentOperationResult.Success,
            PreviousEnvironmentDirectory = appDataPath + $"/Microsoft/UserSecrets/{userSecretId}/secrets.json",
            CurrentEnvironmentDirectory = appDataPath + $"/Microsoft/UserSecrets/{selectedUserSecretValue}/secrets.json",
        };
    }

    public static GetEnvironmentsResult GetEnvironments()
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
        {
            return new GetEnvironmentsResult 
            { 
                OperationResult = UserSecretEnvironmentOperationResult.UnableToFindUserSecrets 
            };
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsDirectoryPath = appDataPath + $"/Microsoft/UserSecrets";

        if (!Directory.Exists(userSecretsDirectoryPath))
        {
            return new GetEnvironmentsResult 
            { 
                OperationResult = UserSecretEnvironmentOperationResult.UnableToFindUserSecretsDirectory 
            };
        }

        var userSecretParentId = userSecretId.Split('_').First();

        var environmentNames = Directory.GetDirectories(userSecretsDirectoryPath)
            .Where(dir => dir.Contains(userSecretParentId + "_"))
            .ToList();

        return new GetEnvironmentsResult
        {
            OperationResult = UserSecretEnvironmentOperationResult.UnableToFindUserSecretsDirectory,
            EnvironmentNames = environmentNames
        };
    }

    public class GetEnvironmentsResult
    {
        public List<string> EnvironmentNames { get; set; } = [];

        public required UserSecretEnvironmentOperationResult OperationResult { get; set; }
    }

    public static UserSecretEnvironmentOperationResult EditEnvironment(string environmentName)
    {
        var userSecretId = GetUserSecretId();

        if (userSecretId == null)
        {
            return UserSecretEnvironmentOperationResult.UnableToFindUserSecrets;
        }

        var userSecretParentId = userSecretId.Split('_').First();

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsDirectoryName = $"{userSecretParentId}_{environmentName.ToUpper()}";
        var userSecretsDirectoryPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretsDirectoryName}";

        if (!Directory.Exists(userSecretsDirectoryPath))
        {
            return UserSecretEnvironmentOperationResult.UnableToFindUserSecretsDirectory;
        }

        Process.Start("notepad.exe", userSecretsDirectoryPath + "/secrets.json");

        return UserSecretEnvironmentOperationResult.Success;
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

    private static UserSecretEnvironmentOperationResult CreateNewUserSecretsEnvironment(string userSecretsId, string environmentName)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var userSecretsPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretsId}/secrets.json";
        
        if (!File.Exists(userSecretsPath))
        {
            return UserSecretEnvironmentOperationResult.UnableToFindUserSecretsDirectory;
        }

        var newDirectoryPath = appDataPath + $"/Microsoft/UserSecrets/{userSecretsId}_{environmentName.ToUpper()}";
         
        if (!Directory.Exists(newDirectoryPath))
        {
            Directory.CreateDirectory(newDirectoryPath);
            File.Copy(userSecretsPath, newDirectoryPath + "/secrets.json");
        }

        return UserSecretEnvironmentOperationResult.Success;
    }
}
