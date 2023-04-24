using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalFunctionCollector : DepthFirstAdapter
{
    public override void InANewFunc(ANewFunc node)
    {
        // Mangler også at store funktions parametre her
        // Se task 4 i TypeChecker
        
    }

    public override void OutANewFunc(ANewFunc node)
    {
        Symbol? funcId = SymbolTable.GetSymbol(node.GetId());
        //throw new Exception("lmao, already declared");
        SymbolTable.AddId(node.GetId(), node, funcId != null ? Symbol.notOk : Symbol.func);
        // throws void, however we need to understand which it can return
        
    }



    public override void InABoolDecl(ABoolDecl node)
    {
        Symbol? boolId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, boolId != null ? Symbol.notOk : Symbol.Bool);
    }

    public override void InAStringDecl(AStringDecl node)
    {
        Symbol? stringId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, stringId != null ? Symbol.notOk : Symbol.String);
    }
    
    public override void InACharDecl(ACharDecl node)
    {
        Symbol? charId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, charId != null ? Symbol.notOk : Symbol.Char);
    }

    public override void InAIntDecl(AIntDecl node)
    {
        Symbol? intId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, intId != null ? Symbol.notOk : Symbol.Int);
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        Symbol? decimalId = SymbolTable.GetSymbol(node.GetId());
        SymbolTable.AddId(node.GetId(), node, decimalId != null ? Symbol.notOk :Symbol.Decimal);
    }

    public override void OutStart(Start node)
    {
        SymbolTable.ResetScope();
    }

    // Assignments
    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? type = SymbolTable.GetSymbol("" + node.GetId());
        Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());    
        
        // if type == null (id was never declared)
        // if type != exprType (Incompatible types)
        SymbolTable.AddNode(node, type == null || type != exprType ? Symbol.notOk : Symbol.ok);
    }
}