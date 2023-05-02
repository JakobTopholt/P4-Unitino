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
        L.Apply(this);
        writer.Write(ope);
        R.Apply(this);
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

    public override void CaseALoopFunc(ALoopFunc node)
    {
        Indent("void loop() {\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                writer.WriteLine(";");
        }
        OutALoopFunc(node);
    }

    public override void OutALoopFunc(ALoopFunc node)
    {
        _indent--;
        Indent("}\n");
    }

    public override void CaseATypedFunc(ATypedFunc node)
    {
        PUnittype unit = node.GetUnittype();
        var standardType = unit as ATypeUnittype;
        if (standardType != null)
        {
            switch (standardType.GetType())
            {
                case AIntType a:
                    Indent("int " + node.GetId().ToString().Trim() + "() {\r\n");
                    break;
                case ADecimalType b:
                    Indent("decimal " + node.GetId().ToString().Trim() + "() {\r\n");
                    break;
                case ABoolType c:
                    Indent("bool " + node.GetId().ToString().Trim() + "() {\r\n");
                    break;
                case ACharType d:
                    Indent("char " + node.GetId().ToString().Trim() + "() {\r\n");
                    break;
                case AStringType e:
                    Indent("string " + node.GetId().ToString().Trim() + "() {\r\n");
                    break;
            }
        }
        var customType = unit as AUnitUnittype;
        if (customType != null)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            // Her skal logikken implementeres 
        }
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                writer.WriteLine(";");
        }
        OutATypedFunc(node);
    }

    public override void OutATypedFunc(ATypedFunc node)
    {
        _indent--;
        Indent("}\n");    
    }

    public override void CaseAUntypedFunc(AUntypedFunc node)
    {
        Indent("func void " + node.GetId().ToString().Trim() + "() {\r\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            if (child is not AScopedStmt)
                writer.WriteLine(";");
        }
        OutAUntypedFunc(node);
    }

    public override void OutAUntypedFunc(AUntypedFunc node)
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
    public override void InADeclStmt(ADeclStmt node)
    {
        base.InADeclStmt(node);
        PUnittype unit = node.GetUnittype();
        var standardType = unit as ATypeUnittype;
        if (standardType != null)
        {
            switch (standardType.GetType())
            {
                case AIntType a:
                    Indent(("int " + node.GetId().ToString().Trim()));
                    break;
                case ADecimalType b:
                    Indent(("decimal " + node.GetId().ToString().Trim()));
                    break;
                case ABoolType c:
                    Indent(("bool " + node.GetId().ToString().Trim()));
                    break;
                case ACharType d:
                    Indent(("char " + node.GetId().ToString().Trim()));
                    break;
                case AStringType e:
                    Indent(("string " + node.GetId().ToString().Trim()));
                    break;
            }
        }
        var customType = unit as AUnitUnittype;
        if (customType != null)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            // Her skal logikken implementeres 
        }
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

    public override void InAPlusassignStmt(APlusassignStmt node)
    {
        Indent($"{node.GetId()}+= ");
    }

    public override void InAMinusassignStmt(AMinusassignStmt node)
    {
        Indent($"{node.GetId()}-= ");
    }

    public override void InAPrefixplusStmt(APrefixplusStmt node)
    {
        Indent("++" + node.GetId().ToString().Trim());
    }

    public override void InASuffixplusStmt(ASuffixplusStmt node)
    {
        Indent(node.GetId().ToString().Trim() + "++");
    }

    public override void InAPrefixminusStmt(APrefixminusStmt node)
    {
        Indent("--" + node.GetId().ToString().Trim());
    }

    public override void InASuffixminusStmt(ASuffixminusStmt node)
    {
        Indent(node.GetId().ToString().Trim() + "--");
    }

    public override void CaseAReturnStmt(AReturnStmt node)
    {
        Indent("return ");
        node.GetExp().Apply(this);
    }

    public override void InADeclassStmt(ADeclassStmt node)
    {
        base.InADeclassStmt(node);
        PUnittype unit = node.GetUnittype();
        var standardType = unit as ATypeUnittype;
        if (standardType != null)
        {
            switch (standardType.GetType())
            {
                case AIntType a:
                    Indent(("int " + node.GetId().ToString().Trim()) + " = ");
                    break;
                case ADecimalType b:
                    Indent(("decimal " + node.GetId().ToString().Trim()) + " = ");
                    break;
                case ABoolType c:
                    Indent(("bool " + node.GetId().ToString().Trim()) + " = ");
                    break;
                case ACharType d:
                    Indent(("char " + node.GetId().ToString().Trim()) + " = ");
                    break;
                case AStringType e:
                    Indent(("string " + node.GetId().ToString().Trim()) + " = ");
                    break;
            }
        }
        var customType = unit as AUnitUnittype;
        if (customType != null)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            
            // Her skal logikken implementeres 
            
        }
    }

    public override void InAIdExp(AIdExp node)
    {
        writer.Write(node.GetId());
    }

    public override void InAFunccallStmt(AFunccallStmt node)
    {
        Indent(node.GetId().ToString().Trim() + "()");
    }

    /*--------------------------------- CaseExp ----------------------------------------------------------------------*/
    public override void CaseADivideExp(ADivideExp node)
    {
        Precedence(node.GetL(),node.GetR(),"/");
    }

    public override void CaseAMultiplyExp(AMultiplyExp node)
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
        if(node.Parent() is not AUntypedFunc or ATypedFunc)
            writer.Write(int.Parse(node.ToString()));
    }

    public override void CaseAUnaryminusExp(AUnaryminusExp node)
    {
        node.GetExp();
        if (node.Parent() is not ATypedFunc or AUntypedFunc)
            writer.Write("-" + node.GetExp().ToString().Trim());
    }

    public override void CaseAOrExp(AOrExp node)
    {
        Precedence(node.GetL(),node.GetR()," || ");
    }

    public override void CaseAAndExp(AAndExp node)
    {
        Precedence(node.GetL(),node.GetR()," && ");
    }

    public override void CaseAEqualExp(AEqualExp node)
    {
        Precedence(node.GetL(),node.GetR()," == ");
    }

    public override void CaseANotequalExp(ANotequalExp node)
    {
        Precedence(node.GetL(),node.GetR()," != ");
    }

    public override void CaseAGreaterExp(AGreaterExp node)
    {
        Precedence(node.GetL(),node.GetR()," > ");
    }

    public override void CaseAGreaterequalExp(AGreaterequalExp node)
    {
        Precedence(node.GetL(),node.GetR()," >= ");
    }

    public override void CaseALessExp(ALessExp node)
    {
        Precedence(node.GetL(),node.GetR()," < ");
    }

    public override void CaseALessequalExp(ALessequalExp node)
    {
        Precedence(node.GetL(),node.GetR()," <= ");
    }

    public override void CaseARemainderExp(ARemainderExp node)
    {
        Precedence(node.GetL(),node.GetR()," % ");
    }

    public override void CaseASuffixplusplusExp(ASuffixplusplusExp node)
    {
        Indent("++" + node.GetExp().ToString().Trim());
    }

    public override void CaseAPrefixplusplusExp(APrefixplusplusExp node)
    {
        Indent(node.GetExp().ToString().Trim() + "++");
    }

    public override void CaseASuffixminusminusExp(ASuffixminusminusExp node)
    {
        Indent(node.GetExp().ToString().Trim() + "--");
    }

    public override void CaseAPrefixminusminusExp(APrefixminusminusExp node)
    {
        Indent("--" + node.GetExp().ToString().Trim());
    }

    public override void CaseALogicalnotExp(ALogicalnotExp node)
    {
        writer.Write($"!{node.GetExp()}");
    }

    public override void CaseACastExp(ACastExp node)
    {
        writer.Write($"(decimal){node.GetExp().ToString().Trim()}");
    }

    public override void CaseAIdExp(AIdExp node)
    {
        writer.Write(node.GetId());
    }

    public override void CaseADecimalExp(ADecimalExp node)
    {
        writer.Write(node.GetDecimal().ToString().Trim());
    }

    public override void CaseAUnitExp(AUnitExp node)
    {
        Indent(node.GetUnitnumber().ToString());
    }
    
    public override void CaseAValueExp(AValueExp node)
    {
        writer.Write("value");
    }

    public override void CaseABooleanExp(ABooleanExp node)
    {
        Indent(node.GetBoolean().ToString());
    }

    public override void CaseAStringExp(AStringExp node)
    {
        Indent(node.GetString().ToString());
    }

    public override void CaseACharExp(ACharExp node)
    {
        Indent(node.GetChar().ToString());
    }

    public override void CaseASubunit(ASubunit node)
    {
        string? unitId = whiteSpace.Replace(((AUnitdecl)node.Parent()).GetId().ToString(),"");
        string? subUnitId = whiteSpace.Replace(node.GetId().ToString(),"");
        Indent($"float {unitId}{subUnitId}(float value){{\n    return ");
        node.GetExp().Apply(this);
        writer.WriteLine(";");
        OutASubunit(node);
    }
    public override void OutASubunit(ASubunit node)
    {
        Indent("}\n");
    }

    public override void CaseAUnitnumber(AUnitnumber node)
    {
        writer.Write(node.GetDecimal() + " " + node.GetSingleunit());
    }

    public override void CaseANumSingleunit(ANumSingleunit node)
    {
        writer.Write(node.GetId());
    }

    public override void CaseADenSingleunit(ADenSingleunit node)
    {
        writer.Write(node.GetId());
    }

    public void Dispose()
    {
        writer.Dispose();
    }
}