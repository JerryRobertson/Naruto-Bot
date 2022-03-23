using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DSharpPlus;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using TousenBot.Commands;
using System.Reflection;

namespace TousenBot
{
    public class Program
    {
        //private readonly IConfiguration _config;
        //private DiscordSocketClient _client;
        //static readonly Bot DiscordBot = new Bot();
        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
            //var bot = new Bot();
            //DiscordBot.RunAsync().GetAwaiter().GetResult();
            //new Bot().RunAsync().GetAwaiter().GetResult();
        }
        static async Task MainAsync()
        {
            //{
            //    Token = "My First Token",
            //    TokenType = TokenType.Bot,
            //    Intents = DiscordIntents.AllUnprivileged
            //});


            // create the configuration
            var _builder = new ConfigurationBuilder()
                .AddJsonFile(path: "config.json");
            Console.WriteLine("Builder built");
            foreach (var x in _builder.Sources)
            {
                Console.WriteLine(x.ToString());

            }
            //.SetBasePath(AppContext.BaseDirectory)

            // build the configuration and assign to _config          
            IConfiguration _config = _builder.Build();
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            using (var services = Bot.ConfigureServices(_config))
            {


                var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

                var config = new DiscordConfiguration()
                {
                    Token = configJson.Token,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    Intents = DiscordIntents.AllUnprivileged,
                    //LogLevel = LogLevel.Debug,
                    //UseInternalLogHandler = true,
                    //AutomaticGuildSync = true
                };

                DiscordClient _client = new DiscordClient(config);
                //_client = services.GetRequiredService<DiscordClient>();

                _client.Ready += Bot.OnClientReady;




                //Console.WriteLine($"Current User: {_client.CurrentUser}");

                _client.UseInteractivity(new InteractivityConfiguration
                {
                    Timeout = TimeSpan.FromSeconds(90)
                });
                var commandsConfig = new CommandsNextConfiguration
                {
                    StringPrefixes = new[] { $"{configJson.Prefix}" },
                    EnableMentionPrefix = true,
                    EnableDms = false,


                    //IgnoreExtraArguments = true
                };

                CommandsNextExtension _commands = _client.UseCommandsNext(commandsConfig);
                //var test = new Naruto();
                //Console.WriteLine($"Test: { test.Client.ShardId}");
                _commands.RegisterCommands<Naruto>();
                //Console.WriteLine($"Client: {_commands.Client.ToString()}");

                await _client.ConnectAsync();
                //await services.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(-1);
            }
        }
    }
}