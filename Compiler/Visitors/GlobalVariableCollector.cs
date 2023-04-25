using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalVariableCollector : DepthFirstAdapter
{
    
    public override void CaseANewFunc(ANewFunc node)
    {
        InANewFunc(node);
        OutANewFunc(node);
    }

    // This override should be implemented in GlobalFunctionCollector
    public override void InANewFunc(ANewFunc node)
    {
        Symbol? funcId = SymbolTable.GetSymbol(node.GetId());
        //throw new Exception("lmao, already declared");
        SymbolTable.AddId(node.GetId(), node, funcId != null ? Symbol.notOk : Symbol.Func);
        // Mangler også at store funktions parametre her
        // Se task 4 i LocalScopeCollector
    }



    public override void InABoolDecl(ABoolDecl node)
    {
        Symbol? boolId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, boolId != null ? Symbol.notOk : Symbol.Bool);
    }

    private PUnittype currentUnit;
    public override void CaseADeclStmt(ADeclStmt node)
    {
        base.CaseADeclStmt(node);
        PUnittype unit = node.GetUnittype();
        //Symbol? unitId = SymbolTable.GetSymbol(node.GetId());
        currentUnit = unit;
        
        
        //SymbolTable.AddId(node.GetId(), node, boolId != null ? Symbol.notOk : Symbol.Bool);
    }

    public override void InASubunit(ASubunit node)
    {
        
        switch ()
        {
            case AIntUnittype a:
                Symbol? intId = SymbolTable.GetSymbol(a);
                SymbolTable.AddId(node.GetId(), node, intId != null ? Symbol.notOk : Symbol.Bool);
                break;
            case ADecimalUnittype b:
                Symbol? decimalId = SymbolTable.GetSymbol(b);
                SymbolTable.AddId(node.GetId(), node, decimalId != null ? Symbol.notOk : Symbol.Bool);
                break;
            case ABoolUnittype c:
                Symbol? boolId = SymbolTable.GetSymbol(c);
                SymbolTable.AddId(node.GetId(), node, boolId != null ? Symbol.notOk : Symbol.Bool);
                break;
            case ACharUnittype d:
                Symbol? charId = SymbolTable.GetSymbol(d);
                SymbolTable.AddId(node.GetId(), node, charId != null ? Symbol.notOk : Symbol.Bool);
                break;
            case AStringUnittype e:
                Symbol? stringId = SymbolTable.GetSymbol(e);
                SymbolTable.AddId(node.GetId(), node, stringId != null ? Symbol.notOk : Symbol.Bool);
                break;
            case ACustomtypeUnittype f:
                PId unitName = f.GetId();
                Symbol? unitId = SymbolTable.GetSymbol(f);
                PUnittype unitType = f;
                
                
                SymbolTable._currentSymbolTable.idToUnit.Add(unitName.ToString(),f);
                
                SymbolTable.AddId(node.GetId(), node, unitId != null ? Symbol.notOk : Symbol.ok);
                break;
        }
    }

    public override void InADeclStmt(ADeclStmt node)
    {
        base.InADeclStmt(node);
    }

    public override void InAStringDecl(AStringDecl node)
    {
        Symbol? stringId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, stringId != null ? Symbol.notOk : Symbol.String);
    }
    
    public override void InACharDecl(ACharDecl node)
    {
        Symbol? charId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, charId != null ? Symbol.notOk : Symbol.Char);
    }

    public override void InAIntDecl(AIntDecl node)
    {
        Symbol? intId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, intId != null ? Symbol.notOk : Symbol.Int);
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        Symbol? decimalId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, decimalId != null ? Symbol.notOk :Symbol.Decimal);
    }

    public override void OutStart(Start node)
    {
        SymbolTable.ResetScope();
    }
    
    public override void OutASubunit(ASubunit node)
    {
        if (SymbolTable.IsInCurrentScope(node.GetId()))
        {
            SymbolTable.AddId(node.GetId(),node,Symbol.notOk);
        }
        else
        {
            //ved ikke om dette er rigtigt. 
            AExpStmt stmt = (AExpStmt)node.GetStmt();
            Symbol? exprType = SymbolTable.GetSymbol(stmt.GetExp());
            Symbol? type = SymbolTable.GetSymbol(node.GetId().Parent());
            SymbolTable.AddId(node.GetId(),node, type != exprType? Symbol.notOk : Symbol.ok);
            //tilføj tjek om der evalueres til ok eller ej?
            //tilføjer subunit til dens parent i Dictionary
            SymbolTable._currentSymbolTable.SubunitToUnit.Add(node.GetId().ToString().Replace(" ", ""),
                    type == exprType? (AUnit)node.Parent() : null);
        }
    }

    public override void OutASingleunit(ASingleunit node)
    {
        //tilføjer simpleunit f.eks: 5ms til typen f.eks Time
        var singleUnit = SymbolTable._currentSymbolTable.SubunitToUnit[node.GetId().ToString().Replace(" ", "")];
        SymbolTable._currentSymbolTable.nodeToUnit.Add(node,singleUnit); 
    }
    
    public override void OutAUnitsExp(AUnitsExp node)
    {
        //tager den første unit såsom 5ms og sammenligner med de andre efterfølgende.
        var aUnit = SymbolTable._currentSymbolTable.nodeToUnit[(ASingleunit)node.GetSingleunit()[0]];
        foreach (ASingleunit singleunit in node.GetSingleunit())
        {
            if (SymbolTable._currentSymbolTable.nodeToUnit[singleunit] != aUnit)
            {
                //ikke sikker
                SymbolTable.AddNode(node,Symbol.notOk);
                return;
            }
        }
        SymbolTable._currentSymbolTable.nodeToUnit.Add(node,aUnit);
        
    }

    // Assignments
    
    /*public override void InAAssignStmt(AAssignStmt node)
    {
       
    }*/
    
}