using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using SboxTools.Console;
using SboxTools.Console.Toolbar;
using Task = System.Threading.Tasks.Task;

namespace SboxTools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(SboxToolsPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ConsoleWindow), Orientation = ToolWindowOrientation.Right, Window = EnvDTE.Constants.vsWindowKindOutput, Style = VsDockStyle.Tabbed)]
    public sealed class SboxToolsPackage : AsyncPackage
    {
        /// <summary>
        /// SboxToolsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "be08f061-729b-4f5a-b06d-d59c9bbe280d";

        public const string guidSboxToolsPackageCmdSet = "748c47d3-5b7f-4990-8001-7176ea6fa267";
        public const int SboxConsoleToolbar = 0x1000;

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ConsoleWindowCommand.InitializeAsync(this);

            await ConnectCommand.InitializeAsync(this);
            await DisconnectCommand.InitializeAsync(this);
            await ToggleCommand.InitializeAsync<ToggleErrorCommand>(this);
            await ToggleCommand.InitializeAsync<ToggleWarnCommand>(this);
            await ToggleCommand.InitializeAsync<ToggleInfoCommand>(this);
            await ToggleCommand.InitializeAsync<ToggleTraceCommand>(this);
            await LoggerComboCommand.InitializeAsync(this);
            await LoggerComboListCommand.InitializeAsync(this);
            await SearchComboCommand.InitializeAsync(this);
        }

        #endregion
    }
}
