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
            output.WriteLine(";");
        }
        OutAProgFunc(node);
    }
    
    public override void OutAProgFunc(AProgFunc node)
    {
        _indent--;
        output.WriteLine("}");
    }

    public override void CaseATypedFunc(ATypedFunc node)
    {
        output.WriteLine("func " + node.GetId() + "{");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this); 
            output.WriteLine(";");
        }
        OutATypedFunc(node);    
    }

    public override void OutATypedFunc(ATypedFunc node)
    {
        _indent--;
        output.WriteLine("}");
        
    }

    public override void CaseAUntypedFunc(AUntypedFunc node)
    {
        output.WriteLine("func " + node.GetId() + "{");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this); 
            output.WriteLine(";");
        }
        OutAUntypedFunc(node);    
    }

    public override void OutAUntypedFunc(AUntypedFunc node)
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

    public override void CaseAForStmt(AForStmt node)
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
            output.WriteLine(";");
        }
        OutAForStmt(node);
    }

    public override void OutAForStmt(AForStmt node)
    {
        _indent--;
        Indent("}\n");
    }

    public override void CaseAIfStmt(AIfStmt node)
    {
        Indent("if(");
        node.GetExp().Apply(this);
        output.WriteLine(") {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this); 
            output.WriteLine(";");
        }
        OutAIfStmt(node);
    }

    public override void OutAIfStmt(AIfStmt node)
    {
        _indent--;
        Indent("}\n");
    }
    
    public override void CaseAWhileStmt(AWhileStmt node)
    {
        Indent("while(");
        node.GetExp().Apply(this);
        output.WriteLine(") {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            output.WriteLine(";");
        }
        OutAWhileStmt(node);
    }

    public override void OutAWhileStmt(AWhileStmt node)
    {
        _indent--;
        Indent("}\n");
    }
    
    public override void CaseAElseifStmt(AElseifStmt node)
    {
        Indent("else if(");
        node.GetExp().Apply(this);
        output.WriteLine(") {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            output.WriteLine(";");
        }
        OutAElseifStmt(node);
    }

    public override void OutAElseifStmt(AElseifStmt node)
    {
        _indent--;
        Indent("}\n");
    }
    
    public override void CaseAElseStmt(AElseStmt node)
    {
        Indent("else {\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            output.WriteLine(";");
        }
        OutAElseStmt(node);
    }

    public override void OutAElseStmt(AElseStmt node)
    {
        _indent--;
        Indent("}\n");
    }
    public override void CaseADowhileStmt(ADowhileStmt node)
    {
        Indent("do {\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            output.WriteLine(";");
        }

        _indent--;
        Indent("} while(");
        node.GetExp().Apply(this);
        output.Write(")\n");
    }

    public override void OutADowhileStmt(ADowhileStmt node)
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
    public override void CaseADivideExp(ADivideExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR(),"/");
    }
    
    public override void CaseAMultiplyExp(AMultiplyExp node)
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