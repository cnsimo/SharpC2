using System;
using System.Linq;
using System.Threading.Tasks;

using SharpC2.Interfaces;
using SharpC2.Models;

namespace SharpC2.Screens
{
    public class ConfigHandlerScreen : Screen
    {
        private readonly IApiService _api;

        public Handler Handler { get; private set; }
        
        public override void AddCommands()
        {
            Commands.Add(new ScreenCommand(name: "show", description: "Show parameters", callback: ShowParameters));
            Commands.Add(new ScreenCommand(name: "set", description: "Set a parameter", callback: SetParameter));
            Commands.Add(new ScreenCommand(name: "start", description: "Start this Handler", callback: StartHandler));
            Commands.Add(new ScreenCommand(name: "stop", description: "Stop this Handler", callback: StopHandler));
            
            ReadLine.AutoCompletionHandler = new ConfigHandlerAutoComplete(this);
        }

        public ConfigHandlerScreen(IApiService api)
        {
            _api = api;
        }

        public override async Task LoadInitialData()
        {
            if (string.IsNullOrEmpty(ScreenName)) return;
            Handler = await _api.GetHandler(ScreenName);
        }

        private async Task<bool> ShowParameters(string[] args)
        {
            Handler = await _api.GetHandler(ScreenName);

            SharpSploitResultList<HandlerParameter> parameters = new();
            parameters.AddRange(Handler.Parameters);
            Console.WriteLine(parameters.ToString());
            
            return true;
        }
        
        private async Task<bool> SetParameter(string[] args)
        {
            if (args.Length < 3) return false;

            var key = args[1];
            var value = args[2];

            var parameter = Handler.Parameters.FirstOrDefault(p =>
                p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (parameter is null)
            {
                CustomConsole.WriteError("Unknown parameter");
                return false;
            }

            parameter.SetValue(value);

            await _api.SetHandlerParameter(ScreenName, key, value);
            return true;
        }
        
        private async Task<bool> StartHandler(string[] args)
        {
            await _api.StartHandler(ScreenName);
            return true;
        }
        
        private async Task<bool> StopHandler(string[] args)
        {
            await _api.StopHandler(ScreenName);
            return true;
        }
    }
    
    public class ConfigHandlerAutoComplete : AutoCompleteHandler
    {
        private readonly ConfigHandlerScreen _screen;

        public ConfigHandlerAutoComplete(ConfigHandlerScreen screen)
        {
            _screen = screen;
        }

        public override string[] GetSuggestions(string text, int index)
        {
            var commands = _screen.Commands.Select(c => c.Name).ToArray();
            var split = text.Split(' ');

            if (split.Length == 1)
            {
                return string.IsNullOrEmpty(split[0])
                    ? commands
                    : commands.Where(c => c.StartsWith(split[0])).ToArray();
            }

            if (split.Length == 2)
            {
                return split[0].StartsWith("help")
                    ? commands
                    : _screen.Handler.Parameters.Select(p => p.Name).ToArray();
            }

            return Array.Empty<string>();
        }
    }
}