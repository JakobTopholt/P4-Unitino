using System.Collections;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalVariableCollector : DepthFirstAdapter
{
    
    public override void CaseAUntypedFunc(AUntypedFunc node)
    {
        InAUntypedFunc(node);
        OutAUntypedFunc(node);
    }
    
    // This override should be implemented in GlobalFunctionCollector
    public override void InAUntypedFunc(AUntypedFunc node)
    {
        Symbol? funcId = SymbolTable.GetSymbol(node.GetId());
        //throw new Exception("lmao, already declared");
        SymbolTable.AddId(node.GetId(), node, funcId != null ? Symbol.notOk : Symbol.Func);
        // Mangler også at store funktions parametre her
        // Se task 4 i LocalScopeCollector
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
    
    public override void OutASubunit(ASubunit node)
    {
        if (SymbolTable.IsInCurrentScope(node.GetId()))
        {
            SymbolTable.AddId(node.GetId(),node,Symbol.notOk);
        }
        else
        {
            //ved ikke om dette er rigtigt. 
            //AExpStmt stmt = (AExpStmt)node.GetStmt();
            Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());
            Symbol? type = SymbolTable.GetSymbol(node.GetId().Parent());
            SymbolTable.AddId(node.GetId(),node, type != exprType? Symbol.notOk : Symbol.ok);
            //tilføj tjek om der evalueres til ok eller ej?
            //tilføjer subunit til dens parent i Dictionary
            SymbolTable._currentSymbolTable.SubunitToUnit.Add(node.GetId().ToString().Replace(" ", ""),
                    Symbol.Decimal == exprType? (AUnitdecl)node.Parent() : null);
        }
    }
    //nye
    public override void OutADecimalSingleunit(ADecimalSingleunit node)
    {
        //tilføjer simpleunit f.eks: 5ms til typen f.eks Time
        var singleUnit = SymbolTable._currentSymbolTable.SubunitToUnit[node.GetId().ToString().Replace(" ", "")];
        SymbolTable._currentSymbolTable.nodeToUnit.Add(node,singleUnit); 
    }
    //gamle
    /*public override void OutASingleunit(PSingleunit node)
    {
        //tilføjer simpleunit f.eks: 5ms til typen f.eks Time
        var singleUnit = SymbolTable._currentSymbolTable.SubunitToUnit[node.GetId().ToString().Replace(" ", "")];
        SymbolTable._currentSymbolTable.nodeToUnit.Add(node,singleUnit); 
    }
    */
    
    /* -----------VIRKER IKKE------------- */
    public override void OutAUnitExp(AUnitExp node)
    {   
        //tager den første unit såsom 5ms og sammenligner med de andre efterfølgende.
       /* var aUnit = SymbolTable._currentSymbolTable.nodeToUnit[(node.GetSingleunit())];
        SymbolTable.AddId(node.GetSingleunit(),node, ? Symbol.notOk : Symbol.ok);
        
        foreach (PSingleunit singleunit in node.GetSingleunit())
        {
            if (SymbolTable._currentSymbolTable.nodeToUnit[singleunit] != aUnit)
            {
                //ikke sikker
                SymbolTable.AddNode(node,Symbol.notOk);
                return;
            }
        }
        SymbolTable._currentSymbolTable.nodeToUnit.Add(node,aUnit);*/
        
    }

    // Assignments

}