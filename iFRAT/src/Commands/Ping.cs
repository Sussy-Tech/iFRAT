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
public static class Ping
{
    //! The perfect command.

    /// <summary>
    /// A command that sends the current ping between the Discord gateway and the bot
    /// </summary>
    /// <param name="sockCommand">The interaction socket.</param>
    /// <returns>A Task representing the on-going asynchronous operation</returns>
    public static async Task Run(SocketSlashCommand sockCommand)
    {
        await sockCommand.DeferAsync();
        await sockCommand.FollowupAsync($"**Pong**!\nThe ping between the **__Discord Gateway__** and the **__Bot__** is of {Shared.DiscordClient.Latency}ms!");
    }

    public static SlashCommandProperties Build()
    {
        return new SlashCommandBuilder
        {
            Name = "ping",
            Description = "A simple command to show the latency between the server and the client (Discord Gateway) <---> (Bot)",
        }.Build();
    }
}