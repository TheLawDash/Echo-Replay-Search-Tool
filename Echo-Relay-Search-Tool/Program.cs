﻿
using System.Diagnostics;

namespace Echo_Replay_Search_Tool
{
    internal class Program
    {
        private static string? lastResult;

        public static void Main(string[] args)
        {
            bool empty = false;
            while (true)
            {
                if (empty)
                    PrintWelcomeMessage(true);
                else
                {
                    PrintWelcomeMessage(true, true);
                }
                empty = false;
                string? directoryPath = GetUserInput("Enter the directory path: ");
                string? searchString = GetUserInput("Enter the search string: ");
                if (!string.IsNullOrEmpty(directoryPath) && !string.IsNullOrEmpty(searchString))
                {
                    ReplayReader searcher = new ReplayReader();
                    var (exists, foundCount, fileCount) = searcher.SearchStringInReplays(directoryPath, searchString);
                    string newDirectoryPath = $"{directoryPath}\\ReplaySearch_{searchString}";
                    if (exists)
                    {
                        if (Directory.Exists(newDirectoryPath) && foundCount != 0)
                        {
                            Process.Start("explorer.exe", newDirectoryPath);
                        }
                    }
                    if(fileCount > 0 && foundCount == 0)
                    {
                        lastResult = $"Success: There were no results found out of the {fileCount} replay files.";
                        continue;
                    }
                    if (fileCount > 0)
                        lastResult = $"Success:  Your results are in {directoryPath}\\ReplaySearch_{searchString}, {foundCount} out of {fileCount} contains \"{searchString}\"";
                    else
                    {
                        lastResult = $"Error: No files found in directory";
                        empty = true;
                    }
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
        private static void PrintWelcomeMessage(bool clear = false, bool newInstance = false)
        {
            if (clear)
                Console.Clear();
            if(newInstance)
            {
                if (lastResult != null)
                {
                    if (lastResult.Contains("Success"))
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{lastResult}\n");
                }
            }
            DisplayMessage("Welcome to the Replay Search Tool!", ConsoleColor.Yellow);
            DisplayMessage("Search through .echoreplay files for specific strings and create symlink shortcuts. Press Ctrl+C to quit.", ConsoleColor.Yellow);
            DisplayMessage("Please note: Depending on how many files you have, and their size, it may take a while for it to complete.", ConsoleColor.Yellow);
            DisplayMessage("\nCredit: Developed by thelawdash on Discord, if you need support, contact him there.\n", ConsoleColor.Green);
            if (clear && !newInstance)
                PrintError("\nError: Please ensure both fields are populated.");
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
    }
}