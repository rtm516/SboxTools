using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using SboxTools.Types;

namespace SboxTools
{
    /// <summary>
    /// Interaction logic for SboxConsoleWindowControl.
    /// </summary>
    public partial class SboxConsoleWindowControl : UserControl
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SboxConsoleWindowControl"/> class.
        /// </summary>
        public SboxConsoleWindowControl()
        {
            this.InitializeComponent();
        }
    }
}