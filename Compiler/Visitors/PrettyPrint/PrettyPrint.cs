using System.Text.RegularExpressions;
using Moduino.analysis;
using Moduino.node;
using Moduino.parser;

namespace Compiler.Visitors;

public class PrettyPrint : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    private TextWriter output;
    private Start ast;
    private PrettyPrint(SymbolTable symbolTable, Start ast, TextWriter output)
    {
        this.output = output;
        this.symbolTable = symbolTable;
        this.ast = ast;
    }
    public static void Run(SymbolTable symbolTable, Start ast, TextWriter output)
    {
        ast.Apply(new PrettyPrint(symbolTable, ast, output));
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

    public void ScopeHandler(PStmt child)
    {
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
            case APlusassignStmt:
            case AMinusassignStmt:
            case ASuffixminusStmt:
            case ASuffixplusStmt:
            case APrefixminusStmt:
            case APrefixplusStmt:
            case ADelayStmt:
            case ASetpinStmt: 
            case AWritepinStmt:    
                output.WriteLine(";");                    
                break;
        }
    }

    public override void CaseAProgGlobal(AProgGlobal node)
    {
        Indent("prog {\r\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        OutAProgGlobal(node);
    }

    public override void OutAProgGlobal(AProgGlobal node)
    {
        _indent--;
        Indent("}\r\n");
    }
    
    public override void CaseALoopGlobal(ALoopGlobal node)
    {
        Indent("loop {\r\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        OutALoopGlobal(node);
    }

    public override void OutALoopGlobal(ALoopGlobal node)
    {
        _indent--;
        Indent("}\r\n");
    }

    public override void CaseATypedGlobal(ATypedGlobal node)
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
            case APinType:
                Indent("pin ");
                break;
        }
        var customType = node.GetType() as AUnitType;
        if (customType != null)
        {
            Indent(customType.ToString().Trim());
            output.Write(" ");
        }
        node.GetId().Apply(this);
        output.Write("(");

        int i = 0;
        var arg = node.GetArg();
        foreach (Node child in node.GetArg())
        {
            child.Apply(this);
            i++;
            if(i != arg.Count)
                output.Write(", ");
        }
        output.Write(") {\r\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        OutATypedGlobal(node);
    }

    public override void OutATypedGlobal(ATypedGlobal node)
    {
        _indent--;
        Indent("}\r\n");    
    }

    public override void CaseAUntypedGlobal(AUntypedGlobal node)
    {
        Indent("func ");
        node.GetId().Apply(this);
        output.Write("(");
        _indent++;
        var arg = node.GetArg();
        for (var i = 0; i < arg.Count; i++)
        {
            var child = (PArg)arg[i];
            child.Apply(this);
            if(i != arg.Count-1)
                output.Write(", ");
        }
        output.Write(") {\r\n");
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        OutAUntypedGlobal(node);
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        _indent--;
        Indent("}\r\n");
    }
    
    public override void CaseAArg(AArg node)
    {
        switch (node.GetType())
        {
            case AIntType:
                output.Write("int ");
                break;
            case ADecimalType:
                output.Write("decimal ");
                break;
            case ABoolType:
                output.Write("bool ");
                break;
            case ACharType:
                output.Write("char ");
                break;
            case AStringType:
                output.Write("string ");
                break;
            case APinType:
                Indent("pin ");
                break;
        }
        if (node.GetType() is AUnitType customType)
        {
            Indent(customType.ToString().Trim());
            output.Write(" ");
        }
        node.GetId().Apply(this);
    }

    public override void CaseAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        Indent($"unit {node.GetId()}");
        output.WriteLine("{");
        _indent++;
        foreach (PSubunit child in node.GetSubunit())
        {
            child.Apply(this);
        }
        _indent--;
        OutAUnitdeclGlobal(node);
    }

    public override void OutAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        output.WriteLine("}");
    }

    public override void CaseASubunit(ASubunit node)
    {
        Indent(node.GetId().Text);
        output.Write(" => ");
        node.GetExp().Apply(this);
        output.WriteLine(";");
    }
    /*---------------------------------------------------------------------------------------------------------------*/
    public override void CaseAIfStmt(AIfStmt node)
    {
        Indent("if(");
        node.GetExp().Apply(this);
        output.WriteLine(") {");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        _indent--;
        OutAIfStmt(node);
    }

    public override void OutAIfStmt(AIfStmt node)
    {
        Indent("}\r\n");
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
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        _indent--;
        OutAForStmt(node);
    }
    
    public override void OutAForStmt(AForStmt node)
    {
        Indent("}\r\n");
    }

    public override void CaseAWhileStmt(AWhileStmt node)
    {
        Indent("while(");
        node.GetExp().Apply(this);
        output.WriteLine(") {");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        _indent--;
        OutAWhileStmt(node);
    }

    public override void OutAWhileStmt(AWhileStmt node)
    {
        Indent("}\r\n");
    }
    
    public override void CaseAElseifStmt(AElseifStmt node)
    {
        Indent("else if(");
        node.GetExp().Apply(this);
        output.WriteLine(") {");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        _indent--;
        OutAElseifStmt(node);
    }

    public override void OutAElseifStmt(AElseifStmt node)
    {
        Indent("}\r\n");
    }
    
    public override void CaseAElseStmt(AElseStmt node)
    {
        Indent("else {\r\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        _indent--;
        OutAElseStmt(node);
    }

    public override void OutAElseStmt(AElseStmt node)
    {
        Indent("}\r\n");
    }
    public override void CaseADowhileStmt(ADowhileStmt node)
    {
        Indent("do {\r\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        _indent--;
        Indent("} while(");
        node.GetExp().Apply(this);
        OutADowhileStmt(node);
    }

    public override void OutADowhileStmt(ADowhileStmt node)
    {
        output.Write(")\r\n");
    }
    
    /*----------------------------------------------------------------------------------------------*/

    public override void CaseADeclStmt(ADeclStmt node)
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
            case APinType:
                Indent("pin ");
                break;
        }
        if (node.GetType() is AUnitType customType)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            Indent(customType.ToString().Trim());
            output.Write(" ");
        }
        node.GetId().Apply(this);
    }
    
    /*---------------------------------------------------------------------------------------------*/
    public override void CaseATernaryExp(ATernaryExp node)
    {
        output.Write("");
        node.GetCond().Apply(this);
        output.Write(" ? ");
        node.GetTrue().Apply(this);
        output.Write(" : ");
        node.GetFalse().Apply(this);
    }

    public override void CaseASetpinStmt(ASetpinStmt node)
    {
        Indent("setpin(");
        node.GetExp().Apply(this);
        output.Write(", ");
        node.GetPinmode().Apply(this);
        OutASetpinStmt(node);
    }

    public override void OutASetpinStmt(ASetpinStmt node)
    {
        output.Write(")");
    }

    public override void CaseAInputPinmode(AInputPinmode node)
    {
        output.Write("input");
    }

    public override void CaseAOutputPinmode(AOutputPinmode node)
    {
        output.Write("output");
    }

    public override void CaseAInputPullupPinmode(AInputPullupPinmode node)
    {
        output.Write("input_pullup");
    }
    public override void CaseAAssignStmt(AAssignStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        output.Write(" = ");
        node.GetExp().Apply(this);
    }
    
    public override void CaseAPlusassignStmt(APlusassignStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        output.Write(" += ");
        node.GetExp().Apply(this);
    }

    public override void CaseAMinusassignStmt(AMinusassignStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        output.Write(" -= ");
        node.GetExp().Apply(this);
    }

    public override void InAPrefixplusStmt(APrefixplusStmt node)
    {
        Indent("++");
    }

    public override void InASuffixplusStmt(ASuffixplusStmt node)
    {
        Indent("");
    }

    public override void OutASuffixplusStmt(ASuffixplusStmt node)
    {
        output.Write("++");
    }

    public override void InAPrefixminusStmt(APrefixminusStmt node)
    {
        Indent("--");
    }
    
    public override void InASuffixminusStmt(ASuffixminusStmt node)
    {
        Indent("");
    }

    public override void OutASuffixminusStmt(ASuffixminusStmt node)
    {
        output.Write("--");
    }
    
    public override void InAReturnStmt(AReturnStmt node)
    {
        Indent("return ");
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
            case APinType:
                Indent("pin ");
                break;
        }
        if (node.GetType() is AUnitType customType)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            Indent(customType.ToString().Trim());
            output.Write(" ");
        }
        node.GetId().Apply(this);
        output.Write(" = ");
        node.GetExp().Apply(this);
    }

    public override void CaseAFunccallExp(AFunccallExp node)
    {
        output.Write("");
        node.GetId().Apply(this);
        output.Write("(");
        var list = node.GetExp();
        int i = 0;
        foreach (Node child in node.GetExp())
        {
            child.Apply(this);
            i++;
            if(i != list.Count)
                output.Write(", ");
        }
        output.Write(")");    
    }

    public override void CaseAFunccallStmt(AFunccallStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        output.Write("(");
        var list = node.GetParams();
        int i = 0;
        foreach (Node child in node.GetParams())
        {
            child.Apply(this);
            i++;
            if(i != list.Count)
                output.Write(", ");
        }
        output.Write(")");
    }
    
    /*--------------------------------------------------------------------------------------*/
    public override void CaseADivideExp(ADivideExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," / ");
    }
    
    public override void CaseAMultiplyExp(AMultiplyExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," * ");
    }

    public override void CaseAPlusExp(APlusExp node)
    {
        PrintPrecedence(node.GetL(), node.GetR(), " + ");
    }
    
    public override void CaseAMinusExp(AMinusExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," - ");
    }
    
    public override void CaseANumberExp(ANumberExp node)
    {
        if(node.Parent() is not AUntypedGlobal or ATypedGlobal)
            output.Write(int.Parse(node.ToString().Trim()));
    }
    
    public override void CaseAUnaryminusExp(AUnaryminusExp node)
    {
        node.GetExp();
        if (node.Parent() is not ATypedGlobal or AUntypedGlobal)
            output.Write("-" + node.GetExp().ToString().Trim());
    }

    public override void CaseAOrExp(AOrExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," || ");
    }

    public override void CaseAAndExp(AAndExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," && ");
    }

    public override void CaseAEqualExp(AEqualExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," == ");
    }

    public override void CaseANotequalExp(ANotequalExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," != ");
    }
    
    public override void CaseAGreaterExp(AGreaterExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," > ");
    }

    public override void CaseAGreaterequalExp(AGreaterequalExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," >= ");
    }

    public override void CaseALessExp(ALessExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," < ");
    }

    public override void CaseALessequalExp(ALessequalExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," <= ");
    }

    public override void CaseARemainderExp(ARemainderExp node)
    {
        PrintPrecedence(node.GetL(),node.GetR()," % ");
    }

    public override void CaseASuffixplusplusExp(ASuffixplusplusExp node)
    {
        output.Write(node.GetExp().ToString().Trim() + "++");
    }
    
    public override void CaseAPrefixplusplusExp(APrefixplusplusExp node)
    {
        output.Write( "++"+ node.GetExp().ToString().Trim());
    }

    public override void CaseASuffixminusminusExp(ASuffixminusminusExp node)
    {
        output.Write(node.GetExp().ToString().Trim() + "--");
    }

    public override void CaseAPrefixminusminusExp(APrefixminusminusExp node)
    {
        output.Write("--" + node.GetExp().ToString().Trim());
    }

    public override void CaseALogicalnotExp(ALogicalnotExp node)
    {
        output.Write($"!{node.GetExp().ToString().Trim()}");
    }
    public override void CaseAReadpinExp(AReadpinExp node)
    {
        output.Write($"readpin({node.GetExp().ToString().Trim()})");
    }
    
    public override void CaseACastExp(ACastExp node)
    {
        switch (node.GetType())
        {
            case AIntType:
                output.Write("(int)");
                node.GetExp().Apply(this);
                break;
            case ADecimalType:
                output.Write("(decimal)");
                node.GetExp().Apply(this);
                break;
            case ABoolType:
                output.Write("(bool)");
                node.GetExp().Apply(this);
                break;
            case ACharType:
                output.Write("(char)");
                node.GetExp().Apply(this);
                break;
            case AStringType:
                output.Write("(string)");
                node.GetExp().Apply(this);
                break;
        }
    }

    public override void CaseAWritepinStmt(AWritepinStmt node)
    {
        Indent("writepin(");
        node.GetExp().Apply(this);
        output.Write(", ");
        node.GetPintoggle().Apply(this);
        OutAWritepinStmt(node);
    }

    public override void OutAWritepinStmt(AWritepinStmt node)
    {
        output.Write(")");
    }

    public override void CaseAHighPintoggle(AHighPintoggle node)
    {
        output.Write("high");
    }

    public override void CaseALowPintoggle(ALowPintoggle node)
    {
        output.Write("low");
    }

    public override void InADelayStmt(ADelayStmt node)
    {
        Indent("delay(");
    }

    public override void OutADelayStmt(ADelayStmt node)
    {
        output.Write(")");
    }
    public override void CaseTId(TId node)
    {
        output.Write(node.Text);
    }

    public override void CaseTDecimal(TDecimal node)
    {
        output.Write(node.ToString().Trim());
    }

    public override void CaseAValueExp(AValueExp node)
    {
        output.Write("value");
    }

    public override void CaseABooleanExp(ABooleanExp node)
    {
        if (node.GetBoolean() is ATrueBoolean)
            output.Write("true");
        else if (node.GetBoolean() is AFalseBoolean)
            output.Write("false");    
    }
    
    public override void CaseAStringExp(AStringExp node)
    {
        output.Write(node.GetString().Text);
    }

    public override void CaseACharExp(ACharExp node)
    {
        output.Write(node.GetChar().Text);
    }
    
    public override void CaseAUnitdecimalExp(AUnitdecimalExp node)
    {
        output.Write(node.GetDecimal().Text);
        node.GetId().Apply(this);   
    }
    public override void CaseAUnitnumberExp(AUnitnumberExp node)
    {
        output.Write(node.GetNumber().Text);
        node.GetId().Apply(this);
    }
}