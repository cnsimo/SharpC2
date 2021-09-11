using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class BackScreenCommand : ScreenCommand
    {
        public override string Name => "back";
        public override string Description => "Back to previous screen";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => GoBack;

        private readonly Screen _screen;

        public BackScreenCommand(Screen screen)
        {
            _screen = screen;
        }

        private Task GoBack(string[] args)
        {
            _screen.ScreenRunning = false;
            return Task.CompletedTask;
        }
    }
}