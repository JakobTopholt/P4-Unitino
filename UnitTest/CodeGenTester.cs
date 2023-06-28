using System.Text;
using System.Text.RegularExpressions;
using Compiler;
using Compiler.Visitors;
using Moduino.node;
using NUnit.Framework;
using static UnitTest.TestUtils;

namespace UnitTest;

[TestFixture]
public class CodeGenTester
{
    private static IEnumerable<TestCaseData>? _cache;
    public static IEnumerable<TestCaseData> CodeGenTests()
    {
        _cache ??= GetTestsFromFile("2Codegen");
        return _cache;
    }

    

    [TestCaseSource(nameof(CodeGenTests))] //Can't call GetTests directly because parameters can't be used in attributes 
    public void CodeGen(Start ast, string expectedOutput)
    {
        List<SymbolTable> list = new();
        SymbolTable symbolTable = new(null, list);
        TypeChecker subunitsExprCheck = new TypeChecker(symbolTable);
        TypeChecker globalDeclCheck = new(symbolTable);
        TypeChecker globalFunctionCheck = new (symbolTable);
        
        UnitVisitor a = new(symbolTable, subunitsExprCheck);
        GlobalScopeVisitor e = new(symbolTable, globalDeclCheck);
        FunctionVisitor b = new(symbolTable, globalFunctionCheck);
        TypeChecker c = new(symbolTable);
        ast.Apply(a);
        ast.Apply(e);
        ast.Apply(b);
        ast.Apply(c);
        using MemoryStream stream = new();
        using StreamWriter writer = new(stream);
        CodeGen codeGen = new(writer, symbolTable);
        ast.Apply(codeGen);
        writer.Flush();
        string code = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int) stream.Length);
        Assert.That(code.Trim().ReplaceLineEndings(), Is.EqualTo(expectedOutput));
    }
    private static readonly Regex SWhitespace = new(@"\s+");

    [SetUp]
    public async Task DownloadCli()
    {
        await ArduinoCompiler.DownloadCliAsync(null);
    }

    [TestCaseSource(nameof(CodeGenTests))] //Can't call GetTests directly because parameters can't be used in attributes 
    public async Task ArduinoVerify(Start ast, string codeGenText)
    {
        string? name = (string?) TestContext.CurrentContext.Test.Properties.Get("Category");
        if (name == null)
        {
            Assert.Inconclusive("No Category?");
            return;
        }
        name = SWhitespace.Replace(name, "").Replace(':', '-');
        // For some reason someFile.ino needs to be in someFile/someFile.ino - https://github.com/arduino/arduino-cli/issues/1968
        string folder = Directory.GetCurrentDirectory() + "\\..\\" + name + "\\"; 
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
            // If a board is plugged in, test towards that
            string[] columns = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (columns.Length != 9 || columns[2] != "Serial") 
                continue;
            string boardFqbn = columns[7];
            Assert.That(await ArduinoCompiler.Compile(folder, null, boardFqbn), Is.True);
            return;
        }
        // Else test toward a generic version
        Assert.That(await ArduinoCompiler.Compile(folder, null, null), Is.True);
    }
}