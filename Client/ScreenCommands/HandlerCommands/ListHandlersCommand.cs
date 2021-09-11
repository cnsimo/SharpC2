using System.Threading.Tasks;

using SharpC2.Models;
using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class ListHandlersCommand : ScreenCommand
    {
        public override string Name => "list";
        public override string Description => "List Handlers";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => ListHandlers;

        private readonly Screen _screen;

        public ListHandlersCommand(Screen screen)
        {
            _screen = screen;
        }

        private Task ListHandlers(string[] args)
        {
            var handlers = ((HandlersScreen)_screen).Handlers;

            SharpSploitResultList<Handler> list = new();
            list.AddRange(handlers);
            
            _screen.Console.WriteLine("");
            _screen.Console.WriteLine(list.ToString());
            _screen.Console.WriteLine("");
            
            return Task.CompletedTask;
        }
    }
}