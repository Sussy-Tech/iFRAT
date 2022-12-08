using System.Diagnostics;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Masked.DiscordNet;
using Masked.DiscordNet.Extensions;
using Spectre.Console;

namespace iFRAT.Commands;

#pragma warning disable MA0004 // Disable -> Use ConfigurateAwait(false); as no SyncCtx is needed.
public sealed class Execute : IDiscordCommand
{
    public static Execute GlobalInstance { get; } = new();
    //TODO: Complete Rewrite of this command. It works only for Windows, furthermore, it's terribly written, and very not likely to work.
    public async Task Run(SocketSlashCommand commandSocket)
    {
        throw new NotImplementedException();
    }
    public SlashCommandProperties Build()
    {
        throw new NotImplementedException();
    }
}