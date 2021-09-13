using System;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Screens;
using SharpC2.Services;

namespace SharpC2.ScreenCommands
{
    public class OpenDroneInteractScreen : ScreenCommand
    {
        public override string Name => "interact";
        public override string Description => "Interact with the specified Drone";
        public override string Usage => "interact [guid]";
        public override Screen.CommandCallback Callback => Interact;

        private readonly Screen _screen;

        public OpenDroneInteractScreen(Screen screen)
        {
            _screen = screen;
        }

        private async Task Interact(string[] args)
        {
            var screen = (DroneScreen)_screen;
            var droneGuid = args[1];

            if (!screen.Drones.Any(d => d.Metadata.Guid.Equals(droneGuid, StringComparison.OrdinalIgnoreCase)))
            {
                screen.Console.PrintError("Unknown Drone");
                return;
            }

            using var interactScreen = (DroneInteractScreen) screen.ScreenFactory.GetScreen(ScreenFactory.ScreenType.DroneInteract, droneGuid);
            await interactScreen.Show();
        }
    }
}