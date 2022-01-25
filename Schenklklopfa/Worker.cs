using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Schenklklopfa
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Initializing Schenklklopfa...");

            _logger.LogInformation("Setting up the bot...");
            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN") ??
                        await File.ReadAllTextAsync(
                            Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN_FILE") ??
                            throw new ArgumentException("Think: what is a bot without a token..."), stoppingToken),
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug
            });

            _logger.LogInformation("Such commands...");
            var cmdNext = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] {Environment.GetEnvironmentVariable("DISCORD_BOT_PREFIX") ?? "!"}
            });
            cmdNext.RegisterCommands<Commands>();

            _logger.LogInformation("Many music connection");
            var llEndpoint = new ConnectionEndpoint
            {
                Hostname = Environment.GetEnvironmentVariable("DISCORD_LAVALINK_HOST") ??
                           throw new ArgumentException("Where Lavalink?"),
                Port = 2333
            };
            var llConfig = new LavalinkConfiguration
            {
                Password = Environment.GetEnvironmentVariable("DISCORD_LAVALINK_PASSWORD") ??
                           await File.ReadAllTextAsync(
                               Environment.GetEnvironmentVariable("DISCORD_LAVALINK_PASSWORD_FILE") ??
                               throw new ArgumentException("Gimme your Lavalink password!"), stoppingToken),
                RestEndpoint = llEndpoint,
                SocketEndpoint = llEndpoint
            };
            var lavalink = discord.UseLavalink();

            _logger.LogInformation("TO THE MOOOOOON!");
            await discord.ConnectAsync();
            await lavalink.ConnectAsync(llConfig);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken); //don't make the CPU go brrr
            }

            _logger.LogInformation("Much cancelled. Goodbye!");
        }
    }
}