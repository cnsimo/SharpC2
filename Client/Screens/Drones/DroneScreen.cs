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
        public ApiService Api { get; }
        
        private readonly SignalRService _signalR;
        
        public List<Drone> Drones { get; } = new();

        public DroneScreen(ApiService api, SignalRService signalR, ScreenFactory screens)
        {
            Api = api;
            _signalR = signalR;
            ScreenFactory = screens;

            _signalR.DroneCheckedIn += OnDroneCheckIn;
            _signalR.DroneDeleted += OnDroneDeleted;
            
            ClientCommands.Add(new ListDronesCommand(this));
            ClientCommands.Add(new RemoveDroneCommand(this));
            ClientCommands.Add(new OpenDroneInteractScreen(this));

            ClientCommands.Add(new OpenHandlersScreenCommand(this));
            ClientCommands.Add(new OpenPayloadsScreenCommand(this));
            ClientCommands.Add(new OpenHostedFilesScreenCommand(this));
            ClientCommands.Add(new OpenCredentialsScreenCommand(this));
            
            ClientCommands.Add(new ExitClientCommand(this));
            
            LoadDroneData().GetAwaiter().GetResult();
        }

        public async Task LoadDroneData()
        {
            var drones = (await Api.GetDrones()).ToArray();

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

            if (previousWord.Equals("interact", StringComparison.OrdinalIgnoreCase) || 
                previousWord.Equals("remove", StringComparison.OrdinalIgnoreCase))
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
        
        private void OnDroneDeleted(string droneGuid)
        {
            Drones.RemoveAll(d => d.Metadata.Guid.Equals(droneGuid, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            _signalR.DroneCheckedIn -= OnDroneCheckIn;
            _signalR.DroneDeleted -= OnDroneDeleted;
        }
    }
}