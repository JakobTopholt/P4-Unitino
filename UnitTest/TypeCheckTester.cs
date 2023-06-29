using System.Text;
using Compiler.Visitors;
using Compiler.Visitors.TypeChecker;
using Moduino.node;
using NUnit.Framework;
using static UnitTest.TestUtils;

namespace UnitTest;

[TestFixture]
public class TypeCheckTester
{
    public static IEnumerable<TestCaseData> TypeCheckTests() => GetTestsFromFile("1Typecheck"); 

    [TestCaseSource(nameof(TypeCheckTests))] //Can't call GetTests directly because parameters can't be used in attributes 
    public void TypeCheck(Start ast, string expectedOutput)
    {
        StringBuilder sb = new();
        TextWriter output = new StringWriter(sb);
        
        SymbolTable symbolTable = new();
        TypeChecker.Run(symbolTable, ast, Console.Out); // TODO: Replace Console.Out with output
        Assert.That(symbolTable.GetSymbol(ast.GetPGrammar()), Is.EqualTo(expectedOutput == "true" ? Symbol.Ok : Symbol.NotOk));
    }
}