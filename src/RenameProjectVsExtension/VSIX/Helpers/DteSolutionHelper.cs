using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace VSIX.Helpers
{
    internal class DteSolutionHelper
    {
        public DTE2 GetDte()
        {
            return Package.GetGlobalService(typeof(DTE)) as DTE2;
        }

        public Project GetSelectedProject()
        {
            var dte = GetDte();
            if (dte.SelectedItems.Count == 0)
                return null;

            var selectedProject = dte.SelectedItems.Item(1).Project;
            return selectedProject;
        }

        public string GetSolutionFullName()
        {
            var dte = GetDte();
            return dte.Solution.FullName;
        }

        public IEnumerable<Project> GetSolutionProjects()
        {
            var dte = GetDte();

            var projects = new List<Project>();
            for (int i = 1; i <= dte.Solution.Projects.Count; i++)
            {
                var project = dte.Solution.Projects.Item(i);
                FindProjects(projects, project);
            }

            return projects;
        }

        public void FindProjects(IList<Project> projects, Project project)
        {
            try
            {
                if (string.IsNullOrEmpty(project.FullName))
                {
                    for (int j = 1; j <= project.ProjectItems.Count; j++)
                    {
                        var projectItem = project.ProjectItems.Item(j);
                        if (projectItem.SubProject != null)
                            FindProjects(projects, projectItem.SubProject);
                    }
                }
                else
                {
                    projects.Add(project);
                }
            }
            catch {/* ignored */}
        }

        public IEnumerable<string> GetProjectFiles(Project project)
        {
            var projectItems = GetProjectItems(project);
            return projectItems
                .Select(a => a.Properties?.Item("FullPath")?.Value?.ToString())
                .Where(it => !string.IsNullOrEmpty(it));
        }

        public IEnumerable<string> GetSolutionFilesExceptSelectedProject(Project selectedProject)
        {
            var solutionFiles = new List<string>();
            foreach (Project project in GetSolutionProjects())
            {
                if (project.FullName != selectedProject.FullName)
                {
                    solutionFiles.AddRange(GetProjectFiles(project));
                }
            }
            return solutionFiles;
        }

        public bool IsProjectFilesSaved(Project project)
        {
            var projectItems = GetProjectItems(project);
            return projectItems.All(a => a.Saved);
        }

        private IEnumerable<ProjectItem> GetProjectItems(Project project)
        {
            var projectItems = new List<ProjectItem>();
            foreach (ProjectItem projectItem in project.ProjectItems)
            {
                FindFiles(projectItems, projectItem);
            }
            return projectItems;
        }

        private void FindFiles(IList<ProjectItem> projectItems, ProjectItem item)
        {
            if (item.ProjectItems == null || item.ProjectItems.Count == 0)
            {
                projectItems.Add(item);
                return;
            }

            var items = item.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = (ProjectItem)items.Current;
                FindFiles(projectItems, currentItem);
            }
        }
    }
}
