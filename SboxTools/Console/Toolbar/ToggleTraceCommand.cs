using Microsoft.VisualStudio.Shell;

namespace SboxTools.Console.Toolbar
{
    internal class ToggleTraceCommand : ToggleCommand
    {
        public override int CommandId => 0x0106;

        public override string Level => "trace";

        public ToggleTraceCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService) { }
    }
}
