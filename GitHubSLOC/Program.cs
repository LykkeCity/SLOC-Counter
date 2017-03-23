using System;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.IO;

namespace GitHubSLOC
{
    class Program
    {
        public static string ReadStringFromConsoleWithName(string name)
        {
            Console.Write(string.Format("Enter GitHub {0}: ", name));
            return Console.ReadLine();
        }

        // Nearly copied from: http://stackoverflow.com/questions/29201697/hide-replace-when-typing-a-password-c
        public static string ReadPassword()
        {
            Console.Write(string.Format("Enter GitHub Password: "));

            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }

        public static async Task DoWork()
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder csvBuilder = new StringBuilder();

            try
            {
                var username = ReadStringFromConsoleWithName("Username");
                var password = ReadPassword();

                IConnection conn = new Connection(new ProductHeaderValue("SLOCCounter"));
                conn.Credentials = new Credentials(username, password);
                IApiConnection apiConn = new ApiConnection(conn);
                IRepositoriesClient repoClient = new RepositoriesClient(apiConn);
                IStatisticsClient statClient = new StatisticsClient(apiConn);

                var allRepos = await repoClient.GetAllForUser("LykkeCity");

                foreach (var repo in allRepos)
                {
                    var freq = await statClient.GetCodeFrequency(repo.Id);

                    long tAddition = 0, tDeletion = 0;
                    foreach (var f in freq.AdditionsAndDeletionsByWeek)
                    {
                        tAddition += f.Additions;
                        tDeletion += f.Deletions;
                    }

                    var outputString = string.Format("{0}:\t\t\t\t{1} SLOC ( +{2} / {3} )", repo.Name, tAddition + tDeletion, tAddition, tDeletion);
                    var csvString = string.Format("{0},{1},{2},{3}", repo.Name, tAddition + tDeletion, tAddition, tDeletion);

                    builder.AppendLine(outputString);
                    csvBuilder.AppendLine(csvString);

                    Console.WriteLine(outputString);
                }

                string date = DateTime.UtcNow.ToLongDateString();
                using (StreamWriter writer = new StreamWriter(string.Format("SLOC_{0}.txt", date)))
                {
                    writer.WriteLine(builder.ToString());
                }

                using (StreamWriter writer = new StreamWriter(string.Format("SLOC_{0}.csv", date)))
                {
                    writer.WriteLine(csvBuilder.ToString());
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
        }
        static void Main(string[] args)
        {
            DoWork().Wait();
        }
    }
}
