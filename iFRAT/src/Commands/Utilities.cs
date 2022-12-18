using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

namespace iFRAT.Commands;
public sealed class Utilities : Masked.DiscordNet.IDiscordCommand
{
    public static Utilities GlobalInstance { get; } = new ();
    public async Task Run(SocketSlashCommand commandSocket) {
        commandSocket.DeferAsync();

        string? arguments = null;
        string subCommand = (string)commandSocket.Data.Options.ElementAt(0).Value;

        if (commandSocket.Data.Options.Count > 1)
            arguments = (string?)commandSocket.Data.Options.ElementAt(1).Value;

        arguments ??= "";  // Set If Null.

        switch (subCommand)
        {
            case "help":
                _ = await commandSocket.FollowupAsync(embed: Extra.Commands.HelpCommand.GetHelpOfCommand(arguments));
                break;

            default:
                _ = await commandSocket.FollowupAsync("Unknown Subcommand. Please verify the syntax of the command, if it continues to fail, the command MAY or MAY **NOT** be implemented correctly.");
                break;
        }
    }
    public SlashCommandProperties Build() {
        var cmdBuilder = new SlashCommandBuilder()
        {
            Name = "utils",
            Description = "This command allows you to access a variety of Sub-Commands available."
        };

        return cmdBuilder.AddOptions(new SlashCommandOptionBuilder() { 
            Name = "subcommand",
            Description = "A Subcommand, these are available via auto completions provided by the bot",
            Type = ApplicationCommandOptionType.String,
            IsAutocomplete = true,
            IsRequired = true,
        }, new()
        {
            Name = "additionalargs",
            Description = "Additional Arguments required by the command, if any",
            Type = ApplicationCommandOptionType.String,
            IsRequired = false,
        }).Build();
    }
    /// <summary>
    /// Obtains the list of available sub commands, which will be given to an auto complete handler to provide auto completition
    /// </summary>
    /// <returns>A dictionary holding the sub comands, the Key is the title of it, the value is the value used by the command to identify them.</returns>
    public Dictionary<string, string> GetSubCommandList() {
        return new Dictionary<string, string>()
        {
            ["List Directories"] =  "dir",
            ["Change Bot Current Directory"] = "cd",
            ["Help"] = "help"
        };
    }
}
