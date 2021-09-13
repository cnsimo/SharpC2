using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PrettyPrompt;
using PrettyPrompt.Completion;

using SharpC2.Models;
using SharpC2.ScreenCommands;
using SharpC2.ScreenCommands.PayloadCommands;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class PayloadsScreen : Screen
    {
        public override string ScreenName => "payloads";
        
        public List<Handler> Handlers { get; } = new();
        public Payload Payload { get; } = new();
        public ApiService Api { get; }


        public PayloadsScreen(ApiService api)
        {
            Api = api;
            
            ClientCommands.Add(new BackScreenCommand(this));
            ClientCommands.Add(new ShowPayloadOptionsCommand(this));
            ClientCommands.Add(new SetPayloadOptionCommand(this));
            ClientCommands.Add(new GeneratePayloadCommand(this));
            
            LoadHandlerData().GetAwaiter().GetResult();
        }
        
        // public override void AddCommands()
        // {
        //     Commands.Add(new ScreenCommand("show", "Show payload options", ShowPayload));
        //     Commands.Add(new ScreenCommand("set", "Set a payload option", SetOption, "set <key> <value>"));
        //     Commands.Add(new ScreenCommand("generate", "Generate payload", GeneratePayload, "generate </output/path>"));
        //     
        //     ReadLine.AutoCompletionHandler = new PayloadsAutoComplete(this);
        // }

        private async Task LoadHandlerData()
        {
            var handlers = await Api.GetHandlers();
            Handlers.AddRange(handlers);
        }

        protected override Task<IReadOnlyList<CompletionItem>> FindCompletions(string input, int caret)
        {
            var textUntilCaret = input[..caret];
            var wordSplits = input[..caret].Split(new[] { ' ', '\n', '.', '(', ')' });
            var previousWordStart = textUntilCaret.LastIndexOfAny(new[] { ' ', '\n', '.', '(', ')' });
            
            var typedWord = previousWordStart == -1
                ? textUntilCaret.ToLower()
                : textUntilCaret[(previousWordStart + 1)..].ToLower();
            
            var previousWord = previousWordStart == -1
                ? ""
                : input[..previousWordStart];

            if (previousWord.Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<IReadOnlyList<CompletionItem>>(
                    new[]
                    {
                        new CompletionItem
                        {
                            StartIndex = previousWordStart + 1,
                            ReplacementText = "handler",
                            DisplayText = "handler",
                            ExtendedDescription =
                                new Lazy<Task<string>>(() => Task.FromResult("Set the payload Handler"))
                        },
                        new CompletionItem
                        {
                            StartIndex = previousWordStart + 1,
                            ReplacementText = "format",
                            DisplayText = "format",
                            ExtendedDescription =
                                new Lazy<Task<string>>(() => Task.FromResult("Set the payload format"))
                        }
                    });
            }

            if (previousWord.Equals("generate", StringComparison.OrdinalIgnoreCase))
            {
                var paths = Utilities.GetPartialPath(typedWord);
                
                return Task.FromResult<IReadOnlyList<CompletionItem>>(
                    paths
                        .Select(path => new CompletionItem
                        {
                            StartIndex = previousWordStart + 1,
                            ReplacementText = path,
                            DisplayText = path,
                            ExtendedDescription = new Lazy<Task<string>>(() => Task.FromResult(Payload.ToString()))
                        })
                        .ToArray()
                );
            }

            if (wordSplits.Length >= 2)
            {
                var split = previousWord.Split(" ");
                if (split[1].Equals("handler", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult<IReadOnlyList<CompletionItem>>(
                        Handlers
                            .Select(h => new CompletionItem
                            {
                                StartIndex = previousWordStart + 1,
                                ReplacementText = h.Name,
                                DisplayText = h.Name,
                                ExtendedDescription = new Lazy<Task<string>>(() => Task.FromResult(h.ToString()))
                            })
                            .ToArray()
                    );
                }
                
                if (split[1].Equals("format", StringComparison.OrdinalIgnoreCase))
                {
                    var formats = Enum.GetValues(typeof(Payload.PayloadFormat)).Cast<Payload.PayloadFormat>();
                    
                    return Task.FromResult<IReadOnlyList<CompletionItem>>(
                        formats
                            .Select(f => new CompletionItem
                            {
                                StartIndex = previousWordStart + 1,
                                ReplacementText = f.ToString(),
                                DisplayText = f.ToString(),
                                ExtendedDescription = new Lazy<Task<string>>(() => Task.FromResult(""))
                            })
                            .ToArray()
                    );
                }
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

        // private Task<bool> SetOption(string[] args)
        // {
        //     if (args.Length < 3) return Task.FromResult(false);
        //     
        //     var key = args[1];
        //     var value = args[2];
        //
        //     if (key.Equals("handler", StringComparison.OrdinalIgnoreCase))
        //         _payload.Handler = value;
        //     
        //     if (key.Equals("format", StringComparison.OrdinalIgnoreCase))
        //         _payload.Format = FormatFromString(value);
        //
        //     return Task.FromResult(true);
        // }
        //
        // private Task<bool> ShowPayload(string[] args)
        // {
        //     SharpSploitResultList<Payload> list = new() { _payload };
        //     Console.WriteLine(list.ToString());
        //     
        //     return Task.FromResult(true);
        // }

        
    }
}