namespace Compiler;

internal static class Program
{
    
    public static async Task Main(string[] args)
    {
        //Usage: plug in Arduino board and run the compiler with "Moduino.exe [file]"
        string? bash = null;
        if (args.Length == 0)
        {
            Console.WriteLine("\nUsage: Compiler filepath [-pretty] [optional path to bash]");
            Console.WriteLine("Bash is only needed for downloading Arduino CLI the first time");
            Console.Write("-pretty option is a cleanup routine for the file instead of compiling");
            return;
        }
        // add .mino only if other fileextension is not specified. This allows to both input -> input.mino and hello.txt to be valid inputfiles
        string inputFile = args[0].Split('/')[^1].Split('\\')[^1].Contains('.') ? Path.GetFullPath(args[0]) : Path.GetFullPath(args[0]) + ".mino";
        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Couldn't find the file:\n" + inputFile);
            return;
        }
        
        if (args.Length >= 2)
        {
            if (args[1] == "-pretty")
            {
                Console.WriteLine("Prettyprinting:\n" + inputFile);
                await ModuinoCompiler.CleanUp(inputFile);
                return;
            }
            bash = args[1];
        }
        
        // Download Arduino CLI while compiling Moduino to Arduino
        Task<bool> downloadCliAsync = ArduinoCompiler.DownloadCliAsync(bash);
        Console.WriteLine("Compiling:\n" + inputFile);
        string? folder = await ModuinoCompiler.CompileMinoToIno(inputFile);
        if (!await downloadCliAsync || folder == null) // CLI download failed / failed to compile code
            return;
        Console.WriteLine("Compiling Arduino ");
        await ArduinoCompiler.InoToAVROnBoard(folder);
    }
}