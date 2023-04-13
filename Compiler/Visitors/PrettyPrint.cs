using Moduino.analysis;
using Moduino.node;
using Moduino.parser;

namespace Compiler.Visitors;

// TODO:
// Check thesis part 3 from discord. Best case, prettyprint reprints the program as it was written so that:
// program.txt --(SableCC)-> Concrete Syntax Tree --(SableCC)-> Abstract Syntax Tree --(PrettyPrint)-> program.txt (don't overwrite program tho ;)) 

class PrettyPrint : DepthFirstAdapter
{
    private void Print(string s)
    {
        Console.Write(s);
    }

    private void PrintPrecedence(Node L, Node R, string ope)
    {
        L.Apply(this);
        Console.Write(ope);
        R.Apply(this);
    }
    
    public override void DefaultIn(Node node)
    {
    }

    public override void DefaultOut(Node node)
    {
        //Console.WriteLine("\n");
    }

    public override void InAProgFunc(AProgFunc node)
    {
        Console.Write("prog {");
    }

    public override void OutAProgFunc(AProgFunc node)
    {
        Console.WriteLine("}");
    }

    public override void InANewFunc(ANewFunc node)
    {
        Console.Write("func " + node.GetId() + "{");
    }

    public override void OutAExpStmt(AExpStmt node)
    {
        if (node.Parent() is ANewFunc)
            Console.Write(";");
    }

    public override void InADeclStmt(ADeclStmt node)
    {
        Console.Write(" int " + node.GetId());
    }

    public override void OutADeclStmt(ADeclStmt node)
    {
        Console.Write(";");
    }

    public override void InAAssignStmt(AAssignStmt node)
    {
        Console.Write(" " + node.GetId() + "= ");
    }

    public override void OutAAssignStmt(AAssignStmt node)
    {
        Console.Write(";");
    }

    public override void OutANewFunc(ANewFunc node)
    {
        Console.WriteLine("}");
    }

    public override void InAFunccallStmt(AFunccallStmt node)
    {
        Console.Write(" " + node + "()");
    }

    public override void OutAFunccallStmt(AFunccallStmt node)
    {
        Console.Write("");
    }

    public override void CaseADivExp(ADivExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR(),"/");
    }

    public override void CaseAMinusExp(AMinusExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR(),"-");
    }

    public override void CaseAMultExp(AMultExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR(),"*");
    }

    public override void CaseAPlusExp(APlusExp node)
    {
        PrintPrecedence(node.GetL(), node.GetR(), "+");
    }

    public override void CaseANumberExp(ANumberExp node)
    {
        Console.Write(int.Parse(node.ToString()) + "");
    }
}