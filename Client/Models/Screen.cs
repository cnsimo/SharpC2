using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PrettyPrompt;
using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;

using SharpC2.ScreenCommands;

namespace SharpC2.Screens
{
    public abstract class Screen
    {
        public abstract string ScreenName { get; }
        public bool ScreenRunning { get; set; } = true;
        public List<ScreenCommand> ClientCommands { get; } = new();

        public delegate Task CommandCallback(string[] args);

        public IConsole Console { get; protected set; }
        public IPrompt Prompt { get; protected set; }

        protected Screen()
        {
            Console = new SystemConsole();
        }

        public async Task Show()
        {
            Prompt = new Prompt(null, new PromptCallbacks
            {
                CompletionCallback = FindCompletions,
                KeyPressCallbacks = { [ConsoleKey.Tab] = KeyPressCallback }
            });

            while (ScreenRunning)
            {
                var response = await Prompt.ReadLineAsync($"[{ScreenName}] > ");

                if (!response.IsSuccess) continue;
                if (string.IsNullOrEmpty(response.Text)) continue;

                var args = response.Text.Split(" ");

                var command = ClientCommands.FirstOrDefault(c =>
                    c.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));

                if (command is null)
                {
                    Console.WriteError("Unknown command");
                    continue;
                }

                await command.Callback(args);
            }
        }

        protected virtual Task<KeyPressCallbackResult> KeyPressCallback(string text, int caret)
        {
            return Task.FromResult<KeyPressCallbackResult>(null);
        }

        protected virtual Task<IReadOnlyList<CompletionItem>> FindCompletions(string input, int caret)
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
                        ExtendedDescription = new Lazy<Task<string>>(() => Task.FromResult(command.Description))
                    })
                    .ToArray()
            );
        }
    }
}