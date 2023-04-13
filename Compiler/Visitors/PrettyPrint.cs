using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class PrettyPrint : DepthFirstAdapter
{
    private int _indent = -1;
    private TextWriter output;

    public PrettyPrint(TextWriter? output = null)
    {
        this.output = output ?? Console.Out;
    }
    private void Print(string s)
    {
        output.Write(new string(' ', _indent * 2) + s);
    }

    private void PrintPrecedence(Node L, Node R, string ope)
    {
        output.Write("(");
        L.Apply(this);
        output.Write(ope);
        R.Apply(this);
        output.Write(")");
    }
    
    public override void DefaultIn(Node node)
    {
        output.Write("\n");
        _indent++;
    }

    public override void DefaultOut(Node node)
    {
        _indent--;
    }

    public override void InStart(Start node)
    {
        base.InStart(node);
        Print("Start");
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
            output.Write(";\n");
        }

        _indent--;
        base.OutAGrammar(node);
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
        output.Write(int.Parse(node.ToString()) + "");
    }
}