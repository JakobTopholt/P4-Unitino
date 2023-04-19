using Compiler.Visitors;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class SymbolTable
{
    private Scope _currentScope;
    private List<Scope> _scopes = new ();
    public SymbolTable()
    {
        _currentScope = new Scope(null);
        _scopes.Add(_currentScope); //Global scope
    }

    public void AddSymbol(string identifier, Symbol symbol)
    {
        //TODO: Mangler tjek for at må ikke deklarerer variable med samme id to gange i samme scope
        _currentScope.AddSymbol(identifier, symbol);
    }

    public void AddReturnSymbol(string identifier, Symbol symbol)
    {
        _currentScope.AddReturnSymbol(identifier, symbol);
    }

    public Scope GetCurrentScope()
    {
        return _currentScope;
    }
    public Scope
    
    public void EnterScope()
    {
        _currentScope = new Scope(_currentScope);
        _scopes.Add(_currentScope);
    }

    public void ExitScope()
    {
        _currentScope = _currentScope.GetParent();
    }
    
}
