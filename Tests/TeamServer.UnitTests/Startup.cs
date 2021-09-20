﻿using Microsoft.Extensions.DependencyInjection;

using TeamServer.Interfaces;
using TeamServer.Services;

namespace TeamServer.UnitTests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IHandlerService, HandlerService>();
            services.AddSingleton<ITaskService, TaskService>();
            services.AddSingleton<IDroneService, DroneService>();
            services.AddSingleton<IServerService, ServerService>();
            services.AddSingleton<ICredentialService, CredentialService>();

            services.AddSignalR();
        }
    }
}