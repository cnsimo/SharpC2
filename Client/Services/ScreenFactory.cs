using System;

using SharpC2.Screens;

namespace SharpC2.Services
{
    public class ScreenFactory
    {
        private readonly ApiService _apiService;
        private readonly SignalRService _signalR;

        public ScreenFactory(ApiService apiService, SignalRService signalR)
        {
            _apiService = apiService;
            _signalR = signalR;
        }

        public Screen GetScreen(ScreenType type, string name = null)
        {
            Screen screen = type switch
            {
                ScreenType.Drones => new DroneScreen(_apiService, _signalR, this),
                ScreenType.Handlers => new HandlersScreen(_apiService, _signalR, this),
                // ScreenType.HandlerConfig => new ConfigHandlerScreen(_apiService),
                ScreenType.DroneInteract => new DroneInteractScreen(name, _apiService, _signalR),
                ScreenType.Payloads => new PayloadsScreen(_apiService),
                // ScreenType.HostedFiles => new HostedFilesScreen(_apiService, _signalR),
                
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            return screen;
        }
        
        public enum ScreenType
        {
            Drones,
            Handlers,
            HandlerConfig,
            DroneInteract,
            Payloads,
            HostedFiles
        }
    }
}