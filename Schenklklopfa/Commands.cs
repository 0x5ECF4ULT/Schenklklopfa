using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using static Schenklklopfa.Ensurer;
using static Schenklklopfa.Utils;

namespace Schenklklopfa
{
    public class Commands : BaseCommandModule
    {
        private readonly CancellationTokenSource _cts = new();

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync("Bonk!");
        }

        [Command("p"), Aliases("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string urlOrSearchString)
        {
            if (!EnsureLavalinkIsConnected(ctx)) //check if Lavalink is connected
            {
                await ctx.RespondAsync("Sorry! I had an internal error.");
                return;
            }

            if (!EnsureMemberIsInAVoiceChannel(ctx, ctx.Member)) //check if the calling member is in a voice channel
            {
                await ctx.RespondAsync("You're not in a voice channel!");
                return;
            }

            if (EnsureMemberIsInAVoiceChannel(ctx,
                ctx.Guild.CurrentMember)) //check if the bot is already in a voice channel
                if (GetVoiceChannelAMemberIsIn(ctx, ctx.Member) !=
                    GetVoiceChannelAMemberIsIn(ctx,
                        ctx.Guild.CurrentMember)) //is it the same as the calling member is in?
                {
                    await ctx.RespondAsync($"I'm already in a voice channel. Try `{ctx.Prefix}leave` :)");
                    return;
                }

            if (string.IsNullOrWhiteSpace(urlOrSearchString))
            {
                await ctx.RespondAsync("Give me something to play?!?!");
                return;
            }

            var llNodeConnection = ctx.Client.GetLavalink().ConnectedNodes.Values.First();

            Uri.TryCreate(urlOrSearchString, UriKind.Absolute,
                out var trackUri); //try to parse the shit out of the passed string
            var res = trackUri != null
                ? llNodeConnection.Rest.GetTracksAsync(trackUri)
                : llNodeConnection.Rest.GetTracksAsync(urlOrSearchString);

            Task<LavalinkGuildConnection> llGuildConnection;
            if (!EnsureMemberIsInAVoiceChannel(ctx, ctx.Guild.CurrentMember) ||
                GetVoiceChannelAMemberIsIn(ctx, ctx.Member) !=
                GetVoiceChannelAMemberIsIn(ctx,
                    ctx.Guild.CurrentMember)) //is the bot in the same channel as the calling member is in?
                llGuildConnection = llNodeConnection.ConnectAsync(GetVoiceChannelAMemberIsIn(ctx,
                    ctx.Member)); //if no connect to the channel
            else
                llGuildConnection = Task.FromResult(llNodeConnection.GetGuildConnection(ctx.Guild));

            //llNodeConnection.PlaybackFinished += OnPlaybackFinishedHandler; //TODO: checks. gets added on every !p

            await res;
            if (res.Result.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"No tracks found for search term: \"{urlOrSearchString}\"");
                return;
            }

            var track = res.Result.Tracks.First();
            await ctx.RespondAsync($"Playing or enqueueing title \"{track.Title}\"");
            await (await llGuildConnection).PlayAsync(track);
        }

        [Command("pause"), Aliases("stop")]
        public async Task Pause(CommandContext ctx)
        {
            if (!EnsureLavalinkIsConnected(ctx)) //check if Lavalink is connected
            {
                await ctx.RespondAsync("Sorry! I had an internal error.");
                return;
            }

            if (!EnsureMemberIsInAVoiceChannel(ctx, ctx.Member)) //check if the calling member is in a voice channel
            {
                await ctx.RespondAsync("You're not in a voice channel!");
                return;
            }

            if (!EnsureMemberIsInAVoiceChannel(ctx, ctx.Guild.CurrentMember)) //check if the bot is in a voice channel
            {
                await ctx.RespondAsync("I'm not in any voice channel!");
                return;
            }

            var llNodeConnection = ctx.Client.GetLavalink().ConnectedNodes.Values.First();
            var llChannelConnection = llNodeConnection.GetGuildConnection(ctx.Guild);
            if (llChannelConnection == null)
            {
                await ctx.RespondAsync("I'm not playing any music!");
                return;
            }

            if (llChannelConnection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("No tracks are loaded!");
                return;
            }

            await llChannelConnection.PauseAsync();
            await ctx.RespondAsync("Paused.");
        }

        [Command("resume")]
        public async Task Resume(CommandContext ctx)
        {
            if (!EnsureLavalinkIsConnected(ctx)) //check if Lavalink is connected
            {
                await ctx.RespondAsync("Sorry! I had an internal error.");
                return;
            }

            if (!EnsureMemberIsInAVoiceChannel(ctx, ctx.Member)) //check if the calling member is in a voice channel
            {
                await ctx.RespondAsync("You're not in a voice channel!");
                return;
            }

            if (!EnsureMemberIsInAVoiceChannel(ctx, ctx.Guild.CurrentMember)) //check if the bot is in a voice channel
            {
                await ctx.RespondAsync("I'm not in any voice channel!");
                return;
            }

            var llNodeConnection = ctx.Client.GetLavalink().ConnectedNodes.Values.First();
            var llChannelConnection = llNodeConnection.GetGuildConnection(ctx.Guild);
            if (llChannelConnection == null)
            {
                await ctx.RespondAsync("I'm not playing any music!");
                return;
            }

            if (llChannelConnection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("No tracks are loaded!");
                return;
            }

            await llChannelConnection.ResumeAsync();
            await ctx.RespondAsync("Resumed.");
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            if (!EnsureLavalinkIsConnected(ctx)) //check if Lavalink is connected
            {
                await ctx.RespondAsync("Sorry! I had an internal error.");
                return;
            }
            
            if (!EnsureMemberIsInAVoiceChannel(ctx, ctx.Guild.CurrentMember)) //check if the bot is in a voice channel
            {
                await ctx.RespondAsync("I'm not in any voice channel!");
                return;
            }

            var llChannelConnection = ctx.Client.GetLavalink().ConnectedNodes.Values.First().GetGuildConnection(ctx.Guild);
            await llChannelConnection.DisconnectAsync();
            await ctx.RespondAsync("Left the voice channel.");
        }

        private async Task OnPlaybackFinishedHandler(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            if (sender.CurrentState.CurrentTrack == null)
                await Task.Delay(1000 * 60 * 5, _cts.Token).ContinueWith(_ =>
                    sender.Guild.GetDefaultChannel()
                        .SendMessageAsync($"I have removed myself from {sender.Channel} due to inactivity."));
        }
        
        [Command("h"), Aliases("help")]
        public async Task Help(CommandContext ctx)
        {
            await ctx.RespondAsync("Commands: ping, play, pause/stop, resume, leave, help");
        }
    }
}
