using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class SymbolTable : DepthFirstAdapter
{
    private Scope currentScope;
    private List<Scope> _scopes = new List<Scope>();
    public SymbolTable()
    {
        currentScope = new Scope(null);
        _scopes.Add(currentScope); //Global scope
    }

    public override void InANewFunc(ANewFunc node)
    {
        currentScope.addSymbol(node,Symbol.func);
        currentScope = new Scope(currentScope);
        _scopes.Add(currentScope);
    }
    //mangler Funktions parametre

    public override void OutANewFunc(ANewFunc node)
    {
        currentScope = currentScope.getParent();
    }

    public override void InABoolDecl(ABoolDecl node)
    {
        currentScope.addSymbol(node,Symbol.Bool);
        
    }

    public override void InAStringDecl(AStringDecl node)
    {
        currentScope.addSymbol(node,Symbol.String);
    }

    public override void InACharDecl(ACharDecl node)
    {
        currentScope.addSymbol(node,Symbol.Char);
    }

    public override void InAIntDecl(AIntDecl node)
    {
        currentScope.addSymbol(node,Symbol.Int);
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        currentScope.addSymbol(node,Symbol.Decimal);
    }

    public override void InAAssignStmt(AAssignStmt node)
    {
        
    }
}