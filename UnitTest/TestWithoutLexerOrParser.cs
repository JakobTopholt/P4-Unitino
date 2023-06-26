using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Compiler.Visitors;
using Moduino.node;
using NUnit.Framework;

namespace UnitTest;

// This is why we don't write tests without the lexer and parser

public class TestWithoutLexerOrParser
{
    private Start _ast;
    private string _prettyPrint;
    private string _codeGen;
    private Symbol _isTypeCorrect;

    [SetUp]
    public void Setup()
    {
        _ast = new Start(
            new AGrammar(
                new List<PGlobal> 
                {
                    new AUnitdeclGlobal(
                        new TId("Time"), 
                        new ArrayList
                        {
                            new ASubunit(
                                new TId("ms"), 
                                new AValueExp()), 
                            new ASubunit(
                                new TId("s"), 
                                new AMultiplyExp(
                                    new AValueExp(), 
                                    new ANumberExp(
                                        new TNumber("1000"))))
                        }), 
                    new AProgGlobal(
                        new List<PStmt>
                        {
                            new ADeclassStmt(
                                new AUnitType(new ArrayList {new ANumUnituse(new TId("Time"))}), 
                                new TId("a"), 
                                new AUnitnumberExp(
                                    new TNumber("1"), 
                                    new TId("s"))), 
                            new AIfStmt(
                                new ABooleanExp(
                                    new ATrueBoolean()), 
                                new List<PStmt>
                                {
                                    new AAssignStmt(
                                        new TId("a"), 
                                        new AUnitdecimalExp(
                                            new TDecimal("7.5"), 
                                            new TId("ms")))
                                })
                        }), 
                    new AUntypedGlobal(
                        new TId("b"), 
                        new ArrayList
                        {
                            new AArg(
                                new AIntType(), 
                                new TId("b"))
                        }, 
                        new ArrayList
                        {
                            new ADeclassStmt(
                                new AStringType(), 
                                new TId("c"), 
                                new AStringExp(
                                    new TString("\"hello \""))), 
                            new AForStmt(
                                new ADeclassStmt(
                                    new AIntType(), 
                                    new TId("i"), 
                                    new ANumberExp(
                                        new TNumber("0"))), 
                                new ALessExp(
                                    new AIdExp(
                                        new TId("i")), 
                                    new AIdExp(
                                        new TId("b"))), 
                                new ASuffixplusStmt(
                                    new TId("i")), 
                                new ArrayList
                                {
                                    new APlusassignStmt(
                                        new TId("c"), 
                                        new APlusExp(
                                            new AIdExp(
                                                new TId("i")), 
                                            new AStringExp(
                                                new TString("\", \""))))
                                }),
                            new AReturnStmt(
                                new AIdExp(new TId("c")))
                        })
                }), 
            new EOF());
        _prettyPrint = @"unit Time {
    ms => value;
    s => value * 1000;
}
prog {
    Time a = 1s;
    if(true) {
        a = 7.5ms;
    }
}
func b(int b) {
    string c = ""hello "";
    for(int i = 0; i < b; i++) {
        c += i + "", "";
    }
    return c;
}
";
        _codeGen = @"void loop() {
}
float UTimems(float value) {
    return value;
}
float UTimes(float value) {
    return value*1000;
}
void setup() {
    float a = 1000;
    if(true) {
        a = 7.5;
    }
}
String Fb(int b) {
    String c = String(""hello "");
    for(int i = 0; i < b; i++) {
        c += String(i)+String("", "");
    }
    return c;
}";
        _isTypeCorrect = Symbol.Ok;
    }
    
    [Test]
    public void PrettyPrint()
    {
        // Arrange
        // We assume that SableCC's AST might become corrupt (doesn't happen though)
        Start ast = (Start) _ast.Clone();

        // Act
        List<SymbolTable> list = new();
        SymbolTable symbolTable = new(null, list);
        TypeChecker subunitsExprCheck = new(symbolTable);
        UnitVisitor a = new(symbolTable, subunitsExprCheck);
        FunctionVisitor b = new(symbolTable);
        TypeChecker c = new(symbolTable);
        ast.Apply(a);
        ast.Apply(b);
        ast.Apply(c);
        
        StringBuilder sb = new();
        TextWriter output = new StringWriter(sb);
        ast.Apply(new PrettyPrint(symbolTable, output));
        
        // Assert
        Assert.That(sb.ToString().ReplaceLineEndings(), Is.EqualTo(_prettyPrint));
    }

    [Test]
    public void TypeCheck()
    {
        // Arrange
        // We assume that SableCC's AST might become corrupt (doesn't happen though)
        Start ast = (Start) _ast.Clone();
        
        // Act
        List<SymbolTable> list = new();
        SymbolTable symbolTable = new(null, list);
        TypeChecker subunitsExprCheck = new TypeChecker(symbolTable);
        
        UnitVisitor a = new(symbolTable, subunitsExprCheck);
        FunctionVisitor b = new(symbolTable);
        TypeChecker c = new(symbolTable);
        ast.Apply(a);
        ast.Apply(b);
        ast.Apply(c);
        
        // Assert
        Assert.That(symbolTable.GetSymbol(ast.GetPGrammar()), Is.EqualTo(Symbol.Ok));
    }

    [Test]
    public void CodeGen()
    {
        // Arrange
        // We assume that SableCC's AST might become corrupt (doesn't happen though)
        Start ast = (Start) _ast.Clone();
        
        // Act
        List<SymbolTable> list = new();
        SymbolTable symbolTable = new(null, list);
        TypeChecker subunitsExprCheck = new TypeChecker(symbolTable);
        UnitVisitor a = new(symbolTable, subunitsExprCheck);
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
        
        // Assert
        Assert.That(code.Trim().ReplaceLineEndings(), Is.EqualTo(_codeGen));
    }
}