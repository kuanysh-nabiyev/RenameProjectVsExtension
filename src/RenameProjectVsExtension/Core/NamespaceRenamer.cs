using System.Collections.Generic;

namespace Core
{
    public class NamespaceRenamer
    {
        public bool IsNecessaryToRename { get; set; }
        public IEnumerable<string> ProjectFiles { get; set; }
        public IEnumerable<string> SolutionFiles { get; set; }
    }
}