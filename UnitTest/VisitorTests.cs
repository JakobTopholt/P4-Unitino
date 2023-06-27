using System.Diagnostics;
using Compiler.Visitors;
using NUnit.Framework;
using System.Text;
using System.Text.RegularExpressions;
using Moduino.node;

namespace UnitTest;

public class VisitorTests
{
    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.PrettyPrintData))]
    public void PrettyPrint(Start ast, string prettyPrint)
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
        Assert.That(sb.ToString().Trim(), Is.EqualTo(prettyPrint));
    }
    
    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.TypeVisitorData))]
    public void TypeCheck(Start ast, bool correct)
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
        Assert.That(symbolTable.GetSymbol(ast.GetPGrammar()), Is.EqualTo(correct ? Symbol.Ok : Symbol.NotOk));
    }
    
    [TestCaseSource(typeof(FilesToTestsConverter), nameof(FilesToTestsConverter.CodeGenData))]
    public void CodeGen(Start ast, string codeGenText)
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
        Assert.That(code.Trim().ReplaceLineEndings(), Is.EqualTo(codeGenText));
    }
}