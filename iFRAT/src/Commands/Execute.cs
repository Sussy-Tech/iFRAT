using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using Discord;
using Discord.WebSocket;

using Masked.DiscordNet;

namespace iFRAT.Commands;

#pragma warning disable MA0004 // Disable -> Use ConfigurateAwait(false); as no SyncCtx is needed.

public sealed class Execute : IDiscordCommand
{
    public static Execute GlobalInstance { get; } = new();

    //TODO: Fix some rough edges.
    public async Task Run(SocketSlashCommand commandSocket)
    {
        await commandSocket.DeferAsync();

        #region Program Args + Info

        string programArgs = "";
        string programName = commandSocket.Data.Options.ElementAt(0).Value.ToString()!;
        bool useCurrentDirectory = (bool)commandSocket.Data.Options.ElementAt(1).Value;

        if (!programName.Contains('/') && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            programName = "/usr/bin/" + programName;
        }
        try
        {
            programArgs = commandSocket.Data.Options.ElementAt(2).Value.ToString()!;
        }
        catch
        {
            programArgs = "";
        }

        #endregion Program Args + Info

        EmbedBuilder embed = new()
        {
            Title = "Shell Executor",
            Description = $"Executing '{programName}', with arguments \'{programArgs}\'..."
        };

        Process proc = new()
        {
            StartInfo = new()
            {
                FileName = programName,
                Arguments = programArgs,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        if (useCurrentDirectory)
            proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;

        Thread watcher = new(async () =>
        {
            ulong stderrMsgId, stdoutMsgId;
            ISocketMessageChannel cnn = commandSocket.Channel;
            Stopwatch wtch = new();
            wtch.Start();
            try
            {
                bool success = proc.Start();

                if (!success)
                {
                    throw new("The program did not start. Are you attempting to reference an already started process?");
                }
                await commandSocket.FollowupAsync(embed: embed.Build());
                stderrMsgId = (await cnn.SendMessageAsync("`NOTE: Standard Error Output will be contained on this message!`\r\n\r\n")).Id;
                stdoutMsgId = (await cnn.SendMessageAsync("`NOTE: Standard Output will be contained on this message!`\r\n\r\n")).Id;

                StringBuilder
                    currOutputERR = new(),
                    currOutputOUT = new();
                StreamReader errOut = proc.StandardError, stdOut = proc.StandardOutput;
                do
                {
                    Task<string> stdErrTask = errOut.ReadToEndAsync();
                    Task<string> stdOutTask = stdOut.ReadToEndAsync();
                    await Task.Delay(500);
                    proc.Refresh();

                    await Task.WhenAll(new Task[] { stdErrTask, stdOutTask });

                    currOutputERR.Append(stdErrTask.Result);
                    currOutputOUT.Append(stdOutTask.Result);

                    try
                    {
                        if (currOutputERR.Length > 4096)
                        {
                            await cnn.ModifyMessageAsync(stderrMsgId, msgProps => msgProps.Embed = new EmbedBuilder() { Title = "Standard Error", Description = String.Join("", currOutputERR.ToString()[0..4093]) + "..." }.Build());
                            await cnn.ModifyMessageAsync(stdoutMsgId, msgProps => msgProps.Embed = new EmbedBuilder() { Title = "Standard Output", Description = String.Join("", currOutputOUT.ToString()[0..4093]) + "..." }.Build());
                        }
                        else
                        {
                            await cnn.ModifyMessageAsync(stderrMsgId, msgProps => msgProps.Embed = new EmbedBuilder() { Title = "Standard Error", Description = currOutputERR.ToString() }.Build());
                            await cnn.ModifyMessageAsync(stdoutMsgId, msgProps => msgProps.Embed = new EmbedBuilder() { Title = "Standard Output", Description = currOutputOUT.ToString() }.Build());
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"error sending message -> {ex}");
                    }
                }
                while (!proc.HasExited);
                if (currOutputERR.Length > 4096)
                {
                    await cnn.ModifyMessageAsync(stderrMsgId, msgProps => msgProps.Embed = new EmbedBuilder() { Title = "Standard Error", Description = String.Join("", currOutputERR.ToString()[0..4093]) + "..." }.Build());
                    await cnn.ModifyMessageAsync(stdoutMsgId, msgProps => msgProps.Embed = new EmbedBuilder() { Title = "Standard Output", Description = String.Join("", currOutputOUT.ToString()[0..4093]) + "..." }.Build());
                }
                else
                {
                    await cnn.ModifyMessageAsync(stderrMsgId, msgProps => msgProps.Embed = new EmbedBuilder() { Title = "Standard Error", Description = currOutputERR.ToString() }.Build());
                    await cnn.ModifyMessageAsync(stdoutMsgId, msgProps => msgProps.Embed = new EmbedBuilder() { Title = "Standard Output", Description = currOutputOUT.ToString() }.Build());
                }
                StringBuilder final = new($"\t- Standard Output:\r\n{currOutputOUT}\r\n\r\n\t- Standard Error:\r\n{currOutputERR}");

                await cnn.SendMessageAsync($"Application Terminated with Exit Code `{proc.ExitCode!}`. Ran for {wtch.ElapsedMilliseconds}ms, Processing Output...");

                string tFile = Path.GetTempFileName();
                string tFile2 = Path.GetTempPath() + $"{programName.Replace('/', '_').Replace('\\', '_')}_output.txt".ToLower();

                await File.WriteAllTextAsync(tFile, final.ToString(), Encoding.UTF8);

                File.Move(tFile, tFile2, true);

                await cnn.SendFileAsync(tFile2, text: "Output Attached.");

                errOut.Dispose();
                stdOut.Dispose();
                File.Delete(tFile2);
            }
            catch (Exception ex)
            {
                await commandSocket.FollowupAsync($"Error Executing Program on Shell. -> **EXCEPTION**:\r\n```\r\n{ex}\r\n```\r\nProgram ran for {wtch.ElapsedMilliseconds}ms.");
                throw;
            }
            finally
            {
                proc.Dispose();
            }
        })
        {
            Name = "Shell Watcher"
        };
        watcher.Start();
    }

    public SlashCommandProperties Build()
    {
        SlashCommandBuilder cmd = new()
        {
            Name = "execute",
            Description = "Executes commands and/or applications on the remote computer"
        };
        cmd.AddOptions(
            new()
            {
                Name = "application",
                Description = "The path in where the application is located.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = true
            },
            new()
            {
                Name = "usecurrentdirectory",
                Description = "Should the command use the current directory as the folder for the application)",
                Type = ApplicationCommandOptionType.Boolean,
                IsRequired = true
            },
            new()
            {
                Name = "arguments",
                Description = "The arguments that are used on the command.",
                Type = ApplicationCommandOptionType.String,
                IsRequired = false
            }
        );
        return cmd.Build();
    }
}