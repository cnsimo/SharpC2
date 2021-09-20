using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class DeleteCredentialCommand : ScreenCommand
    {
        public override string Name => "delete";
        public override string Description => "Delete a credential record";
        public override string Usage => "delete [guid]";
        public override Screen.CommandCallback Callback => DeleteCredential;
        
        private readonly Screen _screen;

        public DeleteCredentialCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task DeleteCredential(string[] args)
        {
            var screen = (CredentialsScreen)_screen;

            if (args.Length < 2)
            {
                screen.Console.PrintError("Not enough arguments");
                return;
            }

            await screen.Api.DeleteCredential(args[1]);
        }
    }
}