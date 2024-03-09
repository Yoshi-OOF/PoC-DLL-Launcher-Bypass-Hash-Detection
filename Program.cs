using System;
using System.Diagnostics;
using System.IO;

class Program
{
    // Chemin vers le répertoire d'installation du jeu
    static string gameInstallationPath = @"B:\SteamLibrary\steamapps\common\Helldivers 2";

    // Chemin vers le répertoire bin du jeu
    static string gameBinaryPath = Path.Combine(gameInstallationPath, "bin");

    // Chemin vers la version.dll d'origine dans le répertoire bin du jeu
    static string originalVersionDllPath = Path.Combine(gameBinaryPath, "version.dll");

    static void Main(string[] args)
    {
        // Vérifiez si le répertoire d'installation du jeu existe
        if (!Directory.Exists(gameInstallationPath))
        {
            Console.WriteLine("Répertoire d'installation du jeu introuvable.");
            return;
        }

        // Vérifiez si le répertoire bin du jeu existe
        if (!Directory.Exists(gameBinaryPath))
        {
            Console.WriteLine("Répertoire bin du jeu introuvable.");
            return;
        }

        // Générer des bytes aléatoires pour injecter dans version.dll
        byte[] garbageBytes = GenerateGarbageBytes(originalVersionDllPath);

        // Créez version.dll avec des bytes aléatoires ajoutés, supprimez l'original et renommez
        ObfuscateAndReplaceVersionDll(originalVersionDllPath, garbageBytes);

        // Lancer le jeu
        LaunchGame();
    }

    static byte[] GenerateGarbageBytes(string filePath)
    {
        // Obtenez la taille de version.dll original
        long fileSize = new FileInfo(filePath).Length;

        // Générer des bytes aléatoires
        byte[] garbageBytes = new byte[fileSize];
        new Random().NextBytes(garbageBytes);

        return garbageBytes;
    }

    static void ObfuscateAndReplaceVersionDll(string originalFilePath, byte[] garbageBytes)
    {
        string tempFilePath = originalFilePath + ".obf";

        // Copiez version.dll vers version.dll.obf
        File.Copy(originalFilePath, tempFilePath, true);

        // Ajoutez des bytes aléatoires à version.dll.obf
        using (var stream = new FileStream(tempFilePath, FileMode.Append))
        {
            stream.Write(garbageBytes, 0, garbageBytes.Length);
        }

        // Supprimez version.dll original
        File.Delete(originalFilePath);

        // Renommez version.dll.obf en version.dll
        File.Move(tempFilePath, originalFilePath);
    }

    static void LaunchGame()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c start steam://rungameid/553850",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du lancement du jeu: {ex.Message}");
        }
    }
}
