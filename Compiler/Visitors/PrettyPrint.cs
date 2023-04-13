using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// TODO:
// Check thesis part 3 from discord. Best case, prettyprint reprints the program as it was written so that:
// program.txt --(SableCC)-> Concrete Syntax Tree --(SableCC)-> Abstract Syntax Tree --(PrettyPrint)-> program.txt (don't overwrite program tho ;)) 

class PrettyPrint : DepthFirstAdapter
{
    private int _indent = -1;
    private void Print(string s)
    {
        Console.Write(new string(' ', _indent * 2) + s);
    }

    private void PrintPrecedence(Node L, Node R, string ope)
    {
        Console.Write("(");
        L.Apply(this);
        Console.Write(ope);
        R.Apply(this);
        Console.Write(")");
    }
    
    public override void DefaultIn(Node node)
    {
        Console.Write("\n");
        _indent++;
    }
    
    public override void InAUnit(AUnit node)
    {
        string? name = node.GetId().ToString();
        string? type = node.GetInt().ToString();
        Console.Write($"unit {name}: {type} {{\n");
    }

    public override void OutAUnit(AUnit node)
    {
        Console.Write("}\n");
    }

    public override void InASubunit(ASubunit node)
    {
        string? name = node.GetId().ToString();
        Console.Write($" {name}=>");
    }

    public override void OutASubunit(ASubunit node)
    {
        Console.Write(";\n");
    }

    public override void DefaultOut(Node node)
    {
        _indent--;
    }

    public override void InStart(Start node)
    {
        base.InStart(node);
        Print("Start ");
    }
    
    public override void CaseAGrammar(AGrammar node)
    {
        base.InAGrammar(node);
        Print("Grammar\n");
        _indent++;
        
        foreach (Node child in node.GetExp())
        {
            Print("");
            child.Apply(this);
            Console.Write(";\n");
        }

        foreach (Node child in node.GetUnit())
        {
            child.Apply(this);
        }

        _indent--;
        base.OutAGrammar(node);
    }

    public override void CaseAUnit(AUnit node)
    {
        InAUnit(node);
        foreach (ASubunit subUnit in node.GetSubunit())
        {
            subUnit.Apply(this);
        }
        OutAUnit(node);
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
        Console.Write(int.Parse(node.ToString()) + "");
    }
}