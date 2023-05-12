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
            symbolTable.AddId(node.GetId().ToString(), node, Symbol.notOk);
        }
    }

    public void AddArgsToScope(Node node, IList args)
    {
        foreach (AArg arg in args)
        {
            string id = arg.GetId().ToString();
            PType type = arg.GetType();
            switch (type)
            {
                case AIntType:
                    symbolTable.AddId(id, node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddId(id, node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddId(id, node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddId(id, node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddId(id, node, Symbol.String);
                    break;
                case AUnitType customType:
                    var unit = symbolTable.GetUnit(customType);
                    if (unit != null)
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.ok);  
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.notOk);
                    }
                    break;
                default:
                    symbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
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

            symbolTable.EnterScope();
            AddArgsToScope(node, node.GetArg());
        }
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
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
        if(symbolTable.IsInExtendedScope(node.GetId()))
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

            symbolTable.EnterScope();
            AddArgsToScope(node, node.GetArg());
        }
    }

    public override void OutATypedGlobal(ATypedGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
        if (symbolTable.GetSymbol(node) == null)
        {
            bool typedIsOk = true;
            foreach (AReturnStmt returnStmt in node.GetStmt().OfType<AReturnStmt>())
            {
                if (symbolTable.GetSymbol(returnStmt) == Symbol.notOk)
                    typedIsOk = false;
            }
            if(!typedIsOk)
                symbolTable.AddNode(node, Symbol.notOk);
        }
        if (symbolTable.GetSymbol(node) == null)
        {
            switch (node.GetType())
            {
                case AIntType:
                    symbolTable.AddNode(node, Symbol.Int);
                    break;
                case ADecimalType:
                    symbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case ABoolType:
                    symbolTable.AddNode(node, Symbol.Bool);
                    break;
                case ACharType:
                    symbolTable.AddNode(node, Symbol.Char);
                    break;
                case AStringType:
                    symbolTable.AddNode(node, Symbol.String);
                    break;
                case AVoidType:
                    symbolTable.AddNode(node, Symbol.Func);
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

    }
    public override void CaseALoopGlobal(ALoopGlobal node)
    {
        InALoopGlobal(node);
        OutALoopGlobal(node);
    }
    public override void InALoopGlobal(ALoopGlobal node)
    {
        // overvej om der skal tjekkes for "loop" allerede er deklareret før
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutALoopGlobal(ALoopGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
        bool loopIsOk = true;
        foreach (PStmt stmt in node.GetStmt())
        {
            if (symbolTable.GetSymbol(stmt) == Symbol.notOk)
                loopIsOk = false;
        }
        symbolTable.AddNode(node, loopIsOk ? Symbol.ok : Symbol.notOk);
    }

    public override void CaseAProgGlobal(AProgGlobal node)
    {
        InAProgGlobal(node);
        OutAProgGlobal(node);
    }
    public override void InAProgGlobal(AProgGlobal node)
    {
        // overvej om der skal tjekkes for "prog" allerede er deklareret før
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAProgGlobal(AProgGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
        bool progIsOk = true;
        foreach (PStmt stmt in node.GetStmt())
        {
            if (symbolTable.GetSymbol(stmt) == Symbol.notOk)
                progIsOk = false;
        }
        symbolTable.AddNode(node, progIsOk ? Symbol.ok : Symbol.notOk);
    }
}