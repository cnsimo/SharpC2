using System.Linq;
using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class GetHelpCommand : ScreenCommand
    {
        public override string Name => "help";
        public override string Description => "Get the help text for a command";
        public override string Usage => "help [command]";
        public override Screen.CommandCallback Callback => GetHelp;

        private readonly Screen _screen;

        public GetHelpCommand(Screen screen)
        {
            _screen = screen;
        }

        private Task GetHelp(string[] args)
        {
            var screen = (DroneInteractScreen)_screen;
            
            switch (args.Length)
            {
                case 1: // just list commands
                {
                    SharpSploitResultList<ScreenCommand> commands = new();
                    commands.AddRange(screen.ClientCommands);
                
                    screen.Console.WriteLine("");
                    screen.Console.WriteLine(commands.ToString());
                    screen.Console.WriteLine("");
                    break;
                }
                
                case 2: // list the specified command
                {
                    var command = screen.ClientCommands.FirstOrDefault(c => c.Name.Equals(args[1]));
                    if (command is null)
                    {
                        screen.Console.PrintError("Unknown command");
                        return Task.CompletedTask;
                    }
                
                    screen.Console.WriteLine("");
                    screen.Console.WriteLine(command.Description);
                    screen.Console.WriteLine($"Usage: {command.Usage}");
                    screen.Console.WriteLine("");
                    break;
                }
            }

            return Task.CompletedTask;
        }
    }
}