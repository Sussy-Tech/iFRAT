using Masked.DiscordNet;

namespace iFRAT;

public struct PreInit
{
    private readonly CommandHelper _commands;
    private string _token;

    public PreInit(CommandHelper commandHelper, string token)
    {
        _commands = commandHelper;
        _token = token;
    }

    /// <summary>
    /// Obtains a copy from the command builder issued by Masked Library
    /// </summary>
    /// <returns>The command builder issued by masked library</returns>
    public CommandHelper GetCommands()
        => _commands;

    /// <summary>
    /// Reads the Bot Token, and then, rewrites it with null.
    /// </summary>
    /// <returns>A String containing the Bot's Token.</returns>
    public string ReadToken()
    {
        try
        {
            return _token;
        }
        finally
        {
            _token = string.Empty;
        }
    }
}