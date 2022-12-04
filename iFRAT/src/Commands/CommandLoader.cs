using Discord;
using Masked.DiscordNet;

namespace iFRAT.Commands;

public static class CommandLoader
{
    public static CommandHelper LoadCommands(CommandHelper commandHelper)
    {
        commandHelper.AddToCommandList(Ping.Build(), async f => await Ping.Run(f));

        // commandHelper.AddToCommandList(Commands.BuildCommandRunner(), async f => await Commands.CommandRunner(f));

        return commandHelper;
    }
}