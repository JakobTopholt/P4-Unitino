using Moduino.analysis;
using Moduino.node;
using Moduino.parser;

namespace Compiler.Visitors;

// TODO:
// Check thesis part 3 from discord. Best case, prettyprint reprints the program as it was written so that:
// program.txt --(SableCC)-> Concrete Syntax Tree --(SableCC)-> Abstract Syntax Tree --(PrettyPrint)-> program.txt (don't overwrite program tho ;)) 

public class PrettyPrint : DepthFirstAdapter
{
    private TextWriter output;

    public PrettyPrint(TextWriter? output = null)
    {
        this.output = output ?? Console.Out;
    }
    private void Print(string s)
    {
        output.Write(s);
    }

    private void PrintPrecedence(Node L, Node R, string ope)
    {
        L.Apply(this);
        output.Write(ope);
        R.Apply(this);
    }

    public override void InAProgFunc(AProgFunc node)
    {
        output.Write("Prog {");
    }
    
    public override void InAUnit(AUnit node)
    {
        string? name = node.GetId().ToString();
        string? type = node.GetInt().ToString();
        output.Write($"unit {name}: {type} {{\n");
    }
    public override void OutAUnit(AUnit node)
    {
        output.Write("}\n");
    }

    public override void InASubunit(ASubunit node)
    {
        string? name = node.GetId().ToString();
        output.Write($"  {name}=>");
    }

    public override void OutASubunit(ASubunit node)
    {
        output.Write(";\n");
    }

    public override void OutAProgFunc(AProgFunc node)
    {
        output.WriteLine("}");
    }

    public override void InANewFunc(ANewFunc node)
    {
        output.Write("func " + node.GetId() + "{");
    }

    public override void OutAExpStmt(AExpStmt node)
    {
        if (node.Parent() is ANewFunc)
            output.Write(";");
    }

    public override void InADeclStmt(ADeclStmt node)
    {
        output.Write(" int " + node.GetDecl());
    }

    public override void OutADeclStmt(ADeclStmt node)
    {
        output.Write(";");
    }

    public override void InAAssignStmt(AAssignStmt node)
    {
        output.Write(" " + node.GetId() + "= ");
    }

    public override void OutAAssignStmt(AAssignStmt node)
    {
        output.Write(";");
    }

    public override void OutANewFunc(ANewFunc node)
    {
        output.WriteLine("}");
    }

    public override void InAFunccallStmt(AFunccallStmt node)
    {
        output.Write(" " + node + "()");
    }

    public override void OutAFunccallStmt(AFunccallStmt node)
    {
        output.Write("");
    }

    public override void CaseADivExp(ADivExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR(),"/");
    }

    public override void CaseAMultExp(AMultExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR(),"*");
    }

    public override void CaseAPlusExp(APlusExp node)
    {
        PrintPrecedence(node.GetL(), node.GetR(), "+");
    }
    
    public override void CaseAMinusExp(AMinusExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR(),"-");
    }
    
    public override void CaseANumberExp(ANumberExp node)
    {
        output.Write(int.Parse(node.ToString()) + "");
    }
}