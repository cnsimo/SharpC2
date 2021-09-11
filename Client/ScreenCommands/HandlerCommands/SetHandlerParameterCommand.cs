using System;
using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class SetHandlerParameterCommand : ScreenCommand
    {
        public override string Name => "set";
        public override string Description => "Set a Handler parameter";
        public override string Usage => "set [handler] [parameter] [value]";
        public override Screen.CommandCallback Callback => SetHandlerParameter;

        private readonly Screen _screen;

        public SetHandlerParameterCommand(Screen screen)
        {
            _screen = screen;
        }

        private Task SetHandlerParameter(string[] args)
        {
            Console.WriteLine(args.Length);
            return Task.CompletedTask;
        }
    }
}