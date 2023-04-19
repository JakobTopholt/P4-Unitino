using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

public class GlobalScopeCollector : DepthFirstAdapter
{
    private SymbolTable _symbolTable;
    //public Dictionary<string, Symbol> isDeclared = new Dictionary<string, Symbol>();

    public GlobalScopeCollector(SymbolTable symbolTable) {
        this._symbolTable = symbolTable;
    }
    
    public override void CaseANewFunc(ANewFunc node)
    {
        InANewFunc(node);
        OutANewFunc(node);
    }
    
    public override void InANewFunc(ANewFunc node)
    {
        _symbolTable.AddSymbol(node.GetId().ToString(), Symbol.func);
        
        // Mangler også at store funktions parametre her
        // Se task 4 i LocalScopeCollector
    }
   

    
    public override void InABoolDecl(ABoolDecl node)
    {
        //isDeclared.Add(node.GetId().ToString(),Symbol.Bool);
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

    // Assignments
    
    public override void InAAssignStmt(AAssignStmt node)
    {
        if (!SymbolTable.ContainsKey(node.GetId().ToString()))
        {
            //throw exception that it isnt declared yet 
        }    
    }
    
}