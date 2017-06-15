using NUnit.Framework;

namespace Core.UnitTests
{
    [TestFixture]
    public class ProjectRenamerTests
    {
        [Test]
        public void GetFolderOfUniqueName_OwnProject_ReturnFolder()
        {
            var projectRenamer = new FakeProjectRenamer();
            var folderName = projectRenamer.GetFolderOfUniqueName(@"TestProjectA\TestProjectA.csproj");
            Assert.AreEqual("TestProjectA", folderName);
        }

        [Test]
        public void GetFolderOfUniqueName_AddedExistingProject_ReturnFolder()
        {
            var projectRenamer = new FakeProjectRenamer();
            var folderName = projectRenamer.GetFolderOfUniqueName(@"..\TestProjectA\TestProjectA.csproj");
            Assert.AreEqual("TestProjectA", folderName);
        }

        [Test]
        public void GetFolderOfUniqueName_AddedExistingProjectFromDesktop_ReturnFolder()
        {
            var projectRenamer = new FakeProjectRenamer();
            var folderName = projectRenamer.GetFolderOfUniqueName(@"..\..\Desktop\Folder1\Folder2\TestProjectA\TestProjectA.csproj");
            Assert.AreEqual("TestProjectA", folderName);
        }

        [TestCase(@"TestProjectA\TestProjectA.csproj")]
        [TestCase(@"..\TestProjectA\TestProjectA.csproj")]
        [TestCase(@"Desktop\Folder1\Folder2\TestProjectA\TestProjectA.csproj")]
        public void ProjectUniqueName_DifferentNames_ReturnFolderAndFileName(string projectUniqueName)
        {
            var projectRenamer = new FakeProjectRenamer()
            {
                ProjectUniqueName = projectUniqueName
            };

            Assert.AreEqual(@"TestProjectA\TestProjectA.csproj", projectRenamer.ProjectUniqueName);
        }

        [TestCase(@"TestProjectA\TestProjectA.csproj")]
        [TestCase(@"..\TestProjectA\TestProjectA.csproj")]
        [TestCase(@"..\..\..\..\Desktop\Folder1\Folder2\TestProjectA\TestProjectA.csproj")]
        [TestCase(@"Desktop\Folder1\TestProjectA\TestProjectA\TestProjectA.csproj")]
        [TestCase(@"..\TestProjectA\TestProjectA\TestProjectA.csproj")]
        public void ProjectUniqueNameNew_DifferentNames_ReturnNewUniqueName(string projectUniqueName)
        {
            var projectRenamer = new FakeProjectRenamer()
            {
                ProjectName = "TestProjectA",
                ProjectNameNew = "TestProjectA_NewName",
                ProjectUniqueName = projectUniqueName
            };

            Assert.AreEqual(@"TestProjectA_NewName\TestProjectA_NewName.csproj", projectRenamer.ProjectUniqueNameNew);
        }

        [TestCase(@"D:\Projects\SolutionFolderName\TestProjectA\TestProjectA.csproj", ExpectedResult = @"D:\Projects\SolutionFolderName\TestProjectA_NewName\TestProjectA_NewName.csproj", Description = "Standard situation")]
        [TestCase(@"D:\Projects\TestProjectA\TestProjectA\TestProjectA.csproj", ExpectedResult = @"D:\Projects\TestProjectA\TestProjectA_NewName\TestProjectA_NewName.csproj", Description = "Project folder is same with solution folder")]
        public string ProjectFullNameNew_DifferentNames_ReturnNewFullName(string projectFullName)
        {
            var projectRenamer = new FakeProjectRenamer()
            {
                ProjectName = "TestProjectA",
                ProjectNameNew = "TestProjectA_NewName",
                ProjectUniqueName = @"TestProjectA\TestProjectA.csproj",
                ProjectFullName = projectFullName
            };

            return projectRenamer.ProjectFullNameNew;
        }
    }
}