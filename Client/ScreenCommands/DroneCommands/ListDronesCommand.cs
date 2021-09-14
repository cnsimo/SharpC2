using System.Linq;
using System.Threading.Tasks;

using SharpC2.Models;
using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class ListDronesCommand : ScreenCommand
    {
        public override string Name => "list";
        public override string Description => "List Drones";
        public override string Usage { get; }
        public override Screen.CommandCallback Callback => ListDrones;

        private readonly Screen _screen;

        public ListDronesCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task ListDrones(string[] args)
        {
            var screen = (DroneScreen)_screen;
            await screen.LoadDroneData();
            
            SharpSploitResultList<Drone> list = new();
            list.AddRange(screen.Drones);

            if (!list.Any())
            {
                _screen.Console.PrintWarning("No Drones");
                return;
            }
            
            _screen.Console.WriteLine("");
            _screen.Console.WriteLine(list.ToString());
            _screen.Console.WriteLine("");
        }
    }
}