using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalScopeCollector : DepthFirstAdapter
{
    private SymbolTable _symbolTable;

    public GlobalScopeCollector(SymbolTable symbolTable) {
        this._symbolTable = symbolTable;
    }
    
    // Implement methods for collecting global variables
    
    // Declaration statements
    public override void InADeclStmt(ADeclStmt node)
    {
        // Save identifier 
        string identifier = node.ToString();
        
        // Save type
        string type = node.GetType().ToString();

        // Add the global variable to the symbol table
        _symbolTable.Add(new SymbolTableEntry(identifier, type, SymbolTable.GlobalScope));
    }
}