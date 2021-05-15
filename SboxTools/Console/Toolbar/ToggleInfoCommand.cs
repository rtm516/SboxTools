using Microsoft.VisualStudio.Shell;

namespace SboxTools.Console.Toolbar
{
    internal class ToggleInfoCommand : ToggleCommand
    {
        public override int CommandId => 0x0105;

        public override string Level => "info";

        public ToggleInfoCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService) { }
    }
}
