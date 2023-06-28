using Compiler.Visitors;
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
        Assert.That(symbolTable.GetSymbol(ast.GetPGrammar()), Is.EqualTo(expectedOutput == "true" ? Symbol.Ok : Symbol.NotOk));
    }
}