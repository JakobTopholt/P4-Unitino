using Compiler.Visitors.TypeChecker.Utils;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors.TypeChecker;

internal class P1GlobalScopeVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    private P3LogicChecker _p3LogicCheckerGlobal;
    private Logger _logger;
    public P1GlobalScopeVisitor(SymbolTable symbolTable, Logger output)
    {
        this.symbolTable = symbolTable;
        _p3LogicCheckerGlobal = new P3LogicChecker(symbolTable, output);
        _logger = output;
    }
    // implement globalscope declarations
    public override void DefaultIn(Node node)
    {
        _logger.EnterNode(node);
    }

    public override void DefaultOut(Node node)
    {
        _logger.ExitNode(node);
    }

    public override void CaseAGrammar(AGrammar node)
    {
        InAGrammar(node);
        {
            Object[] temp = new Object[node.GetGlobal().Count];
            node.GetGlobal().CopyTo(temp, 0);
            for(int i = 0; i < temp.Length; i++)
            {
                if (temp[i] is ADeclstmtGlobal)
                {
                    ((PGlobal) temp[i]).Apply(this);
                }
            }
        }
        OutAGrammar(node);
    }
    public override void OutAGrammar(AGrammar node)
    {
        string symbols = "";
        bool grammarIsOk = true;
        List<PGlobal> globals = node.GetGlobal().OfType<PGlobal>().ToList();
        foreach (PGlobal global in globals)
        {
            symbols += symbolTable.GetSymbol(global).ToString();
            if (symbolTable.GetSymbol(global) == Symbol.NotOk)
                grammarIsOk = false;
        }
        
        _logger.PrintAllErrors();

        symbolTable = symbolTable.ResetScope();
       // symbolTable.AddNode(node, grammarIsOk ? Symbol.Ok : Symbol.NotOk);
    }
    public override void InADeclstmtGlobal(ADeclstmtGlobal node)
    {
        DefaultIn(node);
        PStmt globalStmt = node.GetStmt();
        switch (globalStmt)
        {
            case ADeclStmt decl when !symbolTable.IsInCurrentScope(decl.GetId()):
            case ADeclassStmt declass when !symbolTable.IsInCurrentScope(declass.GetId()):
                return;
            case ADeclStmt decl:
                _logger.ThrowError($"The id: {decl.GetId()} has already been declared before");
                break;
            case ADeclassStmt declass:
                _logger.ThrowError($"The id: {declass.GetId()} has already been declared before");
                break;
        }
        symbolTable.AddNode(node, Symbol.NotOk);
    }
    public override void OutADeclstmtGlobal(ADeclstmtGlobal node)
    {
        if (symbolTable.GetSymbol(node) == null)
            symbolTable.AddNode(node, symbolTable.GetSymbol(node.GetStmt()) != Symbol.NotOk ? Symbol.Ok : Symbol.NotOk);
        DefaultOut(node);
    }

    public override void CaseADeclassStmt(ADeclassStmt node)
    {
        InADeclassStmt(node);
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
        if(node.GetExp() != null)
        {
            _p3LogicCheckerGlobal.symbolTable = symbolTable;
            node.GetExp().Apply(_p3LogicCheckerGlobal);
            symbolTable = _p3LogicCheckerGlobal.symbolTable;
        }
        OutADeclassStmt(node);
    }

    public override void InADeclassStmt(ADeclassStmt node)
    {
        DefaultIn(node);
    }
    public override void OutADeclassStmt(ADeclassStmt node)
    {
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            Node expression = node.GetExp();
            /*typeChecker.symbolTable = symbolTable;
            expression.Apply(typeChecker);
            symbolTable = typeChecker.symbolTable; */
            Symbol? exprType = symbolTable.GetSymbol(expression);
            PType unit = node.GetType();
            switch (unit)
                {
                    case AIntType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Int ? Symbol.Int : Symbol.NotOk);
                        if (exprType != Symbol.Int)
                            _logger.ThrowError("expression is not an Int");
                        break;
                    case ADecimalType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                        if (exprType != Symbol.Decimal)
                            _logger.ThrowError("expression is not an Decimal");
                        break;
                    case ABoolType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                        if (exprType != Symbol.Bool)
                            _logger.ThrowError("expression is not an Bool");
                        break;
                    case ACharType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                        if (exprType != Symbol.Char)
                            _logger.ThrowError("expression is not an Char");
                        break;
                    case AStringType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType is Symbol.String or Symbol.Char ? Symbol.String : Symbol.NotOk);
                        if (exprType != Symbol.String)
                            _logger.ThrowError("expression is not a string or char");
                        break;
                    case APinType:
                        symbolTable.AddId(node.GetId().ToString(), node, exprType is Symbol.Int or Symbol.Pin ? Symbol.Int : Symbol.NotOk);
                        if (exprType is not (Symbol.Int or Symbol.Pin))
                            _logger.ThrowError("expression is not an an pin or integer");
                        break;
                    case AUnitType customType:
                        if (symbolTable.GetUnit(customType, out var unitType) && 
                            symbolTable.GetUnit(expression, out var expType))
                        {
                            if (symbolTable.CompareUnitTypes(unitType, expType))
                            {
                                symbolTable.AddIdToNode(node.GetId().Text, node);
                                symbolTable.AddNodeToUnit(node, unitType);
                                symbolTable.AddNode(node, Symbol.Ok); 
                            }
                            else
                            {
                                symbolTable.AddIdToNode(node.GetId().ToString(), node);
                                symbolTable.AddNode(node, Symbol.NotOk);
                                _logger.ThrowError("expression is not correct unitType");
                            }
                        }
                        else
                        {
                            symbolTable.AddIdToNode(node.GetId().ToString(), node);
                            symbolTable.AddNode(node, Symbol.NotOk);
                            _logger.ThrowError("expression have no unitType associated\n");
                        }
                        break; 
                    default:
                        symbolTable.AddIdToNode(node.GetId().ToString(), node);
                        symbolTable.AddNode(node, Symbol.NotOk);
                        _logger.ThrowError("Wrong declaretype\n");
                        break;
                }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("id is already declared in this scope");
        }
        DefaultOut(node);
    }
    
    
    public override void InADeclStmt(ADeclStmt node)
    {
        DefaultIn(node);
    }

    public override void OutADeclStmt(ADeclStmt node)
    {
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.String);
                    break;
                case APinType:
                    symbolTable.AddId(node.GetId().ToString(), node, Symbol.Pin);
                    break;
                case AUnitType customType:
                    symbolTable.GetUnit(customType, out var unit);
                    symbolTable.AddIdToNode(node.GetId().ToString(), node);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                    break; 
                default:
                    symbolTable.AddNode(node, Symbol.NotOk);
                    _logger.ThrowError("Type not recognized");
                    break;
            }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            _logger.ThrowError("id is already declared in this scope");

        }
        DefaultOut(node);
    }
}