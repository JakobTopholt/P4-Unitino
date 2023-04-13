using System;
using System.IO;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class CodeGen : DepthFirstAdapter, IDisposable
{
    //private StreamWriter writer;
    private StreamWriter writer;
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