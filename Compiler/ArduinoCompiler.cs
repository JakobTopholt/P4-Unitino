using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Compiler;

internal static class ArduinoCompiler
{
    public static async Task DownloadCliAsync(string arduinoCliPath)
    {
        if (File.Exists(arduinoCliPath))
            return;
        Console.WriteLine("Downloading Arduino CLI");
        
        using HttpClient httpClient = new();
        HttpResponseMessage response = await httpClient.GetAsync("https://raw.githubusercontent.com/arduino/arduino-cli/master/install.sh");
        string scriptContent = await response.Content.ReadAsStringAsync();

        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GetCommandInterpreter(),
                Arguments = "-c \"" + scriptContent + "\"",
                EnvironmentVariables = {["BINDIR"] = Directory.GetCurrentDirectory()}
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        Console.WriteLine("finished downloading with code " + process.ExitCode);
    }

    public static async Task InoToAVROnBoard(string filePath)
    {
        filePath += ".ino";
        string boardsTable = await GetBoards();

        // Parse the output to get the Fully Qualified Board Name(FQBN) and port name of the connected device
        string[] rows = boardsTable.Split( Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        foreach (string row in rows)
        {
            string[] column = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (column.Length != 9 || column[2] != "Serial") 
                continue;
            string portName = column[0];
            string boardFQBN = column[7];

            Process compileProcess = await Compile(filePath, portName, boardFQBN);
            Console.WriteLine(compileProcess.ExitCode == 0 ? "Compilation succeeded!" : "Compilation failed.");
            
            Process monitorProcess = await Monitor(portName, boardFQBN);
            Console.WriteLine("Exited with code: " + monitorProcess.ExitCode);

            break;
        }
    }
    private static string GetCommandInterpreter()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "cmd.exe";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "/bin/bash";
        throw new Exception("Unsupported operating system.");
    }

    private static async Task<Process> Compile(string filePath, string portName, string boardFQBN)
    {
        Process compileProcess = new();
        compileProcess.StartInfo.FileName = "arduino-cli";
        compileProcess.StartInfo.Arguments = $"compile --fqbn {boardFQBN} --port {portName} {filePath}";
        compileProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(filePath);
        compileProcess.Start();
        await compileProcess.WaitForExitAsync();
        return compileProcess;
    }
    public static async Task<Process> Monitor(string portName, string boardFQBN)
    {
        Process compileProcess = new();
        compileProcess.StartInfo.FileName = "arduino-cli";
        compileProcess.StartInfo.Arguments = $"monitor --fqbn {boardFQBN} --port {portName}";
        compileProcess.Start();
        await compileProcess.WaitForExitAsync();
        return compileProcess;
    }

    private static async Task<string> GetBoards()
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