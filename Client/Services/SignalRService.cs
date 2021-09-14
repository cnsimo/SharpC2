using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using SharpC2.API.V1.Responses;
using SharpC2.Models;

namespace SharpC2.Services
{
    public class SignalRService
    {
        private readonly CertificateService _certs;

        public SignalRService(CertificateService certs)
        {
            _certs = certs;
        }
        
        // Handlers
        public event Action<HandlerResponse> HandlerLoaded;
        public event Action<HandlerResponse> HandlerStarted;
        public event Action<HandlerResponse> HandlerStopped;
        
        // Hosted Files
        public event Action<string> HostedFileAdded;
        public event Action<string> HostedFileDeleted;
        
        //Drones
        public event Action<DroneMetadata> DroneCheckedIn;
        public event Action<string> DroneDeleted;
        public event Action<DroneMetadata, DroneModule> DroneModuleLoaded;
        public event Action<DroneMetadata, DroneTaskResponse> DroneTasked;
        public event Action<DroneMetadata, int> DroneDataSent;
        public event Action<DroneMetadata, DroneTaskUpdate> DroneTaskRunning;
        public event Action<DroneMetadata, DroneTaskUpdate> DroneTaskComplete;
        public event Action<DroneMetadata, DroneTaskUpdate> DroneTaskCancelled;
        public event Action<DroneMetadata, DroneTaskUpdate> DroneTaskAborted;

        public async Task Connect(string hostname, string port, string nick, string pass)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl($"https://{hostname}:{port}/MessageHub", o =>
                {
                    var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{nick}:{pass}"));
                    o.Headers.Add("Authorization", $"Basic {basic}");
                    o.HttpMessageHandlerFactory = handler => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = _certs.RemoteCertificateValidationCallback
                    };
                })
                .Build();

            
            await connection.StartAsync();

            connection.On<HandlerResponse>("HandlerLoaded", h => HandlerLoaded?.Invoke(h));
            connection.On<HandlerResponse>("HandlerStarted", h => HandlerStarted?.Invoke(h));
            connection.On<HandlerResponse>("HandlerStopped", h => HandlerStopped?.Invoke(h));
            
            connection.On<string>("HostedFileAdded", filename => HostedFileAdded?.Invoke(filename));
            connection.On<string>("HostedFileDeleted", filename => HostedFileDeleted?.Invoke(filename));

            connection.On<DroneMetadata>("DroneCheckedIn", d => DroneCheckedIn?.Invoke(d));
            connection.On<string>("DroneDeleted", d => DroneDeleted?.Invoke(d));
            connection.On<DroneMetadata, DroneModule>("DroneModuleLoaded", (d, m) => DroneModuleLoaded?.Invoke(d, m));
            connection.On<DroneMetadata, DroneTaskResponse>("DroneTasked", (d, t) => DroneTasked?.Invoke(d, t));
            connection.On<DroneMetadata, int>("DroneDataSent", (d, s) => DroneDataSent?.Invoke(d, s));
            connection.On<DroneMetadata, DroneTaskUpdate>("DroneTaskRunning", (r, t) => DroneTaskRunning?.Invoke(r, t));
            connection.On<DroneMetadata, DroneTaskUpdate>("DroneTaskComplete", (d, t) => DroneTaskComplete?.Invoke(d, t));
            connection.On<DroneMetadata, DroneTaskUpdate>("DroneTaskCancelled", (d, t) => DroneTaskCancelled?.Invoke(d, t));
            connection.On<DroneMetadata, DroneTaskUpdate>("DroneTaskAborted", (d, t) => DroneTaskAborted?.Invoke(d, t));
        }
    }
}