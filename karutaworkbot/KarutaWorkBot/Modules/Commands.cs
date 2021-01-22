using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace KarutaWorkBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        [Command("invite")]
        [Alias("inv", "i")]
        public async Task Invite()
        {
            await ReplyAsync("Invite Karuta Work to your server: <https://bit.ly/3mee4US>");
        }

        // [RequireUserPermission(GuildPermission.Administrator)]
        // [Command("k!collection")]
        // [Alias("kcollection", "kc", "kcol", "c", "collection")]
        // public async Task MultiTrade()
        // {
        //     Program.recentUser = Context.User;
        //     await Program.commandChannel.SendMessageAsync($"k!c {Context.User.Id} o:w");
        // }

        // [RequireUserPermission(GuildPermission.Administrator)]
        // [Command("reactions")]
        // [Alias("r")]
        // public async Task Reactions(ulong id)
        // {
        //     var message = await Context.Channel.GetMessageAsync(id);

        //     foreach (KeyValuePair<IEmote, ReactionMetadata> kvp in message.Reactions)
        //     {
        //         Console.WriteLine(kvp.Key.Name + " - " + kvp.Value);
        //     }
        // }

        [Command("predict")]
        [Alias("p", "calc", "calculate")]
        public async Task Predict(string code)
        {
            Program.recentUser = Context.User;
            var eb = new EmbedBuilder();
            eb.WithTitle($"Copy my next messages and send them in the chat to receive the effort prediction of **`{code}`**");

            Program.guideMessage = await Context.Channel.SendMessageAsync("", false, eb.Build());

            Program.predictCard = new Card("", 0, "", "");
            Program.predictCard.code = code;
            Program.predictCard.user = Context.User;

            Program.viewMessage = await Context.Channel.SendMessageAsync($"kv {code}");
        }
    }
}