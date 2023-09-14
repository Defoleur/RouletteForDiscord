using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Discord;
using Discord.Addons.Hosting;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.DB.Contexts;
using DiscordBot.DB.Repositories;
using DiscordBot.DB.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Victoria;
using Victoria.Node;
using Color = Discord.Color;

namespace DiscordBot
{
    public class Program
    {

        public static Task Main(string[] args) => new Program().RunAsync();

        public async Task RunAsync()
        {
            var discordConfig = new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 200,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.All,
            };
            var builder = new HostBuilder()
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = discordConfig;
                    config.Token = "your-token-here";
                })
                .UseCommandService((context, config) =>
                {
                    config = new CommandServiceConfig()
                    {
                        CaseSensitiveCommands = false,
                        LogLevel = LogSeverity.Verbose
                    };
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddHostedService<CommandHandler>()
                        .AddSingleton<CommandService>()
                        .AddSingleton<IPlayersContext, PlayersContext>()
                        .AddSingleton<IPlayerRepository, PlayerRepository>()
                        .AddSingleton<IBetRepository, BetRepository>()
                        .AddSingleton<IDbService, DbService>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}