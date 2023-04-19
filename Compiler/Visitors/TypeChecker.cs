using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class TypeChecker : DepthFirstAdapter
{
    private SymbolTable _symbolTable;
    
    public TypeChecker(SymbolTable symbolTable) {
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
    
    // 5
    // Check for forward referencing when checking assignment. If not declared before (also in global) it is an illegal assignment
    
    
    // Collecting functions and their return values
    /*public void IsDeclared(Node node)
    {
        string symbol = (_symbolTable.GetCurrentScope().GetSymbol(node));
        if ()
        {
            
        }
    }*/
    
    
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
        if (node.Parent() != null)
        {
            _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Bool);
        }
    }

    public override void InAStringDecl(AStringDecl node)
    {
        if (node.Parent() != null)
        {
            _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.String);
        }
    }

    public override void InACharDecl(ACharDecl node)
    {
        if (node.Parent() != null)
        {
            _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Char);
        }
    }

    public override void InAIntDecl(AIntDecl node)
    {
        if (node.Parent() != null)
        {
            _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Int);
        }
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {

        if (node.Parent() != null)
        {
            _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Decimal);
        }
    }
    //Expressions
    
    
    public override void OutADivExp(ADivExp node)
    {
        //Node l = node.GetL();
        //Node r = node.GetR();
        var l =_symbolTable.GetCurrentScope().GetSymbol(node.GetL());
        var r = _symbolTable.GetCurrentScope().GetSymbol(node.GetR());
        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.Decimal);
                break;
            default:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.notOk);
                break;
        }
    }

    public override void OutAMultExp(AMultExp node)
    {
        var l =_symbolTable.GetCurrentScope().GetSymbol(node.GetL());
        var r = _symbolTable.GetCurrentScope().GetSymbol(node.GetR());
        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.Decimal);
                break;
            default:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.notOk);
                break;
        }
    }

    public override void OutAPlusExp(APlusExp node)
    {
        var l =_symbolTable.GetCurrentScope().GetSymbol(node.GetL());
        var r = _symbolTable.GetCurrentScope().GetSymbol(node.GetR());
        
        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.Decimal);
                break;
            case Symbol.Int or Symbol.String when r is Symbol.Int or Symbol.String:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.String);
                break;
            case Symbol.Decimal or Symbol.String when r is Symbol.Decimal or Symbol.String:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.String);
                break;
            default:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.notOk);
                break;
        }
        
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        var l = _symbolTable.GetCurrentScope().GetSymbol(node.GetL());
        var r = _symbolTable.GetCurrentScope().GetSymbol(node.GetR());

        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.Decimal);
                break;
            default:
                _symbolTable.GetCurrentScope().AddSymbol(node,Symbol.notOk);
                break;
        }
    }


    public override void OutAAssignStmt(AAssignStmt node) {
        string varName = node.GetId().ToString();
        string varType = _symbolTable.Get(varName).Type;
        string exprType = GetExpressionType(node.GetExp());

        // Check if the types match
        if (varType != exprType) {
            throw new InvalidOperationException($"Type mismatch in assignment statement: Expected '{varType}', found '{exprType}'.");
        }
   
    }
}