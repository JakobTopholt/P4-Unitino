using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Compiler;

public static class ArduinoCompiler
{
    public static async Task DownloadCliAsync(string scriptLoc)
    {
        if (File.Exists(scriptLoc + "bin\\arduino-cli"))
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

        Console.WriteLine(process.ExitCode == 0 ? "Download succeeded!" : "Download failed.");

    }

    public static async Task InoToAVROnBoard(string filePath)
    {
        filePath += ".ino";
        string boardsTable = await GetBoards();

        // Parse the output to get the Fully Qualified Board Name and port name of the connected device
        string[] rows = boardsTable.Split( Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        foreach (string row in rows)
        {
            string[] column = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (column.Length != 9 || column[2] != "Serial") 
                continue;
            string portName = column[0];
            string boardFqbn = column[7];

            await Compile(filePath, portName, boardFqbn);
            Process monitorProcess = await Monitor(portName, boardFqbn);
            Console.WriteLine("Exited with code: " + monitorProcess.ExitCode);

            break;
        }
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
    public static async Task Compile(string filePath, string? portName, string boardFqbn)
    {
        Process compileProcess = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "arduino-cli",
                Arguments = portName == null 
                    ? $"compile --fqbn {boardFqbn} {filePath}" 
                    : $"compile --fqbn {boardFqbn} --port {portName} {filePath}",
                WorkingDirectory = filePath + "\\..\\bin",
                // The following 3 is needed to read output
                //RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            },
        };
        compileProcess.Start();
        await compileProcess.WaitForExitAsync();
        //Console.WriteLine("output: " + await compileProcess.StandardOutput.ReadToEndAsync());
        string error = await compileProcess.StandardError.ReadToEndAsync();
        if (error.Trim().Length > 0)
        {
            error = InoFilePathRegex.Replace(MainPathRegex.Replace(error, "main.cpp"), "ArduinoFile");
            Console.WriteLine("error:\n" + error);
            throw new Exception("Failed compiling Arduino to AVR");
        }
        
    }

    private static async Task<Process> Monitor(string portName, string boardFqbn)
    {
        Process compileProcess = new();
        compileProcess.StartInfo.FileName = "arduino-cli";
        compileProcess.StartInfo.Arguments = $"monitor --fqbn {boardFqbn} --port {portName}";
        compileProcess.Start();
        await compileProcess.WaitForExitAsync();
        return compileProcess;
    }

    public static async Task<string> GetBoards()
    {
        Process listProcess = new();
        listProcess.StartInfo.FileName = "arduino-cli";
        listProcess.StartInfo.Arguments = "board list";
        listProcess.StartInfo.UseShellExecute = false;
        listProcess.StartInfo.RedirectStandardOutput = true;
        listProcess.Start();

        string output = await listProcess.StandardOutput.ReadToEndAsync();
        await listProcess.WaitForExitAsync();
        return output;
    }
}