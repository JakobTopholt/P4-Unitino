using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalScopeVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    private TypeChecker typeChecker;
    public GlobalScopeVisitor(SymbolTable symbolTable, TypeChecker typeChecker)
    {
        this.symbolTable = symbolTable;
        this.typeChecker = typeChecker;
    }
    
    // implement globalscope declarations
    
    
    
    
    
    
}