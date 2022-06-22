using System.Linq;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Schenklklopfa
{
    public static class Utils
    {
        public static DiscordChannel GetVoiceChannelAMemberIsIn(CommandContext ctx, DiscordMember member) =>
            ctx.Member?.VoiceState.Channel;
    }
}