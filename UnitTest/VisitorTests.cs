using System.Diagnostics;
using Compiler.Visitors;
using NUnit.Framework;
using System.Text;
using System.Text.RegularExpressions;
using Moduino.node;
using Compiler;

namespace UnitTest;

public class VisitorTests
{
    private static readonly Regex WhiteSpace = new(@"\s+");

    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.PrettyPrintData))]
    public void PrettyPrint(Start ast, string prettyPrint)
    {
        StringBuilder sb = new();
        TextWriter output = new StringWriter(sb);
        ast.Apply(new PrettyPrint(output));
        Assert.That(WhiteSpace.Replace(sb.ToString(), ""), 
            Is.EqualTo(prettyPrint));
    }
    
    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.TypeVisitorData))]
    public void TypeCheck(Start ast, bool correct)
    {
        List<SymbolTable> list = new();
        SymbolTable symbolTable = new(null, list);
        UnitVisitor a = new(symbolTable);
        FunctionVisitor b = new(symbolTable);
        TypeChecker c = new(symbolTable);
        ast.Apply(a);
        ast.Apply(b);
        ast.Apply(c);
        Assert.That(symbolTable.GetSymbol(ast.GetPGrammar()), Is.EqualTo(correct ? Symbol.ok : Symbol.notOk));
    }
    
    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.CodeGenData))]
    public void CodeGen(Start ast, string codeGenText)
    {
        List<SymbolTable> list = new();
        SymbolTable symbolTable = new(null, list);
        UnitVisitor a = new(symbolTable);
        FunctionVisitor b = new(symbolTable);
        TypeChecker c = new(symbolTable);
        ast.Apply(a);
        ast.Apply(b);
        ast.Apply(c);
        using MemoryStream stream = new();
        using StreamWriter writer = new(stream);
        CodeGen codeGen = new(writer, symbolTable);
        ast.Apply(codeGen);
        writer.Flush();
        string code = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int) stream.Length);
        Assert.That(code.ToString().Trim().ReplaceLineEndings(), Is.EqualTo(codeGenText));
    }

    [SetUp]
    public async Task DownloadCli()
    {
        await ArduinoCompiler.DownloadCliAsync(Directory.GetCurrentDirectory().Replace("net6.0", ""));
    }
    private static readonly Regex SWhitespace = new(@"\s+");
    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.CodeGenDataForIno))]
    public async Task CheckIfCodeGenWorksInArduino(string name, string codeGenText)
    {
        name = SWhitespace.Replace(name, "").Replace(':', '-');
        string folder = Directory.GetCurrentDirectory() + "\\..\\" + name + "\\"; // For some reason someFile.ino needs to be in someFile/someFile.ino - https://github.com/arduino/arduino-cli/issues/1968
        {
            Directory.CreateDirectory(folder);
            await using StreamWriter textWriter = File.CreateText(folder + name + ".ino");
            await textWriter.WriteAsync(codeGenText);
        }
        string boardsTable = await ArduinoCompiler.GetBoards();

        // Parse the output to get the Fully Qualified Board Name(FQBN) and port name of the connected device
        string[] rows = boardsTable.Split( '\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (string row in rows)
        {
            string[] columns = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (columns.Length != 9 || columns[2] != "Serial") 
                continue;
            string boardFQBN = columns[7];
            try
            {
                await ArduinoCompiler.Compile(folder, null, boardFQBN);
                Assert.Pass("Compiled");
            }
            catch (Exception e)
            {
                Assert.Fail("Didn't compile");
                throw;
            }
            return;
        }
        Assert.Inconclusive("Arduino board not plugged in");

    }
    
}