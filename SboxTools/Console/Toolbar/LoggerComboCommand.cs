using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace SboxTools.Console.Toolbar
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class LoggerComboCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0107;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("748c47d3-5b7f-4990-8001-7176ea6fa267");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        public string CurrentChoice = "All";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private LoggerComboCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var button = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(button);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static LoggerComboCommand Instance
        {
            get;
            private set;
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
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ConnectCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new LoggerComboCommand(package, commandService);
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
            OleMenuCmdEventArgs oOleMenuCmdEventArgs = (OleMenuCmdEventArgs)e;
            string newChoice = oOleMenuCmdEventArgs.InValue as string;

            if (oOleMenuCmdEventArgs.OutValue != IntPtr.Zero)
            {
                Marshal.GetNativeVariantForObject(CurrentChoice, oOleMenuCmdEventArgs.OutValue);
            }
            else if (newChoice != null)
            {
                CurrentChoice = newChoice;
                ConsoleWindow.Instance.ApplyFilters();
            }
        }
    }
}
