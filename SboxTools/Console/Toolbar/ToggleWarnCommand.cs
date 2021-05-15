using Microsoft.VisualStudio.Shell;

namespace SboxTools.Console.Toolbar
{
    internal class ToggleWarnCommand : ToggleCommand
    {
        public override int CommandId => 0x0104;

        public override string Level => "warn";

        public static ToggleWarnCommand Instance
        {
            get;
            private set;
        }

        public ToggleWarnCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
            Instance = this;
        }
    }
}
