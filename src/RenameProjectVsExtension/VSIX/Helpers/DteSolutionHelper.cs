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

            return dte.SelectedItems.Item(1).Project;
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
    }
}
