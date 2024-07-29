using System.Diagnostics;

namespace UserSecretEnvironments.Core
{
    public record EnvironmentManagerResult(OperationType OperationResult)
    {
        public string? ErrorMessage { get; set; } = GetOperationResultErrorMessage(OperationResult);

        private static string GetOperationResultErrorMessage(OperationType OperationResult)
        {
            return OperationResult switch
            {
                OperationType.UnableToFindEnvironment => "User Secrets do not seem to be configured correctly. " +
                        "Make sure you are running this command within a project folder (containing a .csproj file). \n" +
                        "Also Ensure your project has User Secrets set up (i.e. dotnet user-secrets init)",

                OperationType.UnableToFindProjectFile => "User Secrets do not seem to be configured correctly. " +
                        "Make sure you are running this command within a project folder (Containing a .csproj file).",

                OperationType.UnableToFindUserSecrets => "secrets.json file was not found for the selected for the environment provided. " +
                        "Ensure the UserSecrets folder was created correctly. " +
                        $"(i.e. {Process.GetCurrentProcess().ProcessName} init <environmentName?>)",

                _ => "Something went wrong when trying to complete this operation."
            };
        }
    }
}
