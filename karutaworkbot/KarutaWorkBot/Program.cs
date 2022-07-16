using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using KarutaWorkBot.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace KarutaWorkBot
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public static DiscordSocketClient _client;
        public CommandService _commands;
        public Commands commands = new Commands();
        public IServiceProvider _services;

        public static SocketUser recentUser;
        public static Card recentCard;
        public static Card predictCard;
        public static SocketUserMessage recentCollection;
        public static List<Card> recentCardList = new List<Card>();
        public static IMessageChannel commandChannel;

        public static RestUserMessage guideMessage;
        public static RestUserMessage viewMessage;
        public static RestUserMessage workMessage;

        public static bool disabled = true;

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string token = TOKEN;

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();

            await Task.Delay(-1);

            commandChannel = _client.GetChannel(787103017676570635) as IMessageChannel;

            recentUser = _client.CurrentUser;
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            commandChannel = _client.GetChannel(787103017676570635) as IMessageChannel;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            commandChannel = _client.GetChannel(787103017676570635) as IMessageChannel;
            // if (message.Author.IsBot) return; // not using because of how the bot receives work data.

            int argPos = 0;

            Embed embed1 = null;
            //Console.WriteLine(message);
            bool hasDescription = false;
            if (context.User.Id == 646937666251915264 && !message.Content.StartsWith("<@") && !message.Content.StartsWith("@"))
            {
                embed1 = message.Embeds.ElementAtOrDefault(0);
                try
                {
                    //Console.WriteLine($"Got Message: [{embed1.Title}] - [{embed1.Description}]");
                    hasDescription = true;
                }
                catch
                {
                    Console.WriteLine($"Message has no description.");
                }

                if (hasDescription)
                {
                    //Console.WriteLine("Has description!");
                    if (!disabled && recentUser != null && embed1.Description.Contains($"Cards carried by <@{recentUser.Id}>")) // If it is a collection embed
                    {
                        Console.WriteLine($"User was the recent user. [{recentUser.Username}]");
                        recentCollection = message;

                        #region Codes
                        string codePattern = @"\*\*`([A-Za-z0-9]){5,6}`\*\*";
                        Regex rgcode = new Regex(codePattern);
                        MatchCollection codes = rgcode.Matches(embed1.Description);
                        #endregion

                        #region Print
                        string printPattern = @"`#([0-9]){1,5}";
                        Regex rgprint = new Regex(printPattern);
                        MatchCollection prints = rgprint.Matches(embed1.Description);
                        #endregion

                        #region Show & Name
                        string showNamePattern = @"· ([A-Za-z0-9'.!?:;@\-\s]){1,100} · \*\*([A-Za-z0-9'.!?:;@\-\s]){1,100}\*\*";
                        Regex rgshowName = new Regex(showNamePattern);
                        MatchCollection showsNames = rgshowName.Matches(embed1.Description);

                        List<string> shows = new List<string>();
                        List<string> names = new List<string>();
                        foreach (Match showName in showsNames)
                        {
                            shows.Add(showName.ToString().Trim('·').Split('·')[0]);
                            names.Add(showName.ToString().Trim('·').Split('·')[1]);
                        }
                        #endregion

                        List<Card> cardList = new List<Card>();
                        for (int i = 0; i < codes.Count; i++)
                        {
                            cardList.Add(new Card(codes[i].ToString().Trim('*').Trim('`'),
                                Int32.Parse(prints[i].ToString().Trim('`').Trim(' ').TrimStart('#')),
                                shows[i].Trim('*'),
                                names[i].Trim('*')));
                            Card card = cardList[i];
                            Console.WriteLine($"Card: {card.name} - {card.show} : {card.code} - #{card.print}");
                        }
                        await checkCards(cardList, message);
                    }
                    else if (!disabled && recentCard != null && embed1.Description.Contains($"{recentCard.name}") && embed1.Title.Contains("Worker Details"))
                    {
                        Console.WriteLine($"Character work info found for {recentCard.name}");
                        foreach (Card card in recentCardList)
                        {
                            if (card.name == recentCard.name)
                            {
                                card.baseValue = 100;
                                await checkCards(recentCardList, message);
                            }
                        }
                    }
                    else if (predictCard != null)
                    {
                        if (embed1.Description.Contains($"**`{predictCard.code}`**"))
                        {
                            string showNamePattern = @"· ([A-Za-z0-9'.!?:;@\-\s]){1,100} · \*\*([A-Za-z0-9'.!?:;@\-\s]){1,100}\*\*";
                            Regex rgshowName = new Regex(showNamePattern);
                            Match showName = rgshowName.Match(embed1.Description);

                            predictCard.show = showName.ToString().Trim('·').Split('·')[0];
                            predictCard.show = Regex.Replace(predictCard.show.Trim(' ').Trim('*'), @"\s+", " ");

                            predictCard.name = showName.ToString().Trim('·').Split('·')[1];
                            predictCard.name = Regex.Replace(predictCard.name.Trim(' ').Trim('*'), @"\s+", " ");

                            string qualityPattern = @"`([★☆]){4}`";
                            Regex rgQuality = new Regex(qualityPattern);
                            Match quality = rgQuality.Match(embed1.Description);
                            //Console.WriteLine($"{predictCard.name} - [{quality}]");

                            predictCard.quality = quality.ToString().Trim('`');

                            await Task.Delay(1000);
                            predictCard.cardImage = (EmbedImage)embed1.Image;
                            predictCard.cardImageUrl = predictCard.cardImage.ProxyUrl;

                            await message.DeleteAsync();
                            workMessage = await context.Channel.SendMessageAsync($"kwi {predictCard.code}");
                        }
                        else
                        {
                            await message.DeleteAsync();
                            await context.Channel.SendMessageAsync($"That is not the card with code **`{predictCard.code}`**, try again!");
                        }
                    }
                    else if (predictCard != null && embed1.Description.Contains($"{predictCard.name}") && embed1.Title.Contains("Worker Details"))
                    {
                        string effortPattern = @"Effort · \*\*([0-9]){1,100}\*\*";
                        Regex rgEffort = new Regex(effortPattern);
                        Match effortMatch = rgEffort.Match(embed1.Description);

                        predictCard.effort = Int32.Parse(effortMatch.ToString().Split('·')[1].Trim(' ').Trim('*'));

                        if (predictCard.quality != null && predictCard.quality.Equals("★★★★", StringComparison.OrdinalIgnoreCase))
                        {
                            predictCard.quality = "mint";
                            predictCard.damaged = (int)(predictCard.effort / 1.9 / 1.9 / 1.9 / 1.9);
                        }
                        else if (predictCard.quality.Equals("★★★☆", StringComparison.OrdinalIgnoreCase))
                        {
                            predictCard.quality = "excellent";
                            predictCard.damaged = (int)(predictCard.effort / 1.9 / 1.9 / 1.9);
                        }
                        else if (predictCard.quality.Equals("★★☆☆", StringComparison.OrdinalIgnoreCase))
                        {
                            predictCard.quality = "good";
                            predictCard.damaged = (int)(predictCard.effort / 1.9 / 1.9);
                        }
                        else if (predictCard.quality.Equals("★☆☆☆", StringComparison.OrdinalIgnoreCase))
                        {
                            predictCard.quality = "poor";
                            predictCard.damaged = (int)(predictCard.effort / 1.9);
                        }
                        else if (predictCard.quality.Equals("☆☆☆☆", StringComparison.OrdinalIgnoreCase))
                        {
                            predictCard.quality = "damaged";
                            predictCard.damaged = predictCard.effort;
                        }
                        else
                        {
                            Console.WriteLine($"{predictCard.name}'s quality is not readable ({predictCard.quality})");
                        }

                        predictCard.poor =      (int)(predictCard.damaged * 1.9);
                        predictCard.good =      (int)(predictCard.damaged * 1.9 * 1.9);
                        predictCard.excellent = (int)(predictCard.damaged * 1.9 * 1.9 * 1.9);
                        predictCard.mint =      (int)(predictCard.damaged * 1.9 * 1.9 * 1.9 * 1.9);

                        var eb = new EmbedBuilder();

                        eb.WithColor(new Color(139, 71, 179));
                        eb.WithTitle("**Effort Prediction**");
                        eb.WithDescription( $"**Owned by <@{recentUser.Id}>**\n\n" +
                                            $"Character · **{predictCard.name}**\n" +
                                            $"Series · **{predictCard.show}**\n\n" +
                                            $"Current Effort: **{predictCard.effort}**\n\n" +
                                            $"★★★★ · **{predictCard.mint}**\n" +
                                            $"★★★☆ · **{predictCard.excellent}**\n" +
                                            $"★★☆☆ · **{predictCard.good}**\n" +
                                            $"★☆☆☆ · **{predictCard.poor}**\n" +
                                            $"☆☆☆☆ · **{predictCard.damaged}**\n");

                        //eb.WithThumbnailUrl(recentUser.GetAvatarUrl());
                        eb.WithImageUrl(predictCard.cardImageUrl);

                        await message.DeleteAsync();
                        await guideMessage.DeleteAsync();
                        await viewMessage.DeleteAsync();
                        await workMessage.DeleteAsync();
                        await context.Channel.SendMessageAsync("", false, eb.Build());
                        recentUser = null;
                        predictCard = null;
                    }
                }
            }
            if (message.HasStringPrefix("w", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
            if (predictCard != null && context.User == predictCard.user)
            {
                if (message.Content == $"kv {predictCard.code}" || message.Content == $"kwi {predictCard.code}")
                {
                    await Task.Delay(100);
                    await message.DeleteAsync();
                }
            }
        }

        public async Task checkCards(List<Card> cards, SocketUserMessage message)
        {
            foreach (Card card in cards)
            {
                if (card.baseValue == 0) // If the card's work hasn't been checked
                {
                    await Task.Delay(2000);
                    await commandChannel.SendMessageAsync($"kwi {card.code}"); // Check work
                    recentCard = card;
                    recentCardList = cards;
                    return;
                }
            }

            string cardMessage = $"";
            foreach (Card card in cards)
            {
                cardMessage = cardMessage + $"**{card.name.Trim('*').Trim(' ')}** · {card.show.Trim('*').Trim(' ')} **`{card.code}`** - Base Value: **{card.baseValue}**\n";
            }
            await commandChannel.SendMessageAsync(cardMessage);
            Console.WriteLine("\n");
            foreach (KeyValuePair<IEmote, ReactionMetadata> kvp in message.Reactions)
            {
                Console.WriteLine($"{kvp.Key} - {kvp.Value}");
            }
            Console.WriteLine("\n");
            Emoji rightEmoji = new Emoji("➡");
            await Task.Delay(2000);
            await message.AddReactionAsync(rightEmoji);
        }
    }
}
