using System.Text.RegularExpressions;
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
    
    public override void InAProgFunc(AProgFunc node)
    {
        writer.WriteLine("void setup() {");

    }
    
    public override void OutAProgFunc(AProgFunc node)
    {
        writer.WriteLine("}");
    }

    public override void InANewFunc(ANewFunc node)
    {
        writer.WriteLine("func void " + node.GetId() + "{");
    }

    public override void OutANewFunc(ANewFunc node)
    {
        writer.WriteLine("}");
    }

    int i = 0;
    public override void InAExpStmt(AExpStmt node)
    {
        if (node.Parent() is not ANewFunc)
            writer.Write($"  int i{i++} = ");
        else
            writer.Write("    ");
    }
    
    public override void OutAExpStmt(AExpStmt node)
    {
        writer.WriteLine(";");
    }

    public override void CaseAIntDecl(AIntDecl node)
    {
        if(node.Parent().Parent() is ANewFunc or AProgFunc)
            writer.Write("    int " + node.GetId().ToString().Trim());
        else
            writer.Write("int " + node.GetId().ToString().Trim());
    }

    public override void InABoolDecl(ABoolDecl node)
    {
        if(node.Parent().Parent() is ANewFunc or AProgFunc)
            writer.Write("    bool " + node.GetId().ToString().Trim());
        else
            writer.Write("bool " + node.GetId().ToString().Trim());
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        if(node.Parent().Parent() is ANewFunc or AProgFunc)
            writer.Write("    decimal " + node.GetId().ToString().Trim());
        else
            writer.Write("decimal " + node.GetId().ToString().Trim());
    }

    public override void InACharDecl(ACharDecl node)
    {
        if(node.Parent().Parent() is ANewFunc or AProgFunc)
            writer.Write("    char " + node.GetId().ToString().Trim());
        else
            writer.Write("char " + node.GetId().ToString().Trim());
    }

    public override void InAStringDecl(AStringDecl node)
    {
        if(node.Parent().Parent() is ANewFunc or AProgFunc)
            writer.Write("    string " + node.GetId().ToString().Trim());
        else
            writer.Write("string " + node.GetId().ToString().Trim());
    }

    public override void OutADeclStmt(ADeclStmt node)
    {
        writer.WriteLine(";");
    }

    public override void InAAssignStmt(AAssignStmt node)
    {
        writer.Write("    " + node.GetId() + "= ");
    }

    public override void OutAAssignStmt(AAssignStmt node)
    {
        writer.WriteLine(";");
    }

    public override void InAFunccallStmt(AFunccallStmt node)
    {
        writer.Write("    " + node.GetId().ToString().Trim() + "()");
    }

    public override void OutAFunccallStmt(AFunccallStmt node)
    {
        writer.WriteLine(";");
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
    public override void InASubunit(ASubunit node)
    {
        string? unitId = whiteSpace.Replace(((AUnit)node.Parent()).GetId().ToString(),"");
        string? subUnitId = whiteSpace.Replace(node.GetId().ToString(),"");
        string? type = whiteSpace.Replace(((AUnit)node.Parent()).GetTint().ToString(), "");
        writer.Write($"{type} {unitId}{subUnitId}({type} value){{\n return");
    }
    public override void OutASubunit(ASubunit node)
    {
        writer.Write(";\n}\n");
    }
    

    public void Dispose()
    {
        writer.Dispose();
    }
}