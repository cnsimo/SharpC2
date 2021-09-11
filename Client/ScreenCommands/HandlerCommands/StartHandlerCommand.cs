using System;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class StartHandlerCommand : ScreenCommand
    {
        public override string Name => "start";
        public override string Description => "Start the specified Handler";
        public override string Usage => "start [handler-name]";
        public override Screen.CommandCallback Callback => StartHandler;

        private readonly Screen _screen;

        public StartHandlerCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task StartHandler(string[] args)
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

            var handler = await screen.Api.StartHandler(handlerName);
            var index = screen.Handlers.IndexOf(existing);
            screen.Handlers[index] = handler;
        }
    }
}