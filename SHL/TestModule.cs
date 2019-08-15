using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace SHL
{
    // Create a module with no prefix
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        // ~say hello world -> hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
            => ReplyAsync(echo);

        // ReplyAsync is a method on ModuleBase 
    }

    public class SHLModule : ModuleBase<SocketCommandContext>
    {
        [Command("gameindex")]
        public async Task GameIndexAsync(int gameNumber)
        {
            var url = $"http://simulationhockey.com/games/smjhl/S49/Season/SMJHL-{gameNumber}.html";
            var scraper = new GameIndexScraper();
            var goals = scraper.GetGoals("49", "shl", gameNumber);
            var boxscore = scraper.GetBoxScore("49", "shl", gameNumber);

            var goalsString = string.Empty;
            goalsString += "**1st Period:**";
            goalsString += String.Join("\n", goals.Where(x => x.Period == 1).Select(x => x.Info));
            goalsString += "\n**2nd Period:**";
            goalsString += String.Join("\n", goals.Where(x => x.Period == 2).Select(x => x.Info));
            goalsString += "\n**3rd Period:**";
            goalsString += String.Join("\n", goals.Where(x => x.Period == 3).Select(x => x.Info));
            if (goals.Any(x => x.Period == 4))
            {
                goalsString += "\n**OT Period:**";
                goalsString += String.Join("\n", goals.Where(x => x.Period == 4).Select(x => x.Info));
            }
            if (goals.Any(x => x.Period == 5))
            {
                goalsString += "\n**Shootout:**";
                goalsString += String.Join("\n", goals.Where(x => x.Period == 5).Select(x => x.Info));
            }
            await ReplyAsync($"{boxscore}\n\n{goalsString}");
        }
    }

    // Create a module with the 'sample' prefix
    [Group("sample")]
    public class SampleModule : ModuleBase<SocketCommandContext>
    {
        // ~sample square 20 -> 400
        [Command("square")]
        [Summary("Squares a number.")]
        public async Task SquareAsync(
            [Summary("The number to square.")]
            int num)
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }

        // ~sample userinfo --> foxbot#0282
        // ~sample userinfo @Khionu --> Khionu#8708
        // ~sample userinfo Khionu#8708 --> Khionu#8708
        // ~sample userinfo Khionu --> Khionu#8708
        // ~sample userinfo 96642168176807936 --> Khionu#8708
        // ~sample whois 96642168176807936 --> Khionu#8708
        [Command("userinfo")]
        [Summary
            ("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(
            [Summary("The (optional) user to get info from")]
            SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }
    }
}
