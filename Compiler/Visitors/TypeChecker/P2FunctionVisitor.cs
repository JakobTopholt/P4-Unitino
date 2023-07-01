using System.Collections;
using Compiler.Visitors.TypeChecker.Utils;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors.TypeChecker;

// This is the second pass of the typechecker

internal class P2FunctionVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    private P3LogicChecker _p3LogicCheckerGlobal;
    private Logger _logger;
    public P2FunctionVisitor(SymbolTable symbolTable, Logger output)
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
        symbolTable = symbolTable.ResetScope();
        _logger.PrintAllErrors();
    }

    public override void CaseAArg(AArg node)
    {
        InAArg(node);
        if(node.GetType() != null)
        {
            _p3LogicCheckerGlobal.symbolTable = symbolTable;
            node.GetType().Apply(_p3LogicCheckerGlobal);
            symbolTable = _p3LogicCheckerGlobal.symbolTable;
        }
        if(node.GetId() != null)
        {
            _p3LogicCheckerGlobal.symbolTable = symbolTable;
            node.GetId().Apply(_p3LogicCheckerGlobal);
            symbolTable = _p3LogicCheckerGlobal.symbolTable;
        }
        OutAArg(node);
    }

    public override void InAArg(AArg node)
    {
        
    }

    public override void OutAArg(AArg node)
    {
        string id = node.GetId().Text;
        switch (node.GetType())
        {
            case AIntType:
                symbolTable.AddIdToNode(id, node);
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case ADecimalType:
                symbolTable.AddIdToNode(id, node);
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case ABoolType:
                symbolTable.AddIdToNode(id, node);
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case ACharType:
                symbolTable.AddIdToNode(id, node);
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case AStringType:
                symbolTable.AddIdToNode(id, node);
                symbolTable.AddNode(node, Symbol.String);
                break;
            case APinType:
                symbolTable.AddIdToNode(id, node);
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            case AUnitType customType:
            {
                symbolTable.GetUnit(customType, out var unit);
                symbolTable.AddNodeToUnit(node, unit);
                symbolTable.AddNode(node, Symbol.Ok);
                symbolTable.AddIdToNode(id, node);
                break; 
            }
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                symbolTable.AddIdToNode(id, node);
                break;
        }
    }

    public override void CaseAUntypedGlobal(AUntypedGlobal node)
    {
        InAUntypedGlobal(node);
        {
            Object[] temp = new Object[node.GetArg().Count];
            node.GetArg().CopyTo(temp, 0);
            for(int i = 0; i < temp.Length; i++)
            {
                ((PArg) temp[i]).Apply(this);
            }
        }
        OutAUntypedGlobal(node);
    }
    public override void InAUntypedGlobal(AUntypedGlobal node)
    {
        if(symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            symbolTable.AddNode(node, Symbol.NotOk);
        }
        else
        {
            // Save arguments
            List<AArg> args = node.GetArg().OfType<AArg>().ToList();
            List<PType> typeArgs = new List<PType>();
            foreach (AArg arg in args)
            {
                typeArgs.Add(arg.GetType());
            }
            symbolTable.AddFunctionArgs(node, typeArgs);
            symbolTable.AddIdToNode(node.GetId().ToString(), node);
            symbolTable = symbolTable.EnterScope();
            //symbolTable.AddArgsToScope(node, node.GetArg());
        }
        DefaultIn(node);
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        IList stmts = node.GetStmt();
        
        bool untypedIsOk = true;
        foreach (PStmt stmt in stmts)
        {
            _p3LogicCheckerGlobal.symbolTable = symbolTable;
            stmt.Apply(_p3LogicCheckerGlobal);
            symbolTable = _p3LogicCheckerGlobal.symbolTable;
            if (symbolTable.GetSymbol(stmt) == Symbol.NotOk)
            {
                untypedIsOk = false;
            }
        }
        if (!untypedIsOk)
        {
            symbolTable = symbolTable.ExitScope();
            symbolTable.AddNode(node, Symbol.NotOk);
        }
        else
        {
            Symbol firstReturn = Symbol.Func;
            Symbol returnSymbol = Symbol.Func;
            (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unit = new();
            List<AReturnStmt> returnStmts = node.GetStmt().OfType<AReturnStmt>().ToList();
            foreach (AReturnStmt reStmt in returnStmts)
            {
                var symbol = symbolTable.GetSymbol(reStmt);
                if (firstReturn == Symbol.Func && symbol != null)
                {
                    firstReturn = (Symbol)symbol;
                    returnSymbol = (Symbol)symbol;
                    if (symbol == Symbol.Ok)
                    {
                        symbolTable.GetUnit(reStmt, out unit);
                    }
                }
                else
                {
                    if (symbol != firstReturn)
                    {
                        returnSymbol = Symbol.NotOk;
                    }
                }
            }
            symbolTable = symbolTable.ExitScope();
            symbolTable.AddNode(node, returnSymbol);
            if (returnSymbol == Symbol.Ok)
            {
                symbolTable.AddNodeToUnit(node, unit);
            }
        }
        DefaultOut(node);
    }

    public override void CaseATypedGlobal(ATypedGlobal node)
    {
        InATypedGlobal(node);
        {
            Object[] temp = new Object[node.GetArg().Count];
            node.GetArg().CopyTo(temp, 0);
            for(int i = temp.Length - 1; i >= 0; i--)
            {
                ((PArg) temp[i]).Apply(this);
            }
        }
        if(node.GetType() != null)
        {
            _p3LogicCheckerGlobal.symbolTable = symbolTable;
            node.GetType().Apply(_p3LogicCheckerGlobal);
            symbolTable = _p3LogicCheckerGlobal.symbolTable;
        }
        OutATypedGlobal(node);
    }
    public override void InATypedGlobal(ATypedGlobal node)
    {
        if(symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            symbolTable.AddNode(node, Symbol.NotOk);
        }
        else
        {
            // Save arguments
            List<AArg> args = node.GetArg().OfType<AArg>().ToList();
            List<PType> typeArgs = new List<PType>();
            foreach (AArg arg in args)
            {
                typeArgs.Add(arg.GetType());
            }
            symbolTable.AddFunctionArgs(node, typeArgs);
            symbolTable.AddIdToNode(node.GetId().ToString(), node);
            symbolTable = symbolTable.EnterScope();
            //symbolTable.AddArgsToScope(node, node.GetArg());
        }
        DefaultIn(node);
    }

    public override void OutATypedGlobal(ATypedGlobal node)
    {
        IList stmts = node.GetStmt();
        bool typedIsOk = true;
        foreach (PStmt stmt in stmts)
        {
            _p3LogicCheckerGlobal.symbolTable = symbolTable;
            stmt.Apply(_p3LogicCheckerGlobal);
            symbolTable = _p3LogicCheckerGlobal.symbolTable;
        }
        if (!typedIsOk)
        {
            symbolTable = symbolTable.ExitScope();
            symbolTable.AddNode(node, Symbol.NotOk);
        }
        else
        {
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                case APinType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Pin);
                    break;
                case AVoidType:
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNode(node, Symbol.Func);
                    break;
                case AUnitType a:
                    // Missing a reference from this node to the unitType
                    symbolTable.GetUnit(a, out var unit);
                    symbolTable = symbolTable.ExitScope();
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                    break;
            }
        }
        DefaultOut(node);
    }

    public override void InAIfStmt(AIfStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAIfStmt(AIfStmt node)
    {
        DefaultOut(node);
        symbolTable = symbolTable.ExitScope();
    }

    public override void InAElseifStmt(AElseifStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAElseifStmt(AElseifStmt node)
    {
        DefaultOut(node);
        symbolTable = symbolTable.ExitScope();
    }

    public override void InAElseStmt(AElseStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAElseStmt(AElseStmt node)
    {
        DefaultOut(node);
        symbolTable = symbolTable.ExitScope();
    }
    
    public override void InAForStmt(AForStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAForStmt(AForStmt node)
    {
        DefaultOut(node);
        symbolTable = symbolTable.ExitScope();
    }
    

    public override void InAWhileStmt(AWhileStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAWhileStmt(AWhileStmt node)
    {
        DefaultOut(node);
        symbolTable = symbolTable.ExitScope();
    }
    

    public override void InADowhileStmt(ADowhileStmt node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutADowhileStmt(ADowhileStmt node)
    {
        DefaultOut(node);
        symbolTable = symbolTable.ExitScope();
    }

    public override void CaseALoopGlobal(ALoopGlobal node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
        IList stmts = node.GetStmt();
            
        foreach (PStmt stmt in stmts)
        {
            if (stmt is AForStmt or AIfStmt or AElseifStmt or AElseStmt or AWhileStmt or ADowhileStmt)
            {
                symbolTable = symbolTable.EnterScope().ExitScope();
            }
        }
        DefaultOut(node);
        symbolTable = symbolTable.ExitScope();
    }
    public override void CaseAProgGlobal(AProgGlobal node)
    {
        DefaultIn(node);
        symbolTable = symbolTable.EnterScope();
        IList stmts = node.GetStmt();
            
        foreach (PStmt stmt in stmts)
        {
            if (stmt is AForStmt or AIfStmt or AElseifStmt or AElseStmt or AWhileStmt or ADowhileStmt)
            {
                symbolTable = symbolTable.EnterScope().ExitScope();
            }
        }
        DefaultOut(node);
        symbolTable = symbolTable.ExitScope();
    } 
}