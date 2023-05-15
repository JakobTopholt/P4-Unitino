using System.Collections;
using Moduino.node;

namespace Compiler.Visitors;

public class SymbolTable
{
    private readonly List<SymbolTable> _allTables;
    private int _currentTable;
    public SymbolTable(SymbolTable? parent, List<SymbolTable> allTables, int currentTable = 0)
    {
        this.parent = parent;
        allTables.Add(this);
        _currentTable = currentTable;
        _allTables = allTables;
    }
    private readonly SymbolTable? parent;
    private readonly Dictionary<string, Node> idToNode = new();
    private readonly Dictionary<Node, Symbol> nodeToSymbol = new();
    
    public Dictionary<string, AUnitdeclGlobal> itToUnitdecl = new();
    public Dictionary<Node, Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>> nodeToUnit = new();
    
    private readonly Dictionary<string, Node> funcidToNode = new();
    private Dictionary<Node, List<PType>> nodeToArgs = new();
    private Dictionary<Node, Symbol?> nodeToReturn = new();
    public int Prog = 0;
    public int Loop = 0;
    
    // General methods
    public SymbolTable EnterScope()
    {
        return ++_currentTable < _allTables.Count ? _allTables[_currentTable] : new SymbolTable(this, _allTables, _currentTable);
    }

    public SymbolTable ExitScope()
    {
        if (parent == null) 
            return this;
        parent._currentTable = _currentTable;
        return parent;

    }

    public SymbolTable ResetScope()
    {
        SymbolTable table = this;
        while (table.parent != null)
        {
            table = table.parent;
        }

        _currentTable = 0;
        return table;
    }

    public Node GetNodeFromId(string identifier)
    {
        return GetCurrentNodeFromId(identifier);
    }

    private Node GetCurrentNodeFromId(string identifier)
    {
        return idToNode.TryGetValue(identifier.Trim(), out Node node) ? node : parent?.GetCurrentNodeFromId(identifier);
    }
    
    
    public void AddId(string identifier, Node node, Symbol symbol)
    {
        idToNode.Add(identifier.Trim(), node);
        AddNode(node, symbol);
    }
    public void AddNode(Node node, Symbol symbol) => nodeToSymbol.Add(node, symbol);
    public Symbol? GetSymbol(Node node) => GetCurrentSymbol(node);
    public Symbol? GetSymbol(string identifier) => GetCurrentSymbol(identifier);
    private Symbol? GetCurrentSymbol(Node node) => nodeToSymbol.TryGetValue(node, out Symbol symbol) ? symbol : parent?.GetCurrentSymbol(node);
    private Symbol? GetCurrentSymbol(string identifier) => idToNode.TryGetValue(identifier.Trim(), out Node? node) ? GetCurrentSymbol(node) : parent?.GetCurrentSymbol(identifier);
    public bool IsInCurrentScope(TId id) => idToNode.ContainsKey(id.ToString().Trim());
    public bool IsInExtendedScope(string identifier)
    {
        if(idToNode.ContainsKey(identifier.Trim()))
        {
            return true;
        } else if (funcidToNode.ContainsKey(identifier.Trim()))
        {
            return true;
        }
        if (parent == null)
        {
            return false;
        }
        parent?.IsInExtendedScope(identifier.Trim());
        return false;
    }

    private bool _IsInCurrentScope(TId id) =>
        idToNode.ContainsKey(id.ToString().Trim()) || parent != null &&
        parent._IsInCurrentScope(id);
    
    
    
    // Unit methods
    public void AddIdToNode(string identifier, Node node) => idToNode.Add(identifier.Trim(), node);
    public AUnitdeclGlobal? GetUnitFromId(string identifier)
    {
        return GetCurrentUnitFromId(identifier);
    }
    private AUnitdeclGlobal? GetCurrentUnitFromId(string identifier)
    {
        return idToNode.TryGetValue(identifier.Trim(), out Node? result) ? (AUnitdeclGlobal)result : parent?.GetCurrentUnitFromId(identifier);
    }
    public Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>? GetUnit(string identifier) => idToNode.TryGetValue(identifier.Trim(), out Node? node) ? GetUnit(node) : null;
    public Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>? GetUnit(Node expression)
    {
        Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>? temp;
        bool found = nodeToUnit.TryGetValue(expression, out temp);
        return found ? temp : null;
    }
    public void AddNodeToUnit(Node node, Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> unit) => nodeToUnit.Add(node, unit);
    public void AddIdToUnitdecl(string identifier, AUnitdeclGlobal node)
    {
        itToUnitdecl.Add(identifier.Trim(), (AUnitdeclGlobal) node);
    }
    public AUnitdeclGlobal? GetUnitdeclFromId(string identifier) => (GetCurrentUnitdeclFromId(identifier));
    private AUnitdeclGlobal? GetCurrentUnitdeclFromId(string identifier)
    {
        return itToUnitdecl.TryGetValue(identifier.ToString().Trim(), out AUnitdeclGlobal? result)
            ? result
            : parent?.GetCurrentUnitdeclFromId(identifier);
    }
    public bool CompareCustomUnits(Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> unit1, Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>> unit2)
    { 
        List<AUnitdeclGlobal> FuncNums = unit1.Item1;
        List<AUnitdeclGlobal> ReturnNums = unit2.Item1;
        List<AUnitdeclGlobal> FuncDens = unit1.Item2;
        List<AUnitdeclGlobal> ReturnDens = unit2.Item2;
                    
        var sortedFuncNums = FuncNums.OrderBy(x => x).ToList();
        var sortedReturnNums = ReturnNums.OrderBy(x => x).ToList();
        var sortedFuncDens = FuncDens.OrderBy(x => x).ToList();
        var sortedReturnDens = ReturnDens.OrderBy(x => x).ToList();

        return sortedFuncNums.SequenceEqual(sortedReturnNums) && sortedFuncDens.SequenceEqual(sortedReturnDens);
    }
    
    // Function methods
    public void AddIdToFunc(string identifier, Node node)
    {
        funcidToNode.Add(identifier.Trim(), node);
    }

    public Node? GetFuncFromId(string identifier)
    {
        Node? func;
        bool found = funcidToNode.TryGetValue(identifier.Trim(), out func);
        return found ? func : null;
    }
    
    public void AddFunctionArgs(Node node, List<PType> args)
    {
        nodeToArgs.Add(node, args);
    }
    public List<PType>? GetFunctionArgs(Node node)
    {
        bool found = nodeToArgs.TryGetValue(node, out List<PType>? args);
        return found ? args : parent?.GetFunctionArgs(node);
    }
    public void AddReturnSymbol(Node node, Symbol? symbol) => nodeToReturn.Add(node, symbol);
    public Symbol? GetReturnFromNode(Node node)
    {
        bool found = nodeToReturn.TryGetValue(node, out Symbol? returnSymbol);
        return found ? returnSymbol : parent?.GetReturnFromNode(node);
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
    ok, 
    notOk,
}
