using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class TypeChecker : DepthFirstAdapter
{
    private SymbolTable _symbolTable;

    public TypeChecker(SymbolTable symbolTable) {
        this._symbolTable = symbolTable;
    }

    // Implement methods for type checking
    
    public override void OutAssignmentStatement(AssignmentStatement node) {
        string varName = node.Id.Text;
        string varType = _symbolTable.Get(varName).Type;
        string exprType = GetExpressionType(node.Expression);

        // Check if the types match
        if (varType != exprType) {
            throw new InvalidOperationException($"Type mismatch in assignment statement: Expected '{varType}', found '{exprType}'.");
        }
    }
    
}