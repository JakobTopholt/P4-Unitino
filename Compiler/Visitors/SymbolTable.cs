using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// første scan - Globale variable
// andet scan - Lokale variabler + funktionsretur
// Tredje scan - ok/not ok - type checking

public class SymbolTable {
    public const int GlobalScope = 0;
    public const int FunctionReturnType = -1;
    private int _currentScopeLevel;
    private List<SymbolTableEntry> _entries;

    public SymbolTable() {
        _entries = new List<SymbolTableEntry>();
        _currentScopeLevel = GlobalScope;
    }
    
    public int CurrentScopeLevel => _currentScopeLevel;

    public void EnterScope() {
        _currentScopeLevel++;
    }

    public void ExitScope() {
        _entries.RemoveAll(entry => entry.ScopeLevel == _currentScopeLevel);
        _currentScopeLevel--;
    }

    public void Add(SymbolTableEntry entry) {
        _entries.Add(entry);
    }

    public bool Contains(string name, int scopeLevel) {
        return _entries.Any(entry => entry.Name == name && entry.ScopeLevel == scopeLevel);
    }

    public bool Contains(string name) {
        return _entries.Any(entry => entry.Name == name);
    }

    public SymbolTableEntry Get(string name) {
        return _entries.LastOrDefault(entry => entry.Name == name);
    }
    
    /*
    // Fandt ud af .LastOrDeafault gjorde det samme som den her længere logik
    public SymbolTableEntry Get(string name)
    {
        // Find the symbol with the given name in the current scope or outer scopes
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            SymbolTableEntry entry = _entries[i];
            if (entry.Name == name && entry.ScopeLevel <= _currentScopeLevel)
            {
                return entry;
            }
        }
        return null;
    }
    */
}
/*
public class SymbolTable : DepthFirstAdapter
{
    private Scope currentScope;
    private List<Scope> _scopes = new List<Scope>();
    public SymbolTable()
    {
        currentScope = new Scope(null);
        _scopes.Add(currentScope); //Global scope
    }

    public override void InANewFunc(ANewFunc node)
    {
        currentScope.addSymbol(node,Symbol.func);
        currentScope = new Scope(currentScope);
        _scopes.Add(currentScope);
    }
    //mangler Funktions parametre

    public override void OutANewFunc(ANewFunc node)
    {
        currentScope = currentScope.getParent();
    }

    public override void InABoolDecl(ABoolDecl node)
    {
        currentScope.addSymbol(node,Symbol.Bool);
        
    }

    public override void InAStringDecl(AStringDecl node)
    {
        currentScope.addSymbol(node,Symbol.String);
    }

    public override void InACharDecl(ACharDecl node)
    {
        currentScope.addSymbol(node,Symbol.Char);
    }

    public override void InAIntDecl(AIntDecl node)
    {
        currentScope.addSymbol(node,Symbol.Int);
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        currentScope.addSymbol(node,Symbol.Decimal);
    }

    public override void InAAssignStmt(AAssignStmt node)
    {
        
    }
    
}
*/