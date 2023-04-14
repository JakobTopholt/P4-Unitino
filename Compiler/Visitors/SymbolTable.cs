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

    public override void OutANewFunc(ANewFunc node)
    {
        currentScope = currentScope.getParent();
    }
}