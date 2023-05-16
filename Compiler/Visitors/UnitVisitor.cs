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

    public override void OutAGrammar(AGrammar node)
    {
        symbolTable = symbolTable.ResetScope();
    }

    public override void InAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        StateUnit = true;
    }
    public override void OutAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        symbolTable.AddIdToNode(node.GetId().ToString(), node);
        bool subunitsIsOk = true;
        foreach (ASubunit subunit in node.GetSubunit())
        {
            if (symbolTable.GetSymbol(subunit) == Symbol.NotOk)
                subunitsIsOk = false;
        }
        symbolTable.AddNode(node, symbolTable.GetSymbol(node.GetId()) != Symbol.NotOk && subunitsIsOk ? Symbol.Ok : Symbol.NotOk);
        symbolTable.AddIdToUnitdecl(node.GetId().ToString(), node);

        StateUnit = false;
    }
    public override void OutASubunit(ASubunit node)
    {
        StateUnit = false;
        if (!symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            PExp expr = node.GetExp();

            if (symbolTable.GetSymbol(expr) == Symbol.Decimal)
            {
                // Subunit skal have gemt dens relation til parentunit
                symbolTable.AddIdToUnitdecl(node.GetId().ToString().Trim(), (AUnitdeclGlobal) node.Parent());
                symbolTable.AddNode(node, Symbol.Decimal);
            }
            else
            {
                // Expression did not evaluate to valid
                symbolTable.AddNode(node, Symbol.NotOk); 
            }
        }
        else
        {
            // Subunit's Id already declared
            symbolTable.AddNode(node, Symbol.NotOk);
        }
    }
   
}