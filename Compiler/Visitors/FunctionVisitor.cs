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
    public override void OutAArg(AArg node)
    {
        // tilføj til symboltable 
        // Skal nok også ind i tredje pass ad typechecker (det lokale)
        
        if (!symbolTable.IsInCurrentScope(node.GetId()))
        {
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddId(node.GetId(), node, Symbol.String);
                    break;
                case AUnitType customType:
                {
                    var unit = symbolTable.GetUnit(customType);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.ok);
                    break; 
                }
                default:
                    symbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
        else
        {
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
        
    }
    public override void CaseAUntypedGlobal(AUntypedGlobal node)
    {
        InAUntypedGlobal(node);
        OutAUntypedGlobal(node);
    }
    public override void InAUntypedGlobal(AUntypedGlobal node)
    {
        if(symbolTable.IsInExtendedScope(node.GetId()))
        {
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
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

            symbolTable.EnterScope();
        }
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        if (node.GetStmt().OfType<AReturnStmt>().Count() == 0)
        {
            symbolTable.AddNode(node, Symbol.Func);
        }

        switch (symbolTable.GetSymbol(node))
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
            case Symbol.Func:
                symbolTable.AddNode(node, Symbol.Func);
                break;
            default:
                // Implement logikken for custom units her
                if (symbolTable.nodeToUnit.ContainsKey(node))
                {
                    var unit = symbolTable.GetUnit(node);
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.ok); 
                }
                else
                {
                    // not a valid type
                    symbolTable.AddNode(node, Symbol.notOk);
                }
                break;
        }
    }

    public override void CaseATypedGlobal(ATypedGlobal node)
    {
        InATypedGlobal(node);
        OutATypedGlobal(node);
    }
    public override void InATypedGlobal(ATypedGlobal node)
    {
        if(symbolTable.IsInExtendedScope(node.GetId()))
        {
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
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

            symbolTable.EnterScope();
        }
    }
    public override void OutATypedGlobal(ATypedGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
        switch (node.GetType())
        {
            case AIntType:
                symbolTable.AddId(node.GetId(), node, Symbol.Int);
                break;
            case ADecimalType:
                symbolTable.AddId(node.GetId(), node, Symbol.Decimal);
                break;
            case ABoolType:
                symbolTable.AddId(node.GetId(), node, Symbol.Bool);
                break;
            case ACharType:
                symbolTable.AddId(node.GetId(), node, Symbol.Char);
                break;
            case AStringType:
                symbolTable.AddId(node.GetId(), node, Symbol.String);
                break;
            case AVoidType:
                symbolTable.AddId(node.GetId(), node, Symbol.Func);
                break;
            case AUnitType customType:
                var unit = symbolTable.GetUnit(customType);
                symbolTable.AddNodeToUnit(node, unit);
                symbolTable.AddNode(node, Symbol.ok);
                break; 
            default:
                symbolTable.AddNode(node, Symbol.notOk);
                break;
        }

    }
    public override void CaseALoopGlobal(ALoopGlobal node)
    {
        InALoopGlobal(node);
        OutALoopGlobal(node);
    }
    public override void InALoopGlobal(ALoopGlobal node) => symbolTable = symbolTable.EnterScope();
    public override void OutALoopGlobal(ALoopGlobal node) => symbolTable = symbolTable.ExitScope();
    public override void CaseAProgGlobal(AProgGlobal node)
    {
        InAProgGlobal(node);
        OutAProgGlobal(node);
    }
    public override void InAProgGlobal(AProgGlobal node) => symbolTable = symbolTable.EnterScope();
    public override void OutAProgGlobal(AProgGlobal node) => symbolTable = symbolTable.ExitScope();
    
}