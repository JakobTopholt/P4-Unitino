 using Moduino.analysis;
using Moduino.node;

namespace Compiler.Visitors;

// Dette er typecheckeren der indeholder logikken for at typechecke hele filen
// Den bruger symbolTablen vi har populated til at tjekke alt
// Teknisk set tror jeg(?) at denne visitor måske er unødvendig senere hen, baseret på hvordan de 3 andre bliver implementeret
// (Variabel, Function, Return collectors)
// Dette er fordi de også typechecker inden de kan tilføje til symbolTable

public class TypeChecker : DepthFirstAdapter
{
    // TASKS
    // 1.
    // Make sure to only store local variables and not the global ones from before
    // Can be done by checking if currentScope's parent is not null (scope.parent != null) 
    // DONE
    
    // 2.
    // Fix getting a type from the nodes. (Type refers to Symbol enumerator)
    // Right now node.GetType() will give the node's type (eg. ANeWFunc node) and not the type of the value in the node.
    // DONE

    // 3 
    // Implement all the declarations, assignments and all nodes which need to be scanned by the visitor
    // I think control structures such as eg. for loops also will have local scopes so must be handled aswell (When/if we want to implement it)
    // DONE
    
    // 4
    // make sure you cant declare the same identifier twice
    // DONE
    
    // 5 
    // Mangler at håndtere funktions parametre - De skal implementeres i CFG grammaren først (tror jeg?)
    // To do
    
    // 6
    // Check for forward referencing when checking assignment. If not declared before (also in global) it is an illegal assignment
    // GlobalFunctionCollector og TypeChecker
    // DONE
    
    // 7
    // Implement two global scope passes. One to catch free floating variables and the next to store global functions which type return need to be evauluated.
    // This requires local variables to be stored temporarly while evaluating the global.
    // Global varaibles --> Function types --> ok/notok
    // To do
    
    // 8
    // Typecast hierachry
    // We need to implement an understanding of the types precedence. int --> float,  float --> string eg. basicly the implicit typecasting
    // This is probably a feauture we will have to work on more, when we want to implement precedence for custom unit types.
    // To do

    // 9 Nye ændringer i denne branch: Units har nu fået suffix såsom 5s.50ms
    // Custom units typechecking. 
    // Expunits skal bestå af de samme singleunits
    // To do
    
    // 10 
    // Implement custom Units into DelcareStmnt
    // WIP
    
    // 11
    // Implementer et tredje pass for at tjekke og save return type.
    // To do
    
    // 12
    // Implement Custom unit typechecking
    // To do
    
    // 13
    // Implement declareAssignment casen
    // WIP

    // 14
    // Implement logic for the new typed func aswell. 
    // This is basicly the same logic but needs to be handled.
    // These return types can be easly saved
    
    // 15 
    // Typechecking for functioncall (Functioncalls needs to get returntype based on functioncallID)
    // Then compare in exp or whatever
    
  
    // Den skal kun gemme info om functionerne så længe de ikke er global (Ligesom vi gør i DeclStmt)
    public override void InAUntypedFunc(AUntypedFunc node) {
        
        // Tjek for global scope
        
        //SymbolTable.AddId(node.GetId(), node, SymbolTable.IsInCurrentScope(node.GetId())? Symbol.notOk : Symbol.Func);

        // Funktions parametre
        // Foreach paremeter in node.getparemeters
        //   SymbolTable.AddId(parameter.id, parameter,
        //      intparameter => symbol.int, boolparameter => symbol.bool)
        
        SymbolTable.EnterScope();
    }
    public override void OutAUntypedFunc(AUntypedFunc node) 
    {
        //parametre kode?
        SymbolTable.ExitScope();
    }

    public override void InATypedFunc(ATypedFunc node)
    {
        // Tjek for global scope
        // Save function parameters
        
        // Save return type
        
        SymbolTable.EnterScope();
    }

    public override void OutATypedFunc(ATypedFunc node)
    {
        base.OutATypedFunc(node);
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
                    if (a.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node, 
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Int);
                    }
                    break;
                case ADecimalType b:
                    if (b.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node,
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Decimal);
                    }
                    break;
                case ABoolType c:
                    if (c.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node,
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Bool);
                    }
                    break;
                case ACharType d:
                    if (d.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node,
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.Char);
                    }
                    break;
                case AStringType e:
                    if (e.Parent() != null)
                    {
                        SymbolTable.AddId(node.GetId(), node,
                            SymbolTable.IsInCurrentScope(node.GetId()) ? Symbol.notOk : Symbol.String);
                    }
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

    //Expressions
    public override void OutADivExp(ADivExp node)
    {
        if (SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) ||
            SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) &&
                SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {

            }
            else if (SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetL(), out AUnitdecl leftSide) &&
                     SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetR(), out AUnitdecl rightSide))
            {
                SymbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }

            SymbolTable.AddNode(node, Symbol.notOk);
        }
        else
        {
            var l = SymbolTable.GetSymbol(node.GetL());
            var r = SymbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal when r is Symbol.Decimal or Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;

                default:
                    SymbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
    }

    public override void OutAMultExp(AMultExp node)
    {
        if (SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) ||
            SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) &&
                SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {

            }
            else if (SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetL(), out AUnitdecl leftSide) &&
                     SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetR(), out AUnitdecl rightSide))
            {
                SymbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }

            SymbolTable.AddNode(node, Symbol.notOk);
        }
        else
        {
            var l = SymbolTable.GetSymbol(node.GetL());
            var r = SymbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal when r is Symbol.Decimal or Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                default:
                    SymbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
    }

    public override void OutAPlusExp(APlusExp node)
    {
        if (SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) || SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) && SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {
                
            }
            else if (SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetL(),out AUnitdecl leftSide) && SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetR(),out AUnitdecl rightSide))
            {
                SymbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }
            SymbolTable.AddNode(node,Symbol.notOk);  
        }
        else
        {
            var l = SymbolTable.GetSymbol(node.GetL());
            var r = SymbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal or Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Int or Symbol.Decimal when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.String when r is Symbol.Int or Symbol.String:
                    SymbolTable.AddNode(node, Symbol.String);
                    break;
                case Symbol.Int or Symbol.String when r is Symbol.String:
                    SymbolTable.AddNode(node, Symbol.String);
                    break;
                case Symbol.Decimal or Symbol.String when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.String);
                    break;
                case Symbol.String when r is Symbol.String or Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.String);
                    break;
                default:
                    SymbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        
        if (SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) || SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
        {
            if (!SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetL()) && SymbolTable._currentSymbolTable.nodeToUnit.ContainsKey(node.GetR()))
            {
                
            }
            else if (SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetL(),out AUnitdecl leftSide) && SymbolTable._currentSymbolTable.nodeToUnit.TryGetValue(node.GetR(),out AUnitdecl rightSide))
            {
                SymbolTable.AddNode(node, leftSide == rightSide ? Symbol.ok : Symbol.notOk);

                return;
            }
            SymbolTable.AddNode(node,Symbol.notOk);  
        }
        else
        {
            var l = SymbolTable.GetSymbol(node.GetL());
            var r = SymbolTable.GetSymbol(node.GetR());
            switch (l)
            {
                case Symbol.Int when r is Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Int);
                    break;
                case Symbol.Decimal when r is Symbol.Decimal or Symbol.Int:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                case Symbol.Decimal or Symbol.Int when r is Symbol.Decimal:
                    SymbolTable.AddNode(node, Symbol.Decimal);
                    break;
                default:
                    SymbolTable.AddNode(node, Symbol.notOk);
                    break;
            }
        }
    }
    public override void OutAFunccallExp(AFunccallExp node)
    {
        //Symbol? funcId = SymbolTable.GetSymbol(node.GetId());
    }
    
    public override void OutAAssignStmt(AAssignStmt node) {
        Symbol? type = SymbolTable.GetSymbol("" + node.GetId());
        Symbol? exprType = SymbolTable.GetSymbol(node.GetExp());

        // if types match stmt is ok, else notok
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