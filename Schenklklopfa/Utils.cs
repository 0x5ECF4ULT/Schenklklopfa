using System.Linq;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Schenklklopfa
{
    public static class Utils
    {
        public static DiscordChannel GetVoiceChannelAMemberIsIn(CommandContext ctx, DiscordMember member) =>
            ctx.Guild.Channels
                .Single(pair => Ensurer.EnsureChannelIsAVoiceChannel(pair.Value) && pair.Value.Users.Contains(member))
                .Value;
    }
}