using System.IO;
using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class AddHostedFileCommand : ScreenCommand
    {
        public override string Name => "add";
        public override string Description => "Add a hosted file";
        public override string Usage => "add [/path/to/file] [uri]";
        public override Screen.CommandCallback Callback => AddHostedFile;
        
        private readonly Screen _screen;

        public AddHostedFileCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task AddHostedFile(string[] args)
        {
            var screen = (HostedFilesScreen)_screen;
            
            if (args.Length < 2)
            {
                screen.Console.PrintError("Not enough arguments");
                return;
            }
            
            var path = args[1];
            var filename = args[2];
            
            if (!File.Exists(path))
            {
                screen.Console.PrintError($"{path} does not exist.");
                return;
            }

            var content = await File.ReadAllBytesAsync(path);
            await screen.Api.AddHostedFile(content, filename);
        }
    }
}