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
        if(symbolTable.IsInCurrentScope(node.GetId()))
        {
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
        else
        {
            // Save parameters
            IList param = node.GetArg();
            if (param.Count > 0)
                symbolTable.AddFunctionParams(node.GetId(), node, param);

            symbolTable.EnterScope();
            // Tilføj parameters til local scope for funktionen
        }
    }

    public override void OutAUntypedFunc(AUntypedFunc node)
    {
        // save returntype;
        // If no return statements == Symbol.Func (void)
        // But if there is it has to be a reachable return statement in the node
        // All return statements have to evaluate to same type to be correct
        if (symbolTable.GetFuncSymbol(node.GetId().ToString()) == null)
        {
            symbolTable.AddFuncId(node.GetId(),node,Symbol.Func);
        }
        symbolTable = symbolTable.ExitScope();
    }

    public override void CaseATypedFunc(ATypedFunc node)
    {
        InATypedFunc(node);
        OutATypedFunc(node);
    }

    public override void InATypedFunc(ATypedFunc node)
    {
        if (symbolTable.IsInCurrentScope(node.GetId()))
        {
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
        }
        else
        {
            symbolTable = symbolTable.EnterScope();
            IList inputArgs = node.GetArg();
            //TODO ændre symbol.int til den rigtige type funktionen får
            symbolTable.AddFuncId(node.GetId(),node,Symbol.Int);
            // ------WIP-------
            if (inputArgs.Count > 0)
            {
                // Add to local scope
                foreach (AArg? argument in inputArgs)
                {
                    // her skal implementes decl switchen baseret på unittype
                    switch (argument.GetUnittype())
                    {
                        //SymbolTable.AddId(argument.GetId() , node, Symbol.Whatever);
                    }
                }
                // Save parameters in table
                // Change functionidToParams Dictionary to <string, List<PUnittype>> 
                symbolTable.AddFunctionParams(node.GetId(), node, inputArgs);
            }
        }
    }

    public override void OutATypedFunc(ATypedFunc node)
    {
        // Save returntype
        // But if not void it has to have a reachable return statement in the node
        // All return statements have to evaluate to same type to be correct
        symbolTable = symbolTable.ExitScope();
        
    }

    public override void InALoopFunc(ALoopFunc node)
    {
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutALoopFunc(ALoopFunc node)
    {
        symbolTable = symbolTable.ExitScope();
    }

    public override void InAProgFunc(AProgFunc node)
    {
        symbolTable = symbolTable.EnterScope();
    }

    public override void OutAProgFunc(AProgFunc node)
    {
        symbolTable = symbolTable.ExitScope();
    }

    // General logic (Logic that works inside functions)
    public override void OutADeclStmt(ADeclStmt node)
    {
        if (symbolTable.IsInCurrentScope(node.GetId()))
        {
            switch (node.GetUnittype())
            {
                case ATypeUnittype type when type.GetType() is AIntType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Int);
                    break;
                case ATypeUnittype type when type.GetType() is AIntType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Decimal);
                    break;
                case ATypeUnittype type when type.GetType() is AIntType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Bool);
                    break;
                case ATypeUnittype type when type.GetType() is AIntType:
                    symbolTable.AddId(node.GetId(), node, Symbol.Char);
                    break;
                case ATypeUnittype type when type.GetType() is AIntType:
                    symbolTable.AddId(node.GetId(), node, Symbol.String);
                    break;
                case AUnitUnittype customType:
                {
                    // Declared a custom sammensat unit (Ikke en baseunit declaration)
                    IEnumerable<ANumUnituse> numerator = customType.GetUnituse().OfType<ANumUnituse>();
                    IEnumerable<ADenUnituse> denomerator = customType.GetUnituse().OfType<ADenUnituse>();
                    
                    // Declaration validering for sammensat unit her
                    // Check if Numerators or denomarots contains units that does not exist

                    symbolTable.AddNumerators(node.GetId(), node, numerator);
                    symbolTable.AddDenomerators(node.GetId(), node, denomerator);
                    break; 
                }
            }
        }
        else
        {
            symbolTable.AddId(node.GetId(), node, Symbol.notOk);
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
            if (unit is ATypeUnittype standardType)
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
            else if (unit is AUnitUnittype customType)
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