namespace Compiler;

internal static class Program
{
    
    public static async Task Main(string[] args)
    {
        //Usage: plug in Arduino board and run the compiler with "Moduino.exe [file]"
        string inputFile = args.GetValue(0) as string ?? Directory.GetCurrentDirectory() + "/../../../input";
        
        // Download Arduino CLI while compiling Moduino to Arduino
        Task downloadCliAsync = ArduinoCompiler.DownloadCliAsync(Directory.GetCurrentDirectory() + "/");
        
        await ModuinoCompiler.CompileMinoToIno(inputFile);
        await downloadCliAsync;
        await ArduinoCompiler.InoToAVROnBoard(inputFile);
    }
}