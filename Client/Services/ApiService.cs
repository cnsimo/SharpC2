using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using AutoMapper;

using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

using SharpC2.API;
using SharpC2.API.V1.Requests;
using SharpC2.API.V1.Responses;
using SharpC2.Models;

namespace SharpC2.Services
{
    public class ApiService
    {
        private readonly IMapper _mapper;
        private readonly CertificateService _certs;
        private RestClient _client;
        
        public ApiService(IMapper mapper, CertificateService certs)
        {
            _mapper = mapper;
            _certs = certs;
        }
        
        public void InitClient(string hostname, string port, string nick, string pass)
        {
            _client = new RestClient($"https://{hostname}:{port}")
            {
                RemoteCertificateValidationCallback = _certs.RemoteCertificateValidationCallback
            };

            _client.UseNewtonsoftJson();
            _client.AddDefaultHeader("Content-Type", "application/json");

            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{nick}:{pass}"));
            _client.AddDefaultHeader("Authorization", $"Basic {basic}");
        }

        public async Task<IEnumerable<Handler>> GetHandlers()
        {
            var request = new RestRequest(Routes.V1.Handlers, Method.GET);
            var response = await _client.ExecuteAsync<IEnumerable<HandlerResponse>>(request);
            
            return _mapper.Map<IEnumerable<HandlerResponse>, IEnumerable<Handler>>(response.Data);
        }

        public async Task<Handler> GetHandler(string name)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}", Method.GET);
            var response = await _client.ExecuteAsync<HandlerResponse>(request);

            return _mapper.Map<HandlerResponse, Handler>(response.Data);
        }

        public async Task<Handler> LoadHandler(byte[] handler)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}", Method.POST);
            var asm = new LoadAssemblyRequest { Bytes = handler };
            request.AddParameter("application/json", JsonSerializer.Serialize(asm), ParameterType.RequestBody);

            var response = await _client.ExecuteAsync<HandlerResponse>(request);
            return _mapper.Map<HandlerResponse, Handler>(response.Data);
        }

        public async Task<Handler> SetHandlerParameter(string name, string key, string value)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}?key={key}&value={value}", Method.PATCH);
            var response = await _client.ExecuteAsync<HandlerResponse>(request);
            
            return _mapper.Map<HandlerResponse, Handler>(response.Data);
        }

        public async Task<Handler> StartHandler(string name)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}/start", Method.PATCH);
            var response = await _client.ExecuteAsync<HandlerResponse>(request);
            
            return _mapper.Map<HandlerResponse, Handler>(response.Data);
        }

        public async Task<Handler> StopHandler(string name)
        {
            var request = new RestRequest($"{Routes.V1.Handlers}/{name}/stop", Method.PATCH);
            var response = await _client.ExecuteAsync<HandlerResponse>(request);
            
            return _mapper.Map<HandlerResponse, Handler>(response.Data);
        }

        public async Task<byte[]> GeneratePayload(Payload payload)
        {
            var request = new RestRequest($"{Routes.V1.Payloads}/{payload.Handler}/{payload.Format}", Method.GET);
            var response = await _client.ExecuteAsync<PayloadResponse>(request);

            return response.Data.Bytes;
        }

        public async Task<IEnumerable<Drone>> GetDrones()
        {
            var request = new RestRequest(Routes.V1.Drones, Method.GET);
            var response = await _client.ExecuteAsync<IEnumerable<DroneResponse>>(request);

            return _mapper.Map<IEnumerable<DroneResponse>, IEnumerable<Drone>>(response.Data);
        }

        public async Task<Drone> GetDrone(string guid)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{guid}", Method.GET);
            var response = await _client.ExecuteAsync<DroneResponse>(request);

            return _mapper.Map<DroneResponse, Drone>(response.Data);
        }

        public async Task TaskDrone(string guid, string module, string command, string[] args, byte[] artefact)
        {
            var task = new DroneTaskRequest
            {
                Module = module,
                Command = command,
                Arguments = args,
                Artefact = artefact
            };
            
            var request = new RestRequest($"{Routes.V1.Drones}/{guid}/tasks", Method.POST);
            request.AddParameter("application/json", JsonSerializer.Serialize(task), ParameterType.RequestBody);

            await _client.ExecuteAsync(request);
        }

        public async Task<IEnumerable<DroneTask>> GetDroneTasks(string droneGuid)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{droneGuid}/tasks", Method.GET);
            var response = await _client.ExecuteAsync<IEnumerable<DroneTaskResponse>>(request);

            return _mapper.Map<IEnumerable<DroneTaskResponse>, IEnumerable<DroneTask>>(response.Data);
        }

        public async Task<DroneTask> GetDroneTask(string droneGuid, string taskGuid)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{droneGuid}/tasks/{taskGuid}", Method.GET);
            var response = await _client.ExecuteAsync<DroneTaskResponse>(request);

            return _mapper.Map<DroneTaskResponse, DroneTask>(response.Data);
        }

        public async Task CancelPendingTask(string droneGuid, string taskGuid)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{droneGuid}/tasks/{taskGuid}", Method.DELETE);
            var response = await _client.ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new Exception(response.Content);
        }

        public async Task DeleteDrone(string droneGuid)
        {
            var request = new RestRequest($"{Routes.V1.Drones}/{droneGuid}", Method.DELETE);
            await _client.ExecuteAsync(request);
        }

        public async Task<IEnumerable<HostedFile>> GetHostedFiles()
        {
            var request = new RestRequest(Routes.V1.HostedFiles, Method.GET);
            var response = await _client.ExecuteAsync<IEnumerable<HostedFileResponse>>(request);

            return _mapper.Map<IEnumerable<HostedFileResponse>, IEnumerable<HostedFile>>(response.Data);
        }

        public async Task AddHostedFile(byte[] content, string filename)
        {
            var fileRequest = new AddHostedFileRequest
            {
                Content = content,
                Filename = filename
            };
            
            var request = new RestRequest(Routes.V1.HostedFiles, Method.POST);
            request.AddParameter("application/json", JsonSerializer.Serialize(fileRequest), ParameterType.RequestBody);
            
            await _client.ExecuteAsync(request);
        }

        public async Task DeleteHostedFile(string filename)
        {
            var request = new RestRequest($"{Routes.V1.HostedFiles}/{filename}", Method.DELETE);
            await _client.ExecuteAsync(request);
        }
    }
}