using System.Text.RegularExpressions;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class CodeGen : DepthFirstAdapter, IDisposable
{
    //private StreamWriter writer;
    private StreamWriter writer;
    private FileStream stream;
    private static readonly Regex whiteSpace = new(@"\s+");
    public CodeGen(StreamWriter writer)
    {
        this.writer = writer;
    }

    void Precedence(Node L, Node R, string ope)
    {
        writer.Write("(");
        L.Apply(this);
        writer.Write(ope);
        R.Apply(this);
        writer.Write(")");
    }

    public override void InStart(Start node)
    {
        writer.WriteLine("void setup() {");
    }

    public override void OutStart(Start node)
    {
        writer.WriteLine("}");
    }

    public override void CaseAGrammar(AGrammar node)
    {
        int i = 0;
        foreach (Node child in node.GetExp())
        {
            writer.Write($"  int i{i++} = ");
            child.Apply(this);
            writer.WriteLine(";");
        }
        foreach (Node child in node.GetUnit())
        {
            
            child.Apply(this);
        }
    }
    public override void InASubunit(ASubunit node)
    {
        string? unitId = whiteSpace.Replace(((AUnit)node.Parent()).GetId().ToString(),"");
        string? subUnitId = whiteSpace.Replace(node.GetId().ToString(),"");
        string? type = whiteSpace.Replace(((AUnit)node.Parent()).GetInt().ToString(), "");
        writer.Write($"{type} {unitId}{subUnitId}({type} value){{\n return");
    }
    public override void OutASubunit(ASubunit node)
    {
       writer.Write(";\n}\n");
    }

    public override void CaseADivExp(ADivExp node)
    {
        Precedence(node.GetL(),node.GetR(),"/");
    }

    public override void CaseAMultExp(AMultExp node)
    {
        Precedence(node.GetL(),node.GetR(),"*");
    }

    public override void CaseAPlusExp(APlusExp node)
    {
        Precedence(node.GetL(),node.GetR(),"+");
    }

    public override void CaseAMinusExp(AMinusExp node)
    {
        Precedence(node.GetL(),node.GetR(),"-");
    }

    public override void CaseANumberExp(ANumberExp node)
    {
        writer.Write(int.Parse(node.ToString()));
    }

    public void Dispose()
    {
        writer.Dispose();
    }
}