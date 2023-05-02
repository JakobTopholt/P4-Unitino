using System.Collections;
using Moduino.node;

namespace Compiler.Visitors;

public class SymbolTable
{
    private readonly Dictionary<string, Node> idToNode = new();
    private readonly Dictionary<Node, Symbol> nodeToSymbol = new();
    private readonly SymbolTable? parent;
    public Dictionary<string, AUnitdecl> SubunitToUnit = new();
    public Dictionary<Node, AUnitdecl> nodeToUnit = new();


    public Dictionary<string, IList> functionidToParams = new();
    public Dictionary<string, PExp> SubunitToExp = new();
    public Dictionary<string, List<string>> Numerators = new();
    public Dictionary<string, List<string>> Denomerators = new();


    private static SymbolTable _currentSymbolTable = new (null);
    private static readonly List<SymbolTable> AllTables = new() { _currentSymbolTable };
    public static void EnterScope() => _currentSymbolTable = new SymbolTable(_currentSymbolTable);
    public static void ExitScope()
    {
        if (_currentSymbolTable.parent != null) 
            _currentSymbolTable = _currentSymbolTable.parent;
    }

    public static void ResetScope() => _currentSymbolTable = AllTables[0];

    public static Symbol? GetReturnType(PExp expression)
    {
        // Logik for at finde ud af hvilken type/symbol som en expression evalurer til
        return Symbol.ok;
    }

    public static void AddId(TId identifier, Node node, Symbol symbol)
    {
        _currentSymbolTable.idToNode.Add("" + identifier, node);
        AddNode(node, symbol);
    }

    public static void AddNode(Node node, Symbol symbol)
    {
        _currentSymbolTable.nodeToSymbol.Add(node, symbol);
    }
    
    public static void AddSubunit(TId identifier, Node node, PExp expr)
    {
        _currentSymbolTable.SubunitToExp.Add("" + identifier, expr);
        _currentSymbolTable.SubunitToUnit.Add("" + identifier, (AUnitdecl) node);
    }

    public static void AddNumerators(TId identifier, Node node, IEnumerable<ANumUnituse> nums)
    {
        List<string> numerators = new();
        foreach(ANumUnituse num in nums)
        {
            numerators.Add("" + num.GetId());
        }
        _currentSymbolTable.Numerators.Add("" + identifier ,numerators);
        
        // Addnode missing
    }
    public static void AddDenomerators(TId identifier, Node node ,IEnumerable<ADenUnituse> dens)
    {
        List<string> denomerators = new();
        foreach(ADenUnituse den in dens)
        {
            denomerators.Add("" + den.GetId());
        }
        _currentSymbolTable.Denomerators.Add("" + identifier , denomerators);
        
        // Addnode missing
    }

    public static void AddFunctionParams(TId identifier, Node node, IList param)
    {
        _currentSymbolTable.functionidToParams.Add("" + identifier, param);
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
    public static bool IsInCurrentScope(TId id) => _currentSymbolTable.idToNode.ContainsKey(id.ToString());
    public static bool IsInExtendedScope(TId id) => _currentSymbolTable._IsInCurrentScope(id);
    private bool _IsInCurrentScope(TId id) =>
        _currentSymbolTable.idToNode.ContainsKey(id.ToString()) || _currentSymbolTable.parent != null &&
        _currentSymbolTable.parent._IsInCurrentScope(id);
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
