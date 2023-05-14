using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// This is the second pass of the typechecker

public class FunctionVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    public FunctionVisitor(SymbolTable symbolTable)
    {
        this.symbolTable = symbolTable;
    }
    
    public override void OutStart(Start node) => symbolTable = symbolTable.ResetScope();
    
    public override void CaseAUntypedGlobal(AUntypedGlobal node)
    {
        InAUntypedGlobal(node);
        OutAUntypedGlobal(node);
    }
    public override void InAUntypedGlobal(AUntypedGlobal node)
    {
        if(symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
        else
        {
            // Save arguments
            List<PType> args = node.GetArg().OfType<PType>().ToList();
            if (args.Count > 0)
            {
                symbolTable.AddFunctionArgs(node, args);
            }
            symbolTable.AddIdToFunc(node.GetId().ToString(), node);
        }
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        if (symbolTable.GetSymbol(node) == null)
        {
            if (node.GetStmt().OfType<AReturnStmt>().Count() == 0)
            {
                symbolTable.AddNode(node, Symbol.Func);
            }
            else
            {
                AReturnStmt returnType = node.GetStmt().OfType<AReturnStmt>().ToList()[0];
                switch (symbolTable.GetSymbol(returnType))
                {
                    case Symbol.Int:
                        symbolTable.AddNode(node, Symbol.Int);
                        break;
                    case Symbol.Decimal:
                        symbolTable.AddNode(node, Symbol.Decimal);
                        break;
                    case Symbol.Bool:
                        symbolTable.AddNode(node, Symbol.Bool);
                        break;
                    case Symbol.Char:
                        symbolTable.AddNode(node, Symbol.Char);
                        break;
                    case Symbol.String:
                        symbolTable.AddNode(node, Symbol.String);
                        break;
                    default:
                        symbolTable.AddNode(node, symbolTable.GetUnit(returnType) != null ? Symbol.ok : Symbol.notOk);
                        break;
                }
            }
        }
    }

    public override void CaseATypedGlobal(ATypedGlobal node)
    {
        InATypedGlobal(node);
        OutATypedGlobal(node);
    }
    public override void InATypedGlobal(ATypedGlobal node)
    {
        if(symbolTable.IsInExtendedScope(node.GetId().ToString()))
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
        else
        {
            // Save arguments
            List<PType> args = node.GetArg().OfType<PType>().ToList();
            if (args.Count > 0)
            {
                symbolTable.AddFunctionArgs(node, args);
            }
            symbolTable.AddIdToFunc(node.GetId().ToString(), node);
        }
    }

    public override void OutATypedGlobal(ATypedGlobal node)
    {
        if (symbolTable.GetSymbol(node) == null)
        {
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable.AddIdToNode(node.GetId().ToString().Trim(), node);
                    symbolTable.AddNode(node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddIdToNode(node.GetId().ToString().Trim(), node);
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddIdToNode(node.GetId().ToString().Trim(), node);
                    symbolTable.AddNode(node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddIdToNode(node.GetId().ToString().Trim(), node);
                    symbolTable.AddNode(node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddIdToNode(node.GetId().ToString().Trim(), node);
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                case AVoidType:
                    symbolTable.AddIdToNode(node.GetId().ToString().Trim(), node);
                    symbolTable.AddNode(node, Symbol.Func);
                    break;
                case AUnitType customType:
                    var unit = symbolTable.GetUnit(customType);
                    symbolTable.AddIdToNode(node.GetId().ToString().Trim(), node);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.ok);
                    break;
                default:
                    symbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }

    }
    public override void CaseALoopGlobal(ALoopGlobal node)
    {
        InALoopGlobal(node);
        OutALoopGlobal(node);
    }
    public override void InALoopGlobal(ALoopGlobal node)
    {
        symbolTable.Loop++;
        if (symbolTable.Loop != 1)
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }
    public override void CaseAProgGlobal(AProgGlobal node)
    {
        InAProgGlobal(node);
        OutAProgGlobal(node);
    }
    public override void InAProgGlobal(AProgGlobal node)
    {
        symbolTable.Prog++;
        if (symbolTable.Prog != 1)
        {
            symbolTable.AddNode(node, Symbol.notOk);
        }
    }
    
}