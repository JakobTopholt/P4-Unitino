using System;
using System.IO;
using Moduino.analysis;
using Moduino.node;
// TODO: Check grammar.sablecc3 AST for how the tree will look.
// On lowest level such as id and number there is no concrete value, but rather only the string
// Another branch will fix this, so ignore this for now. Until then just use the value in the .toString method as shown
// in CaseANumberExp. Now adjust So that if it is the func.prog node parse it as the Arduino setup() method 
// or if it is func.new it is a function with the func.new.id as it's name. In this branch we don't check anything for
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
    //private StreamWriter writer;
    private StreamWriter writer;
    public CodeGen(StreamWriter writer)
    {
        this.writer = writer;
    }

    void Precedence(Node L, Node R, string ope)
    {
        L.Apply(this);
        writer.Write(ope);
        R.Apply(this);
    }
    private int _indent = -1;
    private void Indent(string s)
    {
        writer.Write(new string(' ', _indent * 4) + s);
    }

    public override void DefaultIn(Node node)
    {
        _indent++;
    }

    public override void DefaultOut(Node node)
    {
        _indent--;
    }

    public override void InAProgFunc(AProgFunc node)
    {
        _indent--;
        Indent("void setup() {\r\n");
        _indent++;
    }
    
    public override void OutAProgFunc(AProgFunc node)
    {
        _indent--;
        Indent("}\r\n");
        _indent++;
    }

    public override void InANewFunc(ANewFunc node)
    {
        _indent--;
        Indent("func void " + node.GetId() + "{\r\n");
        _indent++;
    }

    public override void OutANewFunc(ANewFunc node)
    {
        _indent--;
        Indent("}\r\n");
        _indent++;
    }

    int i = 0;
    public override void InAExpStmt(AExpStmt node)
    {
        if (node.Parent() is not ANewFunc)
            Indent($"int i{i++} = ");
        else
            Indent("");
    }
    
    public override void OutAExpStmt(AExpStmt node)
    {
        _indent--;
        Indent(";\r\n");
        _indent++;
    }

    public override void InAIntDecl(AIntDecl node)
    {
        if (node.Parent().Parent() is ANewFunc or AProgFunc)
        {
            _indent--;
            Indent(("int " + node.GetId().ToString().Trim()));
            _indent++;
        }
        else
            Indent("int " + node.GetId().ToString().Trim());
    }

    public override void InABoolDecl(ABoolDecl node)
    {
        if (node.Parent().Parent() is ANewFunc or AProgFunc)
        {
            _indent--;
            Indent(("bool " + node.GetId().ToString().Trim()));
            _indent++;
        }
        else
            Indent("bool " + node.GetId().ToString().Trim());
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        if (node.Parent().Parent() is ANewFunc or AProgFunc)
        {
            _indent--;
            Indent(("decimal " + node.GetId().ToString().Trim()));
            _indent++;
        }
        else
            Indent("decimal " + node.GetId().ToString().Trim());
    }

    public override void InACharDecl(ACharDecl node)
    {
        if (node.Parent().Parent() is ANewFunc or AProgFunc)
        {
            _indent--;
            Indent(("char " + node.GetId().ToString().Trim()));
            _indent++;
        }
        else
            Indent("char " + node.GetId().ToString().Trim());
    }
    
    public override void InAStringDecl(AStringDecl node)
    {
        if (node.Parent().Parent() is ANewFunc or AProgFunc)
        {
            _indent--;
            Indent(("string " + node.GetId().ToString().Trim()));
            _indent++;
        }
        else
            Indent("string " + node.GetId().ToString().Trim());
    }

    public override void OutADeclStmt(ADeclStmt node)
    {
        _indent--;
        Indent(";\r\n");
        _indent++;
    }

    public override void InAAssignStmt(AAssignStmt node)
    {
        Indent(node.GetId() + "= ");
    }

    public override void OutAAssignStmt(AAssignStmt node)
    {
        _indent--;
        Indent(";\r\n");
        _indent++;
    }

    public override void InAFunccallStmt(AFunccallStmt node)
    {
        Indent(node.GetId().ToString().Trim() + "()");
    }

    public override void OutAFunccallStmt(AFunccallStmt node)
    {
        _indent--;
        Indent(";\r\n");
        _indent++;
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
        if(node.Parent() is not ANewFunc)
            writer.Write(int.Parse(node.ToString()));
    }

    public void Dispose()
    {
        writer.Dispose();
    }
}