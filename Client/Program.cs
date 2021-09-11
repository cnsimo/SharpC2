using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CommandLine;

using Microsoft.Extensions.DependencyInjection;

using SharpC2.Screens;
using SharpC2.Services;

namespace SharpC2
{
    internal class Options
    {
        [Option('s', "server", Required = true, HelpText = "IP or hostname of Team Server.")]
        public string Server { get; set; }
        
        [Option('n', "nick", Required = true, HelpText = "Nickname to connect with.")]
        public string Nick { get; set; }
        
        [Option('p', "password", Required = true, HelpText = "Team Server's shared password.")]
        public string Password { get; set; }
    }

    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            PrintLogo();

            await Parser.Default.ParseArguments<Options>(args)
                .MapResult(RunOptions, HandleParseErrors);
        }

        private static async Task RunOptions(Options opts)
        {
            var sp = BuildServiceProvider();
            var api = sp.GetRequiredService<ApiService>();
            var factory = sp.GetRequiredService<ScreenFactory>();
            var signalR = sp.GetRequiredService<SignalRService>();

            api.InitClient(opts.Server, "8443", opts.Nick, opts.Password);

            // test authentication
            var handlers = await api.GetHandlers();

            if (!handlers.Any())
            {
                Console.WriteLine("[x] Authentication failed");
                return;
            }

            // connect to SignalR
            await signalR.Connect(opts.Server, "8443", opts.Nick, opts.Password);
            
            Console.Clear();

            using var droneScreen = (DroneScreen) factory.GetScreen(ScreenFactory.ScreenType.Drones);
            await droneScreen.Show();
        }

        private static void PrintLogo()
        {
            Console.WriteLine(@"  ___ _                   ___ ___ ");
            Console.WriteLine(@" / __| |_  __ _ _ _ _ __ / __|_  )");
            Console.WriteLine(@" \__ \ ' \/ _` | '_| '_ \ (__ / / ");
            Console.WriteLine(@" |___/_||_\__,_|_| | .__/\___/___|");
            Console.WriteLine(@"                   |_|            ");
            Console.WriteLine(@"    @_RastaMouse                  ");
            Console.WriteLine(@"    @_xpn_                        ");
            Console.WriteLine();
        }

        private static async Task HandleParseErrors(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
                await Console.Error.WriteLineAsync(err?.ToString());
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var sp = new ServiceCollection()
                .AddSingleton<CertificateService>()
                .AddSingleton<ApiService>()
                .AddSingleton<SignalRService>()
                .AddSingleton<ScreenFactory>()
                .AddAutoMapper(typeof(Program));

            return sp.BuildServiceProvider();
        }
    }
}