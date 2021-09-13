using System.Threading.Tasks;

using SharpC2.Models;
using SharpC2.Screens;

namespace SharpC2.ScreenCommands.PayloadCommands
{
    public class ShowPayloadOptionsCommand : ScreenCommand
    {
        public override string Name => "show";
        public override string Description => "Show payload options";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => ShowPayloadOptions;

        private readonly Screen _screen;

        public ShowPayloadOptionsCommand(Screen screen)
        {
            _screen = screen;
        }

        private Task ShowPayloadOptions(string[] args)
        {
            var payload = ((PayloadsScreen)_screen).Payload;
            SharpSploitResultList<Payload> list = new() { payload };

            _screen.Console.WriteLine("");
            _screen.Console.WriteLine(list.ToString());
            _screen.Console.WriteLine("");
            
            return Task.CompletedTask;
        }
    }
}