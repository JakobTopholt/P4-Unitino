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
    }
    public override void CaseAUntypedFunc(AUntypedFunc node)
    {
        InAUntypedFunc(node);
        OutAUntypedFunc(node);
    }
    public override void InAUntypedFunc(AUntypedFunc node)
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

            symbolTable.EnterScope();
        }
    }
    public override void OutAUntypedFunc(AUntypedFunc node)
    {
        // Map node to returnType;
        // Get return type from dictionary in symboltable
        // Add the symbol or unit to its dictionary
        // If void when tryinmg to get from dictionary it must be a void function (symbol.Func)
        // ----- Logic missing here----
        
        
        symbolTable = symbolTable.ExitScope();
    }
    public override void CaseATypedFunc(ATypedFunc node)
    {
        InATypedFunc(node);
        OutATypedFunc(node);
    }
    public override void InATypedFunc(ATypedFunc node)
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

            symbolTable.EnterScope();
        }
    }
    public override void OutATypedFunc(ATypedFunc node)
    {
        // Save returntype
        // But if not void it has to have a reachable return statement in the node
        // All return statements have to evaluate to same type to be correct
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
                // ----- Logic missing here----
                
                
                
                break;
        }

    }
    public override void CaseALoopFunc(ALoopFunc node)
    {
        InALoopFunc(node);
        OutALoopFunc(node);
    }
    public override void InALoopFunc(ALoopFunc node) => symbolTable = symbolTable.EnterScope();
    public override void OutALoopFunc(ALoopFunc node) => symbolTable = symbolTable.ExitScope();
    public override void CaseAProgFunc(AProgFunc node)
    {
        InAProgFunc(node);
        OutAProgFunc(node);
    }
    public override void InAProgFunc(AProgFunc node) => symbolTable = symbolTable.EnterScope();
    public override void OutAProgFunc(AProgFunc node) => symbolTable = symbolTable.ExitScope();
    
}