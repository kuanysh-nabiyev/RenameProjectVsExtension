using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using ChinhDo.Transactions;

[assembly: InternalsVisibleTo("Core.UnitTests")]
namespace Core
{
    public class ProjectRenamer
    {
        protected readonly TxFileManager FileManager;
        private string _projectUniqueName;

        public ProjectRenamer()
        {
            FileManager = new TxFileManager();
            NamespaceRenamer = new NamespaceRenamer();
        }

        public string SolutionFullName { get; set; }
        public string ProjectFullName { get; set; }
        public string ProjectName { get; set; }

        public string ProjectUniqueName
        {
            get
            {
                if (HasFolder(_projectUniqueName))
                    return $@"{GetFolderOfUniqueName(_projectUniqueName)}\{_projectUniqueName.GetFileName()}";
                return _projectUniqueName.GetFileName();
            }
            set => _projectUniqueName = value;
        }

        public string ProjectNameNew { get; set; }
        public IEnumerable<string> SolutionProjects { get; set; }
        public NamespaceRenamer NamespaceRenamer { get; set; }

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
                    RenameNamespaces();
                    RenameUserExtensionFile();
                    RenameAssemblyNameAndDefaultNamespace();
                    RenameAssemblyInformation();
                    RenameInSolutionFile();
                    RenameInProjectReferences();
                    CustomRenameForEachProjectType();
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
            if (this.ProjectFullName.GetDirectoryName() != this.ProjectFullNameNew.GetDirectoryName())
            {
                Directory.Move(this.ProjectFullName.GetDirectoryName(), this.ProjectFullNameNew.GetDirectoryName());
            }
        }

        public void RenameFile()
        {
            var fullNameWithRenamedFolder =
                Path.Combine(ProjectFullNameNew.GetDirectoryName(), ProjectFullName.GetFileName());
            FileManager.Move(fullNameWithRenamedFolder, this.ProjectFullNameNew);
        }

        public void RenameUserExtensionFile()
        {
            var fullNameWithRenamedFolder =
                Path.Combine(ProjectFullNameNew.GetDirectoryName(), ProjectFullName.GetFileName() + ".user");
            if (File.Exists(fullNameWithRenamedFolder))
            {
                FileManager.Move(fullNameWithRenamedFolder, this.ProjectFullNameNew + ".user");
            }
        }

        public virtual void RenameAssemblyNameAndDefaultNamespace()
        {
            string projFileText = File.ReadAllText(ProjectFullNameNew);
            projFileText = projFileText
                .ReplaceWithTag(this.ProjectName, this.ProjectNameNew, "AssemblyName")
                .ReplaceWithTag(this.ProjectName, this.ProjectNameNew, "RootNamespace");

            FileManager.WriteAllText(ProjectFullNameNew, projFileText);
        }

        public virtual void RenameAssemblyInformation()
        {
            var projectDirectory = ProjectFullNameNew.GetDirectoryName();
            var assemblyInfoFilePath = Path.Combine(projectDirectory, @"Properties\AssemblyInfo.cs");
            if (File.Exists(assemblyInfoFilePath))
            {
                string assemblyInfoFileText = File.ReadAllText(assemblyInfoFilePath);

                assemblyInfoFileText = assemblyInfoFileText.Replace(ProjectName, ProjectNameNew);
                FileManager.WriteAllText(assemblyInfoFilePath, assemblyInfoFileText);
            }
        }

        public void RenameInSolutionFile()
        {
            string slnFileText = File.ReadAllText(this.SolutionFullName);
            slnFileText = slnFileText
                .Replace(this.ProjectUniqueName, this.ProjectUniqueNameNew)
                .Replace($"\"{this.ProjectName}\"", $"\"{this.ProjectNameNew}\"");
            FileManager.WriteAllText(this.SolutionFullName, slnFileText);
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
                        FileManager.WriteAllText(projectFullName, projFileText);
                    }
                }
            }
        }

        public void RenameNamespaces()
        {
            if (NamespaceRenamer.IsNecessaryToRename)
            {
                var spaces = @"[ ]{1,}";
                foreach (string projectFilePath in NamespaceRenamer.ProjectFiles)
                {
                    var filePathWithRenamedFolder = projectFilePath.Replace(ProjectFullName.GetDirectoryName(), ProjectFullNameNew.GetDirectoryName());
                    string projFileText = File.ReadAllText(filePathWithRenamedFolder);
                    projFileText = Regex.Replace(
                        projFileText,
                        $"namespace{spaces}{ProjectName}",
                        $"namespace {ProjectNameNew}");
                    projFileText = Regex.Replace(
                        projFileText,
                        $"using{spaces}{ProjectName}",
                        $"using {ProjectNameNew}");
                    projFileText = projFileText.Replace($"{ProjectName}.", $"{ProjectNameNew}.");
                    FileManager.WriteAllText(filePathWithRenamedFolder, projFileText);
                }

                foreach (string filePath in NamespaceRenamer.SolutionFiles)
                {
                    string projFileText = File.ReadAllText(filePath);
                    projFileText = Regex.Replace(
                        projFileText,
                        $"using{spaces}{ProjectName}",
                        $"using {ProjectNameNew}");
                    projFileText = projFileText.Replace($"{ProjectName}.", $"{ProjectNameNew}.");
                    FileManager.WriteAllText(filePath, projFileText);
                }
            }
        }

        public virtual void CustomRenameForEachProjectType()
        {

        }

        private void RollbackRenameFolder()
        {
            if (this.ProjectFullNameNew.GetDirectoryName() != this.ProjectFullName.GetDirectoryName())
            {
                Directory.Move(this.ProjectFullNameNew.GetDirectoryName(), this.ProjectFullName.GetDirectoryName());
            }
        }

        public string GetFolderOfUniqueName(string projectUniqueName)
        {
            var directoryName = projectUniqueName.GetDirectoryName();
            var directoryInfo = new DirectoryInfo(directoryName);
            return directoryInfo.Name;
        }

        private bool HasFolder(string projectName)
        {
            var directoryName = projectName.GetDirectoryName();
            if (String.IsNullOrEmpty(directoryName))
                return false;
            return true;
        }
    }
}
