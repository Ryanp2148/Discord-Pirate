using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordFileUploaderDownloader
{
    class Program
    {
        private static DiscordSocketClient _client;

        static async Task Main(string[] args)
        {
            _client = new DiscordSocketClient();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(title);
            Console.ResetColor();
            Console.WriteLine("This is a discord storage pirater");
            string token = "MTIwOTU5MzU5NTY5MTI3MDE3NQ.GQMuGW.aRjJptKIIbHCsh1NSvZZg0DzYC1q-5R5dnEr_8";

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Log += LogAsync;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Upload a file");
            Console.WriteLine("2. Download a file");
            Console.WriteLine("3. List files");
            Console.WriteLine("100. Exit");
            Console.ResetColor();
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await UploadFile();
                    break;
                case "2":
                    await DownloadFile();
                    break;
                case "3":
                    await ViewAllChannelsAndMessages();
                    break;
                case "100":
                    Console.WriteLine("Exiting program...");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }

        static async Task ViewAllChannelsAndMessages()
        {
            Console.WriteLine("Channels and Files:");

            foreach (var guild in _client.Guilds)
            {
                Console.WriteLine($"Guild: {guild.Name}");
                foreach (var channel in guild.TextChannels)
                {
                    Console.WriteLine($"Channel: {channel.Name}");
                    var messages = await channel.GetMessagesAsync().FlattenAsync();
                    foreach (var message in messages)
                    {
                        foreach (var attachment in message.Attachments)
                        {
                            Console.WriteLine($"- File: {attachment.Filename}");
                        }
                    }
                }
            }
            DownloadFile();
        }



        static async Task UploadFile()
        {
            Console.WriteLine("Enter the file path:");
            string filePath = Console.ReadLine();

            Console.WriteLine("Enter the file name:");
            string fileName = Console.ReadLine();

            Console.WriteLine("Enter the channel name:");
            string channelName = Console.ReadLine();

            var channel = await GetChannelByName(channelName);

            if (channel == null)
            {
                Console.WriteLine("Invalid channel name. Creating a new channel...");

                // Create a new channel with the provided name
                var guild = _client.Guilds.FirstOrDefault();
                if (guild != null)
                {
                    channel = await guild.CreateTextChannelAsync(channelName);
                    if (channel == null)
                    {
                        Console.WriteLine("Failed to create a new channel.");
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"New channel '{channelName}' created successfully.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to find a guild to create a new channel.");
                    return;
                }
            }

            await channel.SendFileAsync(filePath, fileName);
            Console.WriteLine($"File {fileName} uploaded successfully to {channel.Name}.");
        }


        static async Task DownloadFile()
        {
            Console.WriteLine("Enter the channel name:");
            string channelName = Console.ReadLine();

            var channel = await GetChannelByName(channelName);

            if (channel == null)
            {
                Console.WriteLine("Invalid channel name.");
                DownloadFile();
                return;
            }

            Console.WriteLine("Enter the message content:");
            string messageContent = Console.ReadLine();

            var messages = await channel.GetMessagesAsync().FlattenAsync();
            var message = messages.FirstOrDefault(msg => msg.Content == messageContent && msg.Attachments.Count > 0);

            if (message == null)
            {
                Console.WriteLine("Message not found or no file attached to the message.");
                return;
            }

            var attachment = message.Attachments.First();
            var downloadPath = Path.Combine(Environment.CurrentDirectory, attachment.Filename);
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                await httpClient.GetByteArrayAsync(attachment.Url).ContinueWith(async (task) =>
                {
                    var fileBytes = task.Result;
                    await File.WriteAllBytesAsync(downloadPath, fileBytes);
                }).ConfigureAwait(false);
            }

            Console.WriteLine($"File downloaded successfully to {downloadPath}.");
        }


        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        private static async Task<ITextChannel> GetChannelByName(string name)
        {
            foreach (var guild in _client.Guilds)
            {
                var channel = guild.TextChannels.FirstOrDefault(c => c.Name == name);
                if (channel != null)
                    return channel;
            }
            return null;
        }


        public static string title = @"

██████╗░██╗░██████╗░█████╗░░█████╗░██████╗░██████╗░    ██████╗░██╗██████╗░░█████╗░████████╗███████╗
██╔══██╗██║██╔════╝██╔══██╗██╔══██╗██╔══██╗██╔══██╗    ██╔══██╗██║██╔══██╗██╔══██╗╚══██╔══╝██╔════╝
██║░░██║██║╚█████╗░██║░░╚═╝██║░░██║██████╔╝██║░░██║    ██████╔╝██║██████╔╝███████║░░░██║░░░█████╗░░
██║░░██║██║░╚═══██╗██║░░██╗██║░░██║██╔══██╗██║░░██║    ██╔═══╝░██║██╔══██╗██╔══██║░░░██║░░░██╔══╝░░
██████╔╝██║██████╔╝╚█████╔╝╚█████╔╝██║░░██║██████╔╝    ██║░░░░░██║██║░░██║██║░░██║░░░██║░░░███████╗
╚═════╝░╚═╝╚═════╝░░╚════╝░░╚════╝░╚═╝░░╚═╝╚═════╝░    ╚═╝░░░░░╚═╝╚═╝░░╚═╝╚═╝░░╚═╝░░░╚═╝░░░╚══════╝
";
    }
}
