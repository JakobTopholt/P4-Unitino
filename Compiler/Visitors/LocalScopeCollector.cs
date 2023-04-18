using Moduino.analysis;

namespace Compiler.Visitors;

public class LocalScopeCollector : DepthFirstAdapter
{
    private SymbolTable _symbolTable;

    public LocalScopeCollector(SymbolTable symbolTable) {
        this._symbolTable = symbolTable;
    }

    // Implement methods for collecting local variables and functions
    
    
    public override void InLocalVarDeclaration(LocalVarDeclaration node) {
        string varName = node.Id.Text;
        string varType = node.Type.ToString().Trim();

        // Add the local variable to the symbol table
        _symbolTable.Add(new SymbolTableEntry(varName, varType, _symbolTable.CurrentScopeLevel));
    }
    
    public override void InFunctionDeclaration(FunctionDeclaration node) {
        string funcName = node.Id.Text;
        string returnType = node.Type.ToString().Trim();

        // Add the function return type to the symbol table
        _symbolTable.Add(new SymbolTableEntry(funcName, returnType, SymbolTable.FunctionReturnType));

        // Process the function body and add local variables to the symbol table
        _symbolTable.EnterScope();
        node.FunctionBody.Apply(this);
        _symbolTable.ExitScope();
    }
}