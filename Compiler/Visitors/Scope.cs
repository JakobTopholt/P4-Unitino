using Moduino.node;

namespace Compiler.Visitors;

//List<Scope> scopes = new List<Scope>();
//Scope currentScope = new Scope(null);
//currentScope.addSymbol(CurrentNode, Symbol.Int);
//scopes.Add(currentScope);

//currentScope = new Scope(currentScope);

public class Scope
{
    private Dictionary<Node, Symbol> scope = new();
    private Scope? parent;

    public Scope getParent()
    {
        return parent;
    }

    public Scope(Scope? parent)
    {
        this.parent = parent;
    }

    Symbol? getSymbol(Node node)
    {
        return scope.ContainsKey(node) ? scope[node] : parent?.getSymbol(node);
    }

    public void addSymbol(Node node, Symbol symbol)
    {
        scope.Add(node, symbol);
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