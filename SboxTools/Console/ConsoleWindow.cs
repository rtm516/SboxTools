using System;
using System.ComponentModel.Design;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using SboxTools.Console.Toolbar;
using SboxTools.Types;

namespace SboxTools.Console
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("f097cd44-1007-4923-abe4-b10daa88b6ca")]
    public class ConsoleWindow : ToolWindowPane
    {
        public static ConsoleWindow Instance { get; private set; }
        public StackPanel LogPanel { get; }
        public Logs Logs { get; }

        private ClientWebSocket WebSocket;
        private ConsoleWindowControl WindowControl;
        private ScrollViewer LogScroll;


        public bool IsConnected
        {
            get { return WebSocket != null && WebSocket.State == WebSocketState.Open; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleWindow"/> class.
        /// </summary>
        public ConsoleWindow() : base(null)
        {
            Instance = this;

            this.Caption = "s&box Console";

            WindowControl = new ConsoleWindowControl();
            this.Content = WindowControl;

            this.ToolBar = new CommandID(new Guid(SboxToolsPackage.guidSboxToolsPackageCmdSet), SboxToolsPackage.SboxConsoleToolbar);

            LogScroll = WindowControl.FindName("LogScroll") as ScrollViewer;
            LogPanel = WindowControl.FindName("LogPanel") as StackPanel;

            Logs = new Logs();
        }

        public void Connect()
        {
            if (WebSocket != null && WebSocket.State == WebSocketState.Open) return;

            WebSocket = new ClientWebSocket();
            LogPanel.Children.Clear();
            _ = WebSocket.ConnectAsync(new Uri("ws://127.0.0.1:29016"), CancellationToken.None).ContinueWith((t) => System.Threading.Tasks.Task.Run(RecvLoopAsync), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public void Disconnect()
        {
            if (WebSocket != null && WebSocket.State != WebSocketState.Open) return;

            _ = WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).ContinueWith(t => WebSocket = null, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        private async System.Threading.Tasks.Task RecvLoopAsync()
        {
            ArraySegment<Byte> buffer = new ArraySegment<byte>(new byte[8 * 1024]);
            WebSocketReceiveResult result;
            while (true)
            {
                if (WebSocket != null && WebSocket.State != WebSocketState.Open) break;

                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await WebSocket.ReceiveAsync(buffer, CancellationToken.None);
                        await ms.WriteAsync(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType != WebSocketMessageType.Text) continue;

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        Output output = JsonConvert.DeserializeObject<Output>(await reader.ReadToEndAsync());

                        HandleOutput(output);
                    }
                }
            }

            if (WebSocket.State == WebSocketState.Aborted)
            {
                await WebSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "", CancellationToken.None);
            }
        }

        private void HandleOutput(Output output)
        {
            if (output.Type == "ConsoleOutput")
            {
                ConsoleOutput consoleOutput = JsonConvert.DeserializeObject<ConsoleOutput>(output.Data);
                consoleOutput.TimeStamp = DateTime.Now;
                AddLine(consoleOutput);
            }
        }

        private void AddLine(ConsoleOutput consoleOutput)
        {
            Logs.AddLine(consoleOutput);

            // Run back on UI thread
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                bool autoScroll = LogScroll.VerticalOffset == LogScroll.ScrollableHeight;

                DockPanel row = new DockPanel();
                row.DataContext = consoleOutput;

                // Change visibility based on active filters
                row.Visibility = ShouldFilter(consoleOutput) ? Visibility.Visible : Visibility.Collapsed;

                TextBlock timeText = new TextBlock();
                timeText.Text = consoleOutput.TimeStamp.ToString("HH:mm:ss");
                timeText.Style = (Style) WindowControl.Resources["TextBlockTime"];
                row.Children.Add(timeText);

                TextBlock loggerText = new TextBlock();
                loggerText.Text = consoleOutput.Logger.ToUpper();
                loggerText.MinWidth = 140;
                loggerText.Style = (Style) WindowControl.Resources["TextBlockLogger"];
                row.Children.Add(loggerText);

                TextBlock msgText = new TextBlock();
                msgText.Text = consoleOutput.Msg;
                msgText.Style = (Style) WindowControl.Resources["TextBlock" + consoleOutput.Level.FirstCharToUpper()];
                msgText.ContextMenu = (ContextMenu) WindowControl.Resources["LogContextMenu"];
                row.Children.Add(msgText);

                LogPanel.Children.Add(row);

                if (autoScroll)
                {
                    LogScroll.ScrollToEnd();
                }
            });
        }

        public void ApplyFilters()
        {
            foreach (DockPanel logLine in LogPanel.Children)
            {
                logLine.Visibility = ShouldFilter((ConsoleOutput)logLine.DataContext) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static bool ShouldFilter(ConsoleOutput consoleOutput)
        {
            bool visible = true;
            switch (consoleOutput.Level)
            {
                case "error":
                    visible = ToggleErrorCommand.Instance.Button.Checked;
                    break;

                case "warn":
                    visible = ToggleWarnCommand.Instance.Button.Checked;
                    break;

                case "info":
                    visible = ToggleInfoCommand.Instance.Button.Checked;
                    break;

                case "trace":
                    visible = ToggleTraceCommand.Instance.Button.Checked;
                    break;
            }

            // Check logger filter
            if (visible && LoggerComboCommand.Instance.CurrentChoice != "All")
            {
                visible = consoleOutput.Logger == LoggerComboCommand.Instance.CurrentChoice;
            }

            // Check search filter
            if (visible && SearchComboCommand.Instance.CurrentSearch != "")
            {
                visible = consoleOutput.Msg.Contains(SearchComboCommand.Instance.CurrentSearch);
            }

            return visible;
        }
    }
}