using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// Dette er tredje og sidste pass af typecheckeren
// Den bruger symbolTablen vi har populated til at tjekke

public class TypeChecker : exprTypeChecker
{
    
    // TASKS
    
    // 1
    // Implement all the declarations, assignments and all nodes which need to be scanned by the visitor
    // To be controlled

    // 2
    // Implement unitTypechecking into DelcStmnt og DeclAssStmt
    // WIP
    
    // 3 
    // Mangler at håndtere funktions parametre
    // WIP

    // 4
    // Typechecking for functioncall (Functioncalls needs to get returntype based on functioncallID) or node reference?
    // Then compare in exp or whatever
    // To do
    
    // 5
    // Typecast hierachry
    // We need to implement an understanding of the types precedence. int --> float,  float --> string eg. basicly the implicit typecasting
    // This is probably a feauture we will have to work on more, when we want to implement precedence for custom unit types.
    // To do
    
    public TypeChecker(SymbolTable symbolTable) : base(symbolTable)
    {
    }

    public override void InAUntypedFunc(AUntypedFunc node) 
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutAUntypedFunc(AUntypedFunc node) 
    {
        symbolTable = symbolTable.ExitScope();
    }
    public override void InATypedFunc(ATypedFunc node)
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutATypedFunc(ATypedFunc node)
    {
        symbolTable = symbolTable.ExitScope();
    }
    public override void InALoopFunc(ALoopFunc node)
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutALoopFunc(ALoopFunc node)
    {
        symbolTable = symbolTable.ExitScope();
    }
    public override void InAProgFunc(AProgFunc node)
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutAProgFunc(AProgFunc node)
    {
        symbolTable = symbolTable.ExitScope();
    }
}