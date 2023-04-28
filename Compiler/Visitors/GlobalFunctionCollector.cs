using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalFunctionCollector : DepthFirstAdapter
{
    public override void InAUntypedFunc(AUntypedFunc node)
    {
        // Mangler også at store funktions parametre her
        // Se task 4 i TypeChecker
        
    }

    public override void OutAUntypedFunc(AUntypedFunc node)
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
        var standardType = unit as ATypeUnittype;
        if (standardType != null)
        {
            switch (standardType.GetType())
            {
                case AIntType a:
                    Symbol? intId = SymbolTable.GetSymbol(a);
                    SymbolTable.AddId(node.GetId(), node, intId != null ? Symbol.notOk : Symbol.Int);
                    break;
                case ADecimalType b:
                    Symbol? decimalId = SymbolTable.GetSymbol(b);
                    SymbolTable.AddId(node.GetId(), node, decimalId != null ? Symbol.notOk : Symbol.Decimal);
                    break;
                case ABoolType c:
                    Symbol? boolId = SymbolTable.GetSymbol(c);
                    SymbolTable.AddId(node.GetId(), node, boolId != null ? Symbol.notOk : Symbol.Bool);
                    break;
                case ACharType d:
                    Symbol? charId = SymbolTable.GetSymbol(d);
                    SymbolTable.AddId(node.GetId(), node, charId != null ? Symbol.notOk : Symbol.Char);
                    break;
                case AStringType e:
                    Symbol? stringId = SymbolTable.GetSymbol(e);
                    SymbolTable.AddId(node.GetId(), node, stringId != null ? Symbol.notOk : Symbol.String);
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