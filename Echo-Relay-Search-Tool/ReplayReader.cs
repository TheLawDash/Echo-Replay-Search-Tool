using System.IO.Compression;
using System.Diagnostics;
using System.Security.Principal;
namespace Echo_Replay_Search_Tool
{
    public class ReplayReader
    {
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
            int count = 1;
            int successCount = 0;
            foreach (var file in files)
            {
                bool exists = ProcessReplayFile(file, searchString, resultFolderPath, count, files.Count);
                if (exists)
                    successCount++;
                count++;
            }

            PrintSuccess("\nSearch complete! Check the result folder for symlinks to matching files.");
            return (true, successCount, files.Count);
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

        private bool ProcessReplayFile(string filePath, string searchString, string resultFolderPath, int count, int total)
        {
            bool matchFound = CheckIfPlayerExists(filePath, searchString);
            if (matchFound)
            {
                bool exists = SymlinkIfExists(filePath, resultFolderPath);
                if (exists)
                {
                    PrintSuccess($"{count} of {total}: Match found in: {Path.GetFileName(filePath)}");
                }
                return true;
            }
            PrintInfo($"{count} of {total}: No match in: {Path.GetFileName(filePath)}");
            return false;
        }

        private bool CheckIfPlayerExists(string filePath, string searchString)
        {
            using var reader = OpenOrExtract(filePath);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                if (TryFindUsername(line, searchString))
                    return true;  // Exit immediately on match
            }

            return false;  // No match found
        }

        private bool TryFindUsername(ReadOnlySpan<char> line, string searchString)
        {
            // Manually split using tab delimiter to avoid string allocations
            int tabIndex;
            while ((tabIndex = line.IndexOf('\t')) != -1)
            {
                var segment = line.Slice(0, tabIndex);
                if (SegmentContainsSearchString(segment, searchString) &&
                    TryDeserializeSearchModel(segment, out var model) &&
                    model.Username != null &&
                    model.Username.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                line = line.Slice(tabIndex + 1);
            }
            return SegmentContainsSearchString(line, searchString) &&
                   TryDeserializeSearchModel(line, out var finalModel) &&
                   finalModel.Username != null &&
                   finalModel.Username.Contains(searchString, StringComparison.OrdinalIgnoreCase);
        }

        private bool SegmentContainsSearchString(ReadOnlySpan<char> segment, string searchString)
        {
            return segment.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool TryDeserializeSearchModel(ReadOnlySpan<char> json, out SearchModel? result)
        {
            try
            {
                result = System.Text.Json.JsonSerializer.Deserialize<SearchModel>(json.ToString());
                return result != null;
            }
            catch
            {
                result = null;
                return result != null;
            }
        }

        private bool SymlinkIfExists(string targetFilePath, string resultFolderPath)
        {
            string symlinkName = Path.GetFileNameWithoutExtension(targetFilePath) + ".echoreplay";
            string symlinkPath = Path.Combine(resultFolderPath, symlinkName);

            if (!File.Exists(symlinkPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c mklink \"{symlinkPath}\" \"{targetFilePath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                })?.WaitForExit();

                PrintInfo($"Symlink created: {symlinkPath}");
                return false;
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

