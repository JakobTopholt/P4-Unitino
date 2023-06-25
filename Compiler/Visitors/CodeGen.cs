using System.Text.RegularExpressions;
using System;
using System.IO;
using Moduino.analysis;
using Moduino.node;

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
                writer.WriteLine(";");                    
                break;
        }
    }

    private bool prog;
    public override void InAGrammar(AGrammar node)
    {
        if(symbolTable.Prog == 0)
            writer.WriteLine("void setup() {\n}");
        if (symbolTable.Loop == 0)
            writer.WriteLine("void loop() {\n}");
    }

    public override void CaseAProgGlobal(AProgGlobal node)
    {
        InAProgGlobal(node);
        Indent("void setup() {\r\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this); 
            ScopeHandler(child);
        }
        OutAProgGlobal(node);
    }
    
    public override void InAProgGlobal(AProgGlobal node)
    {
         symbolTable = symbolTable.EnterScope();
    }

    public override void OutAProgGlobal(AProgGlobal node)
    {
        _indent--;
        Indent("}\n");
        symbolTable = symbolTable.ExitScope();
    }

    public override void CaseALoopGlobal(ALoopGlobal node)
    {
        InALoopGlobal(node);
        Indent("void loop() {\n");
        _indent++;
        foreach (PStmt child in node.GetStmt())
        {
            child.Apply(this);
            ScopeHandler(child);
        }
        OutALoopGlobal(node);
    }

    public override void InALoopGlobal(ALoopGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutALoopGlobal(ALoopGlobal node)
    {
        _indent--;
        Indent("}\n");
        symbolTable = symbolTable.ExitScope();
    }

    public override void CaseATypedGlobal(ATypedGlobal node)
    {
        InATypedGlobal(node);
        switch (node.GetType())
        {
            case AIntType:
                Indent("int ");
                break;
            case ADecimalType:
                Indent("float ");
                break;
            case ABoolType:
                Indent("bool ");
                break;
            case ACharType:
                Indent("char ");
                break;
            case AStringType:
                Indent("String ");
                break;
        }
        var customType = node.GetType() as AUnitType;
        if (customType != null)
        {
            Indent($"float ");
        }
        node.GetId().Apply(this);
        writer.Write("(");

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
            ScopeHandler(child);
        }
        OutATypedGlobal(node);
    }

    public override void InATypedGlobal(ATypedGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutATypedGlobal(ATypedGlobal node)
    {
        _indent--;
        Indent("}\n");
        symbolTable = symbolTable.ExitScope();
    }

    public override void CaseAUntypedGlobal(AUntypedGlobal node)
    {
        InAUntypedGlobal(node);
        switch (symbolTable.GetSymbol(node))
        {
            case Symbol.Bool:
                Indent("bool ");
                break;
            case Symbol.Char:
                Indent("char ");
                break;
            case Symbol.Decimal:
                Indent("float ");
                break;
            case Symbol.Ok:
            case Symbol.NotOk:
            case Symbol.Func:
                Indent("void ");
                break;
            case Symbol.Int:
                Indent("int ");
                break;
            case Symbol.String:
                Indent("String ");
                break;
        }
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
            ScopeHandler(child);
        }
        OutAUntypedGlobal(node);
    }

    public override void InAUntypedGlobal(AUntypedGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        _indent--;
        Indent("}\n");
        symbolTable = symbolTable.ExitScope();
    }

    public override void CaseAArg(AArg node)
    {
        switch (node.GetType())
        {
            case AIntType:
                writer.Write("int ");
                break;
            case ADecimalType:
                writer.Write("float ");
                break;
            case ABoolType:
                writer.Write("bool ");
                break;
            case ACharType:
                writer.Write("char ");
                break;
            case AStringType:
                writer.Write("String ");
                break;
            case APinType:
                Indent("int ");
                break;
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
            ScopeHandler(child);
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
            ScopeHandler(child);
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
            ScopeHandler(child);
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
            ScopeHandler(child);
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
            ScopeHandler(child);
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
            ScopeHandler(child);
        }
        _indent--;
        Indent("} while(");
        node.GetExp().Apply(this);
        OutADowhileStmt(node);
    }

    public override void OutADowhileStmt(ADowhileStmt node)
    {
        writer.Write(")\n");
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
                Indent("float ");
                break;
            case ABoolType:
                Indent("bool ");
                break;
            case ACharType:
                Indent("char ");
                break;
            case AStringType:
                Indent("String ");
                break;
            case APinType:
                Indent("int ");
                break;
        }
    }
    
    /*---------------------------------------------ExpStmt------------------------------------------------------------*/
    public override void CaseATernaryExp(ATernaryExp node)
    {
        writer.Write("");
        node.GetCond().Apply(this);
        writer.Write("?");
        node.GetTrue().Apply(this);
        writer.Write(":");
        node.GetFalse().Apply(this);
    }

    public override void CaseAWritepinStmt(AWritepinStmt node)
    {
        Indent("digitalWrite(");
        node.GetExp().Apply(this);
        writer.Write(", ");
        node.GetPintoggle().Apply(this);
        OutAWritepinStmt(node);
    }

    public override void OutAWritepinStmt(AWritepinStmt node)
    {
        writer.Write(")");
    }

    public override void CaseASetpinStmt(ASetpinStmt node)
    {
        Indent("pinMode(");
        node.GetExp().Apply(this);
        writer.Write(", ");
        node.GetPinmode().Apply(this);
        OutASetpinStmt(node);
    }

    public override void OutASetpinStmt(ASetpinStmt node)
    {
        writer.Write(")");
    }

    public override void CaseAInputPinmode(AInputPinmode node)
    {
        writer.Write("INPUT");
    }

    public override void CaseAOutputPinmode(AOutputPinmode node)
    {
        writer.Write("OUTPUT");
    }

    public override void CaseAInputPullupPinmode(AInputPullupPinmode node)
    {
        writer.Write("INPUT_PULLUP");
    }

    public override void CaseAHighPintoggle(AHighPintoggle node)
    {
        writer.Write("HIGH");
    }

    public override void CaseALowPintoggle(ALowPintoggle node)
    {
        writer.Write("LOW");
    }

    public override void InADelayStmt(ADelayStmt node)
    {
        Indent("delay(");
    }

    public override void OutADelayStmt(ADelayStmt node)
    {
        writer.Write(")");
    }

    public override void CaseAAssignStmt(AAssignStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        writer.Write(" = ");
        node.GetExp().Apply(this);
    }
    public override void CaseAPlusassignStmt(APlusassignStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        writer.Write(" += ");
        node.GetExp().Apply(this);
    }

    public override void CaseAMinusassignStmt(AMinusassignStmt node)
    {
        Indent("");
        node.GetId().Apply(this);
        writer.Write(" -= ");
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
        writer.Write("++");
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
        writer.Write("--");
    }

    public override void CaseAReturnStmt(AReturnStmt node)
    {
        Indent($"return ");
        node.GetExp().Apply(this);
    }

    public override void CaseADeclassStmt(ADeclassStmt node)
    {
        //throw new Exception(node.GetExp().ToString());
        switch (node.GetType())
        {
            case AIntType a:
                Indent(($"int "));
                break;
            case ADecimalType b:
                Indent(($"float "));
                break;
            case ABoolType c:
                Indent(($"bool "));
                break;
            case ACharType d:
                Indent(($"char "));
                break;
            case AStringType e:
                Indent(($"String "));
                break;
            case APinType:
                Indent("int ");
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

    public override void CaseAFunccallExp(AFunccallExp node)
    {
        writer.Write("");
        node.GetId().Apply(this);
        writer.Write("(");
        var list = node.GetExp();
        int i = 0;
        foreach (Node child in node.GetExp())
        {
            child.Apply(this);
            i++;
            if(i != list.Count)
                writer.Write(", ");
        }
        writer.Write(")");    
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
        Precedence(node.GetL(),node.GetR()," + ");
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
      writer.Write( node.GetExp().ToString().Trim()+ "++");
    }

    public override void CaseAPrefixplusplusExp(APrefixplusplusExp node)
    {
        writer.Write("++"+ node.GetExp().ToString().Trim());
    }

    public override void CaseASuffixminusminusExp(ASuffixminusminusExp node)
    {
        writer.Write(node.GetExp().ToString().Trim() + "--");
    }

    public override void CaseAPrefixminusminusExp(APrefixminusminusExp node)
    {
        writer.Write("--" + node.GetExp().ToString().Trim());
    }

    public override void CaseALogicalnotExp(ALogicalnotExp node)
    {
        writer.Write($"!{node.GetExp().ToString().Trim()}");
    }

    public override void CaseAReadpinExp(AReadpinExp node)
    {
        writer.Write($"digitalRead({node.GetExp().ToString().Trim()})");
    }

    public override void CaseACastExp(ACastExp node)
    {
        PExp expression = node.GetExp();
        Symbol? exprSymbol = symbolTable.GetSymbol(expression);
        if (expression is AIdExp id)
        {
            exprSymbol = symbolTable.GetSymbol(id.GetId().ToString().Trim());
        }
        switch (node.GetType())
        {
            case AIntType:
                writer.Write("(int)");
                node.GetExp().Apply(this);
                break;
            case ADecimalType:
                writer.Write("(float)");
                node.GetExp().Apply(this);
                break;
            case ABoolType:
                writer.Write("(bool)");
                node.GetExp().Apply(this);
                break;
            case ACharType:
                writer.Write("(char)");
                node.GetExp().Apply(this);
                break;
            case AStringType:
                if (exprSymbol is Symbol.Char)
                {
                    writer.Write("String(");
                    node.GetExp().Apply(this);
                    writer.Write(")");
                }
                else if (exprSymbol is Symbol.Int)
                {
                    writer.Write("String(");
                    node.GetExp().Apply(this);
                    writer.Write(")");
                }
                else if (exprSymbol is Symbol.Decimal)
                {
                    writer.Write("String(");
                    node.GetExp().Apply(this);
                    writer.Write(", 6)");
                }
                break;
        }
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
        if (node.GetBoolean() is ATrueBoolean)
            writer.Write("true");
        else if (node.GetBoolean() is AFalseBoolean)
            writer.Write("false");
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
        writer.Write("(float value) {\n    return ");
        node.GetExp().Apply(this);
        writer.WriteLine(";");
        OutASubunit(node);
    }
    public override void OutASubunit(ASubunit node)
    {
        writer.WriteLine("}");
    }

    public override void CaseADenUnituse(ADenUnituse node)
    {
        Indent("0" + node.GetId());
    }

    public override void CaseANumUnituse(ANumUnituse node)
    {
        Indent("1" + node.GetId());
    }

    public override void CaseAUnitdecimalExp(AUnitdecimalExp node)
    {
        AUnitdeclGlobal? test = symbolTable.GetUnitdeclFromId(node.GetId().ToString().Trim());
        writer.Write(test.GetId().ToString().Trim() + node.GetId().ToString().Trim()
                                                    + "(" + node.GetDecimal().ToString().Trim() + ")");    
    }
    public override void CaseAUnitnumberExp(AUnitnumberExp node)
    {
        AUnitdeclGlobal? test = symbolTable.GetUnitdeclFromId(node.GetId().ToString().Trim());
        writer.Write(test.GetId().ToString().Trim() + node.GetId().ToString().Trim()
                                                    + "(" + node.GetNumber().ToString().Trim() + ")");
    }

    public override void OutADeclstmtGlobal(ADeclstmtGlobal node)
    {
        writer.WriteLine(";");
    }

    public void Dispose()
    {
        writer.Dispose();
    }
}