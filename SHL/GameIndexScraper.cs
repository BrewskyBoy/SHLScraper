using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronWebScraper;

namespace SHL
{
    public class GameIndexScraper : WebScraper
    {
        List<string> goals = new List<string>();

        public override void Init(string gameNumber)
        {
            this.LoggingLevel = WebScraper.LogLevel.All;
            this.ObeyRobotsDotTxt = false;
            this.Request("http://simulationhockey.com/games/smjhl/S49/Season/SMJHL-17.html", Parse);
        }
        public override void Parse(Response response)
        {
            foreach (var goal in response.XPath("/html/body/div[1]"))
            {
                string goalString = goal.TextContentClean;
                goals.Add(goalString);
            }
        }

        public List<string> GetGoals()
        {
            return goals;
        }
    }
}
