namespace UserSecretEnvironments.Core
{
    public enum OperationType
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
