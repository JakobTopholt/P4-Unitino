using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// First pass of the typechecker

public class UnitVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    private TypeChecker typeChecker;
    public UnitVisitor(SymbolTable symbolTable, TypeChecker typeChecker)
    {
        this.symbolTable = symbolTable;
        this.typeChecker = typeChecker;

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
        List<ASubunit> subunits = node.GetSubunit().OfType<ASubunit>().ToList();
        foreach (ASubunit subunit in subunits)
        {
            if (symbolTable.GetSymbol(subunit) == Symbol.NotOk)
                subunitsIsOk = false;
        }
        symbolTable.AddNode(node, symbolTable.GetSymbol(node.GetId()) != Symbol.NotOk && subunitsIsOk ? Symbol.Ok : Symbol.NotOk);
        symbolTable.AddIdToUnitdecl(node.GetId().ToString(), node);

        StateUnit = false;
    }

    public override void CaseASubunit(ASubunit node)
    {
        StateUnit = false;
        if (!symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            PExp expression = node.GetExp();
            expression.Apply(typeChecker);
            
            if (symbolTable.GetSymbol(expression) != Symbol.Decimal)
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                return;
            }
            symbolTable.AddNode(node, Symbol.Decimal);
            symbolTable.AddIdToUnitdecl(node.GetId().ToString().Trim(), (AUnitdeclGlobal) node.Parent());
        }
        else
        {
            // Subunit's Id already declared
            symbolTable.AddNode(node, Symbol.NotOk);
        }
    }
}