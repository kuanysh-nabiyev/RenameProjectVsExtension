//------------------------------------------------------------------------------
// <copyright file="RenameCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using Core;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSIX.Helpers;

namespace VSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RenameCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("dcffee83-c24d-4d60-8d48-de509bf2dc6a");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenameCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private RenameCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RenameCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new RenameCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dteSolutionHelper = new DteSolutionHelper();
            var messageBoxHelper = new MessageBoxHelper(this.ServiceProvider);

            var selectedProject = dteSolutionHelper.GetSelectedProject();
            if (!selectedProject.Saved)
            {
                messageBoxHelper.ShowInfoMessage("Build/rebuild project before renaming");
                return;
            }

            if (!dteSolutionHelper.IsProjectFilesSaved(selectedProject))
            {
                messageBoxHelper.ShowInfoMessage("Some files are not saved. Save them or build/rebuild project");
                return;
            }

            var renameOptions = new RenameOptions
            {
                ProjectName = selectedProject.Name,
                ProjectFullName = selectedProject.FullName
            };
            var dialog = new FullRenameDialog(renameOptions);
            var isOkButtonClicked = dialog.ShowModal();

            if (isOkButtonClicked == false)
                return;
            if (renameOptions.ProjectName == selectedProject.Name)
                return;

            try
            {
                var projectRenamer = ProjectRenamerFactory.Create(selectedProject, renameOptions);
                projectRenamer.FullRename();
                messageBoxHelper.ShowSuccessMessage();
            }
            catch (UnauthorizedAccessException uae)
            {
                messageBoxHelper.ShowErrorMessage(uae, "You don't have enough permisssion");
            }
            catch (IOException ioe)
            {
                messageBoxHelper.ShowErrorMessage(ioe, "Close all folders, text editors related to the project");
            }
            catch (Exception ex)
            {
                messageBoxHelper.ShowErrorMessage(ex);
            }
        }
    }
}
