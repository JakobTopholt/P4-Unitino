using System.Text.RegularExpressions;
using Moduino.node;

namespace UnitTest;

public class Data
{
    private static readonly Regex WhiteSpace = new(@"\s+");
    public readonly Start? Ast;
    public readonly string PrettyPrint;
    public readonly string CodeGen;
    public readonly string Name;
    public readonly string Error;
    public readonly bool ShouldFail;
    public Data(Start? ast, string prettyPrint, string codeGen, string name, string error, bool shouldFail)
    {
        Ast = ast;
        PrettyPrint = WhiteSpace.Replace(prettyPrint, "");
        CodeGen = codeGen.TrimStart().ReplaceLineEndings();
        Name = name;
        Error = error;
        ShouldFail = shouldFail;
    }
}