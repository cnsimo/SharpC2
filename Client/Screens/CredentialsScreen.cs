using SharpC2.ScreenCommands;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class CredentialsScreen : Screen
    {
        public override string ScreenName => "credentials";
        
        public ApiService Api { get; }

        public CredentialsScreen(ApiService api)
        {
            Api = api;
            
            ClientCommands.Add(new BackScreenCommand(this));
            ClientCommands.Add(new ListCredentialsCommand(this));
            ClientCommands.Add(new AddCredentialCommand(this));
            ClientCommands.Add(new DeleteCredentialCommand(this));
        }
    }
}