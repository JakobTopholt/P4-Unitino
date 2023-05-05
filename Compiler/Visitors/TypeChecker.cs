using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// Dette er tredje og sidste pass af typecheckeren
// Den bruger symbolTablen vi har decorated til at tjekke om alle noder er okay

public class TypeChecker : exprTypeChecker
{
    // TASKS
    
    // 1
    // Implement all the declarations, assignments and all nodes which need to be scanned by the visitor
    // To be controlled
    
    // 2
    // make sure you cant declare the same identifier twice
    // DONE
    
    // 3 
    // Mangler at håndtere funktions parametre
    // WIP
    
    // 4
    // Check for forward referencing when checking assignment. If not declared before (also in global) it is an illegal assignment
    // GlobalFunctionCollector og TypeChecker
    // DONE

    // 5
    // Typecast hierachry
    // We need to implement an understanding of the types precedence. int --> float,  float --> string eg. basicly the implicit typecasting
    // This is probably a feauture we will have to work on more, when we want to implement precedence for custom unit types.
    // To do

    // 6
    // Implement custom Units into DelcareStmnt
    // WIP
    
    // 7
    // Implementer et tredje pass for at tjekke og save return type.
    // WIP
    
    // 8
    // Implement Custom unit typechecking
    // To do
    
    // 9
    // Implement declareAssignment casen
    // WIP
    
    // 10 
    // Typechecking for functioncall (Functioncalls needs to get returntype based on functioncallID)
    // Then compare in exp or whatever
    // To do
    
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
    public TypeChecker(SymbolTable symbolTable) : base(symbolTable)
    {
        
    }
}