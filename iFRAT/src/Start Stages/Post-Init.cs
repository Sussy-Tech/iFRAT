using Discord;

using iFRAT.Commands;

using Spectre.Console;

namespace iFRAT;

public sealed partial class StartStage
{
    public async Task PostInitialization()
    {
        AnsiConsole.MarkupLine("[maroon][[INFO]] Running [yellow]Post-Initialization[/].[/]");

        AnsiConsole.MarkupLine("[yellow][[INFO/PostInit]] Registering [grey underline]autocomplete[/] for command [red bold]utils[/] for parameter with name [red bold underline]subcommand[/][/]");

        Shared.DiscordClient.AutocompleteExecuted += async autoCompleteHandler =>
        {
            #region Execution Guards

            if (autoCompleteHandler.IsDMInteraction)
                return;

            if (autoCompleteHandler.Type != InteractionType.ApplicationCommandAutocomplete)
                return;

            if (autoCompleteHandler.Data.CommandName != "utils")
                return;

            if (autoCompleteHandler.Data.Current.Name != "subcommand")
                return;

            #endregion Execution Guards

            // Defer Interaction in case we time out a bit
            await autoCompleteHandler.DeferAsync();

            Dictionary<string, string> subCommands = Utilities.GlobalInstance.GetSubCommandList();
            List<AutocompleteResult> autoCompletePossibilities = new();

            for (int i = 0; i < autoCompletePossibilities.Count; i++)
            {
                autoCompletePossibilities.Add(new()
                {
                    Name = subCommands.ElementAt(i).Key,
                    Value = subCommands.ElementAt(i).Value
                });
            }

            await autoCompleteHandler.RespondAsync(autoCompletePossibilities, RequestOptions.Default);
        };

        // TODO: Auto Completition of certain commands which might need it.
    }
}