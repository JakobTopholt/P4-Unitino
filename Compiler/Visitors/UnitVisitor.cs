using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// First pass of the typechecker

public class UnitVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    public UnitVisitor(SymbolTable symbolTable)
    {
        this.symbolTable = symbolTable;
    }
    public static bool StateUnit;
    public override void OutStart(Start node)
    {
        symbolTable = symbolTable.ResetScope();
    }
    public override void InAUnitdecl(AUnitdecl node)
    {
        StateUnit = true;
    }
    public override void OutAUnitdecl(AUnitdecl node)
    {
        StateUnit = false;
    }

    // Subunit skal have gemt dens relation til parentunit
    // Den skal også have typechecket dens expression og gemt den i dictionary.
    // ----------------------Se på Logikken på alt under her---------------------------
    
    public override void OutASubunit(ASubunit node)
    {
        StateUnit = false;
        if (!symbolTable.IsInExtendedScope(node.GetId()))
        {
            PExp expr = node.GetExp();
            if (symbolTable.GetReturnType(expr) == Symbol.Decimal)
            {
                symbolTable.AddSubunit(node.GetId(), node.Parent(), expr);
                symbolTable.AddId(node.GetId(), node, Symbol.ok);
            }
            else
            {
                symbolTable.AddId(node.GetId(), node, Symbol.notOk); 
            }
        }
        else
        {
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
    }
/*
    public override void OutAUnitnumber(AUnitnumber node)
    {
        // S
        base.OutAUnitnumber(node);
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
    
        /* -----------VIRKER IKKE------------- 
    public override void OutAUnitExp(AUnitExp node)
    {   
        //tager den første unit såsom 5ms og sammenligner med de andre efterfølgende.
       var aUnit = symbolTable._currentsymbolTable.nodeToUnit[(node.GetSingleunit()];
        symbolTable.AddId(node.GetSingleunit(),node, ? Symbol.notOk : Symbol.ok);
        
        foreach (PSingleunit singleunit in node.Get())
        {
            if (symbolTable._currentsymbolTable.nodeToUnit[singleunit] != aUnit)
            {
                //ikke sikker
                symbolTable.AddNode(node,Symbol.notOk);
                return;
            }
        }
        symbolTable._currentsymbolTable.nodeToUnit.Add(node,aUnit);  
    }
*/
}