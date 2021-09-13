using System.Threading.Tasks;

using SharpC2.Screens;
using SharpC2.Services;

namespace SharpC2.ScreenCommands
{
    public class OpenPayloadsScreenCommand : ScreenCommand
    {
        public override string Name => "payloads";
        public override string Description => "Generate juicy payloads";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => OpenPayloadsScreen;

        private readonly Screen _screen;

        public OpenPayloadsScreenCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task OpenPayloadsScreen(string[] args)
        {
            var factory = ((DroneScreen)_screen).ScreenFactory;
            var handlersScreen = (PayloadsScreen) factory.GetScreen(ScreenFactory.ScreenType.Payloads);
            await handlersScreen.Show();
        }
    }
}