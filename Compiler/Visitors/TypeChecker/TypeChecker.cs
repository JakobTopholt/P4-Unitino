using Compiler.Visitors.TypeChecker.Utils;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors.TypeChecker;

public class TypeChecker : DepthFirstAdapter
{
    public static void Run(SymbolTable symbolTable, Start ast)
    {
        TextWriter output = Console.Out;
        Run(symbolTable, ast, output);
    }
    public static void Run(SymbolTable symbolTable, Start ast, TextWriter output)
    {
        Logger logger = new(output);
        //Phase 0
        P0UnitVisitor unitVisitor = new(symbolTable, logger);
        ast.Apply(unitVisitor);
        
        //Phase 1
        P1GlobalScopeVisitor globalScopeVisitor = new(symbolTable, logger);
        ast.Apply(globalScopeVisitor);
        
        //Phase 2
        P2FunctionVisitor functionVisitor = new(symbolTable, logger);
        ast.Apply(functionVisitor);
        
        //Phase 3 - LogicChecker inherits StmtLogicChecker and ExpLogicChecker
        P3LogicChecker logicCheckerGlobal = new(symbolTable, logger);
        ast.Apply(logicCheckerGlobal);
    }
}