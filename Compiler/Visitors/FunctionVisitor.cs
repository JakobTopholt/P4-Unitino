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
    
    public override void CaseALoopGlobal(ALoopGlobal node)
    {
     
    }
    public override void CaseAProgGlobal(AProgGlobal node)
    {
  
    }
}