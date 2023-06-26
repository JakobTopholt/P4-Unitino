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
        // add .mino only if other fileextension is not specified. This allows to both input -> input.mino and hello.txt to be valid inputfiles
        string inputFile = args[0].Split('/')[^1].Split('\\')[^1].Contains('.') ? Path.GetFullPath(args[0]) : Path.GetFullPath(args[0]) + ".mino";
        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Couldn't find the file:\n" + inputFile);
            return;
        }
        
        // Download Arduino CLI while compiling Moduino to Arduino
        Task<bool> downloadCliAsync = ArduinoCompiler.DownloadCliAsync(bash);
        Console.WriteLine("Compiling: " + inputFile);
        string? folder = await ModuinoCompiler.CompileMinoToIno(inputFile);
        if (!await downloadCliAsync || folder == null) // CLI download failed / failed to compile code
            return;
        Console.WriteLine("Compiling Arduino ");
        await ArduinoCompiler.InoToAVROnBoard(folder);
    }
}