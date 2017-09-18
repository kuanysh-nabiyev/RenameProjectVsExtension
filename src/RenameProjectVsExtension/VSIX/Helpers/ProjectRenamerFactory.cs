using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using EnvDTE;

namespace VSIX.Helpers
{
    public class ProjectRenamerFactory
    {
        public static ProjectRenamer Create(Project selectedProject, RenameOptions renameOptions)
        {
            var dteSolutionHelper = new DteSolutionHelper();
            ProjectRenamer projectRenamer;
            if (IsSharedProject(selectedProject))
            {
                projectRenamer = new SharedTypeProjectRenamer
                {
                    ProjectUniqueName = Path.ChangeExtension(selectedProject.UniqueName, null),
                };
            }
            else
            {
                projectRenamer = new ProjectRenamer
                {
                    ProjectUniqueName = selectedProject.UniqueName
                };
            }

            projectRenamer.SolutionFullName = dteSolutionHelper.GetSolutionFullName();
            projectRenamer.ProjectFullName = selectedProject.FullName;
            projectRenamer.ProjectName = selectedProject.Name;
            projectRenamer.ProjectNameNew = renameOptions.ProjectName;
            projectRenamer.SolutionProjects = dteSolutionHelper.GetSolutionProjects().Select(it => it.FullName);
            if (renameOptions.IsNecessaryToRenameClassNamespace)
            {
                projectRenamer.NamespaceRenamer = new NamespaceRenamer
                {
                    IsNecessaryToRename = renameOptions.IsNecessaryToRenameClassNamespace,
                    ProjectFiles = dteSolutionHelper.GetProjectFiles(selectedProject),
                    SolutionFiles = dteSolutionHelper.GetSolutionFilesExceptSelectedProject(selectedProject)
                };
            }

            return projectRenamer;
        }

        private static bool IsSharedProject(Project selectedProject)
        {
            if (Path.GetExtension(selectedProject.FileName) == ".shproj")
                return true;
            return false;
        }
    }
}
