using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ChinhDo.Transactions;

namespace Core
{
    public class ProjectRenamer
    {
        private readonly TxFileManager _fileManager;

        public ProjectRenamer()
        {
            _fileManager = new TxFileManager();
        }

        public string SolutionFullName { get; set; }
        public string ProjectFullName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectUniqueName { get; set; }
        public string ProjectNameNew { get; set; }
        public IEnumerable<string> SolutionProjects { get; set; }

        internal string ProjectUniqueNameNew => ProjectUniqueName.Replace(ProjectName, ProjectNameNew);
        internal string ProjectFullNameNew => ProjectFullName.Replace(ProjectUniqueName, ProjectUniqueNameNew);

        public void FullRename()
        {
            RenameFolder();

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    RenameFile();
                    RenameAssemblyNameAndDefaultNamespace();
                    RenameAssemblyInformation();
                    RenameInSolutionFile();
                    RenameInProjectReferences();
                    scope.Complete();
                }
            }
            catch
            {
                RollbackRenameFolder();
                throw;
            }
        }

        public void RenameFolder()
        {
            string projectFolderFullPath = Path.GetDirectoryName(this.ProjectFullName);
            string projectNewFolderFullPath = Path.GetDirectoryName(this.ProjectFullNameNew);
            Directory.Move(projectFolderFullPath, projectNewFolderFullPath);
        }

        public void RenameFile()
        {
            var fullNameWithRenamedFolder =
                Path.Combine(Path.GetDirectoryName(ProjectFullNameNew), Path.GetFileName(ProjectFullName));
            _fileManager.Move(fullNameWithRenamedFolder, this.ProjectFullNameNew);
        }

        public void RenameAssemblyNameAndDefaultNamespace()
        {
            string projFileText = File.ReadAllText(ProjectFullNameNew);
            projFileText = projFileText
                .ReplaceWithTag(this.ProjectName, this.ProjectNameNew, "AssemblyName")
                .ReplaceWithTag(this.ProjectName, this.ProjectNameNew, "RootNamespace");

            _fileManager.WriteAllText(ProjectFullNameNew, projFileText);
        }

        public void RenameAssemblyInformation()
        {
            var projectDirectory = Path.GetDirectoryName(ProjectFullNameNew);
            var assemblyInfoFilePath = Path.Combine(projectDirectory, @"Properties\AssemblyInfo.cs");
            string assemblyInfoFileText = File.ReadAllText(assemblyInfoFilePath);

            assemblyInfoFileText = assemblyInfoFileText.Replace(ProjectName, ProjectNameNew);
            _fileManager.WriteAllText(assemblyInfoFilePath, assemblyInfoFileText);
        }

        public void RenameInSolutionFile()
        {
            string slnFileText = File.ReadAllText(this.SolutionFullName);
            slnFileText = slnFileText
                .Replace(this.ProjectUniqueName, this.ProjectUniqueNameNew)
                .Replace($"\"{this.ProjectName}\"", $"\"{this.ProjectNameNew}\"");
            _fileManager.WriteAllText(this.SolutionFullName, slnFileText);
        }

        public void RenameInProjectReferences()
        {
            foreach (var projectFullName in SolutionProjects)
            {
                if (projectFullName != this.ProjectFullName)
                {
                    string projFileText = File.ReadAllText(projectFullName);
                    if (projFileText.Contains(this.ProjectUniqueName))
                    {
                        projFileText = projFileText
                            .Replace(this.ProjectUniqueName, this.ProjectUniqueNameNew)
                            .ReplaceWithTag(this.ProjectName, this.ProjectNameNew, "Name");
                        _fileManager.WriteAllText(projectFullName, projFileText);
                    }
                }
            }
        }

        public void RollbackRenameFolder()
        {
            string projectFolderFullPath = Path.GetDirectoryName(this.ProjectFullName);
            string projectNewFolderFullPath = Path.GetDirectoryName(this.ProjectFullNameNew);
            Directory.Move(projectNewFolderFullPath, projectFolderFullPath);
        }
    }
}
