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
        symbolTable.AddIdToUnit(node.GetId().ToString(), node);
        StateUnit = false;
    }
    public override void OutAValueExp(AValueExp node)
    {
        symbolTable.AddNode(node, Symbol.Decimal);

        //symbolTable.AddNode(node, UnitVisitor.StateUnit ? Symbol.Decimal : Symbol.notOk);
    }
    public override void OutASubunit(ASubunit node)
    {
        // Subunit skal have gemt dens relation til parentunit
        // Den skal også have typechecket dens expression og gemt den i dictionary.
        StateUnit = false;
        if (!symbolTable.IsInExtendedScope(node.GetId()))
        {
            PExp expr = node.GetExp();

            if (symbolTable.GetSymbol(expr) == Symbol.Decimal)
            {
                symbolTable.AddSubunit(node.GetId(), node.Parent(), expr);
                symbolTable.AddNode(node, Symbol.ok);
            }
            else
            {
                // Expression did not evaluate to valid
                symbolTable.AddId(node.GetId(), node, Symbol.notOk); 
            }
        }
        else
        {
            // Subunit's Id already declared
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }
    public override void OutAUnitType(AUnitType node)
    {
       // Save reference from node to tuple
       List<ANumUnituse> numerator = node.GetUnituse().OfType<ANumUnituse>().ToList();
       List<ADenUnituse> denomerator = node.GetUnituse().OfType<ADenUnituse>().ToList();
       // Implement logic here
       
       List<AUnitdecl> newNums = numerator.Select(x => symbolTable.GetUnitFromId(x.GetId().ToString())).ToList();
       List<AUnitdecl> newDens = numerator.Select(x => symbolTable.GetUnitFromId(x.GetId().ToString())).ToList();
       
       
       Tuple<List<AUnitdecl>, List<AUnitdecl>> unit = new Tuple<List<AUnitdecl>, List<AUnitdecl>>(newNums, newDens);
       symbolTable.AddNodeToUnit(node, unit);

    }
}