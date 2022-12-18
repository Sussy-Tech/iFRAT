using Discord;
using Discord.WebSocket;

using Spectre.Console;

namespace iFRAT;

public sealed partial class StartStage
{
    public async Task Initialization(PreInit preInitialization)
    {
        AnsiConsole.MarkupLine("[maroon][[Init/INFO]] Running [green]Initialization[/].[/]");
        // Register listeners to events.

        AnsiConsole.MarkupLine("[maroon][[Init/INFO]] [yellow italic bold]Registering event listeners[/]...[/]");

        // Load commands.
        Shared.DiscordClient.SlashCommandExecuted += preInitialization.GetCommands().GetSlashCommandHandler();
        AnsiConsole.MarkupLine("[maroon][[Init/INFO]] [yellow italic bold]Registered listener for event [red bold underline]SlashCommandExecuted[/][/]...[/]");

        Shared.DiscordClient.Log += log =>
        {
            AnsiConsole.MarkupLine($"[[Discord.Net Log]] [red bold underline]{log.ToString().EscapeMarkup()}[/]");
            return Task.CompletedTask;
        };
        AnsiConsole.MarkupLine("[maroon][[Init/INFO]] [yellow italic bold]Registered listener for event [red bold underline]Log[/][/]...[/]");

        AnsiConsole.MarkupLine("[maroon][[Init/INFO]] [yellow italic bold]Logging in[/] with [red bold underline]token[/]...[/]");

        // Start Bot
        await Shared.DiscordClient.LoginAsync(TokenType.Bot, token: preInitialization.ReadToken(), validateToken: true)
        .ContinueWith(async x =>
        {
            await x; // await previous task
            AnsiConsole.MarkupLine("[maroon][[Init/INFO]] [yellow italic bold]Starting Up[/]...[/]");
            await Shared.DiscordClient.StartAsync();
        }).ContinueWith(async x =>
        {
            await x;
            Shared.DiscordClient.GuildAvailable += async guild =>
            {
                // The bot is not Administrator, assume the Guild is a Production server, not testing server.
                if (!guild.GetUser(Shared.DiscordClient.CurrentUser.Id).Roles.Any(x => x.Permissions.Administrator))
                {
                    AnsiConsole.MarkupLine($"[maroon][[Init/WARN]] [yellow italic bold][/]Warning: Guild {guild.Name} has the bot without Administrative rights, sending warning message.[/]");
                    _ = guild.Channels.Any(cnn =>
                    {
                        if (cnn.GetChannelType() != ChannelType.Text)
                            return false;

                        var chat = (ISocketMessageChannel)cnn;

                        using (chat.EnterTypingState())
                        {
                            try
                            {
                                chat.SendMessageAsync(":warning: :warning: :warning: This bot requires Administrator Permissions, features may not work as expected! :warning: :warning: :warning:");
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    });
                }

                AnsiConsole.MarkupLine("[maroon][[Init/INFO]] [yellow italic bold]Warming up Commands[/]...[/]");
                await preInitialization.GetCommands().SubmitCommandBuilder(guild);
            };
        });
    }
}