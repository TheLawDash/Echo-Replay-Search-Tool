using System.IO.Compression;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.Json;
namespace Echo_Replay_Search_Tool
{
    public class ReplayReader
    {
        private int _totalCount;
        private int _count;
        private int _successCount;
        public (bool Exists, int FilesFound, int FileCount) SearchStringInReplays(string directoryPath, string searchString)
        {
            PrintHeader("Replay Search Utility");

            if (!Directory.Exists(directoryPath))
            {
                PrintError("Error: Directory does not exist.");
                return (false, 0, 0);
            }

            string resultFolderPath = CreateOrGetResultFolder(directoryPath, searchString);
            PrintInfo($"Searching for: '{searchString}' (case-insensitive)");
            PrintInfo($"Results will be linked in: {resultFolderPath}");

            var files = Directory.EnumerateFiles(directoryPath, "*.echoreplay", SearchOption.TopDirectoryOnly).ToList();
            PrintInfo($"Found {files.Count} .echoreplay files to search.\n");
            _totalCount = files.Count;
            object lockObject = new object();

            Parallel.ForEach(files, (file) =>
            {
                if (ProcessReplayFile(file, searchString, resultFolderPath))
                {
                    lock (lockObject)
                    {
                        _successCount++;
                    }
                }
            });

            PrintSuccess("\nSearch complete! Check the result folder for symlinks to matching files.");
            return (true, _successCount, files.Count);
        }

        private string CreateOrGetResultFolder(string basePath, string searchString)
        {
            string folderName = $"ReplaySearch_{searchString}";
            string fullPath = Path.Combine(basePath, folderName);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return fullPath;
        }

        private bool ProcessReplayFile(string filePath, string searchString, string resultFolderPath)
        {
            if (CheckIfPlayerExists(filePath, searchString))
            {
                _count++;
                CreateShortcutIfExists(filePath, resultFolderPath);
                PrintSuccess($"{_count} of {_totalCount}: Match found in: {Path.GetFileName(filePath)}");
                return true;
            }
            _count++;
            PrintInfo($"{_count} of {_totalCount}: No match in: {Path.GetFileName(filePath)}");
            return false;
        }

        private bool CheckIfPlayerExists(string filePath, string searchString)
        {
            using var reader = OpenOrExtract(filePath);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                if (TryFindUsername(line, searchString))
                    return true;
            }

            return false;
        }

        private bool TryFindUsername(string line, string searchString)
        {
            string[] split = line.Split('\t');
            foreach (var stringObject in split)
            {
                try
                {
                    if (!stringObject.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    var json = JsonSerializer.Deserialize<SearchModel>(stringObject);
                    bool exists = json.TeamsList.Any(x => x.Players.Any(y => y.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
                    return exists;
                }
                catch
                {

                }
            }
            return false;
        }
        public static bool CreateShortcutIfExists(string targetFilePath, string resultFolderPath)
        {
            string shortcutName = Path.GetFileNameWithoutExtension(targetFilePath) + ".lnk";
            string shortcutPath = Path.Combine(resultFolderPath, shortcutName);

            if (!File.Exists(shortcutPath))
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                dynamic shell = Activator.CreateInstance(shellType);

                try
                {
                    var shortcut = shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = targetFilePath;
                    shortcut.Save();

                    Console.WriteLine($"Shortcut created: {shortcutPath}");
                    return false;
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(shell);
                }
            }
            else
            {
                return true;
            }
        }

        private StreamReader OpenOrExtract(string filePath)
        {
            var reader = new StreamReader(filePath);
            char[] buffer = new char[2];
            reader.Read(buffer, 0, buffer.Length);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            if (buffer[0] == 'P' && buffer[1] == 'K')
            {
                var archive = new ZipArchive(reader.BaseStream);
                return new StreamReader(archive.Entries[0].Open());
            }
            return reader;
        }

#pragma warning disable CA1416
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
#pragma warning restore CA1416

        private void PrintHeader(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n=== {message} ===\n");
            Console.ResetColor();
        }

        private void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}

