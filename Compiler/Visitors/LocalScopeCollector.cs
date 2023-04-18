using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class LocalScopeCollector : DepthFirstAdapter
{
    private SymbolTable _symbolTable;

    public LocalScopeCollector(SymbolTable symbolTable) {
        this._symbolTable = symbolTable;
    }

    // TASKS
    // 1.
    // Make sure to only store local variables and not the global ones from before
    // Can be done by checking if currentScope's parent is not null (scope.parent != null)
    
    // 2.
    // Implement methods for getting a type from the nodes. (Type refers to Symbol enumerator)
    // Right now node.GetType() will give the node's type (eg. ANeWFunc node) and not the type of the value in the node.

    // 3 
    // Implement all the declarations, assignments and all nodes which need to be scanned by the visitor
    // I think control structures such as eg. for loops also will have local scopes so must be handled aswell (When/if we want to implement it)
    
    // 4 
    // Mangler at håndtere funktions parametre - De skal implementeres i CFG grammaren først (tror jeg?)
    
    
    // Collecting functions and their return values
    public override void InANewFunc(ANewFunc node) {
        string funcName = node.GetId().ToString();
        string returnType = node.GetType().ToString();

        // Add the function to the symbol table
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.func);
        // Add the returnType of the function to the symbol table
        _symbolTable.AddReturnSymbol(node.GetId().ToString(), node.getReturnType());

        // Funktions parametre
        // De skal stores her (tænker jeg)
        
        _symbolTable.EnterScope();
    }
    public override void OutANewFunc(ANewFunc node) 
    {
        _symbolTable.ExitScope();
    }

    // Collecting local variables
    public override void InABoolDecl(ABoolDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Bool);
        
    }

    public override void InAStringDecl(AStringDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.String);
    }

    public override void InACharDecl(ACharDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Char);
    }

    public override void InAIntDecl(AIntDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Int);
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Decimal);
    }
}