using System.Diagnostics;

namespace UserSecretEnvironments.Core
{
    public record EnvironmentManagerResult(OperationStatus OperationResult)
    {
        public string? ErrorMessage { get; set; } = GetOperationResultErrorMessage(OperationResult);

        private static string GetOperationResultErrorMessage(OperationStatus OperationResult)
        {
            return OperationResult switch
            {
                OperationStatus.UnableToFindEnvironment => "User Secrets do not seem to be configured correctly. " +
                        "Make sure you are running this command within a project folder (containing a .csproj file). \n" +
                        "Also Ensure your project has User Secrets set up (i.e. dotnet user-secrets init)",

                OperationStatus.UnableToFindProjectFile => "User Secrets do not seem to be configured correctly. " +
                        "Make sure you are running this command within a project folder (Containing a .csproj file).",

                OperationStatus.UnableToFindUserSecrets => "secrets.json file was not found for the selected for the environment provided. " +
                        "Ensure the UserSecrets folder was created correctly. " +
                        $"(i.e. {Process.GetCurrentProcess().ProcessName} init <environmentName?>)",

                _ => "Something went wrong when trying to complete this operation."
            };
        }
    }
}
