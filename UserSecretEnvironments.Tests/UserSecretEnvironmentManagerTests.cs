using UserSecretEnvironments.Core;

namespace UserSecretEnvironments.Tests;

[TestClass]
public class UserSecretEnvironmentManagerTests
{
    [TestMethod]
    public void Initialize_Test()
    {
        UserSecretEnvironmentManager.InitializeDefaults();
    }

    [TestMethod]
    public void EditEnvironment_Test()
    {
        var environmentName = "qa";
        UserSecretEnvironmentManager.EditEnvironment(environmentName);
    }

    [TestMethod]
    public void UseEnvironment_Test()
    {
        var environmentName = "qa";
        UserSecretEnvironmentManager.UseEnvironment(environmentName);
    }

    [TestMethod]
    public void Initialize_New_Environments_Test()
    {
        string[] environmentNames = { "staging" };
        UserSecretEnvironmentManager.InitializeEnvironment(environmentNames);
    }

    [TestMethod]
    public void GetEnvironments_Test()
    {
        UserSecretEnvironmentManager.GetEnvironments();
    }
}