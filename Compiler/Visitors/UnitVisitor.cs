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
    public override void OutASubunit(ASubunit node)
    {
        // Subunit skal have gemt dens relation til parentunit
        // Den skal også have typechecket dens expression og gemt den i dictionary.
        StateUnit = false;
        if (!symbolTable.IsInExtendedScope(node.GetId()))
        {
            PExp expr = node.GetExp();
            var symbol = symbolTable.GetSymbolFromExpr(expr);
            if (symbol == Symbol.Decimal)
            {
                symbolTable.AddSubunit(node.GetId(), node.Parent(), expr);
                symbolTable.AddId(node.GetId(), node, Symbol.ok);
            }
            else
            {
                // Expression did not evaluate to decimal
                symbolTable.AddId(node.GetId(), node, Symbol.notOk); 
            }
        }
        else
        {
            // Subunit's Id already declared
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
    }
}