using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // Name of the game to launch
        string gameName = "Helldivers 2";

        // Get the path to the Steam installation
        string steamPath = GetSteamPath();
        string libraryFoldersPath = Path.Combine(steamPath, @"steamapps\libraryfolders.vdf");
        string gameInstallationPath = FindGameInstallationPath(gameName, steamPath, libraryFoldersPath);

        if (string.IsNullOrEmpty(gameInstallationPath))
        {
            Console.WriteLine("Répertoire d'installation du jeu non trouvé.");
            return;
        }

        string gameBinaryPath = Path.Combine(gameInstallationPath, "bin");
        string originalVersionDllPath = Path.Combine(gameBinaryPath, "version.dll");
        string obfuscatedVersionDllPath = Path.Combine(gameBinaryPath, "version.dll.obf");

        // Check if the game binary directory exists
        if (!Directory.Exists(gameBinaryPath))
        {
            Console.WriteLine("Le répertoire binaire du jeu n'a pas été trouvé.");
            return;
        }

        // Generate random bytes to append to the original version.dll
        byte[] garbageBytes = GenerateGarbageBytes(originalVersionDllPath);
        CreateObfuscatedVersionDll(originalVersionDllPath, obfuscatedVersionDllPath, garbageBytes);

        // Replace the original version.dll with the obfuscated version
        ReplaceOriginalDllWithObfuscated(originalVersionDllPath, obfuscatedVersionDllPath);

        // Launch the game
        LaunchGame();
    }
    static string GetSteamPath()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
            {
                if (key != null)
                {
                    Object o = key.GetValue("SteamPath");
                    if (o != null)
                    {
                        return o.ToString().Replace('/', '\\');
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while trying to locate Steam: {ex.Message}");
        }
        return null;
    }

    static string FindGameInstallationPath(string gameName, string steamPath, string libraryFoldersPath)
    {
        // Vérifiez si le jeu est installé dans l'emplacement par défaut
        string defaultPath = Path.Combine(steamPath, @"steamapps\common", gameName);
        if (Directory.Exists(defaultPath)) return defaultPath;

        // Else, check the libraryfolders.vdf file
        // This part could be simplified by using a library like VDFSharp
        if (File.Exists(libraryFoldersPath))
        {
            string[] lines = File.ReadAllLines(libraryFoldersPath);
            foreach (string line in lines)
            {
                if (line.Contains(":"))
                {
                    string[] parts = line.Split('"');
                    string path = parts[parts.Length - 2];
                    string possiblePath = Path.Combine(path, "steamapps", "common", gameName);
                    if (Directory.Exists(possiblePath)) return possiblePath;
                }
            }
        }

        return null; // Game not found
    }

    static byte[] GenerateGarbageBytes(string filePath)
    {
        long fileSize = new FileInfo(filePath).Length;
        byte[] garbageBytes = new byte[fileSize];
        new Random().NextBytes(garbageBytes);
        return garbageBytes;
    }

    static void CreateObfuscatedVersionDll(string originalFilePath, string obfuscatedFilePath, byte[] garbageBytes)
    {
        File.Copy(originalFilePath, obfuscatedFilePath, true);
        using (var stream = new FileStream(obfuscatedFilePath, FileMode.Append))
        {
            stream.Write(garbageBytes, 0, garbageBytes.Length);
        }
    }

    static void ReplaceOriginalDllWithObfuscated(string originalFilePath, string obfuscatedFilePath)
    {
        if (File.Exists(originalFilePath))
        {
            File.Delete(originalFilePath);
        }
        File.Move(obfuscatedFilePath, originalFilePath);
    }

    static void LaunchGame()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c start steam://rungameid/553850", // Change and put the id of the game
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Une erreur est survenue lors du lancement du jeu : {ex.Message}");
        }
    }
}
