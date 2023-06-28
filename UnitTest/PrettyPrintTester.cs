using System.Text;
using Compiler.Visitors;
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
        
        StringBuilder sb = new();
        TextWriter output = new StringWriter(sb);
        ast.Apply(new PrettyPrint(symbolTable, output));
        Assert.That(sb.ToString().Trim(), Is.EqualTo(expectedOutput));
    }
}