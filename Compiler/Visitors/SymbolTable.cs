using System.Collections;
using Moduino.node;

namespace Compiler.Visitors;

public class SymbolTable
{
    private List<SymbolTable> allTables = new();
    public SymbolTable(SymbolTable? parent, List<SymbolTable> allTables)
    {
        this.parent = parent;
        this.allTables = allTables;
    }
    private readonly SymbolTable? parent;
    private readonly Dictionary<string, Node> idToNode = new();
    private readonly Dictionary<Node, Symbol> nodeToSymbol = new();
    
    public Dictionary<string, AUnitdeclGlobal> itToUnitdecl = new();
    public Dictionary<Node, Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>> nodeToUnit = new();
    
    private readonly Dictionary<string, Node> funcidToNode = new();
    private Dictionary<Node, List<PType>> nodeToArgs = new();
    private Dictionary<Node, Symbol?> nodeToReturn = new();
    
    // General methods
    public SymbolTable EnterScope() => new(this, allTables);
    public SymbolTable ExitScope() => parent ?? this;
    public SymbolTable ResetScope()
    {
        SymbolTable table = this;
        while (table.parent != null)
        {
            table = table.parent;
        }
        return table;
    }
    public Symbol? GetSymbolFromExpr(PExp expression)
    {
        Symbol temp;
        bool found = nodeToSymbol.TryGetValue(expression, out temp);
        
        return found ? temp : null;
    }
    public void AddId(TId identifier, Node node, Symbol symbol)
    {
        idToNode.Add("" + identifier, node);
        AddNode(node, symbol);
    }
    public void AddNode(Node node, Symbol symbol) => nodeToSymbol.Add(node, symbol);
    public Symbol? GetSymbol(Node node) => GetCurrentSymbol(node);
    public Symbol? GetSymbol(string identifier) => GetCurrentSymbol(identifier);
    private Symbol? GetCurrentSymbol(Node node) => nodeToSymbol.TryGetValue(node, out Symbol symbol) ? symbol : parent?.GetCurrentSymbol(node);
    private Symbol? GetCurrentSymbol(string identifier) => idToNode.TryGetValue(identifier, out Node? node) ? GetCurrentSymbol(node) : parent?.GetCurrentSymbol(identifier);
    public bool IsInCurrentScope(TId id) => idToNode.ContainsKey(id.ToString());
    public bool IsInExtendedScope(TId id) => _IsInCurrentScope(id);
    private bool _IsInCurrentScope(TId id) =>
        idToNode.ContainsKey(id.ToString()) || parent != null &&
        parent._IsInCurrentScope(id);
    
    // Unit methods
    public void AddIdToNode(string identifier, AUnitdeclGlobal node) => idToNode.Add(identifier.Trim(), node);
    public AUnitdeclGlobal? GetUnitFromId(string identifier)
    {
        return GetCurrentUnitFromId(identifier);
    }
    private AUnitdeclGlobal? GetCurrentUnitFromId(string identifier)
    {
        return idToNode.TryGetValue(identifier.Trim(), out Node? result) ? (AUnitdeclGlobal)result : parent?.GetCurrentUnitFromId(identifier);
    }
    public Tuple<List<AUnitdeclGlobal>, List<AUnitdeclGlobal>>? GetUnit(string identifier) => idToNode.TryGetValue(identifier, out Node? node) ? GetUnit(node) : null;
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
        return funcidToNode[identifier.Trim()];
    }
    
    public void AddFunctionArgs(Node node, List<PType> args)
    {
        nodeToArgs.Add(node, args);
    }
    public List<PType>? GetFunctionArgs(Node node)
    {
        return nodeToArgs[node];
    }
    public void AddReturnSymbol(Node node, Symbol? symbol) => nodeToReturn.Add(node, symbol);
    public Symbol? GetReturnFromNode(Node node) => nodeToReturn[node];
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
