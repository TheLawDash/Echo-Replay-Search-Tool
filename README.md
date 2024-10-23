# Echo Replay Search Tool

## Overview
The **Echo Replay Search Tool** is a utility designed to search through **Echo VR Replay** files to locate a specific player's presence within them. This tool enables you to provide a search string, find replay files with that player, and create a folder with shortcuts (symlinks) to those replay files.

This tool aims to make it easier for users to quickly locate and organize replay files containing specific players, thereby improving accessibility to those files for reviewing and sharing purposes.

## Features
- **Replay File Search**: Searches through `.echoreplay` files for a specified player.
- **Shortcut Creation**: Creates symbolic links to the matching replay files, organized in a dedicated folder.

## Requirements
- **Windows OS**: Symlink creation requires Windows
- **.NET 6.0 or later**: Ensure that your system is equipped with .NET Runtime 6.0 or later.

## Installation
1. Clone this repository using Git:
   ```sh
   git clone https://github.com/username/echo-replay-search-tool.git
   ```
2. Ensure you have the required .NET runtime installed. You can download it from [Microsoft's .NET Download page](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

3. Open the solution in your favorite IDE, such as Visual Studio or Visual Studio Code.

4. Build the solution to restore dependencies and create the executable.

## Usage
1. **Run the Tool**: Launch the tool.
2. **Enter Directory Path**: Enter the path to the directory containing your `.echoreplay` files.
3. **Enter Search String**: Provide the name of the player you are searching for.
4. **View Results**: The tool will search through the replay files, and if the search string is found, symlinks to the relevant replay files will be created in a folder named `ReplaySearch_<searchString>` located in the same directory.

Example commands and interactions:

```
Enter the directory path: C:\ReplayFiles
Enter the search string: PlayerName
```

Upon success, you will see a message similar to:

```
Success: Your results are in C:\ReplayFiles\ReplaySearch_PlayerName, 3 out of 10 files contain "PlayerName".
```

## Details on Key Functionalities
- **Replay Search**: The tool searches `.echoreplay` files for the provided player name by reading through each file's content. If a match is found, it creates a symbolic link to that replay file in the output folder.
- **Symbolic Links**: This feature enables users to easily access the original replay files without moving or copying them.

## Notes
- **File Size and Performance**: Depending on the number and size of `.echoreplay` files, searching may take a considerable amount of time.
- **Case Sensitivity**: Searches are case-insensitive to make finding player names easier.

## Troubleshooting
- **Directory Not Found**: Make sure you enter the correct directory path. Ensure it exists and contains `.echoreplay` files.
- **No Files Found**: If no `.echoreplay` files are detected, verify the directory and ensure the file extensions are correct.

## Credits
Developed by **thelawdash**. If you need support, you can contact him via Discord.

## License
This project is licensed under the [MIT License](LICENSE).

## Contributing
If you would like to contribute to the project, feel free to fork the repository, make your changes, and submit a pull request. Any contributions that help improve the tool are welcome.

