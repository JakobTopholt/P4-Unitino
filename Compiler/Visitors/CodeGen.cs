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
    private SymbolTable symbolTable;

    private StreamWriter writer;
    private static readonly Regex whiteSpace = new(@"\s+");
    public CodeGen(StreamWriter writer, SymbolTable symbolTable )
    {
        this.writer = writer;
        this.symbolTable = symbolTable;

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

    public override void CaseAProgGlobal(AProgGlobal node)
    {
        Indent("void setup() {\r\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this); 
            writer.WriteLine(";");
        }
        OutAProgGlobal(node);
    }

    public override void OutAProgGlobal(AProgGlobal node)
    {
        _indent--;
        Indent("}\n");
    }

    public override void CaseALoopGlobal(ALoopGlobal node)
    {
        Indent("void loop() {\n");
        _indent++;
        foreach (Node child in node.GetStmt())
        {
            child.Apply(this);
            writer.WriteLine(";");
        }
        OutALoopGlobal(node);
    }

    public override void OutALoopGlobal(ALoopGlobal node)
    {
        _indent--;
        Indent("}\n");
    }

    
    public override void CaseATypedGlobal(ATypedGlobal node)
    {
        switch (node.GetType())
        {
            case AIntType:
                Indent("int ");
                node.GetId().Apply(this);
                writer.Write("(");
                break;
            case ADecimalType:
                Indent("decimal ");
                node.GetId().Apply(this);
                writer.Write("(");
                break;
            case ABoolType:
                Indent("bool ");
                node.GetId().Apply(this);
                writer.Write("(");
                break;
            case ACharType:
                Indent("char ");
                node.GetId().Apply(this);
                writer.Write("(");
                break;
            case AStringType:
                Indent("string ");
                node.GetId().Apply(this);
                writer.Write("(");
                break;
        }
        var customType = node.GetType() as AUnitType;
        if (customType != null)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            Indent($"float ");
            node.GetId().Apply(this);
            writer.Write("(");
        }

        int i = 0;
        var arg = node.GetArg();
        foreach (Node child in node.GetArg())
        {
            child.Apply(this);
            i++;
            if(i != arg.Count)
                writer.Write(", ");
        }
        writer.Write(") {\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            switch (child)
            {
                // scoped
                case AWhileStmt:
                case ADowhileStmt:
                case AForStmt:
                case AIfStmt:
                case AElseStmt:
                case AElseifStmt:
                    break;
                // non-scoped
                case ADeclassStmt:
                case ADeclStmt:
                case AAssignStmt:
                case AReturnStmt:
                case AFunccallStmt:
                    writer.WriteLine(";");                    
                    break;
            }
        }
        OutATypedGlobal(node);
    }

    public override void OutATypedGlobal(ATypedGlobal node)
    {
        _indent--;
        Indent("}\n");    
    }

    public override void CaseAUntypedGlobal(AUntypedGlobal node)
    {
        Indent("void ");
        node.GetId().Apply(this);
        writer.Write("(");
        _indent++;
        var arg = node.GetArg();
        for (var i = 0; i < arg.Count; i++)
        {
            var child = (PArg)arg[i];
            child.Apply(this);
            if(i != arg.Count-1)
                writer.Write(", ");
        }

        writer.Write(") {\r\n");
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            switch (child)
            {
                case AWhileStmt:
                case ADowhileStmt:
                case AForStmt:
                case AIfStmt:
                case AElseStmt:
                case AElseifStmt:
                    break;
                case ADeclassStmt:
                case ADeclStmt:
                case AAssignStmt:
                case AReturnStmt:
                case AFunccallStmt:
                    writer.WriteLine(";");                    
                    break;
            }
        }
        OutAUntypedGlobal(node);
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        _indent--;
        Indent("}\n");
    }

    public override void CaseAArg(AArg node)
    {
        switch (node.GetType())
        {
            case AIntType:
                writer.Write("int ");
                break;
            case ADecimalType:
                writer.Write("decimal ");
                break;
            case ABoolType:
                writer.Write("bool ");
                break;
            case ACharType:
                writer.Write("char ");
                break;
            case AStringType:
                writer.Write("string ");
                break;
            case AUnitType customType:
            {
                break;
            }
        }
        node.GetId().Apply(this);
    }

    /*-------------------------------------Control Structures---------------------------------------------------------*/
    public override void CaseAIfStmt(AIfStmt node)
    {
        Indent("if(");
        node.GetExp().Apply(this);
        writer.WriteLine(") {");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            switch (child)
            {
                case AWhileStmt:
                case ADowhileStmt:
                case AForStmt:
                case AIfStmt:
                case AElseStmt:
                case AElseifStmt:
                    break;
                case ADeclassStmt:
                case ADeclStmt:
                case AAssignStmt:
                case AReturnStmt:
                case AFunccallStmt:
                    writer.WriteLine(";");                    
                    break;
            }
        }
        _indent--;
        OutAIfStmt(node);
    }

    public override void OutAIfStmt(AIfStmt node)
    {
        Indent("}\n");
    }

    public override void CaseAForStmt(AForStmt node)
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
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            switch (child)
            {
                case AWhileStmt:
                case ADowhileStmt:
                case AForStmt:
                case AIfStmt:
                case AElseStmt:
                case AElseifStmt:
                    break;
                case ADeclassStmt:
                case ADeclStmt:
                case AAssignStmt:
                case AReturnStmt:
                case AFunccallStmt:
                    writer.WriteLine(";");                    
                    break;
            }
        }
        _indent--;
        OutAForStmt(node);
    }
    
    public override void OutAForStmt(AForStmt node)
    {
        Indent("}\n");
    }

    public override void CaseAWhileStmt(AWhileStmt node)
    {
        Indent("while(");
        node.GetExp().Apply(this);
        writer.WriteLine(") {");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            switch (child)
            {
                case AWhileStmt:
                case ADowhileStmt:
                case AForStmt:
                case AIfStmt:
                case AElseStmt:
                case AElseifStmt:
                    break;
                case ADeclassStmt:
                case ADeclStmt:
                case AAssignStmt:
                case AReturnStmt:
                case AFunccallStmt:
                    writer.WriteLine(";");                    
                    break;
            }
        }
        _indent--;
        OutAWhileStmt(node);
    }

    public override void OutAWhileStmt(AWhileStmt node)
    {
        Indent("}\n");
    }

    public override void CaseAElseifStmt(AElseifStmt node)
    {
        Indent("else if(");
        node.GetExp().Apply(this);
        writer.WriteLine(") {");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            switch (child)
            {
                case AWhileStmt:
                case ADowhileStmt:
                case AForStmt:
                case AIfStmt:
                case AElseStmt:
                case AElseifStmt:
                    break;
                case ADeclassStmt:
                case ADeclStmt:
                case AAssignStmt:
                case AReturnStmt:
                case AFunccallStmt:
                    writer.WriteLine(";");                    
                    break;
            }
        }
        _indent--;
        OutAElseifStmt(node);
    }

    public override void OutAElseifStmt(AElseifStmt node)
    {
        Indent("}\n");
    }

    public override void CaseAElseStmt(AElseStmt node)
    {
        Indent("else {\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            switch (child)
            {
                case AWhileStmt:
                case ADowhileStmt:
                case AForStmt:
                case AIfStmt:
                case AElseStmt:
                case AElseifStmt:
                    break;
                case ADeclassStmt:
                case ADeclStmt:
                case AAssignStmt:
                case AReturnStmt:
                case AFunccallStmt:
                    writer.WriteLine(";");                    
                    break;
            }
        }
        _indent--;
        OutAElseStmt(node);
    }

    public override void OutAElseStmt(AElseStmt node)
    {
        Indent("}\n");
    }

    public override void CaseADowhileStmt(ADowhileStmt node)
    {
        Indent("do {\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            switch (child)
            {
                case AWhileStmt:
                case ADowhileStmt:
                case AForStmt:
                case AIfStmt:
                case AElseStmt:
                case AElseifStmt:
                    break;
                case ADeclassStmt:
                case ADeclStmt:
                case AAssignStmt:
                case AReturnStmt:
                case AFunccallStmt:
                    writer.WriteLine(";");                    
                    break;
            }
        }
        _indent--;
        Indent("} while(");
        node.GetExp().Apply(this);
        writer.Write(")\n");
    }

    public override void OutADowhileStmt(ADowhileStmt node)
    {
        Indent(")\n");
    }
    /*-------------------------------------Decl-----------------------------------------------------------------------*/
    public override void InADeclStmt(ADeclStmt node)
    {
        switch (node.GetType())
        {
            case AIntType:
                Indent("int ");
                break;
            case ADecimalType:
                Indent("decimal ");
                break;
            case ABoolType:
                Indent("bool ");
                break;
            case ACharType:
                Indent("char ");
                break;
            case AStringType:
                Indent("string ");
                break;
            case AUnitType customType:
            {
              

                break;
            }
        }
        node.GetId().Apply(this);
    }
    
    /*---------------------------------------------ExpStmt------------------------------------------------------------*/

    public override void InAAssignStmt(AAssignStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        writer.Write("= ");
    }

    public override void InAPlusassignStmt(APlusassignStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        writer.Write("+= ");
    }

    public override void InAMinusassignStmt(AMinusassignStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        writer.Write("-= ");
    }

    public override void InAPrefixplusStmt(APrefixplusStmt node)
    {
        Indent("");  
        writer.Write("++");
        node.GetId().Apply(this);
    }

    public override void InASuffixplusStmt(ASuffixplusStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        writer.Write("++");
    }

    public override void InAPrefixminusStmt(APrefixminusStmt node)
    {
        Indent("");    
        writer.Write("--");
        node.GetId().Apply(this);
    }

    public override void InASuffixminusStmt(ASuffixminusStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        writer.Write("--");
    }

    public override void CaseAReturnStmt(AReturnStmt node)
    {
        Indent($"return {node.GetExp().ToString().Trim()}");
    }

    public override void CaseADeclassStmt(ADeclassStmt node)
    {
        switch (node.GetType())
        {
            case AIntType a:
                Indent(($"int "));
                break;
            case ADecimalType b:
                Indent(($"decimal "));
                break;
            case ABoolType c:
                Indent(($"bool "));
                break;
            case ACharType d:
                Indent(($"char "));
                break;
            case AStringType e:
                Indent(($"string "));
                break;
        }
        if (node.GetType() is AUnitType customType)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            Indent(($"float "));
        }
        node.GetId().Apply(this);
        writer.Write(" = ");
        node.GetExp().Apply(this);
    }

    public override void CaseAFunccallStmt(AFunccallStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        writer.Write("(");
        var list = node.GetParams();
        int i = 0;
        foreach (Node child in node.GetParams())
        {
            child.Apply(this);
            i++;
            if(i != list.Count)
                writer.Write(", ");
        }
        writer.Write(")");
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
        if(node.Parent() is not AUntypedGlobal or ATypedGlobal)
            writer.Write(int.Parse(node.ToString().Trim()));
    }

    public override void CaseAUnaryminusExp(AUnaryminusExp node)
    {
        node.GetExp();
        if (node.Parent() is not ATypedGlobal or AUntypedGlobal)
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
        switch (node.GetType())
        {
            case AIntType a:
                writer.Write("(int)");
                break;
            case ADecimalType b:
                writer.Write("(decimal)");
                break;
            case ABoolType c:
                writer.Write("(bool)");
                break;
            case ACharType d:
                writer.Write("(char)");
                break;
            case AStringType e:
                writer.Write("(string)"); 
                break;
        }
        node.GetExp().Apply(this);
    }

    public override void CaseTId(TId node)
    {
        writer.Write(node.ToString().Trim());
    }

    public override void CaseTDecimal(TDecimal node)
    {
        writer.Write(node.ToString().Trim());
    }

    public override void CaseAValueExp(AValueExp node)
    {
        writer.Write("value");
    }

    public override void CaseABooleanExp(ABooleanExp node)
    {
        writer.Write(node.GetBoolean().ToString().Trim());
    }

    public override void CaseAStringExp(AStringExp node)
    {
        writer.Write(node.GetString().ToString().Trim());
    }

    public override void CaseACharExp(ACharExp node)
    {
        writer.Write(node.GetChar().ToString().Trim());
    }

    public override void CaseAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        foreach (PSubunit child in node.GetSubunit())
        {
            child.Apply(this);
        }
    }

    public override void CaseASubunit(ASubunit node)
    {
        string? unitId = whiteSpace.Replace(((AUnitdeclGlobal)node.Parent()).GetId().ToString(),"");
        Indent($"float {unitId}");
        node.GetId().Apply(this);
        writer.Write(")(float value) {{\n    return ");
        node.GetExp().Apply(this);
        writer.WriteLine(";");
        OutASubunit(node);
    }
    public override void OutASubunit(ASubunit node)
    {
        writer.WriteLine("}");
    }

    public override void CaseAUnitdecimalExp(AUnitdecimalExp node)
    {
        AUnitdeclGlobal test = symbolTable.SubunitToUnit["" + node.GetId()];
        writer.Write(node.GetDecimal());
    }
    public override void CaseAUnitnumberExp(AUnitnumberExp node)
    {
        AUnitdeclGlobal test = symbolTable.SubunitToUnit["" + node.GetId()];
        writer.Write(node.GetNumber());
    }
    public void Dispose()
    {
        writer.Dispose();
    }
}