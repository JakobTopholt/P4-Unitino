using System.Text.RegularExpressions;
using System;
using System.IO;
using Moduino.analysis;
using Moduino.node;
// TODO: Check grammar.sablecc3 AST for how the tree will look.
// On lowest level such as id and number there is no concrete value, but rather only the string
// Another branch will fix this, so ignore this for now. Until then just use the value in the .toString method as shown
// in CaseANumberExp. Now adjust So that if it is the Func.prog node parse it as the Arduino setup() method 
// or if it is Func.new it is a function with the Func.new.id as it's name. In this branch we don't check anything for
// id. a function contains a list of statements. Create a start curly bracket and then for each statement parse it seperated
// by a semicolon. Note a statement can either be:
//   stmt.exp (from previous example and this is actually meaningless, so ignore this)
//   stmt.assign
//   stmt.decl
//   stmt.funccall
// for exp consider removing all the parenthesis and check if they match the associativity and precedence laws we
// have specified in Overleaf

namespace Compiler.Visitors;

public class CodeGen : DepthFirstAdapter, IDisposable
{
    private StreamWriter writer;
    private static readonly Regex whiteSpace = new(@"\s+");
    public CodeGen(StreamWriter writer)
    {
        this.writer = writer;
    }

    void Precedence(Node L, Node R, string ope)
    {
        writer.Write(L.Parent().Parent().Parent() is ASubunit ? "(" : "");
        L.Apply(this);
        writer.Write(ope);
        R.Apply(this);
        writer.Write(L.Parent().Parent().Parent() is ASubunit ? ");\n" : "");
    }
    private int _indent = 0;
    private void Indent(string s)
    {
        writer.Write(new string(' ', _indent * 4) + s);
    }

    public override void CaseAProgFunc(AProgFunc node)
    {
        Indent("void setup() {\r\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                writer.WriteLine(";");
        }
        OutAProgFunc(node);
    }

    public override void OutAProgFunc(AProgFunc node)
    {
        _indent--;
        Indent("}\n");
    }
    
    public override void CaseANewFunc(ANewFunc node)
    {
        Indent("func void " + node.GetId() + "{\r\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                writer.WriteLine(";");
        }
        OutANewFunc(node);
    }

    public override void OutANewFunc(ANewFunc node)
    {
        _indent--;
        Indent("}\n");
    }

    /*-------------------------------------Control Structures---------------------------------------------------------*/
    public override void CaseAIfScoped(AIfScoped node)
    {
        Indent("if(");
        node.GetExp().Apply(this);
        writer.WriteLine(") {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                writer.WriteLine(";");
        }
        _indent--;
        OutAIfScoped(node);
    }

    public override void OutAIfScoped(AIfScoped node)
    {
        Indent("}\n");
    }

    public override void CaseAForScoped(AForScoped node)
    {
        int _indentholder = _indent;
        Indent("for(");
        _indent = 0;
        node.GetInit().Apply(this);
        writer.Write(("; "));
        node.GetCond().Apply(this);
        writer.Write(("; "));
        node.GetIncre().Apply(this);
        writer.WriteLine(") {");
        _indent = _indentholder;
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                writer.WriteLine(";");
        }
        _indent--;
        OutAForScoped(node);
    }

    public override void OutAForScoped(AForScoped node)
    {
        Indent("}\n");
    }

    public override void CaseAWhileScoped(AWhileScoped node)
    {
        Indent("while(");
        node.GetExp().Apply(this);
        writer.WriteLine(") {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                writer.WriteLine(";");
        }
        _indent--;
        OutAWhileScoped(node);
    }

    public override void OutAWhileScoped(AWhileScoped node)
    {
        Indent("}\n");
    }

    public override void CaseAElseifScoped(AElseifScoped node)
    {
        Indent("else if(");
        node.GetExp().Apply(this);
        writer.WriteLine(") {");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                writer.WriteLine(";");
        }
        _indent--;
        OutAElseifScoped(node);
    }

    public override void OutAElseifScoped(AElseifScoped node)
    {
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
                writer.WriteLine(";");
        }
        _indent--;
        OutAElseScoped(node);
    }

    public override void OutAElseScoped(AElseScoped node)
    {
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
                writer.WriteLine(";");
        }
        _indent--;
        Indent("} while(");
        node.GetExp().Apply(this);
        writer.Write(")\n");
    }

    public override void OutADowhileScoped(ADowhileScoped node)
    {
        Indent(")\n");
    }

    /*-------------------------------------Decl-----------------------------------------------------------------------*/
    
    public override void InAIntDecl(AIntDecl node)
    {
        Indent(("int " + node.GetId().ToString().Trim()));
    }

    public override void InABoolDecl(ABoolDecl node)
    {
        Indent("bool " + node.GetId().ToString().Trim());
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        Indent("decimal " + node.GetId().ToString().Trim());
    }

    public override void InACharDecl(ACharDecl node)
    {
        Indent("char " + node.GetId().ToString().Trim());
    }
    
    public override void InAStringDecl(AStringDecl node)
    {
        Indent("string " + node.GetId().ToString().Trim());
    }
    
    int i = 0;
    public override void InAExpStmt(AExpStmt node)
    {
        if (node.Parent() is AProgFunc)
            Indent($"int i{i++} = ");
        else
            Indent("");
    }
    
    /*---------------------------------------------ExpStmt------------------------------------------------------------*/

    public override void InAAssignStmt(AAssignStmt node)
    {
        Indent(node.GetId() + "= ");
    }

    public override void InAFunccallStmt(AFunccallStmt node)
    {
        Indent(node.GetId().ToString().Trim() + "()");
    }

    /*--------------------------------- CaseExp ----------------------------------------------------------------------*/
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
        if(node.Parent() is not ANewFunc)
            writer.Write(int.Parse(node.ToString()));
    }
    public override void InASubunit(ASubunit node)
    {
        string? unitId = whiteSpace.Replace(((AUnit)node.Parent()).GetId().ToString(),"");
        string? subUnitId = whiteSpace.Replace(node.GetId().ToString(),"");
        string? type = whiteSpace.Replace(((AUnit)node.Parent()).GetTint().ToString(), "");
        Indent($"{type} {unitId}{subUnitId}({type} value){{\n");
        _indent++;
        Indent("return");
        _indent--;
    }
    public override void OutASubunit(ASubunit node)
    {
        Indent("}\n");
    }
    

    public void Dispose()
    {
        writer.Dispose();
    }
}