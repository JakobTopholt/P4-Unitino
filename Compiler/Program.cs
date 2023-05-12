namespace Compiler;

internal static class Program
{
    
    public static async Task Main(string[] args)
    {
        //Usage: plug in Arduino board and run the compiler with "Moduino.exe [file]"
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Moduino file [optional path to bash]");
            Console.WriteLine("Bash is only needed for downloading Arduino CLI the first time");
            return;
        }

        string? bash = args.Length >= 2 ? args[1] : null;

        string inputFile = Path.GetFullPath(args[0]);
        // Download Arduino CLI while compiling Moduino to Arduino
        Task<bool> downloadCliAsync = ArduinoCompiler.DownloadCliAsync(bash);
        Console.WriteLine("Compiling: " + inputFile + ".mino");
        string folder = await ModuinoCompiler.CompileMinoToIno(inputFile);
        if (!await downloadCliAsync) // CLI download failed
            return;
        Console.WriteLine("Compiling Arduino ");
        await ArduinoCompiler.InoToAVROnBoard(folder);
    }
}