using System.Threading.Tasks;

using SharpC2.Screens;
using SharpC2.Services;

namespace SharpC2.ScreenCommands
{
    public class OpenHandlersScreenCommand : ScreenCommand
    {
        public override string Name => "handlers";
        public override string Description => "Manage Handlers";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => OpenHandlersScreen;

        private readonly Screen _screen;

        public OpenHandlersScreenCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task OpenHandlersScreen(string[] args)
        {
            var factory = ((DroneScreen)_screen).ScreenFactory;
            using var handlersScreen = (HandlersScreen) factory.GetScreen(ScreenFactory.ScreenType.Handlers);
            await handlersScreen.Show();
        }
    }
}