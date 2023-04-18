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
        
        // Enter functionscope
        _symbolTable.EnterScope();
    }

    public override void OutANewFunc(ANewFunc node) {
        // Exit functionscope
        _symbolTable.ExitScope();
    }
}