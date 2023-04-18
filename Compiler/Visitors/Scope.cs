using Moduino.node;

namespace Compiler.Visitors;

//List<Scope> scopes = new List<Scope>();
//Scope currentScope = new Scope(null);
//currentScope.addSymbol(CurrentNode, Symbol.Int);
//scopes.Add(currentScope);

//currentScope = new Scope(currentScope);


public class Scope
{
    private Dictionary<string, Symbol> _symbols = new();
    private Scope? _parent;

    public Scope getParent()
    {
        return _parent;
    }

    public Scope(Scope? parent)
    {
        this._parent = parent;
    }

    Symbol? getSymbol(string identifier)
    {
        return _symbols.ContainsKey(identifier) ? _symbols[identifier] : _parent?.getSymbol(identifier);
    }

    public void addSymbol(string identifier, Symbol symbol)
    {
         _symbols.Add(identifier, symbol);
    }
}

public enum Symbol
{
    Int, 
    ok, 
    notOk,
    func,
    Bool,
    Decimal,
    Char,
    String
}
