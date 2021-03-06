using System;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using SboxTools.Types;
using Task = System.Threading.Tasks.Task;

namespace SboxTools.Console.Toolbar
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal class ToggleCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public virtual int CommandId { get; }

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("748c47d3-5b7f-4990-8001-7176ea6fa267");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        public virtual string Level { get; }

        public readonly OleMenuCommand Button;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        internal ToggleCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            Button = new OleMenuCommand(this.Execute, menuCommandID);
            Button.Checked = true;
            commandService.AddCommand(Button);
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
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
        public static async Task InitializeAsync<T>(AsyncPackage package) where T : ToggleCommand
        {
            // Switch to the main thread - the call to AddCommand in ConnectCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Activator.CreateInstance(typeof(T), (AsyncPackage)package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Button.Checked = !Button.Checked;

            ConsoleWindow.Instance.ApplyFilters();
        }
    }
}
