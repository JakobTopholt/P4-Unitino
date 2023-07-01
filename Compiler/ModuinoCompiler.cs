using System.Text;
using Compiler.Visitors;
using Compiler.Visitors.CodeGen;
using Compiler.Visitors.PrettyPrint;
using Compiler.Visitors.TypeChecker;
using Compiler.Visitors.TypeChecker.Utils;
using Moduino.lexer;
using Moduino.node;
using Moduino.parser;

namespace Compiler;

internal static class ModuinoCompiler
{
    public static async Task<string?> CompileMinoToIno(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        string folder;
        if (name != path.Split(Path.DirectorySeparatorChar)[^2]) //Check if moduino file is in folder with same name ex: HelloWorld/HelloWorld.mino
        {
            // For some reason someFile.ino needs to be in someFile/someFile.ino - https://github.com/arduino/arduino-cli/issues/1968
            folder = Path.GetDirectoryName(path) + "\\" + name + "\\";
            Directory.CreateDirectory(folder);
        }
        else
            folder = Path.GetDirectoryName(path);
        string outPath = folder + name + ".ino";
        
        Start start;
        try
        {
            await using FileStream fileStream = File.Open(path, FileMode.Open);
            using TextReader textReader = new StreamReader(fileStream);
            Lexer lexer = new(textReader);
            Parser parser = new(lexer);
            start = parser.Parse();
        }
        catch (LexerException e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
        catch (ParserException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine("    got: " + e.Token);
            return null;
        }

        //start.Apply(new PrettyPrint());
        
        await using FileStream stream = File.Create(outPath);
        await using StreamWriter writer = new(stream);
        
        SymbolTable symbolTable = new();
        TypeChecker.Run(symbolTable, start);
        CodeGen.Run(writer, symbolTable, start);
        
        await writer.FlushAsync();

        return folder;
    }

    public static async Task CleanUp(string path)
    {
        StringBuilder sb = new();
        
        await using (FileStream readStream = File.Open(path, FileMode.Open))
        {
            Start start;
            using TextReader textReader = new StreamReader(readStream);
            try
            {
                Lexer lexer = new(textReader);
                Parser parser = new(lexer);
                start = parser.Parse();
            }
            catch (LexerException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            catch (ParserException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("    got: " + e.Token);
                return;
            }
            
            TextWriter output = new StringWriter(sb);
            SymbolTable symbolTable = new();

            TypeChecker.Run(symbolTable, start);

            PrettyPrint.Run(symbolTable, start, output);
            
        }

        await File.WriteAllTextAsync(path, sb.ToString());

    }
}