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
        var customType = node.GetType() as AUnitType;
        if (customType != null)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            Indent($"float {node.GetId().ToString().Trim()}() {{\n");
        }
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
        Indent("void " + node.GetId().ToString().Trim() + "(");
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
        base.CaseAArg(node);
        switch (node.GetType())
        {
            case AIntType:
                writer.Write(("int " + node.GetId().ToString().Trim()));                    
                break;
            case ADecimalType:
                writer.Write(("decimal " + node.GetId().ToString().Trim()));
                break;
            case ABoolType:
                writer.Write(("bool " + node.GetId().ToString().Trim()));
                break;
            case ACharType:
                writer.Write(("char " + node.GetId().ToString().Trim()));
                break;
            case AStringType:
                writer.Write(("string " + node.GetId().ToString().Trim()));
                break;
            case AUnitType customType:
            {
                // Declared a custom sammensat unit (Ikke en baseunit declaration)
                IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
                IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
                    
                // Declaration validering for sammensat unit her
                // Check if Numerators or denomarots contains units that does not exist

                symbolTable.AddNumerators(node.GetId(), node, numerator);
                symbolTable.AddDenomerators(node.GetId(), node, denomerator);
                break;
            }
        }
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
        base.InADeclStmt(node);
        switch (node.GetType())
        {
            case AIntType:
                Indent(("int " + node.GetId().ToString().Trim()));                    
                break;
            case ADecimalType:
                Indent(("decimal " + node.GetId().ToString().Trim()));
                break;
            case ABoolType:
                Indent(("bool " + node.GetId().ToString().Trim()));
                break;
            case ACharType:
                Indent(("char " + node.GetId().ToString().Trim()));
                break;
            case AStringType:
                Indent(("string " + node.GetId().ToString().Trim()));
                break;
            case AUnitType customType:
            {
                // Declared a custom sammensat unit (Ikke en baseunit declaration)
                IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
                IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
                    
                // Declaration validering for sammensat unit her
                // Check if Numerators or denomarots contains units that does not exist

                symbolTable.AddNumerators(node.GetId(), node, numerator);
                symbolTable.AddDenomerators(node.GetId(), node, denomerator);
                break;
            }
        }
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
        Indent($"return {node.GetExp().ToString().Trim()}");
    }

    public override void InADeclassStmt(ADeclassStmt node)
    {
        switch (node.GetType())
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

        if (node.GetType() is AUnitType customType)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            Indent(($"float {node.GetId().ToString().Trim()} = "));
            node.GetExp().Apply(this);
        }
    }

    public override void InAIdExp(AIdExp node)
    {
        writer.Write(node.GetId());
    }

    public override void CaseAFunccallStmt(AFunccallStmt node)
    {
        Indent(node.GetId().ToString().Trim() + "(");
        var list = node.GetParams();
        for (var i = 0; i < list.Count; i++)
        {
            var child = (ANumberExp)list[i];
            child.Apply(this);
            if(i != list.Count-1)
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

    public override void CaseAIdExp(AIdExp node)
    {
        writer.Write(node.GetId());
    }
    

    public override void CaseAValueExp(AValueExp node)
    {
        writer.Write("value");
    }

    public override void CaseABooleanExp(ABooleanExp node)
    {
        Indent(node.GetBoolean().ToString().Trim());
    }

    public override void CaseAStringExp(AStringExp node)
    {
        Indent(node.GetString().ToString().Trim());
    }

    public override void CaseACharExp(ACharExp node)
    {
        Indent(node.GetChar().ToString().Trim());
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
        string? subUnitId = whiteSpace.Replace(node.GetId().ToString().Trim(),"");
        Indent($"float {unitId}{subUnitId}(float value) {{\n    return ");
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