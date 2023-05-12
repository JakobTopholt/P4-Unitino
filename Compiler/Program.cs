namespace Compiler;

internal static class Program
{
    
    public static async Task Main(string[] args)
    {
        //Usage: plug in Arduino board and run the compiler with "Moduino.exe [file]"
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Moduino [file]");
            return;
        }

        string inputFile = Path.GetFullPath(args[0]);
        // Download Arduino CLI while compiling Moduino to Arduino
        Task downloadCliAsync = ArduinoCompiler.DownloadCliAsync();
        Console.WriteLine("Input file: " + inputFile);
        Console.WriteLine("Compiling Moduino");
        string folder = await ModuinoCompiler.CompileMinoToIno(inputFile);
        await downloadCliAsync;
        Console.WriteLine("Compiling Arduino ");
        await ArduinoCompiler.InoToAVROnBoard(folder);
    }
}