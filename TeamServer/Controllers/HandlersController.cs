using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using SharpC2.API;
using SharpC2.API.V1.Requests;
using SharpC2.API.V1.Responses;

using TeamServer.Handlers;
using TeamServer.Hubs;
using TeamServer.Interfaces;

namespace TeamServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route(Routes.V1.Handlers)]
    public class HandlersController : ControllerBase
    {
        private readonly IHandlerService _handlers;
        private readonly IServerService _server;
        private readonly IHubContext<MessageHub, IMessageHub> _messageHub;
        private readonly IMapper _mapper;

        public HandlersController(IHandlerService handlerService, IServerService serverService,
            IHubContext<MessageHub, IMessageHub> messageHub, IMapper mapper)
        {
            _handlers = handlerService;
            _server = serverService;
            _messageHub = messageHub;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetHandlers()
        {
            var handlers = _handlers.GetHandlers();
            var response = _mapper.Map<IEnumerable<Handler>, IEnumerable<HandlerResponse>>(handlers);
            
            return Ok(response);
        }

        [HttpGet("{name}")]
        public IActionResult GetHandler(string name)
        {
            var handler = _handlers.GetHandler(name);
            if (handler is null) return NotFound();

            var response = _mapper.Map<Handler, HandlerResponse>(handler);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> LoadHandler([FromBody] LoadAssemblyRequest request)
        {
            var handler = _handlers.LoadHandler(request.Bytes);

            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Path.ToUriComponent()}";
            var path = $"{root}/{handler.Name}";

            await _messageHub.Clients.All.HandlerLoaded(handler.Name);

            return Created(path, handler);
        }

        [HttpPut("{name}")]
        public IActionResult SetHandlerParameters(string name, [FromBody] Dictionary<string, string> parameters)
        {
            var handler = _handlers.GetHandler(name);
            if (handler is null) return NotFound();
            
            handler.SetParameters(parameters);
            return NoContent();
        }

        [HttpPatch("{name}")]
        public IActionResult SetHandlerParameter(string name, [FromQuery] string key, [FromQuery] string value)
        {
            var handler = _handlers.GetHandler(name);
            if (handler is null) return NotFound();

            handler.SetParameter(key, value);
            return NoContent();
        }

        [HttpPatch("{name}/start")]
        public async Task<IActionResult> StartHandler(string name)
        {
            var handler = _handlers.GetHandler(name);

            if (handler is null) return NotFound();
            if (handler.Running) return BadRequest("Handler is already running");

            var task = handler.Start();

            if (task.IsFaulted) return BadRequest(task.Exception?.Message);
            await _messageHub.Clients.All.HandlerStarted(name);
            
            return NoContent();
        }

        [HttpPatch("{name}/stop")]
        public async Task<IActionResult> StopHandler(string name)
        {
            var handler = _handlers.GetHandler(name);

            if (handler is null)
                return NotFound();

            if (!handler.Running)
                return BadRequest("Handler is already stopped");
            
            handler.Stop();
            await _messageHub.Clients.All.HandlerStopped(name);
            
            return NoContent();
        }

        [HttpDelete("{name}")]
        public IActionResult RemoveHandler(string name)
        {
            var result = _handlers.RemoveHandler(name);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}