using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TousenBot.Commands;
using TousenBot.Services;
using DSharpPlus.Interactivity.Extensions;
using System.Linq;

namespace TousenBot
{
    public class Bot
    {
        private readonly IConfiguration _config;

        //public InteractivityExtension Interactivity { get; private set; }
        public DiscordClient _client { get; set; }
        public CommandsNextExtension _commands { get; private set; }
        public Bot()
        {
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
            _config = _builder.Build();
        }
        public async Task RunAsync()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            using var services = ConfigureServices(_config);


            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration()
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                //LogLevel = LogLevel.Debug,
                //UseInternalLogHandler = true,
                //AutomaticGuildSync = true
            };

            this._client = new DiscordClient(config);
            //_client = services.GetRequiredService<DiscordClient>();

            this._client.Ready += OnClientReady;




            //Console.WriteLine($"Current User: {_client.CurrentUser}");

            _client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(90)
            });
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { configJson.Prefix },
                EnableMentionPrefix = true,
                EnableDms = false,


                //IgnoreExtraArguments = true
            };

            _commands = _client.UseCommandsNext(commandsConfig);
            var test = new Naruto();
            //Console.WriteLine($"Test: { test.Client.ShardId}");
            _commands.RegisterCommands<Naruto>();
            //Console.WriteLine($"Client: {_commands.Client.ToString()}");

            await _client.ConnectAsync();
            //await services.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(-1);
        }


       public static Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
        public static ServiceProvider ConfigureServices(IConfiguration _config)
        {
            // this returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using csharpi.Services;
            // the config we build is also added, which comes in handy for setting the command prefix!
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<DiscordClient>()
                //.AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }
    }
}
