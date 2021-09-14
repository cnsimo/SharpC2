using System;

using SharpC2.ScreenCommands;
using SharpC2.Services;

namespace SharpC2.Screens
{
    public class HostedFilesScreen : Screen, IDisposable
    {
        public override string ScreenName => "hosted-files";
        
        public ApiService Api { get; }
        private readonly SignalRService _signalR;

        public HostedFilesScreen(ApiService api, SignalRService signalR)
        {
            Api = api;
            
            _signalR = signalR;
            
            ClientCommands.Add(new BackScreenCommand(this));
            ClientCommands.Add(new ListHostedFilesCommand(this));
            ClientCommands.Add(new AddHostedFileCommand(this));
            ClientCommands.Add(new DeleteHostedFileCommand(this));
            
            _signalR.HostedFileAdded += OnHostedFileAdded;
            _signalR.HostedFileDeleted += OnHostedFileDeleted;
        }

        private void OnHostedFileAdded(string filename)
            => Console.PrintMessage($"{filename} uploaded.");
        
        private void OnHostedFileDeleted(string filename)
            => Console.PrintMessage($"{filename} deleted.");

        public void Dispose()
        {
            _signalR.HostedFileAdded -= OnHostedFileAdded;
            _signalR.HostedFileDeleted -= OnHostedFileDeleted;
        }
    }
}