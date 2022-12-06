using System;
using System.Linq;
using Discord;
using Discord.Interactions;
using Masked.DiscordNet;
using Masked.Sys.Extensions;
using Spectre.Console;

namespace iFRAT;

public sealed partial class StartStage
{
    public async Task<PreInit> PreInitialization(string[] arguments)
    {
        // Set Ctrl + C/Break Handler.

        Console.CancelKeyPress += ConsoleExit;

        AnsiConsole.MarkupLine("[maroon][[INFO]] Running [red]Pre-Initialization[/].[/]");

        // Setup commands.
        CommandHelper commandContext = iFRAT.Commands.CommandLoader.LoadCommands(new());

        string token = File.ReadAllText("./TOKEN");

        return new(commandContext, token);
    }

    private static void ConsoleExit(object? sender, ConsoleCancelEventArgs args)
    {
        // If it is testing mode, we should close normally, else just force it to not close.
        Console.WriteLine("\nReceived SIGINT!");

        Console.WriteLine($"SIGINT TRIGGERED BY {args.SpecialKey}");

        // End the process 'gracefully'.
        Console.WriteLine("Disposing, Logging Out and Stopping DiscordSocketClient, Disposing HttpClient and any Disposeable object that lives all-along the program\'s execution time...");

        // HTTPClient disposal.
        Shared.HttpClient.Dispose();

        // Discord Client disposal
        Shared.DiscordClient.LogoutAsync();
        Shared.DiscordClient.StopAsync();
        Shared.DiscordClient.Dispose();
    }
}