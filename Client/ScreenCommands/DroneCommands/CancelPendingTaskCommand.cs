using System;
using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class CancelPendingTaskCommand : ScreenCommand
    {
        public override string Name => "cancel";
        public override string Description => "Cancel a pending task before it's collected by the Drone";
        public override string Usage => "cancel [task-guid]";
        public override Screen.CommandCallback Callback => CancelPendingTask;

        private readonly Screen _screen;

        public CancelPendingTaskCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task CancelPendingTask(string[] args)
        {
            var screen = (DroneInteractScreen)_screen;
            
            if (args.Length < 2)
            {
                screen.Console.PrintError("Not enough arguments");
                return;
            }

            var taskGuid = args[1];

            try
            {
                await screen.Api.CancelPendingTask(screen.ScreenName, taskGuid);
            }
            catch (Exception e)
            {
                screen.Console.PrintWarning(e.Message);
            }
        }
    }
}