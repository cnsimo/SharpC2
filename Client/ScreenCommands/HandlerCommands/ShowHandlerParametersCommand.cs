using System;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Models;
using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class ShowHandlerParametersCommand : ScreenCommand
    {
        public override string Name => "show";
        public override string Description => "Show Handler parameters";
        public override string Usage => "show [handler]";
        public override Screen.CommandCallback Callback => ShowHandlerParameters;

        private readonly Screen _screen;

        public ShowHandlerParametersCommand(Screen screen)
        {
            _screen = screen;
        }

        private Task ShowHandlerParameters(string[] args)
        {
            if (args.Length < 1)
            {
                _screen.Console.PrintError("Not enough arguments");
                return Task.CompletedTask;
            }
            
            var handlerName = args[1];
            var handler = ((HandlersScreen)_screen).Handlers.FirstOrDefault(h =>
                h.Name.Equals(handlerName, StringComparison.OrdinalIgnoreCase));

            if (handler is null)
            {
                _screen.Console.PrintError("Unknown Handler");
                return Task.CompletedTask;
            }

            SharpSploitResultList<HandlerParameter> list = new();
            list.AddRange(handler.Parameters);
            
            _screen.Console.WriteLine("");
            _screen.Console.WriteLine(list.ToString());
            _screen.Console.WriteLine("");

            return Task.CompletedTask;
        }
    }
}