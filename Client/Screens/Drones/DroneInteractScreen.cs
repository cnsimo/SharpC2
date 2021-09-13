using System;
using System.Collections.Generic;
using System.IO;
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
    public class DroneInteractScreen : Screen, IDisposable
    {
        public override string ScreenName { get; }
        
        private readonly ApiService _api;
        private readonly SignalRService _signalR;

        public Drone Drone { get; private set; }

        public DroneInteractScreen(string droneGuid, ApiService api, SignalRService signalR)
        {
            ScreenName = droneGuid;
            
            _api = api;
            _signalR = signalR;
            
            ClientCommands.Add(new BackScreenCommand(this));

            _signalR.DroneModuleLoaded += OnDroneModuleLoaded;
            _signalR.DroneTasked += OnDroneTasked;
            _signalR.DroneDataSent += OnDroneDataSent;
            _signalR.DroneTaskRunning += OnDroneTaskRunning;
            _signalR.DroneTaskComplete += OnDroneTaskComplete;
            _signalR.DroneTaskAborted += OnDroneTaskAborted;
            _signalR.DroneTaskCancelled += OnDroneTaskCancelled;

            LoadDroneData().GetAwaiter().GetResult();
        }

        private void AddModuleCommandsToScreen(IEnumerable<DroneModule.Command> commands)
        {
            foreach (var command in commands)
            {
                var droneCommand = new DroneCommand(
                    command.Name,
                    command.Description,
                    command.Usage,
                    ExecuteDroneCommand);

                var existing = ClientCommands.FirstOrDefault(c =>
                    c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                {
                    var index = ClientCommands.IndexOf(existing);
                    ClientCommands[index] = droneCommand;
                }
                else
                {
                    ClientCommands.Add(droneCommand);   
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

            return Task.FromResult<IReadOnlyList<CompletionItem>>(
                ClientCommands
                    .Where(command => command.Name.StartsWith(typedWord))
                    .Select(command => new CompletionItem
                    {
                        StartIndex = previousWordStart + 1,
                        ReplacementText = command.Name,
                        DisplayText = command.Name,
                        ExtendedDescription = new Lazy<Task<string>>(() => Task.FromResult($"{command.Description}{Environment.NewLine}{command.Usage}"))
                    })
                    .ToArray()
            );
        }

        private async Task ExecuteDroneCommand(string[] args)
        {
            // get the module from alias
            var module = Drone.Modules.FirstOrDefault(m =>
                m.Commands.Any(c =>
                    c.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase)));
        
            if (module is null)
            {
                Console.PrintError("Module not found");
                return;
            }
        
            var command = module.Commands.FirstOrDefault(c =>
                c.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
        
            if (command is null)
            {
                Console.PrintError("Unknown command");
                return;
            }
        
            // drop the alias
            args = args[1..].ToArray();
        
            var artefact = Array.Empty<byte>();
        
            // process all the args
            var commandArgs = command.Arguments?.ToArray();
        
            // first check the count to see if there are enough
            if (args.Length < commandArgs?.Where(a => !a.Optional).Count())
            {
                Console.PrintError("Not enough arguments");
                return;
            }
        
            string filePath = null;
        
            if (args.Length > 0)
            {
                for (var i = 0; i < commandArgs?.Where(a => !a.Optional).Count(); i++)
                {
                    // is the arg none-optional
                    if (commandArgs[i].Optional && string.IsNullOrEmpty(args[i]))
                    {
                        Console.PrintError($"{commandArgs[i].Label} is mandatory.");
                        return;
                    }
        
                    // is the arg a file
                    if (!commandArgs[i].Artefact) continue;
        
                    filePath = args[i];
                    if (!File.Exists(filePath))
                    {
                        Console.PrintError($"{filePath} not found.");
                        return;
                    }
        
                    var extension = Path.GetExtension(filePath);
                    
                    artefact = extension.Equals(".ps1")
                        ? Encoding.UTF8.GetBytes(await File.ReadAllTextAsync(filePath))
                        : await File.ReadAllBytesAsync(filePath);
                }
            }
        
            // remove filepath from args
            if (!string.IsNullOrEmpty(filePath))
                args = args.Where(s => !s.Equals(filePath)).ToArray();
        
            await _api.TaskDrone(ScreenName, module.Name, command.Name, args, artefact);
        }

        private void OnDroneModuleLoaded(DroneMetadata metadata, DroneModule module)
        {
            if (!ScreenName.Equals(metadata.Guid, StringComparison.OrdinalIgnoreCase)) return;

            var existing = Drone.Modules.FirstOrDefault(m =>
                m.Name.Equals(module.Name));

            if (existing is not null)
            {
                var index = Drone.Modules.IndexOf(existing);
                Drone.Modules[index] = module;
            }
            else
            {
                Drone.Modules.Add(module);    
            }
            
            AddModuleCommandsToScreen(module.Commands);
        }

        private void OnDroneTasked(DroneMetadata metadata, DroneTaskResponse task)
        {
            if (!ScreenName.Equals(metadata.Guid, StringComparison.OrdinalIgnoreCase)) return;
            Console.PrintMessage($"Drone tasked ({task.TaskGuid}) to run {task.Command}");
        }
        
        private void OnDroneDataSent(DroneMetadata metadata, int size)
        {
            if (!ScreenName.Equals(metadata.Guid, StringComparison.OrdinalIgnoreCase)) return;
            Console.PrintMessage($"Drone checked in. Sent {size} bytes.");
        }

        private void OnDroneTaskRunning(DroneMetadata metadata, DroneTaskUpdate task)
        {
            if (!ScreenName.Equals(metadata.Guid, StringComparison.OrdinalIgnoreCase)) return;
            if (task.Result is null || task.Result.Length == 0) return;
        
            var text = Encoding.UTF8.GetString(task.Result);
            Console.PrintMessage("Output received:");
            Console.WriteLine(text);
        }
        
        private void OnDroneTaskComplete(DroneMetadata metadata, DroneTaskUpdate task)
        {
            if (!ScreenName.Equals(metadata.Guid, StringComparison.OrdinalIgnoreCase)) return;
        
            if (task.Result is not null && task.Result.Length > 0)
            {
                var text = Encoding.UTF8.GetString(task.Result);
                Console.PrintMessage("Output received:");
                Console.WriteLine(text);
            }
        
            Console.PrintMessage("Task complete.");
        }
        
        private void OnDroneTaskAborted(DroneMetadata metadata, DroneTaskUpdate task)
        {
            if (!ScreenName.Equals(metadata.Guid, StringComparison.OrdinalIgnoreCase)) return;
            Console.PrintWarning("Task threw an exception.");
        
            if (task.Result is null || task.Result.Length <= 0) return;
        
            var text = Encoding.UTF8.GetString(task.Result);
            Console.PrintWarning(text);
        }
        
        private void OnDroneTaskCancelled(DroneMetadata metadata, DroneTaskUpdate task)
        {
            if (!ScreenName.Equals(metadata.Guid, StringComparison.OrdinalIgnoreCase)) return;
            Console.PrintWarning($"Task {task.TaskGuid} cancelled");
        }

        private async Task LoadDroneData()
        {
            Drone = await _api.GetDrone(ScreenName);

            foreach (var module in Drone.Modules)
                AddModuleCommandsToScreen(module.Commands);
        }

        public void Dispose()
        {
            _signalR.DroneModuleLoaded -= OnDroneModuleLoaded;
            _signalR.DroneTasked -= OnDroneTasked;
            _signalR.DroneDataSent -= OnDroneDataSent;
            _signalR.DroneTaskRunning -= OnDroneTaskRunning;
            _signalR.DroneTaskComplete -= OnDroneTaskComplete;
            _signalR.DroneTaskAborted -= OnDroneTaskAborted;
            _signalR.DroneTaskCancelled -= OnDroneTaskCancelled;
        }
    }
}