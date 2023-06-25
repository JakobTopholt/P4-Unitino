using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// This is the second pass of the typechecker

public class FunctionVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    private TypeChecker typeChecker;
    public FunctionVisitor(SymbolTable symbolTable, TypeChecker typeChecker)
    {
        this.symbolTable = symbolTable;
        this.typeChecker = typeChecker;
    }
    
    public override void OutAGrammar(AGrammar node)
    {
        symbolTable = symbolTable.ResetScope();
    }

    public override void CaseAArg(AArg node)
    {
        typeChecker.symbolTable = symbolTable;
        InAArg(node);
        if(node.GetType() != null)
        {
            node.GetType().Apply(typeChecker);
        }
        if(node.GetId() != null)
        {
            node.GetId().Apply(typeChecker);
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
    }

    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        IList stmts = node.GetStmt();
        typeChecker.symbolTable = symbolTable;
        foreach (PStmt stmt in stmts)
        {
            stmt.Apply(typeChecker);
            /*if (stmt is AReturnStmt returnStmt)
            {
                Symbol? symbol = symbolTable.GetSymbol(returnStmt);
                if (symbol != Symbol.NotOk)
                {
                    if (symbol != Symbol.Ok)
                    {
                        symbolTable.AddNode(node, (Symbol)symbol);
                    }
                    else
                    {
                        symbolTable.GetUnit(returnStmt, out var unit);
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                }
            } */
        }

        symbolTable = symbolTable.ExitScope();
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
    }

    public override void OutATypedGlobal(ATypedGlobal node)
    {
        IList stmts = node.GetStmt();
        typeChecker.symbolTable = symbolTable;
        foreach (PStmt stmt in stmts)
        {
            stmt.Apply(typeChecker);
        }
        symbolTable = symbolTable.ExitScope();
    }

    public override void CaseALoopGlobal(ALoopGlobal node)
    {
     
    }
    public override void CaseAProgGlobal(AProgGlobal node)
    {
  
    }
}