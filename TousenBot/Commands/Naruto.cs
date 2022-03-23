using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using DSharpPlus.Interactivity;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using Google.Apis.Sheets.v4.Data;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using TousenBot.Repositories;
using TousenBot.Models;
using TousenBot;
using TousenBot.Data;
using TousenBot.Services;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using DSharpPlus.Interactivity.Extensions;

namespace TousenBot.Commands
{
    public class Naruto : BaseCommandModule /*BaseExtension*/
    {
        static readonly string connectionString = new ConfigurationService().GetConfiguration().ConnectionString;
        static readonly IJutsuRepository jutsuRepository = new JutsuRepository(new ApplicationDbContext(connectionString));
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "Naruto5e";
        static readonly string SpreadsheetId = "1a_0sm__-SFph3lzxDEX9FuChLFI-R5COz6jhMGG8Kq0";
        static readonly string jutsu = "Jutsu";
        static readonly string feat = "Feat";
        static readonly string con = "Condition";
        static readonly List<string> Tracker = new();
        static string Turn = null;
        static int Round = 1;
        static bool RoundChange = false;


        GoogleCredential credential;
        //UserCredential credential;
        static SheetsService service;

        //        private static DiscordClient _client = new DiscordClient(ConfigSetup());
        //private static DiscordChannel _channel;
        //private static InteractivityExtension _interactivity;
        //private static InteractivityModule _interactivity;

        
        //protected override void Setup(DiscordClient client)
        //{

        //    _channel = client.GetChannelAsync(1).Result;
        //    var guilds = client.Guilds.Values;
        //    Console.WriteLine($"Channel: {_channel.ToString()}");
            

        //    _interactivity = client.UseInteractivity(new InteractivityConfiguration());
        //}
        public static List<string> ExtractTextFromPDF(string filePath)
        {
            PdfReader pdfReader = new(filePath);
            PdfDocument pdfDoc = new(pdfReader);
            var result = new List<string>();
            for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string pageContent = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                result.Add(pageContent);
            }
            pdfDoc.Close();
            pdfReader.Close();
            return result;
        }
        [Command("Import")]
        public async Task Import(CommandContext ctx, string args)

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
            if (args.Contains("google"))
            {
                var sheetLink = args.Split("/");
                var sheetId = sheetLink[sheetLink.Length - 1].Contains("edit") ? sheetLink[sheetLink.Length - 2] : sheetLink[sheetLink.Length - 1];
                var range = "A2:BB184";
                var request = service.Spreadsheets.Values.Get(sheetId, range);
                var response = request.Execute();
                var values = response.Values;
                var infoRow = values[4];
                var name = infoRow[2];
                infoRow = values[10];
                var AC = infoRow[17];
                //string init; string str; string strMod; string dex; string dexMod;
                Console.WriteLine($"Name: {name} | AC: {AC}");
            }

        }
        [Command("Register"), Description("Register as a user for the bot")]
        public async Task Register(CommandContext ctx)
        {
            await ctx.Channel.TriggerTypingAsync();
            try
            {
                IUserRepository userRepository = new UserRepository(new ApplicationDbContext(connectionString));
                var user = new User() { Name = ctx.Member.Mention };
                Console.WriteLine("Before checking to see if user exists.");
                if (!userRepository.UserExist(user.Name))
                {
                    Console.WriteLine("Before checking to see if user exists.");
                    userRepository.Add(user);
                    await ctx.Channel.SendMessageAsync("Registered!");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("User already exists.");

                }
            }
            catch (Exception ex)
            {
                await ctx.Channel.SendMessageAsync("Failed to register new user: " + ex.Message);
            }

        }
        [Command("test")]
        public async Task Test(CommandContext ctx, params string[] args)
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

            var result = ReadEntries2(string.Join(" ", args));
            var _interactivity = ctx.Client.GetInteractivity();
            List<Page> pages = new();
            foreach (var res in result)
            {

               pages.AddRange(_interactivity.GeneratePagesInEmbed(res));
            }

        }
        [Command("jutsu"), Aliases("j"), Description("Used to look up information of jutsu.")]
        public async Task Nin(CommandContext ctx, [Description("The search parameter, IE Jutsu Name. Can use 's for an exact search.")] params string[] args)
        {

            await ctx.Channel.TriggerTypingAsync();

            //using (var stream = new FileStream("Naruto5e.json", FileMode.Open, FileAccess.Read))
            //{
            //    credential = GoogleCredential.FromStream(stream)
            //      .CreateScoped(Scopes)
            //      .CreateWithUser("sheets@naruto5e.iam.gserviceaccount.com");
            //}
            //service = new SheetsService(new BaseClientService.Initializer()
            //{
            //    HttpClientInitializer = credential,
            //    ApplicationName = ApplicationName
            //});
            //var result = ReadEntries(string.Join(" ", args));
            var result2 = GetJutsu(string.Join(" ", args));
            //var interactivity = ctx.Client.GetInteractivityModule();
            var interactivity = ctx.Client.GetInteractivity();
            List<Page> pages = new();
            Console.WriteLine(result2.Count);
            if (result2.Count > 0)
            {
                foreach (var res in result2)
                {
                    var text = @$"{res.Name}
{res.Info} 
{res.Description}";
                    var page = interactivity.GeneratePagesInEmbed(text, DSharpPlus.Interactivity.Enums.SplitType.Line);
                    pages.AddRange(page);
                }
                //foreach (var res in result)
                //{

                //    pages.AddRange(interactivity.GeneratePagesInEmbeds(res));
                //}
                //var msg = await ctx.Channel.GetMessageAsync(ctx.Channel.LastMessageId);
                //await ctx.Channel.DeleteMessageAsync(msg, "Testing");
                await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages);
            }

            else
            {
                await ctx.Channel.SendMessageAsync("No Results Found");

            }
        }
        static List<Jutsu> GetJutsu(string input)
        {
            IJutsuRepository tempJutsuRepository = new JutsuRepository(new ApplicationDbContext(connectionString));

            var response = new List<Jutsu>();
            if (!string.IsNullOrEmpty(input))
            {
                response.AddRange(tempJutsuRepository.GetLike(input));
            }
            response.Sort((x, y) => x.Name.CompareTo(y.Name));
            return response;
        }
        static List<string> ReadEntries(string input)
        {
            if (input.Length < 1)
            {
                return new List<string>() { "Please use your words..." };
            }

            var range = $"{jutsu}!A3:P912";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
            var range2 = $"WIP!A:C";
            var request2 = service.Spreadsheets.Values.Get(SpreadsheetId, range2);
            //      var response = service.Spreadsheets.Values.Get(SpreadsheetId, range).ExecuteAsync().Result;
            try
            {
                var response = request.Execute();
                var response2 = request2.Execute();
                var values = response.Values;
                var values2 = response2.Values;
                List<string> result = new List<string>();
                if ((values != null && values.Count > 0) && (values2 != null && values2.Count > 0))
                {
                    if (values != null && values.Count > 0)
                    {
                        foreach (var row in values)
                        {
                            string compare = row[0].ToString().ToLower();
                            if (input.Contains("'"))
                            {

                                if (compare.Equals(input.Replace("'", "").ToLower()))
                                {
                                    result.Add($"\n{row[0]}\n{row[6]}\n{Regex.Replace((row[15]).ToString(), @"\r\n?|\n", " ").Replace("At Higher", "\nAt Higher").Replace("Combination", "\nCombination").Replace("•", "\n•")}");
                                    return result;
                                }
                            }
                            else
                            {
                                if (compare.Contains(input.ToLower()))
                                {
                                    result.Add($"\n{row[0]}\n{row[6]}\n{Regex.Replace((row[15]).ToString(), @"\r\n?|\n", " ").Replace("At Higher", "\nAt Higher").Replace("Combination", "\nCombination").Replace("•", "\n•")}");
                                }
                            }
                        }
                    }
                    if (values2 != null && values2.Count > 0)
                    {
                        foreach (var row in values2)
                        {
                            string compare = row[0].ToString().ToLower();
                            if (input[0].Equals("'"))
                            {

                                if (compare.Equals(input.Replace("'", "").ToLower()))
                                {
                                    result.Add($"\n{row[0]}\n{row[1]}\n{Regex.Replace((row[2]).ToString(), @"\r\n?|\n", " ").Replace("At Higher", "\nAt Higher").Replace("Combination", "\nCombination").Replace("•", "\n•")}");
                                    return result;
                                }
                            }
                            else
                            {
                                if (compare.Contains(input.ToLower()))
                                {
                                    result.Add($"\n{row[0]}\n{row[1]}\n{Regex.Replace((row[2]).ToString(), @"\r\n?|\n", " ").Replace("At Higher", "\nAt Higher").Replace("Combination", "\nCombination").Replace("•", "\n•")}");
                                }
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
        static List<string> ReadEntries2(string input)
        {

            if (input.Length < 1)
            {
                return new List<string>() { "Please use your words..." };
            }

            var range = $"{jutsu}!A3:P912";
            var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
            var range2 = $"WIP!A:C";
            var request2 = service.Spreadsheets.Values.Get(SpreadsheetId, range2);
            //      var response = service.Spreadsheets.Values.Get(SpreadsheetId, range).ExecuteAsync().Result;
            try
            {
                var response = request.Execute();
                var response2 = request2.Execute();
                var values = response.Values;
                var values2 = response2.Values;
                List<string> result = new List<string>();
                if ((values != null && values.Count > 0) && (values2 != null && values2.Count > 0))
                {
                    if (values != null && values.Count > 0)
                    {
                        foreach (var row in values)
                        {
                            Jutsu j = new Jutsu();
                            j.Name = $"{row[0]}";
                            j.Info = $"{row[6]}";
                            j.Description = $"{Regex.Replace((row[15]).ToString(), @"\r\n?|\n", " ").Replace("At Higher", "\nAt Higher").Replace("Combination", "\nCombination").Replace("•", "\n•")}";
                            jutsuRepository.Add(j);
                        }
                    }
                    if (values2 != null && values2.Count > 0)
                    {
                        foreach (var row in values2)
                        {

                            Jutsu j = new Jutsu();
                            j.Name = $"{row[0]}";
                            j.Info = $"{row[1]}";
                            j.Description = $"{Regex.Replace((row[2]).ToString(), @"\r\n?|\n", " ").Replace("At Higher", "\nAt Higher").Replace("Combination", "\nCombination").Replace("•", "\n•")}";
                            jutsuRepository.Add(j);
                            //using (var context = new ApplicationDbContext(connectionString))
                            //{
                            //    if (context.Database.CanConnect())
                            //    {
                            //        context.Jutsu.Add(j);
                            //    }
                            //    else { Console.WriteLine("Failed to connect"); }
                            //}
                            result.Add($"\n{row[0]}\n{row[1]}\n{Regex.Replace((row[2]).ToString(), @"\r\n?|\n", " ").Replace("At Higher", "\nAt Higher").Replace("Combination", "\nCombination").Replace("•", "\n•")}");

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
        [Command("massaddjutsu"), Aliases("maj"), Description("Used to look add information of jutsu.")]
        public async Task MassAddEntry(CommandContext ctx)
        {
            var _interactivity = ctx.Client.GetInteractivity();
            string text = File.ReadAllText(@"C:\Users\Public\TestFolder\Jutsu.txt");
            var list = Regex.Replace(text, @"\r\n?|\n", " ").Split("|");
            var x = 1;
            //using (var stream = new FileStream("Naruto5e.json", FileMode.Open, FileAccess.Read))
            //{
            //    credential = GoogleCredential.FromStream(stream)
            //      .CreateScoped(Scopes)
            //      .CreateWithUser("sheets@naruto5e.iam.gserviceaccount.com");
            //}
            //service = new SheetsService(new BaseClientService.Initializer()
            //{
            //    HttpClientInitializer = credential,
            //    ApplicationName = ApplicationName
            //});
            List<Page> pages = new List<Page>();
            List<Jutsu> jutsuListToAdd = new List<Jutsu>();
            List<Jutsu> jutsuListToUpdate = new List<Jutsu>();
            while (x <= list.Count())
            {
                var DBExist2 = FindJutsu(Regex.Replace(list[x - 1], @"\r\n?|\n", " ").Trim());
                var DBExist = FindJutsu(list[x - 1].Trim());
                if (DBExist || DBExist2)
                {
                    Jutsu jutsuToUpdate = jutsuRepository.GetByExactName(Regex.Replace(list[x - 1], @"\r\n?|\n", " ").Trim());
                    jutsuToUpdate.Name = Regex.Replace(list[x - 1], @"\r\n?|\n", " ").Trim();
                    jutsuToUpdate.Info = @$"{list[x].Trim().Replace("Rank:", "\nRank:").Replace("Casting Time:", "\nCasting Time:").Replace("Range:", "\nRange:").Replace("Duration:", "\nDuration:").Replace("Components:", "\nComponents:").Replace("Cost:", "\nCost:").Replace("Keywords:", "\nKeywords:")}";
                    jutsuToUpdate.Description = $@"{Regex.Replace(list[x + 1], @"\r\n?|\n", " ").Trim().Replace("At Higher", "\nAt Higher").Replace("Combination", "\nCombination").Replace("•", "\n•")}";
                    if (!jutsuToUpdate.Info.Contains("Classification:"))
                    {
                        await ctx.Channel.SendMessageAsync($"Check after {list[x - 4]}").ConfigureAwait(false);
                        break;
                    }
                    try
                    {
                        jutsuListToUpdate.Add(jutsuToUpdate);
                        pages.AddRange(_interactivity.GeneratePagesInEmbed($"{jutsuToUpdate.Name} updated successfully!"));
                        x++; x++; x++;
                    }
                    catch
                    {
                        await ctx.Channel.SendMessageAsync($"Error occured on: {list[x - 1]}").ConfigureAwait(false);
                        break;

                    }
                }
                else
                {
                    Jutsu jutsuToAdd = new Jutsu()
                    {
                        Name = Regex.Replace(list[x - 1], @"\r\n?|\n", " ").Trim(),
                        Info = @$"{list[x].Trim().Replace("Rank:", "\nRank:").Replace("Casting Time:", "\nCasting Time:").Replace("Range:", "\nRange:").Replace("Duration:", "\nDuration:").Replace("Components:", "\nComponents:").Replace("Cost:", "\nCost:").Replace("Keywords:", "\nKeywords:")}",
                        Description = $@"{Regex.Replace(list[x + 1], @"\r\n?|\n", " ").Trim().Replace("At Higher", "\nAt Higher").Replace("Combination", "\nCombination").Replace("•", "\n•")}"
                    };
                    if (!jutsuToAdd.Info.Contains("Classification:"))
                    {
                        await ctx.Channel.SendMessageAsync($"Check after {list[x - 4]}").ConfigureAwait(false);
                        break;
                    }
                    try
                    {

                        jutsuListToAdd.Add(jutsuToAdd);
                        pages.AddRange(_interactivity.GeneratePagesInEmbed($"{jutsuToAdd.Name} added successfully!"));
                        x++; x++; x++;

                    }
                    catch
                    {
                        await ctx.Channel.SendMessageAsync($"Error occured on: {list[x - 1]}").ConfigureAwait(false);
                        break;

                    }
                }
                //var baseExist = FindEntry(list[x - 1].Trim());
                //var extendedExist = FindWIPEntry(list[x - 1]);
                //if (baseExist != -1 || extendedExist != -1)
                //{
                //    var trash = UpdateEntry($"{list[x - 1].Trim()} | {list[x].Trim()} | {list[x + 1].Trim()}");
                //    await ctx.Channel.SendMessageAsync($"{list[x - 1]} updated successfully!").ConfigureAwait(false);
                //    x++; x++; x++;
                //}
                //else
                //{
                //    var res = AddEntry($"{list[x - 1]} | {list[x]} | {list[x + 1]}");
                //    if (res)
                //    {
                //        await ctx.Channel.SendMessageAsync($"{list[x - 1]} added successfully").ConfigureAwait(false);
                //        x++; x++; x++;
                //    }
                //    else
                //    {
                //        await ctx.Channel.SendMessageAsync($"Error occured on: {list[x - 1]}").ConfigureAwait(false);
                //        break;
                //    }
                //}

            }
            if (jutsuListToAdd != null)
            {
                //foreach (var item in jutsuListToAdd)
                //{
                //    Console.WriteLine(item.Name);
                //}
                jutsuRepository.Add(jutsuListToAdd);
            }
            if (jutsuListToUpdate != null)
            {
                //foreach (var item in jutsuListToUpdate)
                //{
                //    Console.WriteLine(item.Name);

                //}
                jutsuRepository.Update(jutsuListToAdd);
            }
            await _interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages);
            //await ctx.Channel.SendMessageAsync("Check console.").ConfigureAwait(false);
        }
        bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]) && !Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }
        [Command("masswritejutsu"), Aliases("mwj"), Description("Used to look add information of jutsu.")]
        public async Task MassWriteJutsu(CommandContext ctx)
        {
            //var interactivity = ctx.Client.GetInteractivityModule();
            string text = File.ReadAllText(@"C:\Users\Public\TestFolder\Remaining.txt");
            string newText = string.Empty;
            foreach (string line in System.IO.File.ReadLines(@"C:\Users\Public\TestFolder\Remaining.txt"))
            {
                if (IsAllUpper(line))
                {
                    newText += $"|{line}";
                }
                else
                {
                    newText += line;
                }

            }
            File.WriteAllText(@"C:\Users\Public\TestFolder\NewList.txt", newText);
            await ctx.Channel.SendMessageAsync("Check path.").ConfigureAwait(false);
        }

        [Command("addjutsu"), Aliases("aj"), Description("Used to look add information of jutsu.")]
        public async Task AddEntry(CommandContext ctx, [Description("The search parameter, IE Jutsu Name. Can use 's for an exact search.")] params string[] args)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => (e.Name == "Mod") || (e.Name == "DM")) == null && Turn != ctx.Member.Mention)
            {
                await ctx.Channel.SendMessageAsync("Incorrect User.").ConfigureAwait(false);
            }
            else
            {

                await ctx.Channel.TriggerTypingAsync();

                //using (var stream = new FileStream("Naruto5e.json", FileMode.Open, FileAccess.Read))
                //{
                //    credential = GoogleCredential.FromStream(stream)
                //      .CreateScoped(Scopes)
                //      .CreateWithUser("sheets@naruto5e.iam.gserviceaccount.com");
                //}
                //service = new SheetsService(new BaseClientService.Initializer()
                //{
                //    HttpClientInitializer = credential,
                //    ApplicationName = ApplicationName
                //});


                var result = AddJutsu(string.Join(" ", args));
                if (result == true)
                {
                    //await ctx.Channel.DeleteMessageAsync(await ctx.Channel.GetMessageAsync(ctx.Channel.LastMessageId));
                    var text = string.Join(" ", args).Split("|");
                    //Console.WriteLine(text[0].ToString());
                    await ctx.Channel.SendMessageAsync($"{text[0]} Successfully added!");
                }
                else { await ctx.Channel.SendMessageAsync("Uh-Oh!"); }
            }


        }
        //[Command("updatejutsu"), Aliases("uj"), Description("Used to look add information of jutsu.")]
        //public async Task UpdateEntry(CommandContext ctx, [Description("The search parameter, IE Jutsu Name. Can use 's for an exact search.")] params string[] args)
        //{
        //    if (ctx.Member.Roles.ToList().FirstOrDefault(e => (e.Name == "Mod") || (e.Name == "DM")) == null && Turn != ctx.Member.Mention)
        //    {
        //        await ctx.Channel.SendMessageAsync("Incorrect User.").ConfigureAwait(false);
        //    }
        //    else
        //    {

        //        await ctx.Channel.TriggerTypingAsync();

        //        using (var stream = new FileStream("Naruto5e.json", FileMode.Open, FileAccess.Read))
        //        {
        //            credential = GoogleCredential.FromStream(stream)
        //              .CreateScoped(Scopes)
        //              .CreateWithUser("sheets@naruto5e.iam.gserviceaccount.com");
        //        }
        //        service = new SheetsService(new BaseClientService.Initializer()
        //        {
        //            HttpClientInitializer = credential,
        //            ApplicationName = ApplicationName
        //        });


        //        var result = UpdateEntry(string.Join(" ", args));
        //        if (result == true) { await ctx.Channel.SendMessageAsync("Successfully updated!"); }
        //        else { await ctx.Channel.SendMessageAsync("Uh-Oh!"); }

        //    }

        //}
        static bool AddJutsu(string input)
        {
            if (input.Length < 1) { return false; }
            try
            {
                var text = input.Split("|");
                var jutsuToAdd = new Jutsu()
                {
                    Name = text[0],
                    Info = text[1],
                    Description = text[2]
                };
                jutsuRepository.Add(jutsuToAdd);
                return true;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        static bool AddEntry(string input)
        {
            if (input.Length < 1) { return false; }
            try
            {
                var jutsuToAdd = input.Split("|");
                var jutsuName = jutsuToAdd[0].Replace("`", string.Empty);
                var jutsuInfo = jutsuToAdd[1].Replace("`", string.Empty);
                var jutsuDesc = jutsuToAdd[2].Replace("`", string.Empty);
                var range = $"WIP!A:C";
                var valueRange = new ValueRange();
                var objectList = new List<object>() { jutsuName, jutsuInfo, jutsuDesc };
                valueRange.Values = new List<IList<object>> { objectList };
                var appedRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
                appedRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appedResponse = appedRequest.Execute();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        static bool FindJutsu(string search)
        {
            IJutsuRepository tempJutsuRepository = new JutsuRepository(new ApplicationDbContext(connectionString));
            if (tempJutsuRepository.GetByExactName(search) != null)
            {
                return true;
            }
            return false;
        }
        //static int FindWIPEntry(string search)
        //{
        //    //var range = $"{jutsu}!A3:P912";
        //    //var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
        //    var range = $"WIP!A:C";
        //    var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
        //    try
        //    {
        //        var response = request.Execute();
        //        var values = response.Values;
        //        foreach (var row in values)
        //        {
        //            string compare = row[0].ToString().ToLower().Trim();

        //            if (compare.Equals(search.ToLower().Trim()))
        //            {
        //                return values.IndexOf(row);
        //            }

        //        }
        //    }
        //    catch (Google.GoogleApiException ex)
        //    {
        //        return -1;
        //    }
        //    catch (Exception ex)
        //    {
        //        return -1;
        //    }

        //    return -1;
        //}
        //static int FindEntry(string search)
        //{
        //    //var range = $"{jutsu}!A3:P912";
        //    //var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
        //    var range = $"{jutsu}!A1:P1000";
        //    var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
        //    try
        //    {
        //        var response = request.Execute();
        //        var values = response.Values;
        //        search = search.ToLower().Trim();
        //        foreach (var row in values)
        //        {
        //            string compare = row[0].ToString().ToLower().Trim();

        //            if (compare.Equals(search))
        //            {
        //                return values.IndexOf(row);
        //            }

        //        }
        //    }
        //    catch (Google.GoogleApiException ex)
        //    {
        //        Console.WriteLine($"Google Exception: {ex.Message}");
        //        return -1;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"System Exception: {ex.Message}");
        //        return -1;
        //    }

        //    return -1;
        //}
        //static bool UpdateEntry(string input)
        //{
        //    if (input.Length < 1) { return false; }
        //    try
        //    {
        //        var jutsuToAdd = input.Split("|");
        //        //Console.WriteLine($"{jutsuToAdd[0]} {jutsuToAdd[1]} {jutsuToAdd[2]} ");
        //        var jutsuName = jutsuToAdd[0].Replace("`", string.Empty);
        //        var jutsuInfo = jutsuToAdd[1].Replace("`", string.Empty);
        //        var jutsuDesc = jutsuToAdd[2].Replace("`", string.Empty);
        //        var WIPEntry = FindWIPEntry(jutsuName);
        //        var JutsuEntry = FindEntry(jutsuName);
        //        var range = "";
        //        if (WIPEntry != -1)
        //        {
        //            range = $"WIP!A{WIPEntry + 1}";
        //            var valueRange = new ValueRange();
        //            var objectList = new List<object>() { jutsuName, jutsuInfo, jutsuDesc };
        //            valueRange.Values = new List<IList<object>> { objectList };
        //            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
        //            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        //            var updateResponse = updateRequest.Execute();
        //            return true;
        //        }
        //        if (JutsuEntry != -1)
        //        {
        //            range = $"{jutsu}!A{JutsuEntry + 1}";
        //            var valueRange = new ValueRange();
        //            var objectList = new List<object>() { jutsuName };
        //            valueRange.Values = new List<IList<object>> { objectList };
        //            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
        //            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        //            updateRequest.Execute();
        //            range = $"{jutsu}!G{JutsuEntry + 1}";
        //            objectList = new List<object>() { jutsuInfo };
        //            valueRange.Values = new List<IList<object>> { objectList };
        //            updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
        //            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        //            updateRequest.Execute();
        //            range = $"{jutsu}!P{JutsuEntry + 1}";
        //            objectList = new List<object>() { jutsuDesc };
        //            valueRange.Values = new List<IList<object>> { objectList };
        //            updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
        //            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
        //            var updateResponse = updateRequest.Execute();
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Console.WriteLine($"Exception: {ex.Message}");
        //        return false;
        //    }
        //}

        [Command("Feat"), Aliases("f"), Description("Used to look up information of Feats.")]
        public async Task Feat(CommandContext ctx, [Description("The search parameter, IE condition Name. Can use 's for an exact search.")] params string[] args)
        {
            await ctx.Channel.TriggerTypingAsync();
            var result = ReadFeatsDB(string.Join(" ", args));
            var _interactivity = ctx.Client.GetInteractivity();
            List<Page> pages = new List<Page>();

            foreach (var res in result)
            {
                pages.AddRange(_interactivity.GeneratePagesInEmbed($@"{res.Name}
{res.Info}"));
            }
            await _interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages);
            //var msg = await ctx.Channel.GetMessageAsync(ctx.Channel.LastMessageId);
            //await ctx.Channel.DeleteMessageAsync(msg);
        }
        static List<Feat> ReadFeatsDB(string input)
        {
            if (input.Length < 1)
            {
                return new List<Feat>() { new Models.Feat() { Name = "No Results...", Info = "" } };
            }
            var tempFeatRepository = new FeatRepository(new ApplicationDbContext(connectionString));
            return tempFeatRepository.GetLike(input);



        }
        static List<string> ReadFeats(string input)
        {
            if (input.Length < 1)
            {
                return new List<string>() { "Please use your words..." };
            }

            var range = $"{feat}!A3:P630";
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
            Console.WriteLine("Found command");
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
            var _interactivity = ctx.Client.GetInteractivity();
            List<Page> pages = new List<Page>();

            foreach (var res in result)
            {
                pages.AddRange(_interactivity.GeneratePagesInEmbed(res));
            }

            await _interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.Member, pages).ConfigureAwait(false);
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

            var range = $"{con}!A3:P28";
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
            
            var msgList = await ctx.Channel.GetMessagesAsync(50);
            var msg = msgList.Where(m => m.Author.Username == "TousenBot").First();

            await ctx.Channel.DeleteMessageAsync(msg);
        }
        [Command("add"), Description("Command used to add numbers using the syntax 'add number number ...'")]
        public async Task Add(CommandContext ctx, params int[] numbers)
        {
            await ctx.Channel.TriggerTypingAsync();
            await ctx.Channel.SendMessageAsync(numbers.Sum().ToString()).ConfigureAwait(false);
        }
        [Command("sub"), Description("Command used to add numbers using the syntax 'add number number ...'")]
        public async Task Sub(CommandContext ctx, params int[] numbers)
        {
            var start = numbers[0];
            for (int i = 1; i < numbers.Length; i++)
            {
                start -= numbers[i];
            }
            await ctx.Channel.TriggerTypingAsync();

            await ctx.Channel.SendMessageAsync(start.ToString()).ConfigureAwait(false);
        }
        [Command("div"), Description("Command used to add numbers using the syntax 'add number number ...'")]
        public async Task Div(CommandContext ctx, decimal num1, decimal num2)
        {
            Console.WriteLine(num1 / num2);
            decimal result = num1 / num2;
            await ctx.Channel.TriggerTypingAsync();
            await ctx.Channel.SendMessageAsync(result.ToString()).ConfigureAwait(false);
        }
        [Command("MeleeCritRoll"),Aliases("MCrit"),Description("Roll on the melee crit table!")]
        public async Task MCrit(CommandContext ctx)
        {
            var resultArray = Roll(1, 100);
            var result = Convert.ToInt32(resultArray[0].Replace("[", string.Empty).Replace("]", string.Empty));
            await ctx.Channel.TriggerTypingAsync();
            var message1 = CritMessage1("melee",result);
            var message2 = CritMessage2("melee",result);
            await ctx.Channel.SendMessageAsync(message1).ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(message2).ConfigureAwait(false);

        }
        private string CritMessage2(string type, int result)
        {
            if (type.Equals("melee"))
            {
                switch (result)
                {
                    case 1:
                        return "Regular critical hit.";
                    case >= 2 and <= 5:
                        return "You can choose to gain advantage on all attacks against your target until the end of your next turn, but if you do all enemies have advantage on their attack rolls against you until the end of your next turn.";
                    case >= 6 and <= 9:
                        return "You can choose to gain advantage on all attacks against your target next turn, your target has advantage on their attack rolls against you until the end of your next turn.";
                    case >= 10 and <= 14:
                        return "You gain advantage on all attacks against your target until the end of your next turn.";
                    case >= 15 and <= 19:
                        return "You gain advantage on all attacks against your target until the end of your next turn.";
                    case >= 20 and <= 24:
                        return "After your turn you move to the top of the initiative order.";
                    case >= 25 and <= 29:
                        return "You gain +2 to your AC against your target, and advantage on all savings throws from effects originating from your target until your next turn.";
                    case >= 30 and <= 39:
                        return "After your attack you can choose to attempt to grapple your opponent if you have a free hand, or attempt to shove your opponent if both hands are in use.";
                    case >= 40 and <= 49:
                        return "After your attack you can choose to automatically succeed in grappling your opponent if you have a free hand, or shoving your opponent if both hands are in use.";
                    case >= 50 and <= 59:
                        return "You are able to take the disarm action after your attack";
                    case >= 60 and <= 69:
                        return "You are able to take the disarm action after your attack, and can steal your opponents weapon if you have a free hand. Otherwise you can knock it up to 20 feet away.";
                    case >= 70 and <= 74:
                        return "You are able to use the dodge action after your attack.";
                    case >= 75 and <= 79:
                        return "Your target is knocked prone.";
                    case >= 80 and <= 84:
                        return "Your target is surprised until the end of their next turn.";
                    case >= 85 and <= 89:
                        return "Roll an additional set of damage dice above and beyond your normal critical roll.";
                    case >= 90 and <= 94:
                        return "Roll an additional set of damage dice above and beyond your normal critical roll, and the target suffers one unit of exhaustion.";
                    case >= 95 and <= 99:
                        return "Roll an additional set of damage dice above and beyond your normal critical roll, and the target suffers a permanent injury chosen by the DM. The permanent injury can be healed with extended rest of a length determined by the DM, but the attack leaves a scar.";
                    case 100:
                        return "Roll an additional set of damage dice above and beyond your normal critical roll, and the target suffers 1 unit of exhaustion, and the target suffers a permanent injury chosen by the DM. The permanent injury can be healed with extended rest of a length determined by the DM, but the attack leaves a scar.";
                    default:
                        break;
                }

            }
            return "Type not found";

        }
        private string CritMessage1(string type, int result)
        {
            if (type.Equals("melee"))
            {
                switch (result)
                {
                    case 1:
                        return "You feel accomplished, but nothing remarkable happens.";
                    case >= 2 and <= 5:
                        return "You feel it is imperative to press the advantage no matter the cost.";
                    case >= 6 and <= 9:
                        return "You feel it is imperative to press the advantage, but maintain awareness of your surroundings.";
                    case >= 10 and <= 14:
                        return "You know how to press the advantage.";
                    case >= 15 and <= 19:
                        return "As you are fighting, you notice an effective route to escape danger.";
                    case >= 20 and <= 24:
                        return "You feel the eb and flow of the battle, and know where to make your next move.";
                    case >= 25 and <= 29:
                        return "You begin to recognize patterns in your opponents fighting technique.";
                    case >= 30 and <= 39:
                        return "You are able to maneuver towards your opponent while attacking, and attempt to harass them.";
                    case >= 40 and <= 49:
                        return "You are able to maneuver towards your opponent while attacking and harass them.";
                    case >= 50 and <= 59:
                        return "You attempt to disarm your opponent.";
                    case >= 60 and <= 69:
                        return "You kick your target’s weapon out of their hands.";
                    case >= 70 and <= 74:
                        return "Your senses heighten and you become aware of threats around the battlefield.";
                    case >= 75 and <= 79:
                        return "Your attack knocks your target over.";
                    case >= 80 and <= 84:
                        return "Your strike surprises your opponent.";
                    case >= 85 and <= 89:
                        return "You strike with great force.";
                    case >= 90 and <= 94:
                        return "You strike with extreme force.";
                    case >= 95 and <= 99:
                        return "You strike with debilitating force.";
                    case 100:
                        return "You strike with devastating force.";
                    default:
                        break;
                }

            }
            return "Type not found";
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
            //var rand = new Random();
            int result = 0;
            string output = "";
            for (int x = 0; x < amount; x++)
            {
                var rand = new byte[4];
                var number = 0;
                while (number < 1 || number > die)
                {
                using (var rng = new RNGCryptoServiceProvider())
                    rng.GetBytes(rand);
                number = Math.Abs(BitConverter.ToInt32(rand, 0));
                number = Convert.ToInt32(number % (die - amount + 1) + amount);
                }
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
        private int SubArray(string[] toAdd)
        {
            int result = 0;
            foreach (string x in toAdd)
            {
                result -= Int32.Parse(x);
            }
            return result;
        }
        [Command("Next"), Aliases("End"), Description("Pushes initiative to the next turn.")]
        public async Task Next(CommandContext ctx)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => (e.Name == "Mod") || (e.Name == "DM")) == null && Turn != ctx.Member.Mention)
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
        [Command("Remove"), Aliases("Kill"), Description("Remove's a player from the tracker.")]
        public async Task Remove(CommandContext ctx, string player)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => (e.Name == "Mod") || (e.Name == "DM")) == null)
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
        [Command("IAdd"), Description("Adds a player before a referenced player. Otherwise, adds to end of tracker.")]
        public async Task iAdd(CommandContext ctx, string player, string reference = null)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => (e.Name == "Mod") || (e.Name == "DM")) == null)
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
        [Command("IShow"), Description("Shows the initiative.")]
        public async Task iShow(CommandContext ctx)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => (e.Name == "Mod") || (e.Name == "DM")) == null)
            {
                await ctx.Channel.SendMessageAsync("Improper Access!").ConfigureAwait(false);
            }
            var temp = new List<string>();
            temp.AddRange(Tracker);
            if (ctx.Guild.Members.FirstOrDefault(e => e.Value.Mention == Turn).Value.Mention != null)
            {
                temp[temp.FindIndex(e => e.Equals(Turn))] = $">{ctx.Guild.Members.First(e => e.Value.Mention == Turn).Value.Nickname}<";
            }
            if (ctx.Guild.Members.FirstOrDefault(e => e.Value.Mention == Turn).Value.Mention == null)
            {
                temp[temp.FindIndex(e => e.Equals(Turn))] = $">{Turn}<";
            }
            //foreach (var person in temp)
            //{
            //    if (ctx.Guild.Members.FirstOrDefault(e => e.Mention == person) != null)
            //    {
            //        //                   temp[temp.FindIndex(e => e.Equals(person))] = $"{string.Join("", ctx.Guild.Members.First(e => e.Mention == person).Nickname)}";
            //        temp[temp.FindIndex(e => e.Equals(person))] = $"{string.Join("", ctx.Guild.Members.First(e => e.Mention == person).Nickname)}";
            //   }
            //}

            var list = string.Join("\n", temp);
            await ctx.Channel.SendMessageAsync($"`\n{list}`").ConfigureAwait(false);

        }
        [Command("Initiative"), Aliases("Tracker", "Track"), Description("Command used to start initiative tracker")]
        public async Task Init(CommandContext ctx, params string[] players)
        {
            if (ctx.Member.Roles.ToList().FirstOrDefault(e => (e.Name == "Mod") || (e.Name == "DM")) == null)
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
