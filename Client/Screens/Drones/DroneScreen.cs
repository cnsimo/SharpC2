using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrettyPrompt.Completion;
using SharpC2.Models;
using SharpC2.ScreenCommands;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class DroneScreen : Screen, IDisposable
    {
        public override string ScreenName => "drones";

        public ScreenFactory ScreenFactory { get; }
        
        private readonly ApiService _api;
        private readonly SignalRService _signalR;
        
        public List<Drone> Drones { get; } = new();

        public DroneScreen(ApiService api, SignalRService signalR, ScreenFactory screens)
        {
            _api = api;
            _signalR = signalR;
            ScreenFactory = screens;

            _signalR.DroneCheckedIn += OnDroneCheckIn;
            
            ClientCommands.Add(new ListDronesCommand(this));
            ClientCommands.Add(new OpenDroneInteractScreen(this));
            
            ClientCommands.Add(new OpenHandlersScreenCommand(this));
            ClientCommands.Add(new OpenPayloadsScreenCommand(this));
            
            ClientCommands.Add(new ExitClientCommand(this));
            
            LoadDroneData().GetAwaiter().GetResult();
        }

        public async Task LoadDroneData()
        {
            var drones = (await _api.GetDrones()).ToArray();

            foreach (var drone in drones)
            {
                var existing = Drones.FirstOrDefault(d =>
                    d.Metadata.Guid.Equals(drone.Metadata.Guid, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                {
                    var index = Drones.IndexOf(existing);
                    Drones[index] = drone;
                }
                else
                {
                    Drones.Add(drone);
                }
            }
        }

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

            if (previousWord.Equals("interact", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<IReadOnlyList<CompletionItem>>(
                    Drones
                        .Select(d => new CompletionItem
                        {
                            StartIndex = previousWordStart + 1,
                            ReplacementText = d.Metadata.Guid,
                            DisplayText = d.Metadata.Guid,
                            ExtendedDescription = new Lazy<Task<string>>(() => Task.FromResult(d.ToString()))
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

        // public override void AddCommands()
        // {
        //     Commands.Add(new ScreenCommand(name: "list", description: "List Drones", callback: ListDrones));
        //     Commands.Add(new ScreenCommand(name: "handlers", description: "Go to Handlers", callback: OpenHandlerScreen));
        //     Commands.Add(new ScreenCommand(name: "hosted-files", description: "Go to Hosted Files", callback: OpenHostedFilesScreen));
        //     Commands.Add(new ScreenCommand(name: "payloads", description: "Go to Payloads", callback: OpenPayloadsScreen));
        //     Commands.Add(new ScreenCommand(name: "interact", description: "Interact with the given Drone", usage: "interact <drone>", callback: DroneInteract));
        // }
        //
        // private async Task<bool> ListDrones(string[] args)
        // {
        //     var drones = await _api.GetDrones();
        //
        //     foreach (var drone in drones)
        //     {
        //         var existing = Drones.FirstOrDefault(d => d.Guid.Equals(drone.Guid, StringComparison.OrdinalIgnoreCase));
        //         if (existing is not null) Drones.Remove(existing);
        //         
        //         Drones.Add(drone);
        //     }
        //
        //     SharpSploitResultList<Drone> list = new();
        //     list.AddRange(Drones);
        //     Console.WriteLine(list.ToString());
        //
        //     return true;
        // }
        //
        // private async Task<bool> OpenHandlerScreen(string[] args)
        // {
        //     using var screen = _screens.GetScreen(ScreenType.Handlers);
        //     screen.SetName("handlers");
        //     await screen.LoadInitialData();
        //     screen.AddCommands();
        //     await screen.Show();
        //     
        //     // reset autocomplete
        //     ReadLine.AutoCompletionHandler = new DronesAutoComplete(this);
        //
        //     return true;
        // }
        //
        // private async Task<bool> OpenHostedFilesScreen(string[] args)
        // {
        //     using var screen = _screens.GetScreen(ScreenType.HostedFiles);
        //     screen.SetName("hosted-files");
        //     screen.AddCommands();
        //     await screen.Show();
        //     
        //     // reset autocomplete
        //     ReadLine.AutoCompletionHandler = new DronesAutoComplete(this);
        //
        //     return true;
        // }
        //
        // private async Task<bool> OpenPayloadsScreen(string[] args)
        // {
        //     using var screen = _screens.GetScreen(ScreenType.Payloads);
        //     screen.SetName("payloads");
        //     screen.AddCommands();
        //     await screen.LoadInitialData();
        //     await screen.Show();
        //     
        //     // reset autocomplete
        //     ReadLine.AutoCompletionHandler = new DronesAutoComplete(this);
        //
        //     return true;
        // }
        //
        // private async Task<bool> DroneInteract(string[] args)
        // {
        //     if (args.Length < 2) return false;
        //     var drone = args[1];
        //     if (string.IsNullOrEmpty(drone)) return false;
        //     
        //     using var screen = _screens.GetScreen(ScreenType.DroneInteract);
        //     screen.SetName(drone);
        //     await screen.LoadInitialData();
        //     screen.AddCommands();
        //     await screen.Show();
        //     
        //     // reset autocomplete
        //     ReadLine.AutoCompletionHandler = new DronesAutoComplete(this);
        //
        //     return true;
        // }
        
        private void OnDroneCheckIn(DroneMetadata metadata)
        {
            var existing = Drones.FirstOrDefault(d => d.Metadata.Guid.Equals(metadata.Guid));
            
            if (existing is not null)
            {
                existing.CheckIn();
                return;
            }
            
            Console.PrintMessage($"New Drone \"{metadata.Guid}\" checked in from {metadata.Hostname} as {metadata.Username}.");
            Drones.Add(new Drone(metadata));
        }

        public void Dispose()
        {
            _signalR.DroneCheckedIn -= OnDroneCheckIn;
        }
    }
}