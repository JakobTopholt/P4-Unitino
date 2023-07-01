using Compiler.Visitors.TypeChecker.Utils;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors.TypeChecker;

// First pass of the typechecker

internal class P0UnitVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    private P3LogicChecker _p3LogicCheckerGlobal;
    private Logger _logger;
    public P0UnitVisitor(SymbolTable symbolTable, Logger output)
    {
        this.symbolTable = symbolTable;
        _p3LogicCheckerGlobal = new P3LogicChecker(symbolTable, output);
        _logger = output;
    }

    public override void DefaultIn(Node node)
    {
        _logger.EnterNode(node);
    }

    public override void DefaultOut(Node node)
    {
        _logger.ExitNode(node);
    }
    public override void OutAGrammar(AGrammar node)
    {
        _logger.PrintAllErrors();
        symbolTable = symbolTable.ResetScope();
    }

    public override void InAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        if (symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError($"UnitId: {node.GetId()} has already been declared before");
        }
        DefaultIn(node);
    }
    public override void OutAUnitdeclGlobal(AUnitdeclGlobal node)
    {
        symbolTable.AddIdToNode(node.GetId().ToString(), node);
        bool subunitsIsOk = true;
        List<ASubunit> subunits = node.GetSubunit().OfType<ASubunit>().ToList();
        foreach (ASubunit subunit in subunits)
        {
            if (symbolTable.GetSymbol(subunit) == Symbol.NotOk)
            {
                subunitsIsOk = false;
            }
        }
        symbolTable.AddNode(node, subunitsIsOk ? Symbol.Ok : Symbol.NotOk);
        symbolTable.AddIdToUnitdecl(node.GetId().ToString(), node);

        DefaultOut(node);
    }

    public override void CaseASubunit(ASubunit node)
    {
        DefaultIn(node);
        if (!symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            PExp expression = node.GetExp();
            expression.Apply(_p3LogicCheckerGlobal);
            
            if (symbolTable.GetSymbol(expression) != Symbol.Decimal)
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                _logger.ThrowError($"expression did not evaluate to a float or num value");
                return;
            }
            symbolTable.AddNode(node, Symbol.Decimal);
            symbolTable.AddIdToUnitdecl(node.GetId().ToString().Trim(), (AUnitdeclGlobal) node.Parent());
            symbolTable.AddSubUnitExp(node);
        }
        else
        {
            // Subunit's Id already declared
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError($"{node.GetId()} has allready been declared");
        }
        DefaultOut(node);
    }
}