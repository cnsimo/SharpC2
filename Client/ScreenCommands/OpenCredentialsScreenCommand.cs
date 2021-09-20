using System.Threading.Tasks;

using SharpC2.Screens;
using SharpC2.Services;

namespace SharpC2.ScreenCommands
{
    public class OpenCredentialsScreenCommand : ScreenCommand
    {
        public override string Name => "credentials";
        public override string Description => "Manage credentials";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => OpenCredentialsScreen;
        
        private readonly Screen _screen;

        public OpenCredentialsScreenCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task OpenCredentialsScreen(string[] args)
        {
            var factory = ((DroneScreen)_screen).ScreenFactory;
            var screen = factory.GetScreen(ScreenFactory.ScreenType.Credentials);
            await screen.Show();
        }
    }
}