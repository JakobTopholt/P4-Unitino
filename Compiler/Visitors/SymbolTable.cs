using System.Collections;
using System.Threading.Tasks.Dataflow;
using Moduino.node;

namespace Compiler.Visitors;

public class SymbolTable
{
    private readonly List<SymbolTable> _allTables;
    public int _currentTable;
    public int currentScope;
    public SymbolTable(SymbolTable? parent, List<SymbolTable> allTables, int currentTable = 0, int scope = 0)
    {
        _parent = parent;
        allTables.Add(this);
        _currentTable = currentTable;
        _allTables = allTables;
        currentScope = scope;
    }
    public readonly SymbolTable? _parent;
    private readonly Dictionary<string, Node> _idToNode = new();
    public readonly Dictionary<Node, Symbol> _nodeToSymbol = new();
    
    public readonly Dictionary<string, AUnitdeclGlobal> IdToUnitDecl = new();
    public readonly Dictionary<Node, (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>)> NodeToUnit = new();
    
    private readonly Dictionary<string, Node> _funcIdToNode = new();
    private readonly Dictionary<Node, List<PType>> _nodeToArgs = new();
    public int Prog = 0;
    public int Loop = 0;

    
    
    // General methods
    public SymbolTable EnterScope()
    {
        currentScope++;
        return ++_currentTable < _allTables.Count ? _allTables[_currentTable] : new SymbolTable(this, _allTables, _currentTable, currentScope);
    }

    public SymbolTable ExitScope()
    {
        currentScope--;
        if (_parent == null) 
            return this;
        _parent._currentTable = _currentTable;
        _parent.currentScope = currentScope;
        return _parent;

    }
    public SymbolTable ResetScope()
    {
        SymbolTable table = this;
        while (table._parent != null)
        {
            table = table._parent;
        }
        currentScope = 0;
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
    public bool GetUnit(string identifier, out (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unit)
    {
        unit = (null, null);
        GetNodeFromId(identifier.Trim(), out Node? node);
        if (node == null)
        {
            return false;
        }
        else
        {
            return GetUnit(node, out unit);  
        }
    }
    public bool GetUnit(Node expression, out (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unit)
    {
        return NodeToUnit.TryGetValue(expression, out unit)
               || (_parent != null && _parent.GetUnit(expression, out unit));
    }
    
    public void AddNodeToUnit(Node node, (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unit)
    {
        NodeToUnit.Add(node, unit);
    }

    public void AddIdToUnitdecl(string identifier, AUnitdeclGlobal node) => IdToUnitDecl.Add(identifier.Trim(), node);
    public AUnitdeclGlobal? GetUnitdeclFromId(string identifier) => IdToUnitDecl.TryGetValue(identifier.Trim(), out AUnitdeclGlobal? result)
            ? result : _parent?.GetUnitdeclFromId(identifier);
    public bool CompareUnitTypes((SortedList<string, AUnitdeclGlobal> nums, SortedList<string, AUnitdeclGlobal> dens) unit1, (SortedList<string, AUnitdeclGlobal> nums, SortedList<string, AUnitdeclGlobal> dens) unit2)
    {
        return unit1.nums.SequenceEqual(unit2.nums) && unit1.dens.SequenceEqual(unit2.dens);
    }
    
    
    // Function methods
    public void AddIdToFunc(string identifier, Node node) => _funcIdToNode.Add(identifier.Trim(), node);
    public Node? GetFuncFromId(string identifier)
    {
        bool found = _funcIdToNode.TryGetValue(identifier, out Node? func);
        return found ? func : null;
    }
    public void AddArgsToScope(Node node, IList args)
    {
        foreach (AArg arg in args)
        {
            string id = arg.GetId().Text;
            PType type = arg.GetType();
            switch (type)
            {
                case AIntType:
                case ADecimalType:
                case ABoolType:
                case ACharType:
                case AStringType:
                case APinType:
                case AUnitType:
                    AddIdToNode(id, arg);
                    break;
                default:
                    AddNode(node, Symbol.NotOk);
                    break;
            }
        }
    }
    public void AddFunctionArgs(Node node, List<PType> args) => _nodeToArgs.Add(node, args);
    public List<PType>? GetFunctionArgs(Node node)
    {
        bool found = _nodeToArgs.TryGetValue(node, out List<PType>? args);
        return found ? args : _parent?.GetFunctionArgs(node);
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
    Pin,
    Ok, 
    NotOk,
}
