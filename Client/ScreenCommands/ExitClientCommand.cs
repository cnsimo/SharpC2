using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class ExitClientCommand : ScreenCommand
    {
        public override string Name => "exit";
        public override string Description => "Exit the client";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => ExitClient;

        private readonly Screen _screen;

        public ExitClientCommand(Screen screen)
        {
            _screen = screen;
        }

        private Task ExitClient(string[] args)
        {
            _screen.ScreenRunning = false;
            return Task.CompletedTask;
        }
    }
}