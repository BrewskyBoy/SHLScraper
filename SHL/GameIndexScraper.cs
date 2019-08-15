using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using RestSharp;
using Int16 = System.Int16;

namespace SHL
{
    public class GameIndexScraper
    {
        List<Goal> goals = new List<Goal>();
        List<Penalty> penalties = new List<Penalty>();

        public GameIndexScraper() { }

        public string GetBoxScore(string seasonNumber, string leagueCode, int gameNumber)
        {
            var web = new HtmlWeb();
            var doc = web.Load(
                $"http://simulationhockey.com/games/{leagueCode.ToLower()}/S{seasonNumber}/Season/{leagueCode.ToUpper()}-{gameNumber}.html");

            var nodes = doc.DocumentNode.SelectNodes("//table[contains(@class, 'STHSGame_GoalsTable')]/tr");
            var table = new DataTable("GoalsTable");

            var headers = nodes[0]
                .Elements("td")
                .Select(th => th.InnerText.Trim());
            foreach (var header in headers)
            {
                table.Columns.Add(header);
            }

            var rows = nodes.Select(tr => tr
                .Elements("td")
                .Select(td => td.InnerText.Trim())
                .ToArray());
            foreach (var row in rows)
            {
                table.Rows.Add(row);
            }

            var columnMaxLengths = new Dictionary<int, int>();
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var maxLength = 0;
                foreach (DataRow row in table.Rows)
                {
                    if (row.Field<string>(i).Length > maxLength)
                    {
                        maxLength = row.Field<string>(i).Length;
                    }
                }
                columnMaxLengths.Add(i, maxLength);

                foreach (DataRow row in table.Rows)
                {
                    if (row.Field<string>(i).Length != maxLength)
                    {
                        row.SetField(i, row.Field<string>(i).PadRight(maxLength));
                    }
                }
            }

            var columnWidthStrings = new List<string>();
            for (var i = 0; i < columnMaxLengths.Count; i++)
            {
                columnWidthStrings.Add(new string('-', columnMaxLengths[i] + 2));
            }

            var borderString = $"+{string.Join("+", columnWidthStrings)}+";
            var headerString = $"| {string.Join(" | ", table.Rows[0].ItemArray)} |";
            var awayString = $"| {string.Join(" | ", table.Rows[1].ItemArray)} |";
            var homeString = $"| {string.Join(" | ", table.Rows[2].ItemArray)} |";

            return
                $"```{borderString}\n{headerString}\n{borderString}\n{awayString}\n{borderString}\n{homeString}\n{borderString}```";
        }

        public List<Goal> GetGoals(string seasonNumber, string leagueCode, int gameNumber)
        {
            var web = new HtmlWeb();
            var doc = web.Load(
                $"http://simulationhockey.com/games/{leagueCode.ToLower()}/S{seasonNumber}/Season/{leagueCode.ToUpper()}-{gameNumber}.html");

            for (int i = 1; i <= 5; i++)
            {
                try
                {
                    foreach (HtmlNode row in doc.DocumentNode.SelectNodes(
                        $"//div[contains(@class, 'STHSGame_GoalPeriod{i}')]"))
                    {
                        goals.Add(new Goal
                        {
                            Period = i,
                            Info = row.InnerText
                        });
                    }
                }
                catch (Exception e)
                {

                }
            }

            return goals;
        }
    }

    public class Goal
    {
        public int Period { get; set; }
        public string Info { get; set; }
    }

    public class Penalty
    {
        public int Period { get; set; }
        public string Info { get; set; }
    }
}
