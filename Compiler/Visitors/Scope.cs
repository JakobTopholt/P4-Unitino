using Moduino.node;

namespace Compiler.Visitors;

public class Scope
{
    private Dictionary<string, Symbol> _symbols = new();
    private Dictionary<string, Symbol> _symbolsReturnType = new();
    private Scope? _parent;

    public Scope GetParent()
    {
        return _parent;
    }

    public Scope(Scope? parent)
    {
        this._parent = parent;
    }
    
    public Symbol? GetSymbol(string identifier)
    {
        return _symbols.ContainsKey(identifier) ? _symbols[identifier] : _parent?.GetSymbol(identifier);
    }

    public Symbol GetReturnSymbol(string identifier)
    {
        // Check if identifier is a functionSymbol just for good measure
        // This function should throw an exception if null is returned
        switch (_symbols[identifier])
        {
            case Symbol.func:
                // Return the return type/symbol of function
                return _symbolsReturnType.ContainsKey(identifier) ? _symbolsReturnType[identifier] : null;
                break;
            default:
                return null;
        }
    }

    public void AddSymbol(string identifier, Symbol symbol)
    {
         _symbols.Add(identifier, symbol);
    }
    public void AddReturnSymbol(string identifier, Symbol returnVal)
    {
        switch (_symbols[identifier])
        {
            case Symbol.func:
                _symbolsReturnType.Add(identifier, returnVal);
                break;
            default:
                // throw exception is not a function
                break;
        }
    }
}

public enum Symbol
{
    Bool,
    Int, 
    Decimal,
    Char,
    String,
    func,
    ok, 
    notOk,
}
