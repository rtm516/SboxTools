using System;
using System.ComponentModel.Design;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
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
        public static ConsoleWindow Instance
        {
            get;
            private set;
        }

        private ClientWebSocket WebSocket;
        private ConsoleWindowControl WindowControl;
        private ScrollViewer LogScroll;
        public StackPanel LogPanel;

        public bool IsConnected
        {
            get
            {
                return WebSocket != null && WebSocket.State == WebSocketState.Open;
            }
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
        }

        public void Connect()
        {
            if (WebSocket != null && WebSocket.State == WebSocketState.Open) return;

            WebSocket = new ClientWebSocket();
            LogPanel.Children.Clear();
            WebSocket.ConnectAsync(new Uri("ws://127.0.0.1:29016"), CancellationToken.None).ContinueWith((t) => System.Threading.Tasks.Task.Run(RecvLoop));
        }

        public void Disconnect()
        {
            if (WebSocket != null && WebSocket.State != WebSocketState.Open) return;

            WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None).ContinueWith(t => WebSocket = null);
        }

        private async System.Threading.Tasks.Task RecvLoop()
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
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    if (result.MessageType != WebSocketMessageType.Text) continue;

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        Output output = JsonConvert.DeserializeObject<Output>(await reader.ReadToEndAsync());

                        await HandleOutput(output);
                    }
                }
            }

            if (WebSocket.State == WebSocketState.Aborted)
            {
                await WebSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "", CancellationToken.None);
            }
        }

        private async System.Threading.Tasks.Task HandleOutput(Output output)
        {
            if (output.Type == "ConsoleOutput")
            {
                ConsoleOutput consoleOutput = JsonConvert.DeserializeObject<ConsoleOutput>(output.Data);
                AddLine(consoleOutput, DateTime.Now);
            }
        }

        // TODO: Remove these extra args as its super messy
        private void AddLine(ConsoleOutput consoleOutput, DateTime now)
        {
            // Run back on UI thread
            WindowControl.Dispatcher.BeginInvoke(new Action(() =>
            {
                bool autoScroll = LogScroll.VerticalOffset == LogScroll.ScrollableHeight;

                DockPanel row = new DockPanel();
                row.DataContext = consoleOutput;

                TextBlock timeText = new TextBlock();
                timeText.Text = now.ToString("HH:mm:ss");
                timeText.Style = (Style)WindowControl.Resources["TextBlockTime"];
                row.Children.Add(timeText);

                TextBlock loggerText = new TextBlock();
                loggerText.Text = consoleOutput.Logger.ToUpper();
                loggerText.MinWidth = 140;
                loggerText.Style = (Style)WindowControl.Resources["TextBlockLogger"];
                row.Children.Add(loggerText);

                TextBlock msgText = new TextBlock();
                msgText.Text = consoleOutput.Msg;
                msgText.Style = (Style)WindowControl.Resources["TextBlock" + consoleOutput.Level.FirstCharToUpper()];
                msgText.ContextMenu = (ContextMenu)WindowControl.Resources["LogContextMenu"];
                row.Children.Add(msgText);

                LogPanel.Children.Add(row);

                if (autoScroll)
                {
                    LogScroll.ScrollToEnd();
                }
            }), null);

        }
    }
}
