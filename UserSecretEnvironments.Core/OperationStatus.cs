namespace UserSecretEnvironments.Core
{
    public enum OperationStatus
    {
        Success,
        Failure,
        PartialInitialization,
        UnableToFindEnvironment,
        UnableToFindUserSecrets,
        UnableToFindUserSecretsDirectory,
        UnableToFindProjectFile
    }
}
