using System.Text;
using Compiler.Visitors;
using Compiler.Visitors.TypeChecker;
using Moduino.node;
using NUnit.Framework;
using static UnitTest.TestUtils;

namespace UnitTest;

[TestFixture]
public class PrettyPrintTester
{
    public static IEnumerable<TestCaseData> PrettyPrintTests() => GetTestsFromFile("0Prettyprint");
    
    [TestCaseSource(nameof(PrettyPrintTests))] //Can't call GetTests directly because parameters can't be used in attributes 
    public void PrettyPrint(Start ast, string expectedOutput)
    {
        SymbolTable symbolTable = new();
        TypeChecker.Run(symbolTable, ast);
        
        StringBuilder sb = new();
        TextWriter output = new StringWriter(sb);
        Compiler.Visitors.PrettyPrint.Run(symbolTable, ast, output);
        
        Assert.That(sb.ToString().Trim(), Is.EqualTo(expectedOutput));
    }
}