using Moduino.node;

namespace Compiler.Visitors;

public class SymbolTable
{
    private readonly Dictionary<string, Node> idToNode = new();
    private readonly Dictionary<Node, Symbol> nodeToSymbol = new();
    private readonly SymbolTable? parent;
    public Dictionary<string, AUnit> SubunitToUnit = new();
    public Dictionary<Node, AUnit> nodeToUnit = new();

    public Dictionary<string, AUnit> idToUnit = new();
    public Dictionary<AUnit, PUnittype> unitToType = new();



    public static SymbolTable _currentSymbolTable = new (null);
    private static readonly List<SymbolTable> AllTables = new() { _currentSymbolTable };
    public static void EnterScope() => _currentSymbolTable = new SymbolTable(_currentSymbolTable);
    public static void ExitScope()
    {
        if (_currentSymbolTable.parent != null) 
            _currentSymbolTable = _currentSymbolTable.parent;
    }

    public static void ResetScope() => _currentSymbolTable = AllTables[0];

    public static void AddId(PId identifier, Node node, Symbol symbol)
    {
        _currentSymbolTable.idToNode.Add("" + identifier, node);
        AddNode(node, symbol);
    }

    public static void AddNode(Node node, Symbol symbol)
    {
        _currentSymbolTable.nodeToSymbol.Add(node, symbol);
    }
    
    private SymbolTable(SymbolTable? parent)
    {
        this.parent = parent;
        AllTables.Add(this);
    }
    
    public static Symbol? GetSymbol(Node node) => _currentSymbolTable.GetCurrentSymbol(node);
    public static Symbol? GetSymbol(string identifier) => _currentSymbolTable.GetCurrentSymbol(identifier);
    private Symbol? GetCurrentSymbol(string identifier) => idToNode.TryGetValue(identifier, out Node? node) ? GetCurrentSymbol(node) : parent?.GetCurrentSymbol(identifier);
    private Symbol? GetCurrentSymbol(Node node) => nodeToSymbol.TryGetValue(node, out Symbol symbol) ? symbol : parent?.GetCurrentSymbol(node);
    public static bool IsInCurrentScope(PId id) => _currentSymbolTable.idToNode.ContainsKey(id.ToString());
}

public enum Symbol
{
    Bool,
    Int, 
    Decimal,
    Char,
    String,
    Func,
    ok, 
    notOk,
}
