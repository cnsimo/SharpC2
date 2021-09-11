using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrettyPrompt.Completion;
using SharpC2.API.V1.Responses;
using SharpC2.Models;
using SharpC2.ScreenCommands;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class HandlersScreen : Screen, IDisposable
    {
        public override string ScreenName => "handlers";
        
        public ApiService Api { get; }
        
        private readonly SignalRService _signalR;
        private readonly ScreenFactory _screens;

        public List<Handler> Handlers { get; } = new();

        public HandlersScreen(ApiService api, SignalRService signalR, ScreenFactory screens)
        {
            Api = api;
            
            _screens = screens;
            _signalR = signalR;

            _signalR.HandlerLoaded += OnHandlerLoaded;
            _signalR.HandlerStarted += OnHandlerStarted;
            _signalR.HandlerStopped += OnHandlerStopped;
            
            ClientCommands.Add(new BackScreenCommand(this));
            ClientCommands.Add(new ListHandlersCommand(this));
            ClientCommands.Add(new StartHandlerCommand(this));
            
            LoadHandlerData().GetAwaiter().GetResult();
        }

        private async Task LoadHandlerData()
        {
            var handlers = await Api.GetHandlers();
            Handlers.AddRange(handlers);
        }

        // public override void AddCommands()
        // {
        //     Commands.Add(new ScreenCommand("load", "Load a Handler DLL", LoadHandler, "load </path/to/handler.dll"));
        //     Commands.Add(new ScreenCommand("list", "List Handlers", ListHandlers));
        //     Commands.Add(new ScreenCommand("config", "Configure the given Handler", ConfigHandler, "config <handler>"));
        //     Commands.Add(new ScreenCommand("start", "Start the given Handler", StartHandler, "start <handler>"));
        //     Commands.Add(new ScreenCommand("stop", "Stop the given Handler", StopHandler, "stop <handler>"));
        // }

        protected override Task<IReadOnlyList<CompletionItem>> FindCompletions(string input, int caret)
        {
            var textUntilCaret = input[..caret];
            var previousWordStart = textUntilCaret.LastIndexOfAny(new[] { ' ', '\n', '.', '(', ')' });
            var typedWord = previousWordStart == -1
                ? textUntilCaret.ToLower()
                : textUntilCaret[(previousWordStart + 1)..].ToLower();
            var previousWord = previousWordStart == -1
                ? ""
                : input[..previousWordStart];

            if (previousWord.Equals("start", StringComparison.OrdinalIgnoreCase) ||
                previousWord.Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<IReadOnlyList<CompletionItem>>(
                    Handlers
                        .Select(handler => new CompletionItem
                        {
                            StartIndex = previousWordStart + 1,
                            ReplacementText = handler.Name,
                            DisplayText = $"{previousWord} {handler.Name}",
                            ExtendedDescription = new Lazy<Task<string>>(() => Task.FromResult(GetHandlerParametersAsString(handler.Parameters)))
                        })
                        .ToArray()
                );
            }

            return Task.FromResult<IReadOnlyList<CompletionItem>>(
                ClientCommands
                    .Where(command => command.Name.StartsWith(typedWord))
                    .Select(command => new CompletionItem
                    {
                        StartIndex = previousWordStart + 1,
                        ReplacementText = command.Name,
                        DisplayText = command.Name,
                        ExtendedDescription = new Lazy<Task<string>>(() => Task.FromResult(command.Description))
                    })
                    .ToArray()
            );
        }

        private string GetHandlerParametersAsString(IEnumerable<HandlerParameter> parameters)
        {
            var sb = new StringBuilder();

            foreach (var parameter in parameters)
                sb.AppendLine($"{parameter.Name} : {parameter.Value}");

            return sb.ToString();
        }

        private void OnHandlerLoaded(HandlerResponse handler)
            => Console.PrintMessage($"Handler \"{handler.Name}\" loaded");

        private void OnHandlerStarted(HandlerResponse handler)
            => Console.PrintMessage($"Handler \"{handler.Name}\" started.");

        private void OnHandlerStopped(HandlerResponse handler)
            => Console.PrintMessage($"Handler \"{handler.Name}\" stopped.");

        // private async Task<bool> LoadHandler(string[] args)
        // {
        //     if (args.Length < 2){
        //         Console.WriteError("Specify a handler: load <handler>");
        //         return false;
        //     }
        //     var path = args[1];
        //     
        //     if (!File.Exists(path))
        //     {
        //         Console.WriteError($"{path} not found");
        //         return false;
        //     }
        //
        //     var bytes = await File.ReadAllBytesAsync(path);
        //     await _api.LoadHandler(bytes);
        //     return true;
        // }
        //
        // private async Task<bool> ListHandlers(string[] args)
        // {
        //     Handlers = (await _api.GetHandlers()).ToArray();
        //
        //     SharpSploitResultList<Handler> handlers = new();
        //     handlers.AddRange(Handlers);
        //     Console.WriteLine(handlers.ToString());
        //
        //     return true;
        // }
        //
        // private async Task<bool> ConfigHandler(string[] args)
        // {
        //     if (args.Length < 2){
        //         Console.WriteError("Specify a handler: config <handler>");
        //         return false;
        //     }
        //     var handler = args[1];
        //     using var screen = _screens.GetScreen(ScreenType.HandlerConfig);
        //     screen.SetName(handler);
        //     await screen.LoadInitialData();
        //     screen.AddCommands();
        //     await screen.Show();
        //     
        //     // reset autocomplete here
        //     ReadLine.AutoCompletionHandler = new HandlersAutoComplete(this);
        //
        //     return true;
        // }
        //
        // private async Task<bool> StartHandler(string[] args)
        // {
        //     if (args.Length < 2){
        //         CustomConsole.WriteError("Specify a handler: start <handler>");
        //         return false;
        //     }
        //     var handler = args[1];
        //     await _api.StartHandler(handler);
        //     
        //     return true;
        // }
        //
        // private async Task<bool> StopHandler(string[] args)
        // {
        //     if (args.Length < 2){
        //         CustomConsole.WriteError("Specify a handler: stop <handler>");
        //         return false;
        //     }
        //     var handler = args[1];
        //     await _api.StopHandler(handler);
        //     
        //     return true;
        // }

        public void Dispose()
        {
            _signalR.HandlerLoaded -= OnHandlerLoaded;
            _signalR.HandlerStarted -= OnHandlerStarted;
            _signalR.HandlerStopped -= OnHandlerStopped;
        }
    }
}