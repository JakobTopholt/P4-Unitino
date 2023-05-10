using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// Dette er tredje og sidste pass af typecheckeren
// Den bruger symbolTablen vi har populated til at tjekke

public class TypeChecker : exprTypeChecker
{
    
    // TASKS
    // 1.
    // Make sure to only store local variables and not the global ones from before
    // Can be done by checking if currentScope's parent is not null (scope.parent != null) 
    // DONE
    
    // 2.
    // Fix getting a type from the nodes. (Type refers to Symbol enumerator)
    // Right now node.GetType() will give the node's type (eg. ANeWFunc node) and not the type of the value in the node.
    // DONE

    // 3 
    // Implement all the declarations, assignments and all nodes which need to be scanned by the visitor
    // I think control structures such as eg. for loops also will have local scopes so must be handled aswell (When/if we want to implement it)
    // DONE
    
    // 4
    // make sure you cant declare the same identifier twice
    // DONE
    
    // 5 
    // Mangler at håndtere funktions parametre
    // WIP
    
    // 6
    // Check for forward referencing when checking assignment. If not declared before (also in global) it is an illegal assignment
    // GlobalFunctionCollector og TypeChecker
    // DONE

    // 7
    // Typecast hierachry
    // We need to implement an understanding of the types precedence. int --> float,  float --> string eg. basicly the implicit typecasting
    // This is probably a feauture we will have to work on more, when we want to implement precedence for custom unit types.
    // To do

    // 8 Nye ændringer i denne branch: Units har nu fået suffix såsom 5s.50ms
    // Custom units typechecking. 
    // Expunits skal bestå af de samme singleunits
    // To do
    
    // 9 
    // Implement custom Units into DelcareStmnt
    // WIP
    
    // 10
    // Implementer et tredje pass for at tjekke og save return type.
    // To do
    
    // 11
    // Implement Custom unit typechecking
    // To do
    
    // 12
    // Implement declareAssignment casen
    // WIP

    // 13
    // Implement logic for the new typed func aswell. 
    // This is basicly the same logic but needs to be handled.
    // These return types can be easly saved
    
    // 14 
    // Typechecking for functioncall (Functioncalls needs to get returntype based on functioncallID)
    // Then compare in exp or whatever


    public TypeChecker(SymbolTable symbolTable) : base(symbolTable)
    {
    }

    public override void InAUntypedGlobal(AUntypedGlobal node) 
    {
        symbolTable = symbolTable.EnterScope();
    }
    public override void OutAUntypedGlobal(AUntypedGlobal node) 
    {
        symbolTable = symbolTable.ExitScope();
    }

    public override void InATypedGlobal(ATypedGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
        
    }

    public override void OutATypedGlobal(ATypedGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
    }
    
    public override void InALoopGlobal(ALoopGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutALoopGlobal(ALoopGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
    }

    public override void InAProgGlobal(AProgGlobal node)
    {
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAProgGlobal(AProgGlobal node)
    {
        symbolTable = symbolTable.ExitScope();
    }
}