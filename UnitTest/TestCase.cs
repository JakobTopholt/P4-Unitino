using Moduino.node;

namespace UnitTest;

public class TestCase
{
    public readonly Start Ast;
    public readonly string PrettyPrint;
    public readonly string CodeGen;

    public TestCase(Start ast, string prettyPrint, string codeGen)
    {
        Ast = ast;
        PrettyPrint = prettyPrint;
        CodeGen = codeGen;
    }
}