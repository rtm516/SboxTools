using Microsoft.VisualStudio.Shell;

namespace SboxTools.Console.Toolbar
{
    internal class ToggleErrorCommand : ToggleCommand
    {
        public override int CommandId => 0x0103;

        public override string Level => "error";

        public static ToggleErrorCommand Instance
        {
            get;
            private set;
        }

        public ToggleErrorCommand(AsyncPackage package, OleMenuCommandService commandService) : base(package, commandService)
        {
            Instance = this;
        }
    }
}
