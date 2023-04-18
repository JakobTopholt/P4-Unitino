using Compiler.Visitors;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// første scan - Globale variable
// andet scan - Lokale variabler + funktionsretur
// Tredje scan - ok/not ok - type checking

public class SymbolTable
{
    private Scope _currentScope;
    private List<Scope> _scopes = new ();
    public SymbolTable()
    {
        _currentScope = new Scope(null);
        _scopes.Add(_currentScope); //Global scope
    }

    public void AddSymbol(string identifier, Symbol symbol)
    {
        _currentScope.addSymbol(identifier, symbol);
    }

    public Scope GetCurrentScope()
    {
        return _currentScope;
    }
    
    public void EnterScope()
    {
        _currentScope = new Scope(_currentScope);
        _scopes.Add(_currentScope);
    }

    public void ExitScope()
    {
        _currentScope = _currentScope.getParent();
    }
    
    
    /*
    
    public override void InANewFunc(ANewFunc node)
    {
        _currentScope.addSymbol(node.GetId().ToString(),Symbol.func);
        _currentScope = new Scope(_currentScope);
        _scopes.Add(_currentScope);
    }
    //mangler Funktions parametre

    public override void OutANewFunc(ANewFunc node)
    {
        _currentScope = _currentScope.getParent();
    }

    public override void InABoolDecl(ABoolDecl node)
    {
        _currentScope.addSymbol(node.GetId().ToString(),Symbol.Bool);
        
    }

    public override void InAStringDecl(AStringDecl node)
    {
        _currentScope.addSymbol(node.GetId().ToString(),Symbol.String);
    }

    public override void InACharDecl(ACharDecl node)
    {
        _currentScope.addSymbol(node.GetId().ToString(),Symbol.Char);
    }

    public override void InAIntDecl(AIntDecl node)
    {
        _currentScope.addSymbol(node.GetId().ToString(),Symbol.Int);
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        _currentScope.addSymbol(node.GetId().ToString(),Symbol.Decimal);
    }

    public override void InAAssignStmt(AAssignStmt node)
    {
        
    }
 */   
}
