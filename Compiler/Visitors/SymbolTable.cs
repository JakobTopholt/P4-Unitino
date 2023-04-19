using Moduino.node;

namespace Compiler.Visitors;

public class SymbolTable
{
    private Dictionary<string, Symbol> _symbols = new();
    private Dictionary<string, Symbol> _symbolsReturnType = new();
    private SymbolTable? _parent;

    private static SymbolTable _currentSymbolTable = new (null);
    private static List<SymbolTable> allTables = new() { _currentSymbolTable };
    
    public static SymbolTable GetCurrentScope() => _currentSymbolTable;
    public static void EnterScope() => _currentSymbolTable = new SymbolTable(_currentSymbolTable);
    public static void ExitScope() => _currentSymbolTable = _currentSymbolTable.GetParent();
    public static void ResetScope() => _currentSymbolTable = allTables[0];

    public static void AddSymbol(string identifier, Symbol symbol)
    {
        _currentSymbolTable._symbols.Add(identifier, symbol);
    }

    public static void AddReturnSymbol(string identifier, Symbol returnVal)
    {
        switch (_currentSymbolTable._symbols[identifier])
        {
            case Symbol.func:
                _currentSymbolTable._symbolsReturnType.Add(identifier, returnVal);
                break;
            default:
                // throw exception is not a function
                break;
        }
    }
    
    private SymbolTable GetParent()
    {
        return _parent;
    }

    private SymbolTable(SymbolTable? parent)
    {
        _parent = parent;
        allTables.Add(this);
    }

    public Symbol? GetSymbol(string identifier)
    {
        return _symbols.TryGetValue(identifier, out Symbol symbol) ? symbol : _parent?.GetSymbol(identifier);
    }

    public Symbol? GetReturnSymbol(string identifier)
    {
        // Check if identifier is a functionSymbol just for good measure
        // This function should throw an exception if null is returned
        switch (_symbols[identifier])
        {
            case Symbol.func:
                // Return the return type/symbol of function
                return _symbolsReturnType.TryGetValue(identifier, out Symbol value) ? value : null;
            default:
                return null;
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
