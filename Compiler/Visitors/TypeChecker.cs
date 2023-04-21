using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class TypeChecker : DepthFirstAdapter
{
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
        
        SymbolTable.AddId(node.GetId(), node, Symbol.func);

        // Funktions parametre
        // De skal stores her (tænker jeg)
        // Foreach paremeter in node.getparemeters
        //   SymbolTable.AddId(parameter.id, parameter,
        //      intparameter => symbol.int, boolparameter => symbol.bool)
        
        SymbolTable.EnterScope();
    }
    public override void OutANewFunc(ANewFunc node) 
    {
        SymbolTable.ExitScope();
    }

    // Collecting local variables
    public override void InABoolDecl(ABoolDecl node)
    {
        if (node.Parent() != null)
        {
            SymbolTable.AddId(node.GetId(), node, Symbol.Bool);
        }
    }

    public override void InAStringDecl(AStringDecl node)
    {
        if (node.Parent() != null)
        {
            SymbolTable.AddId(node.GetId(), node, Symbol.String);
        }
    }

    public override void InACharDecl(ACharDecl node)
    {
        if (node.Parent() != null)
        {
            SymbolTable.AddId(node.GetId(), node, Symbol.Char);
        }
    }

    public override void InAIntDecl(AIntDecl node)
    {
        if (node.Parent() != null)
        {
            SymbolTable.AddId(node.GetId(), node, Symbol.Int);
        }
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {

        if (node.Parent() != null)
        {
            SymbolTable.AddId(node.GetId(), node, Symbol.Decimal);
        }
    }
    //Expressions
    
    
    public override void OutADivExp(ADivExp node)
    {
        //Node l = node.GetL();
        //Node r = node.GetR();
        var l =SymbolTable.GetSymbol(node.GetL());
        var r = SymbolTable.GetSymbol(node.GetR());
        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                SymbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                SymbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                SymbolTable.AddNode(node, Symbol.notOk);
                break;
        }
    }

    public override void OutAMultExp(AMultExp node)
    {
        var l =SymbolTable.GetSymbol(node.GetL());
        var r = SymbolTable.GetSymbol(node.GetR());
        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                SymbolTable.AddNode(node,Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                SymbolTable.AddNode(node,Symbol.Decimal);
                break;
            default:
                SymbolTable.AddNode(node,Symbol.notOk);
                break;
        }
    }

    public override void OutAPlusExp(APlusExp node)
    {
        var l =SymbolTable.GetSymbol(node.GetL());
        var r = SymbolTable.GetSymbol(node.GetR());
        
        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                SymbolTable.AddNode(node,Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                SymbolTable.AddNode(node,Symbol.Decimal);
                break;
            case Symbol.Int or Symbol.String when r is Symbol.Int or Symbol.String:
                SymbolTable.AddNode(node,Symbol.String);
                break;
            case Symbol.Decimal or Symbol.String when r is Symbol.Decimal or Symbol.String:
                SymbolTable.AddNode(node,Symbol.String);
                break;
            default:
                SymbolTable.AddNode(node,Symbol.notOk);
                break;
        }
        
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        var l = SymbolTable.GetSymbol(node.GetL());
        var r = SymbolTable.GetSymbol(node.GetR());

        switch (l)
        {
            case Symbol.Int when r is Symbol.Int:
                SymbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                SymbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                SymbolTable.AddNode(node, Symbol.notOk);
                break;
        }
    }


    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? type = SymbolTable.GetSymbol("" + node.GetId());
        Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());

        // if types match stmt is ok, else notok
        SymbolTable.AddNode(node, type != exprType ? Symbol.notOk : Symbol.ok);
    }
}