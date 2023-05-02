using Compiler.Visitors;
using NUnit.Framework;
using System.Text;
using System.Text.RegularExpressions;
using Moduino.node;

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

    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.CodeGenData))]
    public void CodeGen(Start ast, string codeGenText)
    {
        using MemoryStream stream = new();
        using StreamWriter writer = new(stream);
        CodeGen codeGen = new(writer);
        ast.Apply(codeGen);
        writer.Flush();
        string code = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int) stream.Length);
        Assert.That(code.ReplaceLineEndings(), Is.EqualTo(codeGenText));
    }

    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.TypeVisitorData))]
    public void TypeCheck(Start ast, bool correct)
    {
        GlobalVariableCollector b = new();
        LocalScopeCollector a = new();
        TypeChecker c = new();
        ast.Apply(a);
        ast.Apply(b);
        ast.Apply(c);
        Assert.That(SymbolTable.GetSymbol(ast.GetPGrammar()), Is.EqualTo(Symbol.ok));
    }
}