using System;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class StopHandlerCommand : ScreenCommand
    {
        public override string Name => "stop";
        public override string Description => "Stop the specified Handler";
        public override string Usage => "stop [handler]";
        public override Screen.CommandCallback Callback => StopHandler;

        private readonly Screen _screen;

        public StopHandlerCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task StopHandler(string[] args)
        {
            var screen = (HandlersScreen)_screen;
            
            if (args.Length < 2)
            {
                _screen.Console.PrintError("Handler name required");
                return;
            }

            var handlerName = args[1];
            var existing = screen.Handlers.FirstOrDefault(h =>
                h.Name.Equals(handlerName, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                _screen.Console.PrintError("Unknown Handler name");
                return;
            }

            var handler = await screen.Api.StopHandler(handlerName);
            var index = screen.Handlers.IndexOf(existing);
            screen.Handlers[index] = handler;
        }
    }
}