using System.Threading.Tasks;

using SharpC2.Screens;
using SharpC2.Services;

namespace SharpC2.ScreenCommands
{
    public class OpenHostedFilesScreenCommand : ScreenCommand
    {
        public override string Name => "hosted-files";
        public override string Description => "Host files on the default HTTP Handler";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => OpenHostedFilesScreen;
        
        private readonly Screen _screen;

        public OpenHostedFilesScreenCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task OpenHostedFilesScreen(string[] args)
        {
            var factory = ((DroneScreen)_screen).ScreenFactory;
            using var hostedFilesScreen = (HostedFilesScreen) factory.GetScreen(ScreenFactory.ScreenType.HostedFiles);
            await hostedFilesScreen.Show();
        }
    }
}