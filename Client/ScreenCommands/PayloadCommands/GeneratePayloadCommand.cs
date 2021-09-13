using System;
using System.IO;
using System.Threading.Tasks;

using SharpC2.Screens;

namespace SharpC2.ScreenCommands
{
    public class GeneratePayloadCommand : ScreenCommand
    {
        public override string Name => "generate";
        public override string Description => "Generate payload";
        public override string Usage => "generate [/path/to/output]";
        public override Screen.CommandCallback Callback => GeneratePayload;

        private readonly Screen _screen;

        public GeneratePayloadCommand(Screen screen)
        {
            _screen = screen;
        }

        private async Task GeneratePayload(string[] args)
        {
            var path = args[1];
            var targetDirectory = Path.GetDirectoryName(path);
            if (!Directory.Exists(targetDirectory))
            {
                _screen.Console.PrintError("Target directory does not exist.");
                return;
            }
            
            var screen = (PayloadsScreen)_screen;
            
            if (string.IsNullOrEmpty(screen.Payload.Handler))
            {
                _screen.Console.PrintError("Please specify a Handler.");
                return;
            }
            
            var payload = await screen.Api.GeneratePayload(screen.Payload);
            
            try
            {
                await File.WriteAllBytesAsync(path, payload);
            }
            catch (Exception e)
            {
                _screen.Console.PrintError(e.Message);
                return;
            }
            
            screen.Console.PrintMessage($"Saved {payload.Length} bytes.");
        }
    }
}