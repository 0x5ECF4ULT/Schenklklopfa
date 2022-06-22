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

        public static bool EnsureCallingMemberIsInAVoiceChannel(CommandContext ctx) =>
            ctx.Member?.VoiceState.Channel != null;

        public static bool EnsureMemberIsInAVoiceChannel(DiscordMember member) =>
            member?.VoiceState.Channel != null;

        public static bool EnsureLavalinkIsConnected(CommandContext ctx) =>
            ctx.Client.GetLavalink().ConnectedNodes.Any();
    }
}