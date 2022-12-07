using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Masked.DiscordNet.Extensions;
using Spectre.Console;

namespace iFRAT.Commands;

#pragma warning disable MA0004 // Disable -> Use ConfigurateAwait(false); as no SyncCtx is needed.
/// <summary>
/// The Ping Command, Guaranteed to always work, else, Discord Bots are doomed..
/// </summary>
public sealed class Ping : Masked.DiscordNet.IDiscordCommand
{
    public static Ping GlobalInstance { get; } = new();
    //! The perfect command.

    public async Task Run(SocketSlashCommand sockCommand)
    {
        await sockCommand.DeferAsync();
        await sockCommand.FollowupAsync($"**Pong**!\nThe ping between the **__Discord Gateway__** and the **__Bot__** is of {Shared.DiscordClient.Latency}ms!");
    }

    public SlashCommandProperties Build()
    {
        return new SlashCommandBuilder
        {
            Name = "ping",
            Description = "A simple command to show the latency between the server and the client (Discord Gateway) <---> (Bot)",
        }.Build();
    }
}