using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var projectRenamer = new ProjectRenamer()
            {
                SolutionFullName = @"C:\Users\KUANYSH-PC\Documents\visual studio 2017\Projects\TestSolutionForRenaming\TestSolutionForRenaming.sln",
                ProjectFullName = @"C:\Users\KUANYSH-PC\Documents\visual studio 2017\Projects\TestSolutionForRenaming\TestProjectA\TestProjectA.csproj",
                ProjectName = "TestProjectA",
                ProjectUniqueName = @"TestProjectA\TestProjectA.csproj",
                ProjectNameNew = "TestProjectA_NewName",
                SolutionProjects = new List<string>
                {
                    @"C:\Users\KUANYSH-PC\Documents\visual studio 2017\Projects\TestSolutionForRenaming\TestProjectA\TestProjectA.csproj",
                    @"C:\Users\KUANYSH-PC\Documents\visual studio 2017\Projects\TestSolutionForRenaming\TestProjectB\TestProjectB.csproj",
                    @"C:\Users\KUANYSH-PC\Documents\visual studio 2017\Projects\TestSolutionForRenaming\TestProjectC\TestProjectC.csproj",
                }
            };

            projectRenamer.FullRename();
        }
    }
}
