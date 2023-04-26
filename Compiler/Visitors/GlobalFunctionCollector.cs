using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalFunctionCollector : DepthFirstAdapter
{
    public override void InANewFunc(ANewFunc node)
    {
        // Mangler også at store funktions parametre her
        // Se task 4 i TypeChecker
        
    }

    public override void OutANewFunc(ANewFunc node)
    {
        Symbol? funcId = SymbolTable.GetSymbol(node.GetId());
        //throw new Exception("lmao, already declared");
        SymbolTable.AddId(node.GetId(), node, funcId != null ? Symbol.notOk : Symbol.Func);
        // throws void, however we need to understand which it can return
        
    }
    
    // Overvej om den burde være i et In/Out 
    public override void CaseADeclStmt(ADeclStmt node)
    {
        base.CaseADeclStmt(node);
        PUnittype unit = node.GetUnittype();
        //Symbol? unitId = SymbolTable.GetSymbol(node.GetId());
        switch (unit)
        {
            case AIntUnittype a:
                Symbol? intId = SymbolTable.GetSymbol(a);
                SymbolTable.AddId(node.GetId(), node, intId != null ? Symbol.notOk : Symbol.Int);
                break;
            case ADecimalUnittype b:
                Symbol? decimalId = SymbolTable.GetSymbol(b);
                SymbolTable.AddId(node.GetId(), node, decimalId != null ? Symbol.notOk : Symbol.Decimal);
                break;
            case ABoolUnittype c:
                Symbol? boolId = SymbolTable.GetSymbol(c);
                SymbolTable.AddId(node.GetId(), node, boolId != null ? Symbol.notOk : Symbol.Bool);
                break;
            case ACharUnittype d:
                Symbol? charId = SymbolTable.GetSymbol(d);
                SymbolTable.AddId(node.GetId(), node, charId != null ? Symbol.notOk : Symbol.Char);
                break;
            case AStringUnittype e:
                Symbol? stringId = SymbolTable.GetSymbol(e);
                SymbolTable.AddId(node.GetId(), node, stringId != null ? Symbol.notOk : Symbol.String);
                break;
            case ACustomtypeUnittype f:
                // Er ikke implementeret ordentligt overhovedet
                // Er en Task beasicly
                
                PId unitName = f.GetId();
                Symbol? unitId = SymbolTable.GetSymbol(f);
                SymbolTable._currentSymbolTable.idToUnit.Add(unitName.ToString(),f);
                
                SymbolTable.AddId(node.GetId(), node, unitId != null ? Symbol.notOk : Symbol.ok);
                break;
        }
    }

    public override void OutStart(Start node)
    {
        SymbolTable.ResetScope();
    }

    // Assignments
    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? type = SymbolTable.GetSymbol("" + node.GetId());
        Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());    
        
        // if type == null (id was never declared)
        // if type != exprType (Incompatible types)
        SymbolTable.AddNode(node, type == null || type != exprType ? Symbol.notOk : Symbol.ok);
    }
}