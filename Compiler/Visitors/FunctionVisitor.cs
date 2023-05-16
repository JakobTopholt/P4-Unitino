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
    
    public override void OutAGrammar(AGrammar node)
    {
        symbolTable = symbolTable.ResetScope();
    }

    public override void CaseAUntypedGlobal(AUntypedGlobal node)
    {
        InAUntypedGlobal(node);
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
            List<PType> args = node.GetArg().OfType<PType>().ToList();
            symbolTable.AddFunctionArgs(node, args);
            symbolTable.AddIdToNode(node.GetId().ToString(), node);
        }
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        if (symbolTable.GetSymbol(node) == null)
        {
            if (node.GetStmt().OfType<AReturnStmt>().Count() == 0)
            {
                symbolTable.AddReturnSymbol(node, Symbol.Func);
            }
            else
            {
                AReturnStmt returnType = node.GetStmt().OfType<AReturnStmt>().ToList()[0];
                switch (symbolTable.GetSymbol(returnType))
                {
                    case Symbol.Int:
                        symbolTable.AddReturnSymbol(node, Symbol.Int);
                        break;
                    case Symbol.Decimal:
                        symbolTable.AddReturnSymbol(node, Symbol.Decimal);
                        break;
                    case Symbol.Bool:
                        symbolTable.AddReturnSymbol(node, Symbol.Bool);
                        break;
                    case Symbol.Char:
                        symbolTable.AddReturnSymbol(node, Symbol.Char);
                        break;
                    case Symbol.String:
                        symbolTable.AddReturnSymbol(node, Symbol.String);
                        break;
                    default:
                        symbolTable.AddReturnSymbol(node, symbolTable.GetUnit(returnType, out _) ? Symbol.Ok : Symbol.NotOk);
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
        }
    }

    public override void OutATypedGlobal(ATypedGlobal node)
    {
        if (symbolTable.GetSymbol(node) == null)
        {
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable.AddReturnSymbol(node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddReturnSymbol(node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddReturnSymbol(node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddReturnSymbol(node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddReturnSymbol(node, Symbol.String);
                    break;
                case AVoidType:
                    symbolTable.AddReturnSymbol(node, Symbol.Func);
                    break;
                case AUnitType customType:
                    symbolTable.GetUnit(customType, out var unit);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddReturnSymbol(node, Symbol.Ok);
                    break;
                default:
                    symbolTable.AddReturnSymbol(node, Symbol.NotOk);
                    break;
            }
        }

    }
    public override void CaseALoopGlobal(ALoopGlobal node)
    {
     
    }
    public override void CaseAProgGlobal(AProgGlobal node)
    {
  
    }
}