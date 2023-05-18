using System.Collections;
using Moduino.node;

namespace Compiler.Visitors;

public class SymbolTable
{
    private readonly List<SymbolTable> _allTables;
    private int _currentTable;
    public SymbolTable(SymbolTable? parent, List<SymbolTable> allTables, int currentTable = 0)
    {
        _parent = parent;
        allTables.Add(this);
        _currentTable = currentTable;
        _allTables = allTables;
    }
    private readonly SymbolTable? _parent;
    private readonly Dictionary<string, Node> _idToNode = new();
    private readonly Dictionary<Node, Symbol> _nodeToSymbol = new();
    
    public readonly Dictionary<string, AUnitdeclGlobal> IdToUnitDecl = new();
    public readonly Dictionary<Node, (List<AUnitdeclGlobal> numerator, List<AUnitdeclGlobal> denominator)> NodeToUnit = new();
    
    private readonly Dictionary<string, Node> _funcIdToNode = new();
    private readonly Dictionary<Node, List<PType>> _nodeToArgs = new();
    private readonly Dictionary<Node, Symbol?> _nodeToReturn = new();
    public int Prog = 0;
    public int Loop = 0;

    // General methods
    public SymbolTable EnterScope()
    {
        return ++_currentTable < _allTables.Count ? _allTables[_currentTable] : new SymbolTable(this, _allTables, _currentTable);
    }

    public SymbolTable ExitScope()
    {
        if (_parent == null) 
            return this;
        _parent._currentTable = _currentTable;
        return _parent;

    }
    public SymbolTable ResetScope()
    {
        SymbolTable table = this;
        while (table._parent != null)
        {
            table = table._parent;
        }

        _currentTable = 0;
        return table;
    }
    public bool IsInCurrentScope(TId id) => _idToNode.ContainsKey(id.ToString().Trim());
    public bool IsInExtendedScope(string identifier)
    {
        if(_idToNode.ContainsKey(identifier.Trim()))
            return true;
        if (_funcIdToNode.ContainsKey(identifier.Trim()))
            return true;
        if (_parent == null)
            return false;
        return _parent?.IsInExtendedScope(identifier.Trim()) ?? false;
    }
    
    public bool GetNodeFromId(string identifier, out Node node) => _idToNode.TryGetValue(identifier.Trim(), out node) 
                                                                   || (_parent != null && _parent.GetNodeFromId(identifier, out node));
    
    public void AddId(string identifier, Node node, Symbol symbol)
    {
        _idToNode.Add(identifier.Trim(), node);
        AddNode(node, symbol);
    }
    public void AddNode(Node node, Symbol symbol) => _nodeToSymbol.Add(node, symbol);
    public Symbol? GetSymbol(Node node) => _nodeToSymbol.TryGetValue(node, out Symbol symbol) ? symbol : _parent?.GetSymbol(node);
    public Symbol? GetSymbol(string identifier) => _idToNode.TryGetValue(identifier.Trim(), out Node? node) ? GetSymbol(node) : _parent?.GetSymbol(identifier);
    
    
    // Unit methods
    public void AddIdToNode(string identifier, Node node) => _idToNode.Add(identifier.Trim(), node);
    public bool GetUnit(string identifier, out (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unit)
    {
        unit = (null, null);
        return _idToNode.TryGetValue(identifier.Trim(), out Node? node) && GetUnit(node, out unit);
    }
    public bool GetUnit(Node expression, out (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unit) => NodeToUnit.TryGetValue(expression, out unit);
    public void AddNodeToUnit(Node node, (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unit) => NodeToUnit.Add(node, unit);
    public void AddIdToUnitdecl(string identifier, AUnitdeclGlobal node) => IdToUnitDecl.Add(identifier.Trim(), node);
    public AUnitdeclGlobal? GetUnitdeclFromId(string identifier) => IdToUnitDecl.TryGetValue(identifier.Trim(), out AUnitdeclGlobal? result)
            ? result : _parent?.GetUnitdeclFromId(identifier);
    public bool CompareCustomUnits((List<AUnitdeclGlobal> funcNums, List<AUnitdeclGlobal> funcDens) unit1, (List<AUnitdeclGlobal> returnNums, List<AUnitdeclGlobal> returnDens) unit2)
    {
        var sortedFuncNums = unit1.funcNums.OrderBy(x => x).ToList();
        var sortedReturnNums = unit2.returnNums.OrderBy(x => x).ToList();
        var sortedFuncDens = unit1.funcDens.OrderBy(x => x).ToList();
        var sortedReturnDens = unit2.returnDens.OrderBy(x => x).ToList();

        return sortedFuncNums.SequenceEqual(sortedReturnNums) && sortedFuncDens.SequenceEqual(sortedReturnDens);
    }
    
    
    // Function methods
    public void AddIdToFunc(string identifier, Node node) => _funcIdToNode.Add(identifier.Trim(), node);
    public Node? GetFuncFromId(string identifier)
    {
        bool found = _funcIdToNode.TryGetValue(identifier, out Node? func);
        return found ? func : null;
    }
    public void AddFunctionArgs(Node node, List<PType> args) => _nodeToArgs.Add(node, args);
    public List<PType>? GetFunctionArgs(Node node)
    {
        bool found = _nodeToArgs.TryGetValue(node, out List<PType>? args);
        return found ? args : _parent?.GetFunctionArgs(node);
    }
    public void AddReturnSymbol(Node node, Symbol? symbol) => _nodeToReturn.Add(node, symbol);
    public Symbol? GetReturnFromNode(Node node)
    {
        bool found = _nodeToReturn.TryGetValue(node, out Symbol? returnSymbol);
        return found ? returnSymbol : _parent?.GetReturnFromNode(node);
    }
}

public enum Symbol
{
    Bool,
    Int, 
    Decimal,
    Char,
    String,
    Func,
    Ok, 
    NotOk,
}
