using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalScopeCollector : DepthFirstAdapter
{
    public override void CaseANewFunc(ANewFunc node)
    {
        InANewFunc(node);
        OutANewFunc(node);
    }
    
    public override void InANewFunc(ANewFunc node)
    {
        SymbolTable.AddId(node.GetId(), node, Symbol.func);
        
        // Mangler også at store funktions parametre her
        // Se task 4 i LocalScopeCollector
    }
   

    
    public override void InABoolDecl(ABoolDecl node)
    {
        SymbolTable.AddId(node.GetId(), node, Symbol.Bool);
        
    }

    public override void InAStringDecl(AStringDecl node)
    {
        SymbolTable.AddId(node.GetId(), node, Symbol.String);
    }

    public override void InACharDecl(ACharDecl node)
    {
        SymbolTable.AddId(node.GetId(), node, Symbol.Char);
    }

    public override void InAIntDecl(AIntDecl node)
    {
        SymbolTable.AddId(node.GetId(), node, Symbol.Int);
    }

    public override void InADecimalDecl(ADecimalDecl node)
    {
        SymbolTable.AddId(node.GetId(), node, Symbol.Decimal);
    }

    public override void OutStart(Start node)
    {
        SymbolTable.ResetScope();
    }

    // Assignments
    
    /*public override void InAAssignStmt(AAssignStmt node)
    {
       
    }*/
    
}