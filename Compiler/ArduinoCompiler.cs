using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Compiler;

public static class ArduinoCompiler
{
    public static async Task DownloadCliAsync()
    {
        string scriptLoc = Directory.GetCurrentDirectory() + "\\";
        if (File.Exists(scriptLoc + "bin\\arduino-cli.exe"))
            return;
        Console.WriteLine("Downloading Arduino CLI to " + scriptLoc + "bin\\arduino-cli");
        {
            using HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.GetAsync("https://raw.githubusercontent.com/arduino/arduino-cli/master/install.sh");
            await using StreamWriter textWriter = File.CreateText(scriptLoc + "cliDownload.sh");
            await textWriter.WriteAsync(await response.Content.ReadAsStringAsync());
        }
        string arduinoCliPath = scriptLoc.Replace(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "")
            .Replace('\\', '/');
        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GetCommandInterpreter(),
                Arguments = "cliDownload.sh",
                Environment = {["BINDIR"] = arduinoCliPath},
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = scriptLoc
                
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine("Download failed.");
            return;
        }
        File.Move(scriptLoc + "bin\\arduino-cli", scriptLoc + "bin\\arduino-cli.exe");
        Console.WriteLine("Download succeeded!");
    }

    public static async Task InoToAVROnBoard(string folder)
    {
        string boardsTable = await GetBoards();

        // Parse the output to get the Fully Qualified Board Name and port name of the connected device
        string[] rows = boardsTable.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (string row in rows)
        {
            string[] column = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (column.Length != 9 || column[2] != "Serial") 
                continue;
            string portName = column[0];
            string boardFqbn = column[7];

            bool success = await Compile(folder, portName, boardFqbn);
            if (!success)
                Console.WriteLine("Failed to compile");
            else
                await Monitor(portName, boardFqbn);
            return;
        }
        Console.WriteLine("Couldn't find an Arduino device to compile towards");
    }
    private static string GetCommandInterpreter()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "bash.exe";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "/bin/bash";
        throw new Exception("Unsupported operating system.");
    }

    private static readonly Regex MainPathRegex = new(@"C:\\.*main\.cpp");
    private static readonly Regex InoFilePathRegex = new(@"C:\\.*\.ino");
    public static async Task<bool> Compile(string folder, string? portName, string boardFqbn)
    {
        Process compileProcess = ArduinoCli(portName == null
            ? $"compile --fqbn {boardFqbn} {folder}"
            : $"compile --fqbn {boardFqbn} --port {portName} {folder}", true);
        
        compileProcess.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory() + "\\bin";
        compileProcess.Start();
        await compileProcess.WaitForExitAsync();
        string error = await compileProcess.StandardError.ReadToEndAsync();
        if (error.Trim().Length <= 0) 
            return true;
        error = InoFilePathRegex.Replace(MainPathRegex.Replace(error, "main.cpp"), "File");
        Console.WriteLine("error:\n" + error);
        return false;
    }

    private static async Task<Process> Monitor(string portName, string boardFqbn)
    {
        Process compileProcess = ArduinoCli($"monitor --fqbn {boardFqbn} --port {portName}", false);
        compileProcess.Start();
        await compileProcess.WaitForExitAsync();
        return compileProcess;
    }

    public static async Task<string> GetBoards()
    {
        Process listProcess = ArduinoCli("board list", true);
        listProcess.StartInfo.RedirectStandardOutput = true;
        listProcess.Start();

        string output = await listProcess.StandardOutput.ReadToEndAsync();
        await listProcess.WaitForExitAsync();
        return output;
    }

    private static Process ArduinoCli(string args, bool redirect)
    {
        return new Process
        {
            StartInfo = redirect
                ? new ProcessStartInfo
                {
                    FileName = "arduino-cli",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
                : new ProcessStartInfo
                {
                    FileName = "arduino-cli",
                    Arguments = args
                }
        };
    }
}