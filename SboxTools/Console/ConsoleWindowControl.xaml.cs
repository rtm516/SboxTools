using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SboxTools.Types;

namespace SboxTools.Console
{
    /// <summary>
    /// Interaction logic for ConsoleWindowControl.
    /// </summary>
    public partial class ConsoleWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWindowControl"/> class.
        /// </summary>
        public ConsoleWindowControl()
        {
            this.InitializeComponent();
        }

        private ConsoleOutput GetConsoleOutput(object sender)
        {
            // TODO: Clean up this
            DockPanel logLine = (DockPanel) ((TextBlock) ((ContextMenu) ((MenuItem) sender).Parent).PlacementTarget).Parent;
            return (ConsoleOutput) logLine.DataContext;
        }

        private void OpenStackTrace(object sender, RoutedEventArgs e)
        {
            ConsoleOutput consoleOutput = GetConsoleOutput(sender);

            if (!consoleOutput.HasStack) return;

            foreach (string stackLine in consoleOutput.StackLines)
            {
                Match match = Regex.Match(stackLine.Trim(), "^at (.+?)( in (.+):line (.+))?$");
                if (match.Success && match.Groups[3].Success)
                {
                    OpenFile(match.Groups[3].Value, int.Parse(match.Groups[4].Value));
                }
            }
        }

        private void CopyLine(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GetConsoleOutput(sender).Msg);
        }

        private void OpenFile(string filename, int line)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2 dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            dte2.MainWindow.Activate();
            _ = dte2.ItemOperations.OpenFile(filename, Constants.vsViewKindTextView);
            ((TextSelection)dte2.ActiveDocument.Selection).GotoLine(line, true);
        }
    }
}