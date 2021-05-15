using Microsoft.VisualStudio.Shell;

namespace SboxTools.Console.Toolbar
{
    internal class ToggleInfoCommand : ToggleCommand
    {
        public override int CommandId => 0x0105;

        public override string Level => "info";

        public static ToggleInfoCommand Instance
        {
            get;
            private set;
        }

        public ToggleInfoCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
            Instance = this;
        }
    }
}
