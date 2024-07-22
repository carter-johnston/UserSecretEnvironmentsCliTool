using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using UserSecretEnvironments.Core;

namespace UserSecretEnvironments.Tests
{
    [TestClass]
    public class DirectoryTests
    {
        [TestMethod]
        public void LocalUserSecretsFolderExists()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            Assert.IsTrue(Directory.Exists(appDataPath+"/Microsoft/UserSecrets"));
        }

        [TestMethod]
        public void GetLocalProjectFile() 
        {
            var _ = DirectoryUtil.GetLocalProjectFilePath();
        }
    }
}