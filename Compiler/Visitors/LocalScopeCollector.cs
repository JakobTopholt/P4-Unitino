using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// This is the second pass of the typechecker
// It needs to figure out whats going on inside Global functions
// Typecheck if their inside is correct and what return type it evaluates to

// Globale funktioner må gerne refere til hindanden i deres krop.
// Men de må ikke refere til hindanen som deres returtyper
// However så skal den referede funktion have en defineret returværdi som ikke er void.
// Så kan vi typecheck og throw notOks som vi plejer

public class LocalScopeCollector : DepthFirstAdapter
{
    public override void OutStart(Start node)
    {
        SymbolTable.ResetScope();
    }
    public override void InAUntypedFunc(AUntypedFunc node)
    {
        SymbolTable.EnterScope();
        // Save funktions parametre her
  
    }

    public override void OutAUntypedFunc(AUntypedFunc node)
    {
        // Save returnType
        
        SymbolTable.ExitScope();
    }

    public override void InATypedFunc(ATypedFunc node)
    {
        SymbolTable.EnterScope(); 
    }

    public override void OutATypedFunc(ATypedFunc node)
    {
        SymbolTable.ExitScope();
    }
    
    public override void OutADeclStmt(ADeclStmt node)
    {
        PUnittype unit = node.GetUnittype();
        var standardType = unit as ATypeUnittype;
        if (standardType != null)
        {
            switch (standardType.GetType())
            {
                case AIntType a:
                    Symbol? intId = SymbolTable.GetSymbol(a);
                    SymbolTable.AddId(node.GetId(), node, intId != null ? Symbol.notOk : Symbol.Int);
                    break;
                case ADecimalType b:
                    Symbol? decimalId = SymbolTable.GetSymbol(b);
                    SymbolTable.AddId(node.GetId(), node, decimalId != null ? Symbol.notOk : Symbol.Decimal);
                    break;
                case ABoolType c:
                    Symbol? boolId = SymbolTable.GetSymbol(c);
                    SymbolTable.AddId(node.GetId(), node, boolId != null ? Symbol.notOk : Symbol.Bool);
                    break;
                case ACharType d:
                    Symbol? charId = SymbolTable.GetSymbol(d);
                    SymbolTable.AddId(node.GetId(), node, charId != null ? Symbol.notOk : Symbol.Char);
                    break;
                case AStringType e:
                    Symbol? stringId = SymbolTable.GetSymbol(e);
                    SymbolTable.AddId(node.GetId(), node, stringId != null ? Symbol.notOk : Symbol.String);
                    break;
            }
        }
        var customType = unit as AUnitUnittype;
        if (customType != null)
        {
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
            
            // Her skal logikken implementeres 
            
        }
    }
    
    // Assignments
    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? type = SymbolTable.GetSymbol("" + node.GetId());
        Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());    
        
        // if type == null (id was never declared)
        // if type != exprType (Incompatible types)
        SymbolTable.AddNode(node, type == null || type != exprType ? Symbol.notOk : Symbol.ok);
    }
    public override void OutADeclassStmt(ADeclassStmt node)
    { 
        // Assignment have to be typechecked before Decl should add to symboltable
        bool declared = SymbolTable.IsInCurrentScope(node.GetId());
        if (!declared)
        {
            Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());
            PUnittype unit = node.GetUnittype();
            var standardType = unit as ATypeUnittype;
            if (standardType != null)
            {
                switch (standardType.GetType())
                {
                    case AIntType a:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Int ? Symbol.notOk : Symbol.Int);
                        break;
                    case ADecimalType b:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Decimal ? Symbol.notOk : Symbol.Decimal);
                        break;
                    case ABoolType c:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Bool ? Symbol.notOk : Symbol.Bool);
                        break;
                    case ACharType d:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.Char ? Symbol.notOk : Symbol.Char);
                        break;
                    case AStringType e:
                        SymbolTable.AddId(node.GetId(), node, exprType == Symbol.String ? Symbol.notOk : Symbol.String);
                        break;
                }
            }
            var customType = unit as AUnitUnittype;
            if (customType != null)
            {
                IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
                IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();

                // Her skal logikken implementeres 

            }
        }
        else
        {
            SymbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
    }
}