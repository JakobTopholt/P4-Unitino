using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// This the first pass of the typechecker
// Saves global varibles in the symbolTable
// Saves Customunits.
// It also saves that a global function exists (but knows doesnt know anything if it is untyped at this point)

public class GlobalVariableCollector : exprTypeChecker
{
    public static bool StateUnit;
    public override void OutStart(Start node)
    {
        SymbolTable.ResetScope();
    }
    // Skip the inside of Functions in the first pass
    public override void CaseAUntypedFunc(AUntypedFunc node)
    {
        InAUntypedFunc(node);
        OutAUntypedFunc(node);
    }
    public override void InAUntypedFunc(AUntypedFunc node)
    {
        SymbolTable.AddId(node.GetId(), node, 
            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Func);
        // Save parameters

    }
    
    public override void CaseATypedFunc(ATypedFunc node)
    {
        InATypedFunc(node);
        OutATypedFunc(node);
    }
    
    public override void InATypedFunc(ATypedFunc node)
    {
        SymbolTable.AddId(node.GetId(), node, 
            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Func);
        
        // Save parameters
        // Save returntype asswell
        
    }

    public override void OutADeclStmt(ADeclStmt node)
    {
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
        // Declared a custom unit (Sammensat)
        var customType = unit as AUnitUnittype;
        if (customType != null)
        {
            // If Numerators or denomarots contains units that does not exist
            
            
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();

            SymbolTable.AddNumerators(node.GetId(), node, numerator);
            SymbolTable.AddDenomerators(node.GetId(), node, denomerator);
        }
    }
    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? type = SymbolTable.GetSymbol("" + node.GetId());
        Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());    
        
        // if type == null (id was never declared) (The reason we dont use .isInCurrentScope here is we want to iclude foward refrences
        // if type != exprType (Incompatible types)
        SymbolTable.AddNode(node, type == null || type != exprType ? Symbol.notOk : Symbol.ok);
    }

    public override void OutADeclassStmt(ADeclassStmt node)
    { 
        // Assignment have to be typechecked before Decl should add to symboltable
        bool declared = SymbolTable.IsInCurrentScope(node.GetId());
        if (!declared)
        {
            Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());
            PUnittype unit = node.GetUnittype();
            var standardType = unit as ATypeUnittype;
            if (standardType != null)
            {
                switch (standardType.GetType())
                {
                    case AIntType a:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Int ? Symbol.notOk : Symbol.Int);
                        break;
                    case ADecimalType b:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Decimal ? Symbol.notOk : Symbol.Decimal);
                        break;
                    case ABoolType c:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Bool ? Symbol.notOk : Symbol.Bool);
                        break;
                    case ACharType d:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Char ? Symbol.notOk : Symbol.Char);
                        break;
                    case AStringType e:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.String ? Symbol.notOk : Symbol.String);
                        break;
                }
            }
            var customType = unit as AUnitUnittype;
            if (customType != null)
            {
                IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
                IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
                
                // Mangler assignment typecheck logic

                SymbolTable.AddNumerators(node.GetId(), node, numerator);
                SymbolTable.AddDenomerators(node.GetId(), node, denomerator);
                
            }
        }
        else
        {
            SymbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
    }
    public override void InAUnitdecl(AUnitdecl node)
    {
        StateUnit = true;
    }
    public override void OutAUnitdecl(AUnitdecl node)
    {
        StateUnit = false;
        // A Custom Unit declaration
        
        SymbolTable.AddId(node.GetId(), node, Symbol.notOk);
    }

    // Subunit skal have gemt dens relation til parentunit
    // Den skal også have typechecket dens expression og gemt den i dictionary.
    // ----------------------Se på Logikken på alt under her---------------------------
    
    public override void OutASubunit(ASubunit node)
    {
        StateUnit = false;
        if (!SymbolTable.IsInExtendedScope(node.GetId()))
        {
            PExp expr = node.GetExp();
            if (SymbolTable.GetReturnType(expr) == Symbol.Decimal)
            {
                SymbolTable.AddSubunit(node.GetId(), node.Parent(), expr);
                SymbolTable.AddId(node.GetId(), node, Symbol.ok);
            }
            else
            {
                SymbolTable.AddId(node.GetId(), node, Symbol.notOk); 
            }
        }
        else
        {
            SymbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
    }

    public override void OutADecimalUnitnumber(ADecimalUnitnumber node)
    {
        // S
        base.OutADecimalUnitnumber(node);
    }

    public override void OutANumSingleunit(ANumSingleunit node)
    {
        // Skal gemmes til dens id
        base.OutANumSingleunit(node);
    }

    public override void OutADenSingleunit(ADenSingleunit node)
    {
        // Skal gemmes til dens id
        base.OutADenSingleunit(node);
    }
    
    public override void OutAUnitExp(AUnitExp node)
    {
        base.OutAUnitExp(node);
    }


    // ----------------------Se på Logikken på alt under her---------------------------
    /*public override void OutASubunit(ASubunit node)
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
    
    /*public override void OutADecimalExp(ADecimalExp node)
    {
        //tilføjer simpleunit f.eks: 5ms til typen f.eks Time
        var singleUnit = SymbolTable._currentSymbolTable.SubunitToUnit[node.ToString().Replace(" ", "")];
        SymbolTable._currentSymbolTable.nodeToUnit.Add(node,singleUnit); 
    }*/

    /* -----------VIRKER IKKE------------- 
    public override void OutAUnitExp(AUnitExp node)
    {   
        //tager den første unit såsom 5ms og sammenligner med de andre efterfølgende.
       var aUnit = SymbolTable._currentSymbolTable.nodeToUnit[(node.GetSingleunit()];
        SymbolTable.AddId(node.GetSingleunit(),node, ? Symbol.notOk : Symbol.ok);
        
        foreach (PSingleunit singleunit in node.Get())
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
*/
}