using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class LocalScopeCollector : DepthFirstAdapter
{
    private SymbolTable _symbolTable;

    public LocalScopeCollector(SymbolTable symbolTable) {
        this._symbolTable = symbolTable;
    }

    // Implement methods for collecting local variables and functions
    // declaration 
    // if scope.parent != null
    
    
    // LocalvarDecl
    public override void InADeclStmt(ADeclStmt node) {
        // Save identifier 
        string identifier = node.ToString();
        
        // Save type
        string type = node.GetType().ToString();

        // Add the local variable to the symbol table
        _symbolTable.Add(new SymbolTableEntry(identifier, type, _symbolTable.CurrentScopeLevel));
    }
    
    // function declaration
    public override void InANewFunc(ANewFunc node) {
        string funcName = node.ToString();
        string returnType = node.GetType().ToString();

        // Add the function to the symbol table
        _symbolTable.Add(new SymbolTableEntry(funcName, returnType, SymbolTable.FunctionReturnType));
        
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.func);
        // Enter functionscope
        _symbolTable.EnterScope();
    }
    //mangler Funktions parametre

    public override void OutANewFunc(ANewFunc node) {
        // Exit functionscope
        _symbolTable.ExitScope();
    }

    public override void InABoolDecl(ABoolDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Bool);
        
    }

    public override void InAStringDecl(AStringDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.String);
    }

    public override void InACharDecl(ACharDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Char);
    }

    public override void InAIntDecl(AIntDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Int);
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.Decimal);
    }
}