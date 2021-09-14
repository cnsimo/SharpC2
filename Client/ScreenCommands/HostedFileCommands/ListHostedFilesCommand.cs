using System.Linq;
using System.Threading.Tasks;

using SharpC2.Models;
using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class ListHostedFilesCommand : ScreenCommand
    {
        public override string Name => "list";
        public override string Description => "List hosted files";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => ListHostedFiles;

        private readonly Screen _screen;

        public ListHostedFilesCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task ListHostedFiles(string[] args)
        {
            var screen = (HostedFilesScreen)_screen;
            var files = await screen.Api.GetHostedFiles();
            
            SharpSploitResultList<HostedFile> list = new();
            list.AddRange(files);

            if (!list.Any())
            {
                screen.Console.PrintWarning("No hosted files");
                return;
            }
            
            screen.Console.WriteLine("");
            screen.Console.WriteLine(list.ToString());
            screen.Console.WriteLine("");
        }
    }
}