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

            symbolTable.EnterScope();
        }
    }
    public override void OutAUntypedGlobal(AUntypedGlobal node)
    {
        // Map node to returnType;
        // Get return type from dictionary in symboltable
        // Add the symbol or unit to its dictionary
        // If void when tryinmg to get from dictionary it must be a void function (symbol.Func)
        // ----- Logic missing here----
        
        
        symbolTable = symbolTable.ExitScope();
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

            symbolTable.EnterScope();
        }
    }
    public override void OutATypedGlobal(ATypedGlobal node)
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