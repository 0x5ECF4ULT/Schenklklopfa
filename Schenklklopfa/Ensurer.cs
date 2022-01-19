using System;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Schenklklopfa
{
    public static class Ensurer
    {
        public static bool EnsureChannelIsAVoiceChannel(DiscordChannel channel) => channel.Type == ChannelType.Voice;

        public static bool EnsureMemberIsInAVoiceChannel(CommandContext ctx, DiscordMember member) =>
            ctx.Guild.Channels
                .Any(pair => EnsureChannelIsAVoiceChannel(pair.Value) && pair.Value.Users.Contains(member));

        public static bool EnsureLavalinkIsConnected(CommandContext ctx) =>
            ctx.Client.GetLavalink().ConnectedNodes.Any();
    }
}