using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.PlatformUI;

namespace VSIX
{
    /// <summary>
    /// Interaction logic for FullRenameDialog.xaml
    /// </summary>
    public partial class FullRenameDialog : DialogWindow
    {
        public FullRenameDialog(RenameOptions renameOptions)
        {
            InitializeComponent();
            DataContext = renameOptions;
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }

    public class RenameOptions
    {
        public string ProjectName { get; set; }
        public bool IsNecessaryToRenameClassNamespace { get; set; }

        public string ProjectFullName { get; set; }
        public bool CanRenameNamespace => ProjectFullName.EndsWith(".csproj");
    }
}
