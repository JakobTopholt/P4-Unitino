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
    public Dictionary<Node, Tuple<List<AUnitdecl>, List<AUnitdecl>>> nodeToUnit = new();

    private Dictionary<string, IList> functionidToParams = new();
    public Dictionary<TId, PType> funcToReturn = new();

    private Dictionary<string, AUnitdecl> idToUnit = new();
    public Dictionary<string, AUnitdecl> SubunitToUnit = new();
    private Dictionary<string, PExp> SubunitToExp = new();
    private Dictionary<string, List<string>> Numerators = new();
    private Dictionary<string, List<string>> Denomerators = new();
    
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

    public void AddIdToUnit(string identifier, AUnitdecl node)
    {
        idToUnit.Add(identifier, node);
    }
    
    public AUnitdecl? GetUnitFromId(string identifier)
    {
        return idToUnit[identifier];
    }
    
    public Tuple<List<AUnitdecl>, List<AUnitdecl>>? GetUnit(Node expression)
    {
        Tuple<List<AUnitdecl>, List<AUnitdecl>>? temp;
        bool found = nodeToUnit.TryGetValue(expression, out temp);
        
        return found ? temp : null;
    }
    public void AddNodeToUnit(Node node, Tuple<List<AUnitdecl>, List<AUnitdecl>> unit)
    {
        nodeToUnit.Add(node, unit);
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
    public void AddSubunit(TId identifier, Node node, PExp expr)
    {
        SubunitToExp.Add(identifier.ToString().Trim(), expr);
        SubunitToUnit.Add(identifier.ToString().Trim(), (AUnitdecl) node);
    }
    public AUnitdecl? GetUnitFromSubunit(TId identifier)
    {
        return SubunitToUnit[identifier.ToString().Trim()];
    }
    public void AddNumerators(TId identifier, Node node, IEnumerable<ANumUnituse> nums)
    {
        List<string> numerators = new();
        foreach(ANumUnituse num in nums)
        {
            numerators.Add("" + num.GetId());
        }
        Numerators.Add("" + identifier ,numerators);
        
        // Addnode missing
    }
    public void AddDenomerators(TId identifier, Node node ,IEnumerable<ADenUnituse> dens)
    {
        List<string> denomerators = new();
        foreach(ADenUnituse den in dens)
        {
            denomerators.Add("" + den.GetId());
        }
        Denomerators.Add("" + identifier , denomerators);
        
        // Addnode missing
    }

    public void AddFunctionParams(TId identifier, Node node, IList param) => functionidToParams.Add("" + identifier, param);
    public IList? GetFunctionParams(TId identifier) => functionidToParams["" + identifier];
    public void AddFirstReturnToFunction(TId identifier, PType unit) => funcToReturn.Add(identifier, unit);
    public bool DoesReturnStmtMatch(TId identifier, PType? unit) => funcToReturn[identifier] == unit;
    private Symbol? GetCurrentSymbol(Node node) => nodeToSymbol.TryGetValue(node, out Symbol symbol) ? symbol : parent?.GetCurrentSymbol(node);
    private Symbol? GetCurrentSymbol(string identifier) => idToNode.TryGetValue(identifier, out Node? node) ? GetCurrentSymbol(node) : parent?.GetCurrentSymbol(identifier);
    public Symbol? GetSymbol(Node node) => GetCurrentSymbol(node);
    public Symbol? GetSymbol(string identifier) => GetCurrentSymbol(identifier);
    public bool IsInCurrentScope(TId id) => idToNode.ContainsKey(id.ToString());
    public bool IsInExtendedScope(TId id) => _IsInCurrentScope(id);
    private bool _IsInCurrentScope(TId id) =>
        idToNode.ContainsKey(id.ToString()) || parent != null &&
        parent._IsInCurrentScope(id);
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
