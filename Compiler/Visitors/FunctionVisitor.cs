using System.Collections;
using Compiler.Visitors.TypeCheckerDir;
using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// This is the second pass of the typechecker

public class FunctionVisitor : DepthFirstAdapter
{
    private SymbolTable symbolTable;
    public FunctionVisitor(SymbolTable symbolTable)
    {
        this.symbolTable = symbolTable;
    }
    // Overvej om jeg mangler at kalde base.InAxx(node);
    // Collect func declarations
    // Validate return stmts
    public override void OutStart(Start node)
    {
        symbolTable = symbolTable.ResetScope();
    }

    // Function logic
    public override void CaseAUntypedFunc(AUntypedFunc node)
    {
        InAUntypedFunc(node);
        OutAUntypedFunc(node);
    }
    public override void InAUntypedFunc(AUntypedFunc node)
    {
        symbolTable.AddId(node.GetId(), node, 
            symbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Func);
        
        // Save parameters
        IList param = node.GetArg();
        if (param.Count > 0)
            symbolTable.AddFunctionParams(node.GetId(), node, param);
    }

    public override void OutAUntypedFunc(AUntypedFunc node)
    {
        // save returntype;
        // If no return statements == Symbol.Func (void)
        // But if there is it has to be a reachable return statement in the node
        // All return statements have to evaluate to same type to be correct

    }

    public override void CaseATypedFunc(ATypedFunc node)
    {
        InATypedFunc(node);
        OutATypedFunc(node);
    }
    
    public override void InATypedFunc(ATypedFunc node)
    {
        symbolTable.AddId(node.GetId(), node, 
            symbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Func);
        
        // Save parameters
        IList param = node.GetArg();
        if (param.Count > 0)
            symbolTable.AddFunctionParams(node.GetId(), node, param);
        
    }

    public override void OutATypedFunc(ATypedFunc node)
    {
        // Save returntype
        PUnittype? returnType = node.GetUnittype();

        // But if not void it has to have a reachable return statement in the node
        // All return statements have to evaluate to same type to be correct
        
        
    }

    // General logic (Logic that works inside functions)
    public override void OutADeclStmt(ADeclStmt node)
    {
        PUnittype unit = node.GetUnittype();
        var standardType = unit as ATypeUnittype;
        if (standardType != null)
        {
            switch (standardType.GetType())
            {
                case AIntType a:
                    Symbol? intId = symbolTable.GetSymbol(a);
                    symbolTable.AddId(node.GetId(), node, intId != null ? Symbol.notOk : Symbol.Int);
                    break;
                case ADecimalType b:
                    Symbol? decimalId = symbolTable.GetSymbol(b);
                    symbolTable.AddId(node.GetId(), node, decimalId != null ? Symbol.notOk : Symbol.Decimal);
                    break;
                case ABoolType c:
                    Symbol? boolId = symbolTable.GetSymbol(c);
                    symbolTable.AddId(node.GetId(), node, boolId != null ? Symbol.notOk : Symbol.Bool);
                    break;
                case ACharType d:
                    Symbol? charId = symbolTable.GetSymbol(d);
                    symbolTable.AddId(node.GetId(), node, charId != null ? Symbol.notOk : Symbol.Char);
                    break;
                case AStringType e:
                    Symbol? stringId = symbolTable.GetSymbol(e);
                    symbolTable.AddId(node.GetId(), node, stringId != null ? Symbol.notOk : Symbol.String);
                    break;
            }
        }
        // Declared a custom unit (Sammensat)
        var customType = unit as AUnitUnittype;
        if (customType != null)
        {
            // If Numerators or denomarots contains units that does not exist
            
            
            IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
            IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();

            symbolTable.AddNumerators(node.GetId(), node, numerator);
            symbolTable.AddDenomerators(node.GetId(), node, denomerator);
        }
    }
    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? type = symbolTable.GetSymbol("" + node.GetId());
        Symbol? exprType = symbolTable.GetSymbol(node.GetExp());    
        
        // if type == null (id was never declared) (The reason we dont use .isInCurrentScope here is we want to iclude foward refrences
        // if type != exprType (Incompatible types)
        symbolTable.AddNode(node, type == null || type != exprType ? Symbol.notOk : Symbol.ok);
    }

    public override void OutADeclassStmt(ADeclassStmt node)
    { 
        // Assignment have to be typechecked before Decl should add to symboltable
        bool declared = symbolTable.IsInCurrentScope(node.GetId());
        if (!declared)
        {
            Symbol? exprType = symbolTable.GetSymbol(node.GetExp());
            PUnittype unit = node.GetUnittype();
            var standardType = unit as ATypeUnittype;
            if (standardType != null)
            {
                switch (standardType.GetType())
                {
                    case AIntType a:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.Int ? Symbol.notOk : Symbol.Int);
                        break;
                    case ADecimalType b:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.Decimal ? Symbol.notOk : Symbol.Decimal);
                        break;
                    case ABoolType c:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.Bool ? Symbol.notOk : Symbol.Bool);
                        break;
                    case ACharType d:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.Char ? Symbol.notOk : Symbol.Char);
                        break;
                    case AStringType e:
                        symbolTable.AddId(node.GetId(), node, exprType == Symbol.String ? Symbol.notOk : Symbol.String);
                        break;
                }
            }
            var customType = unit as AUnitUnittype;
            if (customType != null)
            {
                IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
                IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
                
                // Mangler assignment typecheck logic

                symbolTable.AddNumerators(node.GetId(), node, numerator);
                symbolTable.AddDenomerators(node.GetId(), node, denomerator);
                
            }
        }
        else
        {
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
    }
}