
using System.Diagnostics;

namespace EchoRelaySearchTool
{
    internal class Program
    {
        private static string? lastResult;

        public static void Main(string[] args)
        {
            if (!ReplayReader.IsAdministrator())
            {
                PrintError("This application requires administrator privileges to create symlinks.");
                RestartAsAdmin();
                return;
            }

            PrintWelcomeMessage();
            bool empty = false;
            while (true)
            {
                if (empty)
                    PrintWelcomeMessage(true);
                else
                    ClearScreenWithLastResult();
                empty = false;
                string? directoryPath = GetUserInput("Enter the directory path: ");
                string? searchString = GetUserInput("Enter the search string: ");
                if (!string.IsNullOrEmpty(directoryPath) && !string.IsNullOrEmpty(searchString))
                {
                    ReplayReader searcher = new ReplayReader();
                    var (exists, foundCount, fileCount) = searcher.SearchStringInReplays(directoryPath, searchString);
                    if (fileCount > 0)
                        lastResult = $"Success:  Your results are in {directoryPath}\\ReplaySearch_{searchString}, {foundCount} out of {fileCount} contains \"{searchString}\"";
                    else
                        lastResult = $"Error: No files found in directory";
                    continue;
                }
                empty = true;
                continue;
            }
        }
        private static string? GetUserInput(string prompt)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(prompt);
            Console.ResetColor();
            return Console.ReadLine()?.Trim();
        }
        private static void RestartAsAdmin()
        {
            var exeName = Environment.ProcessPath;

            var startInfo = new ProcessStartInfo
            {
                FileName = exeName,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                PrintError($"Failed to restart with admin privileges: {ex.Message}");
            }
        }

        private static void PrintWelcomeMessage(bool clear = false)
        {
            if (clear)
                Console.Clear();
            DisplayMessage("Welcome to the Replay Search Tool!", ConsoleColor.Yellow);
            DisplayMessage("Search through .echoreplay files for specific strings and create symlink shortcuts. Press Ctrl+C to quit.", ConsoleColor.Yellow);
            DisplayMessage("\nCredit: Developed with support from thelawdash\n", ConsoleColor.Green);
            if (clear)
                PrintError("\n\nError: Please ensure both fields are populated.");
        }
        private static void DisplayMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void ClearScreenWithLastResult()
        {
            if (lastResult == null)
                return;
            Console.Clear();
            if (lastResult.Contains("Success"))
                Console.ForegroundColor = ConsoleColor.Green;
            else
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{lastResult}\n");
            Console.ResetColor();
        }
    }
}