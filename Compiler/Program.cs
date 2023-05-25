namespace Compiler;

internal static class Program
{
    
    public static async Task Main(string[] args)
    {
        //Usage: plug in Arduino board and run the compiler with "Moduino.exe [file]"
        if (args.Length == 0)
        {
            Console.WriteLine("\nUsage: Compiler filepath [optional path to bash]");
            Console.WriteLine("Bash is only needed for downloading Arduino CLI the first time");
            return;
        }

        string? bash = args.Length >= 2 ? args[1] : null;

        string inputFile = Path.GetFullPath(args[0]);
        if (!File.Exists(inputFile + ".mino"))
        {
            Console.WriteLine("Couldn't find the file:\n" + inputFile + ".mino");
            return;
        }
        
        // Download Arduino CLI while compiling Moduino to Arduino
        Task<bool> downloadCliAsync = ArduinoCompiler.DownloadCliAsync(bash);
        Console.WriteLine("Compiling: " + inputFile + ".mino");
        string folder = await ModuinoCompiler.CompileMinoToIno(inputFile);
        if (!await downloadCliAsync) // CLI download failed
            return;
        Console.WriteLine("Compiling Arduino ");
        await ArduinoCompiler.InoToAVROnBoard(folder);
        //TODO: make a UI as alternative to CLI :D
    }
}