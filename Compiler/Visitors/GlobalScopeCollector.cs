using Moduino.analysis;

namespace Compiler.Visitors;

public class GlobalScopeCollector : DepthFirstAdapter
{
    private SymbolTable _symbolTable;

    public GlobalScopeCollector(SymbolTable symbolTable) {
        this._symbolTable = symbolTable;
    }
    
    // Implement methods for collecting global variables
    
    
    // Find ud af hvad er declaration statement fx hedder i vores AST
    public override void InAVarDeclarationStatement(AVarDeclarationStatement node)
    {
        string varName = node.Id.Text;
        string varType = node.Type.ToString().Trim();

        // Add the global variable to the symbol table
        _symbolTable.Add(new SymbolTableEntry(varName, varType, SymbolTable.GlobalScope));
    }
}