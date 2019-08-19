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

        public string GetSchedule(string seasonNumber, string leagueCode, string team)
        {
            var web = new HtmlWeb();
            var doc = web.Load(
                $"http://simulationhockey.com/games/{leagueCode.ToLower()}/S{seasonNumber}/Season/{leagueCode.ToUpper()}-ProTeamSchedule.html");


            var teamIdNode =
                doc.DocumentNode.SelectNodes($"//h1[contains(@class, 'TeamSchedulePro_{team.ToUpper()}')]");

            var teamId = teamIdNode.First().InnerText.Replace(" ", "");

            var headerNodes = doc.DocumentNode.SelectNodes($"//div[contains(@id, 'STHS_JS_Team_{teamId}')]/table/thead/tr");
            var bodyNodes = doc.DocumentNode.SelectNodes($"//div[contains(@id, 'STHS_JS_Team_{teamId}')]/table/tr");

            var table = new DataTable("ScheduleTable");

            var headers = headerNodes[0]
                .Elements("th")
                .Select(th => th.InnerText.Trim())
                .ToArray();

            for (var i = 0; i < headers.Length; i++)
            {
                table.Columns.Add(i.ToString());
            }

            table.Rows.Add(headers);

            var rows = bodyNodes.Select(tr => tr
                .Elements("td")
                .Select(td => td.InnerText.Trim())
                .ToArray());
            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row[6]))
                {
                    continue;
                }
                if (table.Rows.Count >= 6)
                {
                    table.Rows.RemoveAt(1);
                }
                table.Rows.Add(row);
            }

            table.Columns.RemoveAt(10);
            table.Columns.RemoveAt(9);
            table.Columns.RemoveAt(0);

            var stringBuilder = new StringBuilder();
            for (var i = 1; i < table.Rows.Count; i++)
            {
                var wasOT = !string.IsNullOrEmpty(table.Rows[i].Field<string>(6));
                var wasSO = !string.IsNullOrEmpty(table.Rows[i].Field<string>(7));
                var extraString = wasOT ? " (OT)" : wasSO ? " (SO)" : string.Empty;
                stringBuilder.AppendLine($"**Game #{table.Rows[i].Field<string>(0)} - {table.Rows[i].Field<string>(5)}{extraString}**");
                stringBuilder.AppendLine($"{table.Rows[i].Field<string>(1)}: {table.Rows[i].Field<string>(2)}");
                stringBuilder.AppendLine($"{table.Rows[i].Field<string>(3)}: {table.Rows[i].Field<string>(4)}");
                stringBuilder.AppendLine(string.Empty);
            }

            /*
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

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(borderString);
            stringBuilder.AppendLine(headerString);
            stringBuilder.AppendLine(borderString);

            for (var i = 1; i < table.Rows.Count; i++)
            {
                stringBuilder.AppendLine($"| {string.Join(" | ", table.Rows[i].ItemArray)} |");
                stringBuilder.AppendLine(borderString);
            }

    */
            return
                $"```{stringBuilder}```";
        }

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

        public string GetGoals(string seasonNumber, string leagueCode, int gameNumber)
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

            return goalsString;
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
