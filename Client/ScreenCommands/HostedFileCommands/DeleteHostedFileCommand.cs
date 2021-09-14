using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class DeleteHostedFileCommand : ScreenCommand
    {
        public override string Name => "delete";
        public override string Description => "Delete a hosted file";
        public override string Usage => "delete [filename]";
        public override Screen.CommandCallback Callback => DeleteHostedFile;
        
        private readonly Screen _screen;

        public DeleteHostedFileCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task DeleteHostedFile(string[] args)
        {
            var screen = (HostedFilesScreen)_screen;
            
            if (args.Length < 2)
            {
                screen.Console.PrintError("Not enough arguments");
                return;
            }
            
            var filename = args[1];
            await screen.Api.DeleteHostedFile(filename);
        }
    }
}