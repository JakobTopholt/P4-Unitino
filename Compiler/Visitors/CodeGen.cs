using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// TODO: Check grammar.sablecc3 AST for how the tree will look.
// On lowest level such as id and number there is no concrete value, but rather only the string
// Another branch will fix this, so ignore this for now. Until then just use the value in the .toString method as shown
// in CaseANumberExp.

// TODO: Convert unit to functions in Arduino. A future branch will handle usage of the functions. so ex:
// unit Time : int {
//      ms => (6+5);
//      s => 5*4:
// }
// => int Timems(int value) { // value unused for now
//      return (6+5)
// }
// int Times(int value) {
//      return 5*4;
// }

class CodeGen : DepthFirstAdapter, IDisposable
{
    private StreamWriter writer;
    private FileStream stream;
    public CodeGen(string dest)
    {
        stream = File.Create(dest);
        writer = new(stream);
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
        stream.Dispose();
    }
}