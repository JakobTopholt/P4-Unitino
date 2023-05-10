using Compiler.Visitors;
using Moduino.lexer;
using Moduino.node;
using Moduino.parser;

namespace Compiler;

internal static class ModuinoCompiler
{
    public static async Task CompileMinoToIno(string inputFile)
    {
        Start start;
        await using (FileStream fileStream = File.Open(inputFile + ".mino", FileMode.Open))
        using (TextReader textReader = new StreamReader(fileStream))
        {
            Lexer lexer = new(textReader);
            Parser parser = new(lexer);
            start = parser.Parse();
        }

        start.Apply(new PrettyPrint());

        List<SymbolTable> AllTables = new();

        SymbolTable symbolTable = new(null, AllTables);
        start.Apply(new UnitVisitor(symbolTable));
        start.Apply(new FunctionVisitor(symbolTable));
        start.Apply(new TypeChecker(symbolTable));

        // Codegen Visitor
        await using (FileStream stream = File.Create(Directory.GetCurrentDirectory() + "/../../../" + inputFile + ".ino"))
        await using (StreamWriter writer = new(stream))
        {
            using CodeGen codegen = new(writer, symbolTable);
            start.Apply(codegen);
            await writer.FlushAsync();
        }
    }
}