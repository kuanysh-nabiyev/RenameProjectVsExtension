using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSIX.Helpers
{
    internal class MessageBoxHelper
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageBoxHelper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowSuccessMessage()
        {
            VsShellUtilities.ShowMessageBox(
                _serviceProvider,
                "Full rename was completed",
                "Success",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public void ShowErrorMessage(Exception ex, string title = "Error")
        {
            VsShellUtilities.ShowMessageBox(
                _serviceProvider,
                $"{ex.Message}\n{ex.StackTrace}",
                title,
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        public void ShowInfoMessage(string message, string title = "")
        {
            VsShellUtilities.ShowMessageBox(
                _serviceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
