using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;

using Masked.DiscordNet.Extensions;

namespace iFRAT.Extra.Commands;
public class HelpCommand
{
    public static Embed GetHelpOfCommand(string subCommandName) {
        EmbedBuilder builder = new();
        builder.SetRandomColor();

        switch (subCommandName)
        {
            case "cd":
                builder.Title = "cd => Change Directory";
                builder.Description =
                    "This command is used to change the current directory of the bot, makes running programs easier." +
                    "\nSyntax: **/utils cd C:/Windows**" +
                    "\n**cd** -> This value corresponds to the ´subcommand´ argument" +
                    "\n**C:/Windows** -> This value corresponds to the ´additionalargs´ argument";
                break;

            case "dir":
                builder.Title = "dir => List Directory";
                builder.Description =
                    "This command is used to list the contents of the current directory of the bot." + 
                    "\nSyntax: **/utils dir**" +
                    "\n**dir** -> This value corresponds to the ´subcommand´ argument";
                break;

            default: // How to use the help command...
                builder.Title = "help => Help on Commands";
                builder.Description =
                    "This command is used to obtain help on the available sub comands and their syntax." +
                    "\nSyntax: **/utils help cd**" +
                    "\n**help** -> This value corresponds to the ´subcommand´ argument" +
                    "\n**cd** -> This value corresponds to the ´additionalargs´ argument";

                break;
        }

        return builder.Build();
    }
}
