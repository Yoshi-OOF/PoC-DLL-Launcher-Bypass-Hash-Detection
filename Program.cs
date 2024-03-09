using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // Nom du jeu pour lequel le launcher est destiné
        string gameName = "Helldivers 2";

        // Tentative de trouver le répertoire d'installation du jeu dans les emplacements communs de Steam
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

        // Vérifiez si le répertoire binaire du jeu existe
        if (!Directory.Exists(gameBinaryPath))
        {
            Console.WriteLine("Le répertoire binaire du jeu n'a pas été trouvé.");
            return;
        }

        // Générez des octets aléatoires et créez le DLL obfusqué
        byte[] garbageBytes = GenerateGarbageBytes(originalVersionDllPath);
        CreateObfuscatedVersionDll(originalVersionDllPath, obfuscatedVersionDllPath, garbageBytes);

        // Supprimez le fichier version.dll original et renommez le nouveau fichier
        ReplaceOriginalDllWithObfuscated(originalVersionDllPath, obfuscatedVersionDllPath);

        // Lancez le jeu via le lien Steam
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
        // Essayez d'abord le répertoire d'installation par défaut
        string defaultPath = Path.Combine(steamPath, @"steamapps\common", gameName);
        if (Directory.Exists(defaultPath)) return defaultPath;

        // Sinon, lisez le fichier libraryfolders.vdf pour trouver d'autres répertoires de bibliothèques
        // Cette partie est simplifiée ; vous devrez peut-être analyser le fichier VDF correctement selon sa structure
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

        return null; // Le jeu n'a pas été trouvé dans les emplacements communs
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
                Arguments = "/c start steam://rungameid/553850", // Remplacez "your_game_id_here" par l'ID de votre jeu
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
