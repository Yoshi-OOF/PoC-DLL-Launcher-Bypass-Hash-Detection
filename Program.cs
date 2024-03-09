using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;

class Program
{
    // Path to the game's installation directory
    static string gameInstallationPath = @"B:\SteamLibrary\steamapps\common\Helldivers 2";

    // Path to the game's binary folder
    static string gameBinaryPath = Path.Combine(gameInstallationPath, "bin");

    // Path to the original version.dll
    static string originalVersionDllPath = Path.Combine(Environment.SystemDirectory, "version.dll");

    // Path to the obfuscated version.dll
    static string obfuscatedVersionDllPath = Path.Combine(gameBinaryPath, "version.dll.obf");

    static void Main(string[] args)
    {
        // Check if the game installation directory exists
        if (!Directory.Exists(gameInstallationPath))
        {
            Console.WriteLine("Game installation directory not found.");
            return;
        }

        // Check if the game binary directory exists
        if (!Directory.Exists(gameBinaryPath))
        {
            Console.WriteLine("Game binary directory not found.");
            return;
        }

        // Generate random garbage bytes to inject into version.dll
        byte[] garbageBytes = GenerateGarbageBytes(originalVersionDllPath);

        // Create obfuscated version.dll by appending garbage bytes
        CreateObfuscatedVersionDll(originalVersionDllPath, obfuscatedVersionDllPath, garbageBytes);

        // Replace the original version.dll with the obfuscated version.dll
        File.Copy(obfuscatedVersionDllPath, originalVersionDllPath, true);

        // Launch the game
        LaunchGame();
    }

    static byte[] GenerateGarbageBytes(string filePath)
    {
        // Get the size of the original version.dll
        long fileSize = new FileInfo(filePath).Length;

        // Generate random garbage bytes
        byte[] garbageBytes = new byte[fileSize];
        new Random().NextBytes(garbageBytes);

        return garbageBytes;
    }

    static void CreateObfuscatedVersionDll(string originalFilePath, string obfuscatedFilePath, byte[] garbageBytes)
    {
        // Copy the original version.dll to obfuscated version.dll
        File.Copy(originalFilePath, obfuscatedFilePath, true);

        // Append garbage bytes to the obfuscated version.dll
        using (var stream = new FileStream(obfuscatedFilePath, FileMode.Append))
        {
            stream.Write(garbageBytes, 0, garbageBytes.Length);
        }
    }

    static void LaunchGame()
    {
        try
        {
            // Command to start the Game
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
            Console.WriteLine($"An error occurred while trying to launch the game: {ex.Message}");
        }
    }
}
