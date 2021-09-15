using System.Threading.Tasks;
using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class AddCredentialCommand : ScreenCommand
    {
        public override string Name => "add";
        public override string Description => "Add a credential record manually";
        public override string Usage => "add [username] [domain] [password] [source]";
        public override Screen.CommandCallback Callback => AddCredential;
        
        private readonly Screen _screen;

        public AddCredentialCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task AddCredential(string[] args)
        {
            var screen = (CredentialsScreen)_screen;
            
            if (args.Length < 4)
            {
                screen.Console.PrintError("Not enough arguments");
                return;
            }

            var username = "";
            var domain = "";
            var password = "";
            var source = "API";

            if (args.Length >= 4)
            {
                username = args[1];
                domain = args[2];
                password = args[3];
            }
            
            if (args.Length == 5)
            {
                source = args[4];
            }

            await screen.Api.AddCredential(username, domain, password, source);
        }
    }
}