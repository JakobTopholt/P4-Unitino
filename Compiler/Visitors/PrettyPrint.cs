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

    private void PrintPrecedence(Node L, Node R, string ope)
    {
        L.Apply(this);
        output.Write(ope);
        R.Apply(this);
    }

    private int _indent = 0;
    private void Indent(string s)
    {
        output.Write(new string(' ', _indent * 4) + s);
    }

    public override void CaseAProgFunc(AProgFunc node)
    {
        output.WriteLine("Prog {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                output.WriteLine(";");
        }
        OutAProgFunc(node);
    }
    
    public override void OutAProgFunc(AProgFunc node)
    {
        _indent--;
        output.WriteLine("}");
    }

    public override void CaseAFuncFunc(AFuncFunc node)
    {
        output.WriteLine("func " + node.GetId() + "{");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                output.WriteLine(";");
        }
        OutAFuncFunc(node);
    }

    public override void OutAFuncFunc(AFuncFunc node)
    {
        _indent--;
        output.WriteLine("}");
    }

    public override void CaseAUnitdecl(AUnitdecl node)
    {
        string? name = node.GetId().ToString();
        string? type = node.GetSubunit().ToString();
        Indent($"unit {name}: {type}{{\n");
        _indent++;
        foreach (Node child in node.GetSubunit())
        {
            child.Apply(this);
            output.WriteLine(";");
        }
        _indent--;
        OutAUnitdecl(node);
    }

    public override void OutAUnitdecl(AUnitdecl node)
    {
        output.Write("}\n");
    }

    public override void CaseASubunit(ASubunit node)
    {
        string? name = node.GetId().ToString();
        Indent($"{name}=> ");
        _indent--;
        node.GetExp().Apply(this);
        _indent++;
        OutASubunit(node);
    }

    public override void CaseAForScoped(AForScoped node)
    {
        int _indentholder = _indent;
        Indent("for(");
        _indent = 0;
        node.GetInit().Apply(this);
        output.Write(("; "));
        node.GetCond().Apply(this);
        output.Write(("; "));
        node.GetIncre().Apply(this);
        output.WriteLine(") {");
        _indent = _indentholder;
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                output.WriteLine(";");
        }
        OutAForScoped(node);
    }

    public override void OutAForScoped(AForScoped node)
    {
        _indent--;
        Indent("}\n");
    }

    public override void CaseAIfScoped(AIfScoped node)
    {
        Indent("if(");
        node.GetExp().Apply(this);
        output.WriteLine(") {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                output.WriteLine(";");
        }
        OutAIfScoped(node);
    }

    public override void OutAIfScoped(AIfScoped node)
    {
        _indent--;
        Indent("}\n");
    }
    
    public override void CaseAWhileScoped(AWhileScoped node)
    {
        Indent("while(");
        node.GetExp().Apply(this);
        output.WriteLine(") {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                output.WriteLine(";");
        }
        OutAWhileScoped(node);
    }

    public override void OutAWhileScoped(AWhileScoped node)
    {
        _indent--;
        Indent("}\n");
    }
    
    public override void CaseAElseifScoped(AElseifScoped node)
    {
        Indent("else if(");
        node.GetExp().Apply(this);
        output.WriteLine(") {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                output.WriteLine(";");
        }
        OutAElseifScoped(node);
    }

    public override void OutAElseifScoped(AElseifScoped node)
    {
        _indent--;
        Indent("}\n");
    }
    
    public override void CaseAElseScoped(AElseScoped node)
    {
        Indent("else {\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                output.WriteLine(";");
        }
        OutAElseScoped(node);
    }

    public override void OutAElseScoped(AElseScoped node)
    {
        _indent--;
        Indent("}\n");
    }
    public override void CaseADowhileScoped(ADowhileScoped node)
    {
        Indent("do {\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                output.WriteLine(";");
        }

        _indent--;
        Indent("} while(");
        node.GetExp().Apply(this);
        output.Write(")\n");
    }

    public override void OutADowhileScoped(ADowhileScoped node)
    {
        Indent(")\n");
    }

    public override void CaseADeclStmt(ADeclStmt node)
    {
        Indent((node.GetUnittype() + " " + node.GetId().ToString().Trim()));
    }

    public override void InAAssignStmt(AAssignStmt node)
    {
        Indent(node.GetId().ToString().Trim() + " = ");
    }

    public override void InAFunccallStmt(AFunccallStmt node)
    {
        Indent(node.ToString().Trim() + "()");
    }

    public override void CaseAExpStmt(AExpStmt node)
    {
        Indent("");
        node.GetExp().Apply(this);
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