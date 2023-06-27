using System.Text.RegularExpressions;
using Moduino.node;

namespace UnitTest;

public class Data
{
    public readonly Start? Ast;
    public readonly string PrettyPrint;
    public readonly string CodeGen;
    public readonly string Name;
    public readonly string Error;
    public readonly bool? ShouldFail;
    public Data(string name, Start? ast, string error, string prettyPrint, string codeGen, bool? shouldFail)
    {
        Ast = ast;
        Error = error;
        PrettyPrint = prettyPrint;
        CodeGen = codeGen.TrimStart().ReplaceLineEndings();
        Name = name;
        ShouldFail = shouldFail;
    }
    public Data(string name, Start? ast, string error, string prettyPrint, string codeGen)
    {
        Ast = ast;
        Error = error;
        PrettyPrint = prettyPrint;
        CodeGen = codeGen.TrimStart().ReplaceLineEndings();
        Name = name;
        ShouldFail = null;
    }
    public Data(string name, Start? ast, string error, string prettyPrint)
    {
        Ast = ast;
        Error = error;
        PrettyPrint = prettyPrint;
        CodeGen = "";
        Name = name;
        ShouldFail = null;
    }
}