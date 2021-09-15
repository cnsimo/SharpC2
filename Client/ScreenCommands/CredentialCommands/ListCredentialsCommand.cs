using System.Linq;
using System.Threading.Tasks;

using SharpC2.Models;
using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class ListCredentialsCommand : ScreenCommand
    {
        public override string Name => "list";
        public override string Description => "List credentials";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => ListCredentials;

        private readonly Screen _screen;

        public ListCredentialsCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task ListCredentials(string[] args)
        {
            var screen = (CredentialsScreen)_screen;
            var credentials = await screen.Api.GetCredentials();
            
            SharpSploitResultList<CredentialRecord> list = new();
            list.AddRange(credentials);

            if (!list.Any())
            {
                screen.Console.PrintWarning("No credentials");
                return;
            }
            
            screen.Console.WriteLine("");
            screen.Console.WriteLine(list.ToString());
            screen.Console.WriteLine("");
        }
    }
}