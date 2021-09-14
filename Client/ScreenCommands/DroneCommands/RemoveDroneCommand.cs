using System;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class RemoveDroneCommand : ScreenCommand
    {
        public override string Name => "remove";
        public override string Description => "Remove an inactive Drone";
        public override string Usage => "remove [drone]";
        public override Screen.CommandCallback Callback => RemoveDrone;

        private readonly Screen _screen;

        public RemoveDroneCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task RemoveDrone(string[] args)
        {
            var screen = (DroneScreen)_screen;
            var droneGuid = args[1];

            if (!screen.Drones.Any(d => d.Metadata.Guid.Equals(droneGuid, StringComparison.OrdinalIgnoreCase)))
            {
                screen.Console.PrintError("Unknown Drone Guid.");
                return;
            }

            await screen.Api.DeleteDrone(droneGuid);
        }
    }
}