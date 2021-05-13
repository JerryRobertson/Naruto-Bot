using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using DSharpPlus.Entities;

namespace TousenBot.Commands
{
    public class FunCommands : BaseModule
    {
        static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static readonly string ApplicationName = "Naruto5e";
        static readonly string SpreadsheetId = "1a_0sm__-SFph3lzxDEX9FuChLFI-R5COz6jhMGG8Kq0";
        static readonly string jutsu = "Jutsu";
        static List<string> Tracker = new List<string>();
        static string Turn = null;
        static int Round = 1;
        static bool RoundChange = false;

        GoogleCredential credential;
        //UserCredential credential;
        static SheetsService service;

        private static DiscordClient _client;
        private static InteractivityModule _interactivity;
        protected override void Setup(DiscordClient client)
        {
            _client = client;
            _interactivity = client.UseInteractivity(new InteractivityConfiguration());
        }
        [Command("jutsu"), Aliases("j"), Description("Used to look up information of jutsu.")]
        public async Task Nin(CommandContext ctx, [Description("The search parameter, IE Jutsu Name. Can use 's for an exact search.")] params string[] args)
        {
            await ctx.Channel.TriggerTypingAsync();

            using (var stream = new FileStream("Naruto5e.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                  .CreateScoped(Scopes)
                  .CreateWithUser("sheets@naruto5e.iam.gserviceaccount.com");
            }
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });


            var result = ReadEntries(string.Join(" ", args));
            var interactivity = ctx.Client.GetInteractivityModule();
            List<Page> pages = new List<Page>();
            foreach (var res in result)
            {

                pages.AddRange(interactivity.GeneratePagesInEmbeds(res));
            }
            await interactivity.SendPaginatedMessage(ctx.Channel, ctx.Member, pages);
            //var msg = await ctx.Channel.GetMessageAsync(ctx.Channel.LastMessageId);
            //await ctx.Channel.DeleteMessageAsync(msg, "Testing");
        }
        static List<string> ReadEntries(string input)
        {
            if (input.Length < 1)
            {
                return new List<string>() { "Please use your words..." };
            }

            var range = $"{jutsu}!A3:P811";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
            //      var response = service.Spreadsheets.Values.Get(SpreadsheetId, range).ExecuteAsync().Result;
            try
            {
                var response = request.Execute();
                var values = response.Values;
                List<string> result = new List<string>();
                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        string compare = row[0].ToString().ToLower();
                        if (input.Contains("'"))
                        {

                            if (compare.Equals(input.Replace("'", "").ToLower()))
                            {
                                result.Add($"\n{row[0]}\n{row[6]}\n{row[15]}");
                                return result;
                            }
                        }
                        else
                        {
                            if (compare.Contains(input.ToLower()))
                            {
                                result.Add($"\n{row[0]}\n{row[6]}\n{row[15]}");
                            }
                        }
                    }
                    if (result.Count > 0)
                    {
                        result.Sort();
                        //if (result.Count > 5)
                        //{
                        //    result.RemoveRange(5, result.Count - 5);
                        //}

                        return result;
                    }
                    else { return new List<string>() { "No Matching Jutsu Found." }; }
                }
                else
                {
                    return new List<string>() { "No Data Found" };
                }
            }
            catch (Google.GoogleApiException ex)
            {
                return new List<string>() { $"Exception: Google Catch: {ex.Message} " };
            }
            catch (Exception ex)
            {
                return new List<string>() { $"Exception: System Catch: {ex.InnerException} {ex.Message}" };
            }
        }
        [Command("Feat"), Aliases("f", "feat"), Description("Used to look up information of Feats.")]
        public async Task Feat(CommandContext ctx, [Description("The search parameter, IE condition Name. Can use 's for an exact search.")] params string[] args)
        {
            await ctx.Channel.TriggerTypingAsync();
            using (var stream = new FileStream("Naruto5e.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                  .CreateScoped(Scopes)
                  .CreateWithUser("sheets@naruto5e.iam.gserviceaccount.com");
            }
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
            var result = ReadFeats(string.Join(" ", args));
            var interactivity = ctx.Client.GetInteractivityModule();
            List<Page> pages = new List<Page>();

            foreach (var res in result)
            {
                pages.AddRange(interactivity.GeneratePagesInEmbeds(res));
            }
            await interactivity.SendPaginatedMessage(ctx.Channel, ctx.Member, pages);
            //var msg = await ctx.Channel.GetMessageAsync(ctx.Channel.LastMessageId);
            //await ctx.Channel.DeleteMessageAsync(msg);
        }
        static List<string> ReadFeats(string input)
        {
            if (input.Length < 1)
            {
                return new List<string>() { "Please use your words..." };
            }

            var range = $"{jutsu}!A836:P1000";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
            //      var response = service.Spreadsheets.Values.Get(SpreadsheetId, range).ExecuteAsync().Result;
            try
            {
                var response = request.Execute();
                var values = response.Values;
                List<string> result = new List<string>();
                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        string compare = row[0].ToString().ToLower();
                        if (input.Contains("'"))
                        {

                            if (compare.Equals(input.Replace("'", "").ToLower()))
                            {
                                result.Add($"\n{row[0]}\n{row[15]}");
                                return result;
                            }
                        }
                        else
                        {
                            if (compare.Contains(input.ToLower()))
                            {
                                result.Add($"\n{row[0]}\n{row[15]}");
                            }
                        }
                    }
                    if (result.Count > 0)
                    {
                        result.Sort();
                        if (result.Count > 5)
                        {
                            result.RemoveRange(5, result.Count - 5);
                        }

                        return result;
                    }
                    else { return new List<string>() { "No Matching Condition Found." }; }
                }
                else
                {
                    return new List<string>() { "No Data Found" };
                }
            }
            catch (Google.GoogleApiException ex)
            {
                return new List<string>() { $"Exception: Google Catch: {ex.Message} " };
            }
            catch (Exception ex)
            {
                return new List<string>() { $"Exception: System Catch: {ex.InnerException} {ex.Message}" };
            }
        }
        [Command("condition"), Aliases("con"), Description("Used to look up information of Conditions.")]
        public async Task Cond(CommandContext ctx, [Description("The search parameter, IE condition Name. Can use 's for an exact search.")] params string[] args)
        {
            await ctx.Channel.TriggerTypingAsync();
            using (var stream = new FileStream("Naruto5e.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                  .CreateScoped(Scopes)
                  .CreateWithUser("sheets@naruto5e.iam.gserviceaccount.com");
            }
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });


            var result = ReadConditions(string.Join(" ", args));
            var interactivity = ctx.Client.GetInteractivityModule();
            List<Page> pages = new List<Page>();

            foreach (var res in result)
            {
                pages.AddRange(interactivity.GeneratePagesInEmbeds(res));
            }

            await interactivity.SendPaginatedMessage(ctx.Channel, ctx.Member, pages).ConfigureAwait(false);
            //var msgList = await ctx.Channel.GetMessagesAsync(50, ctx.Channel.LastMessageId);
            //var msg = msgList.Where(m => m.Author.Username == "TousenBot").First();
            //await ctx.Channel.DeleteMessageAsync(msg, "Testing");
        }
        static List<string> ReadConditions(string input)
        {
            if (input.Length < 1)
            {
                return new List<string>() { "Please use your words..." };
            }

            var range = $"{jutsu}!A812:P835";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
            //      var response = service.Spreadsheets.Values.Get(SpreadsheetId, range).ExecuteAsync().Result;
            try
            {
                var response = request.Execute();
                var values = response.Values;
                List<string> result = new List<string>();
                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        string compare = row[0].ToString().ToLower();
                        if (input.Contains("'"))
                        {

                            if (compare.Equals(input.Replace("'", "").ToLower()))
                            {
                                result.Add($"\n{row[0]}\n{row[15]}");
                                return result;
                            }
                        }
                        else
                        {
                            if (compare.Contains(input.ToLower()))
                            {
                                result.Add($"\n{row[0]}\n{row[15]}");
                            }
                        }
                    }
                    if (result.Count > 0)
                    {
                        result.Sort();
                        if (result.Count > 5)
                        {
                            result.RemoveRange(5, result.Count - 5);
                        }

                        return result;
                    }
                    else { return new List<string>() { "No Matching Condition Found." }; }
                }
                else
                {
                    return new List<string>() { "No Data Found" };
                }
            }
            catch (Google.GoogleApiException ex)
            {
                return new List<string>() { $"Exception: Google Catch: {ex.Message} " };
            }
            catch (Exception ex)
            {
                return new List<string>() { $"Exception: System Catch: {ex.InnerException} {ex.Message}" };
            }
        }
        [Command("ping"), Description("First Command Added..")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.TriggerTypingAsync();
            await ctx.Channel.SendMessageAsync($"Pong").ConfigureAwait(false);
            var msgList = await ctx.Channel.GetMessagesAsync(50, ctx.Channel.LastMessageId);
            var msg = msgList.Where(m => m.Author.Username == "TousenBot").First();

            await ctx.Channel.DeleteMessageAsync(msg);
        }
        [Command("add"), Description("Command used to add numbers using the syntax 'add number number ...'")]
        public async Task Add(CommandContext ctx, params int[] numbers)
        {
            await ctx.Channel.TriggerTypingAsync();
            await ctx.Channel.SendMessageAsync(numbers.Sum().ToString()).ConfigureAwait(false);
        }
        [Command("roll"), Aliases("r"), Description("Command used to roll dice")]
        public async Task Roll(CommandContext ctx, [Description("Example 2d6.")] string dice, [Description("Description of the roll, Optional.")] params string[] textArray)
        {
            await ctx.Channel.TriggerTypingAsync();
            string text = "";
            if (textArray.Length > 0)
            {
                text += string.Join(" ", textArray);
            }
            string[] input = dice.Split("d");
            var rand = new Random();
            if (input[1].Contains("+"))
            {


                string[] newString = input[1].Split("+");
                string[] addition = newString[1..(newString.Length)];
                int addResult = AddArray(addition);
                input = new string[] { input[0], newString[0], addResult.ToString() };

            }
            try
            {
                int die = Int32.Parse(input[1]);
                int amount = Int32.Parse(input[0]);
                string output = "";
                int total = 0;
                var roll = Roll(amount, die);
                output = roll[0];
                total = Int32.Parse(roll[1]);
                if (input.Length > 2)
                {
                    output += " + " + input[2];
                    total += Int32.Parse(input[2]);
                }
                if (text.Equals("")) { text = dice; }
                await ctx.Channel.SendMessageAsync($"**{text}**:  {output}").ConfigureAwait(false);
                await ctx.Channel.SendMessageAsync("Total Roll: " + total).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ctx.Channel.SendMessageAsync(ex.Message).ConfigureAwait(false);

            }
        }
        [Command("rr")]
        public async Task MultiRoll(CommandContext ctx, int times, string dice, params string[] textArray)
        {
            await ctx.Channel.TriggerTypingAsync();
            string text = "";
            if (textArray.Length > 0)
            {
                text += string.Join(" ", textArray);
            }
            string[] input = dice.Split("d");
            var rand = new Random();
            if (input[1].Contains("+"))
            {

                string[] newString = input[1].Split("+");
                input = new string[] { input[0], newString[0], newString[1] };

            }
            try
            {
                int die = Int32.Parse(input[1]);
                int amount = Int32.Parse(input[0]);
                string output = "";
                int total = 0;
                int tempTotal = 0;
                if (text.Equals(" ")) { text = dice; }
                for (int y = 0; y < times; y++)
                {
                    tempTotal = 0;
                    for (int x = 0; x < amount; x++)
                    {
                        int number = rand.Next(1, die);
                        output += "[" + number.ToString() + "] ";
                        tempTotal += number;
                        total += number;
                    }
                    if (input.Length > 2)
                    {

                        await ctx.Channel.SendMessageAsync(text + ": " + output + " + " + input[2] + ": " + (tempTotal + Int32.Parse(input[2]))).ConfigureAwait(false);
                    }
                    if (input.Length < 3) await ctx.Channel.SendMessageAsync(text + ": " + output + ": " + tempTotal).ConfigureAwait(false);
                    output = "";
                }
                await ctx.Channel.SendMessageAsync("Total Roll: " + total).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ctx.Channel.SendMessageAsync(ex.Message).ConfigureAwait(false);

            }
        }
        private string[] Roll(int amount, int die)
        {
            var rand = new Random();
            int result = 0;
            string output = "";
            for (int x = 0; x < amount; x++)
            {
                int number = rand.Next(1, die);
                output += "[" + number.ToString() + "] ";
                result += number;
            }
            string[] array = new string[] { output, result.ToString() };
            return array;
        }
        private int AddArray(string[] toAdd)
        {
            int result = 0;
            foreach (string x in toAdd)
            {
                result += Int32.Parse(x);
            }
            return result;
        }
        [Command("Next"), Aliases("next", "End", "end"), Description("Pushes initiative to the next turn.")]
        public async Task Next(CommandContext ctx)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => e.Name == "Council") == null && Turn != ctx.Member.Mention)
            {
                await ctx.Channel.SendMessageAsync("It's not your turn!").ConfigureAwait(false);
            }
            else
            {
                DisplayTurn(Tracker);
                if (RoundChange == true)
                {
                    RoundChange = false;
                    await ctx.Channel.SendMessageAsync($"Round {Round}!").ConfigureAwait(false);
                }
                await ctx.Channel.SendMessageAsync($"Turn: {Turn}").ConfigureAwait(false);
            }
        }
        [Command("Remove"), Aliases("remove", "Kill", "kill"), Description("Remove's a player from the tracker.")]
        public async Task Remove(CommandContext ctx, string player)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => e.Name == "Council") == null)
            {
                await ctx.Channel.SendMessageAsync("Improper Access!").ConfigureAwait(false);
            }
            else if (Turn == player)
            {
                await ctx.Channel.SendMessageAsync("Can't remove a player while it's their turn!").ConfigureAwait(false);
            }
            else
            {
                try
                {
                    Tracker.Remove(player);
                    await ctx.Channel.SendMessageAsync($"Removed: {player}").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await ctx.Channel.SendMessageAsync($"{player} is not on the tracker.").ConfigureAwait(false);
                }

            }

        }
        [Command("IAdd"), Aliases("iAdd", "iadd"), Description("Adds a player before a referenced player. Otherwise, adds to end of tracker.")]
        public async Task iAdd(CommandContext ctx, string player, string reference = null)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => e.Name == "Council") == null)
            {
                await ctx.Channel.SendMessageAsync("Improper Access!").ConfigureAwait(false);
            }
            else if (Tracker.Exists(e => e == player))
            {
                await ctx.Channel.SendMessageAsync($"{player} is already on the tracker!").ConfigureAwait(false);
            }
            else
            {
                if (reference == null)
                {
                    Tracker.Add(player);
                    await ctx.Channel.SendMessageAsync($"Added {player} to end of list.").ConfigureAwait(false);
                }
                else if (Tracker.Exists(e => e == reference))
                {
                    Tracker.Insert(Tracker.FindIndex(e => e == reference), player);
                    await ctx.Channel.SendMessageAsync($"Added {player} before {reference}.").ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"{reference} is not on the tracker!").ConfigureAwait(false);
                }
            }

        }
        [Command("IShow"), Aliases("iShow", "ishow"), Description("Shows the initiative.")]
        public async Task iShow(CommandContext ctx)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => e.Name == "Council") == null)
            {
                await ctx.Channel.SendMessageAsync("Improper Access!").ConfigureAwait(false);
            }
            var temp = new List<string>();
            temp.AddRange(Tracker);
            if (ctx.Guild.Members.FirstOrDefault(e => e.Mention == Turn) != null)
            {
                temp[temp.FindIndex(e => e.Equals(Turn))] = $">{ctx.Guild.Members.First(e => e.Mention == Turn).Nickname}<";
            }
            if (ctx.Guild.Members.FirstOrDefault(e => e.Mention == Turn) == null)
            {
                temp[temp.FindIndex(e => e.Equals(Turn))] = $">{Turn}<";
            }

            var list = string.Join("\n", temp);
            await ctx.Channel.SendMessageAsync($"```\n{list}```").ConfigureAwait(false);

        }
        [Command("Initiative"), Aliases("Tracker", "Track", "track", "tracker"), Description("Command used to start initiative tracker")]
        public async Task Init(CommandContext ctx, params string[] players)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => e.Name == "Council") == null)
            {
                await ctx.Channel.SendMessageAsync("Proper Access Needed").ConfigureAwait(false);
            }
            else
            {
                Round = 1;
                Tracker.Clear();
                Tracker.AddRange(players);

                DisplayTurn(Tracker);
                if (RoundChange == true)
                {
                    RoundChange = false;
                    await ctx.Channel.SendMessageAsync($"Round {Round}!").ConfigureAwait(false);
                }
                await ctx.Channel.SendMessageAsync($"Turn: {Turn}").ConfigureAwait(false);
            }
        }
        private void DisplayTurn(List<string> players, int init = 0)
        {
            if (Turn == null)
            {
                RoundChange = true;
                Turn = players[init];
            }
            else
            {
                init = players.FindIndex(e => Turn == e) + 1;
                if (init >= players.Count)
                {
                    RoundChange = true;
                    init = 0;
                    Round++;
                }
                Turn = players[init];
            }
        }
    }
}
