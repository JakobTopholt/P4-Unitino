using System.Collections;
using Microsoft.VisualBasic;

namespace Compiler.Visitors.TypeCheckerDir;
using Moduino.node;
using Moduino.analysis;

public class exprTypeChecker : stmtTypeChecker
{
    public exprTypeChecker(SymbolTable symbolTable) : base(symbolTable)
    {

    }
    
    public override void OutAExpExp(AExpExp node)
    {
        Node exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        Symbol? type = symbolTable.GetSymbol(exp);
        switch (type)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.Func:
                symbolTable.AddNode(node, Symbol.Func);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            case Symbol.NotOk:
                symbolTable.AddNode(node, Symbol.NotOk);
                break;
            default:
                symbolTable.GetUnit(exp, out var unit);
                symbolTable.AddNodeToUnit(node, unit);
                symbolTable.AddNode(node, Symbol.Ok);
                break;
        }
    }
    
    public override void OutADecimalExp(ADecimalExp node) => symbolTable.AddNode(node, Symbol.Decimal);
    public override void OutANumberExp(ANumberExp node) => symbolTable.AddNode(node, Symbol.Int);
    public override void OutABooleanExp(ABooleanExp node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutATrueBoolean(ATrueBoolean node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutAFalseBoolean(AFalseBoolean node) => symbolTable.AddNode(node, Symbol.Bool);
    public override void OutAStringExp(AStringExp node) => symbolTable.AddNode(node, Symbol.String);
    public override void OutACharExp(ACharExp node) => symbolTable.AddNode(node, Symbol.Char);

    public override void InAValueExp(AValueExp node)
    {
        locations.Push(IndentedString($"In value expression\n"));
        indent++;
    }

    public override void OutAValueExp(AValueExp node)
    {
        if (symbolTable.currentScope == 1)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("Value expression is not legal in globalscope");
        }
        else
        {
            symbolTable.AddNode(node, Symbol.Decimal); 
        }
        /*
        Node? parent = node.Parent();
            while (parent is not AUnitdeclGlobal)
            {
                parent = parent.Parent();
            }
            if (parent is AUnitdeclGlobal)
            { 
                symbolTable.AddNode(node, Symbol.Decimal); 
            }
            else
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("Value expressions have to be in a unitdeclaration\n");
            } 
        }*/
        PrintError();
        indent--;
        
    }

    public override void InAFunccallExp(AFunccallExp node)
    {
        locations.Push(IndentedString($"In funccall: {node.GetId()}()\n"));
        indent++;
    }

    public override void OutAFunccallExp(AFunccallExp node)
    {
        // Funccall som en del af et return udtryk
        // returnvalue har betydning for om det er et correct call

        /* if (symbolTable.currentScope == )
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("Funccall expression is not legal in globalscope");
        }
        else
        { */
        /* 
         * The issue is somthing with the symboltables current scope, and its attempt to get a nodeToSymbol or nodeToUnit call
         * on a functionDefinition which was declared in a larger scope
         * 
         */

            symbolTable.GetNodeFromId(node.GetId().ToString(), out Node xxx);
            List<PType>? args = symbolTable.GetFunctionArgs(xxx);
            List<PExp>? parameters = node.GetExp().OfType<PExp>().ToList();

            int argAmount = args.Count;
            int paramAmount = parameters.Count;
            if (argAmount != paramAmount)
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("Not same amount of Arguments and Parameters\n");
            }
            else
            {
                bool matches = true;
                for (int i = 0; i < args.Count; i++)
                {
                    Symbol? paramSymbol;
                    Node? returnUnit;
                    if (parameters[i] is AIdExp x)
                    {
                        paramSymbol = symbolTable.GetSymbol(x.GetId().ToString().Trim());
                        symbolTable.GetNodeFromId(x.GetId().ToString().Trim(), out returnUnit);
                    }
                    else
                    {
                        paramSymbol = symbolTable.GetSymbol(parameters[i]);
                        returnUnit = parameters[i];
                    }

                    switch (args[i])
                    {
                        case AIntType:
                            if (paramSymbol != Symbol.Int)
                            {
                                matches = false;
                                tempResult += IndentedString("Parameter should be of type Int\n");
                            }

                            break;
                        case ADecimalType:
                            if (paramSymbol != Symbol.Decimal)
                            {
                                matches = false;
                                tempResult += IndentedString("Parameter should be of type Decimal\n");
                            }

                            break;
                        case ABoolType:
                            if (paramSymbol != Symbol.Bool)
                            {
                                matches = false;
                                tempResult += IndentedString("Parameter should be of type Bool\n");
                            }

                            break;
                        case ACharType:
                            if (paramSymbol != Symbol.Char)
                            {
                                matches = false;
                                tempResult += IndentedString("Parameter should be of type Char\n");
                            }

                            break;
                        case AStringType:
                            if (paramSymbol != Symbol.String)
                            {
                                matches = false;
                                tempResult += IndentedString("Parameter should be of type String\n");
                            }

                            break;
                        case APinType:
                            if (paramSymbol != Symbol.Pin)
                            {
                                matches = false;
                                tempResult += IndentedString("Parameter should be of type Pin\n");
                            }

                            break;
                        case AUnitType argType:
                            if (symbolTable.GetUnit(argType, out var argUnit) &&
                                symbolTable.GetUnit(returnUnit, out var paramUnit) &&
                                !symbolTable.CompareUnitTypes(argUnit, paramUnit))
                            {
                                matches = false;
                                tempResult += IndentedString($"Parameter nr{i}: is not same unitType as Argument\n");
                                symbolTable.AddNodeToUnit(node, argUnit);
                            }

                            break;
                        default:
                            matches = false;
                            break;
                    }
                }

                if (matches)
                {
                    symbolTable.GetNodeFromId(node.GetId().ToString(), out Node result);
                    //symbolTable = symbolTable.ResetScope();
                    var symbol = symbolTable.GetSymbol(result);
                    switch (symbol)
                    {
                        case Symbol.Int:
                            symbolTable.AddNode(node, Symbol.Int);
                            break;
                        case Symbol.Decimal:
                            symbolTable.AddNode(node, Symbol.Decimal);
                            break;
                        case Symbol.Bool:
                            symbolTable.AddNode(node, Symbol.Bool);
                            break;
                        case Symbol.Char:
                            symbolTable.AddNode(node, Symbol.Char);
                            break;
                        case Symbol.String:
                            symbolTable.AddNode(node, Symbol.String);
                            break;
                        case Symbol.Pin:
                            symbolTable.AddNode(node, Symbol.Pin);
                            break;
                        default:
                            if (symbolTable.GetUnit(result, out var unit))
                            {
                                symbolTable.AddNodeToUnit(node, unit);
                                symbolTable.AddNode(node, Symbol.Ok);
                            }
                            else
                            {
                                symbolTable.AddNode(node, Symbol.NotOk);
                            }
                            break;
                    }
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    // Allready added error message
                }
            }
            PrintError();
        indent--;
    }

    public override void InAIdExp(AIdExp node)
    {
        locations.Push( IndentedString($"in IdExp: {node.GetId()}\n"));
        indent++;
    }

    public override void OutAIdExp(AIdExp node)
    {
        switch (symbolTable.GetSymbol(node.GetId().ToString().Trim()))
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                if (symbolTable.GetUnit(node.GetId().ToString().Trim(), out var unit))
                {
                    symbolTable.AddNodeToUnit(node, unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString($"{node.GetId()} is not a valid id (no value associated with it)\n");
                }
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAReadpinExp(AReadpinExp node)
    {
        locations.Push( IndentedString($"in readpin expression: {node}\n"));
        indent++;
    }

    public override void OutAReadpinExp(AReadpinExp node)
    {
        if (symbolTable.currentScope == 0)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("A readpin expression is not legal in globalscope");
        }
        else
        {
            Symbol? readpinType = symbolTable.GetSymbol(node.GetExp());
            if (node.GetExp() is AIdExp id)
            {
                readpinType = symbolTable.GetSymbol(id.GetId());
            }
            if (readpinType is Symbol.Pin or Symbol.Int)
            {
                symbolTable.AddNode(node, Symbol.Pin);
            }
            else
            {
                symbolTable.AddNode(node, Symbol.NotOk);
                if (readpinType == null)
                {
                    tempResult += IndentedString("The id does not exist in this scope\n");
                }
                else
                {
                    tempResult += IndentedString("Readpin expression is not Pin or integer type");          
                }
            }
        }
        PrintError();
        indent--;
    }

    public override void InAUnitdecimalExp(AUnitdecimalExp node)
    {
        locations.Push(IndentedString($"In unitdecimal: {node.GetDecimal().ToString() + node.GetId()}\n"));
        indent++;
    }

    public override void OutAUnitdecimalExp(AUnitdecimalExp node)
    {
        // A single unitnumber eg. 50ms
        AUnitdeclGlobal unitType = symbolTable.GetUnitdeclFromId(node.GetId().ToString().Trim());
        if (unitType != null)
        {
            // Create a new unit tuple and add the unitnumber as a lone numerator
            SortedList<string, AUnitdeclGlobal> nums = new SortedList<string, AUnitdeclGlobal>();
            nums.Add(unitType.GetId().ToString().Trim(), unitType);
            SortedList<string, AUnitdeclGlobal> dens = new SortedList<string, AUnitdeclGlobal>();
            var unit = (nums, dens);
            symbolTable.AddNodeToUnit(node, unit);
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else
        {
            // Id is not a valid subunit
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString($"{node.GetId()} does not exist\n");
        }
        PrintError();
        indent--;
    }

    public override void InAUnitnumberExp(AUnitnumberExp node)
    {
        locations.Push( IndentedString($"In unitnumber: {node.GetNumber().ToString() + node.GetId()}\n"));
        indent++;
    }

    public override void OutAUnitnumberExp(AUnitnumberExp node)
    {
        // A single unitnumber eg. 50ms
        AUnitdeclGlobal unitType = symbolTable.GetUnitdeclFromId(node.GetId().ToString().Trim());
        if (unitType != null)
        {
            // Create a new unit tuple and add the unitnumber as a lone numerator
            SortedList<string, AUnitdeclGlobal> nums = new SortedList<string, AUnitdeclGlobal>();
            nums.Add(unitType.GetId().ToString().Trim(), unitType);
            SortedList<string, AUnitdeclGlobal> dens = new SortedList<string, AUnitdeclGlobal>();
            var unit = (nums, dens);

            // Map node to the unit
            symbolTable.AddNodeToUnit(node, unit);
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else
        {
            // Id is not a valid subunit
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString($"{node.GetId()} does not exist\n");
        }
        PrintError();
        indent--;
    }

    public override void InADivideExp(ADivideExp node)
    {
        locations.Push(IndentedString($"In divideExpression: {node.GetL() + "/" + node.GetR()}\n"));
        indent++;
    }

    public override void OutADivideExp(ADivideExp node)
    {
        Node leftExpr = node.GetL();
        if (leftExpr is AIdExp idLeft)
        {
            symbolTable.GetNodeFromId(idLeft.GetId().ToString().Trim(), out leftExpr);
        }
        Node rightExpr = node.GetR();
        if (rightExpr is AIdExp idRight)
        {
            symbolTable.GetNodeFromId(idRight.GetId().ToString().Trim(), out rightExpr);
        }
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        /* if (leftExpr is ANumberExp or ADecimalExp && rightExpr is AIdExp id)
        {
            AUnitdeclGlobal? unitDecl = symbolTable.GetUnitdeclFromId(id.GetId().ToString().Trim());
            if (unitDecl is null || !symbolTable.GetUnit(unitDecl, out var tuple)) 
                return;
            symbolTable.AddNodeToUnit(node, tuple);
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else */
        {
          switch (leftSymbol)
            {
            case Symbol.Int or Symbol.Pin when rightSymbol is Symbol.Int or Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(rightExpr);
                if (symbolTable.GetUnit(leftExpr, out (SortedList<string, AUnitdeclGlobal> num, SortedList<string, AUnitdeclGlobal> den) left) 
                      && symbolTable.GetUnit(rightExpr, out (SortedList<string, AUnitdeclGlobal> num, SortedList<string, AUnitdeclGlobal> den) right))
                {
                    SortedList<string, AUnitdeclGlobal> unitLeftNums = left.num;
                    SortedList<string, AUnitdeclGlobal> unitLeftDens = left.den;
                    SortedList<string, AUnitdeclGlobal> unitRightNums = right.num;
                    SortedList<string, AUnitdeclGlobal> unitRightDens = right.den;
// 0
                    List<AUnitdeclGlobal> numOverlap = unitLeftNums.Values.Intersect(unitRightNums.Values).ToList();
                    List<AUnitdeclGlobal> denOverlap = unitLeftDens.Values.Intersect(unitRightDens.Values).ToList();

                    List<AUnitdeclGlobal> numerators = unitLeftNums.Values.Except(numOverlap).Union(unitRightDens.Values.Except(denOverlap)).ToList();
                    List<AUnitdeclGlobal> denominators = unitRightNums.Values.Except(numOverlap).Union(unitLeftDens.Values.Except(denOverlap)).ToList();
                    if (numerators.Count == 0 && denominators.Count == 0)
                    {
                        symbolTable.AddNode(node, Symbol.Decimal);
                    }
                    else
                    {
                        SortedList<string, AUnitdeclGlobal> newNums = new SortedList<string, AUnitdeclGlobal>();
                        SortedList<string, AUnitdeclGlobal> newDens = new SortedList<string, AUnitdeclGlobal>();
                        
                        foreach (AUnitdeclGlobal num in numerators)
                        {
                            newNums.Add(num.GetId().ToString().Trim(),num);
                        }
                        foreach (AUnitdeclGlobal den in denominators)
                        {
                            newDens.Add(den.GetId().ToString().Trim(),den);;
                        }
                        (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unituse = (newNums, newDens);
                            symbolTable.AddNodeToUnit(node, unituse);
                            symbolTable.AddNode(node, Symbol.Ok);
                    }
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unit;
                    if (leftContainsUnit ? symbolTable.GetUnit(leftExpr, out unit) : rightContainsUnit && symbolTable.GetUnit(rightExpr, out unit))
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("No valid units in expression\n");
                    }
                }
                else
                {
                    // not valid input expressions to a divide expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Not a valid divide expression! only use allowed types\n");
                }
                break;
            }  
        }
        PrintError();
        indent--;
    }

    public override void InAMultiplyExp(AMultiplyExp node)
    {
        locations.Push(IndentedString($"In multiplyExpression: {node.GetL() + "*" + node.GetR()}\n"));
        indent++;
    }

    public override void OutAMultiplyExp(AMultiplyExp node)
    {
        Node leftExpr = node.GetL();
        if (leftExpr is AIdExp idLeft)
        {
            symbolTable.GetNodeFromId(idLeft.GetId().ToString().Trim(), out leftExpr);
        }
        Node rightExpr = node.GetR();
        if (rightExpr is AIdExp idRight)
        {
            symbolTable.GetNodeFromId(idRight.GetId().ToString().Trim(), out rightExpr);
        }
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        switch (leftSymbol)
        {
            case Symbol.Int or Symbol.Pin when rightSymbol is Symbol.Int or Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(rightExpr);
                if (symbolTable.NodeToUnit.ContainsKey(leftExpr) && symbolTable.NodeToUnit.ContainsKey(rightExpr))
                {
                    if (!(symbolTable.GetUnit(leftExpr, out (SortedList<string, AUnitdeclGlobal> num, SortedList<string, AUnitdeclGlobal> den) left) 
                          && symbolTable.GetUnit(rightExpr, out (SortedList<string, AUnitdeclGlobal> num, SortedList<string, AUnitdeclGlobal> den) right)))
                        return;

                    SortedList<string, AUnitdeclGlobal> leftNums = left.num;
                    SortedList<string, AUnitdeclGlobal> leftDens = left.den;
                    SortedList<string, AUnitdeclGlobal> rightNums = right.num;
                    SortedList<string, AUnitdeclGlobal> rightDens = right.den;
// (Volume / weight) * weight = Volume
                    List<AUnitdeclGlobal> numOverlap = leftNums.Values.Intersect(rightDens.Values).ToList();
                    List<AUnitdeclGlobal> denOverlap = leftDens.Values.Intersect(rightNums.Values).ToList();

                    // fix logic
                    List<AUnitdeclGlobal> numerators = leftNums.Values.Except(numOverlap).Union(rightNums.Values.Except(denOverlap)).ToList();
                    List<AUnitdeclGlobal> denomerators = rightDens.Values.Except(numOverlap).Union(leftDens.Values.Except(denOverlap)).ToList();
                    
                    if (numerators.Count == 0 && denomerators.Count == 0)
                    {
                        symbolTable.AddNode(node, Symbol.Decimal);
                    }
                    else
                    {
                        SortedList<string, AUnitdeclGlobal> newNums = new SortedList<string, AUnitdeclGlobal>();
                        SortedList<string, AUnitdeclGlobal> newDens = new SortedList<string, AUnitdeclGlobal>();
                        foreach (AUnitdeclGlobal num in numerators)
                        {
                            newNums.Add(num.GetId().ToString().Trim(),num);
                        }
                        foreach (AUnitdeclGlobal den in denomerators)
                        {
                            newDens.Add(den.GetId().ToString().Trim(),den);;
                        }

                        (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unituse = (newNums, newDens);
                        symbolTable.AddNodeToUnit(node, unituse);
                        symbolTable.AddNode(node, Symbol.Ok);
                        
                    }
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    (SortedList<string, AUnitdeclGlobal>, SortedList<string, AUnitdeclGlobal>) unit;
                    if (leftContainsUnit ? symbolTable.GetUnit(leftExpr, out unit) : rightContainsUnit && symbolTable.GetUnit(rightExpr, out unit))
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("No valid units in expression\n");
                    }
                        
                }
                else
                {
                    // not valid input expressions to a multiply expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Not a valid multiply expression! only use allowed types\n");
                }
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAPlusExp(APlusExp node)
    {
        locations.Push(IndentedString($"In plusExpression: {node.GetL() + "+" + node.GetR()}\n"));
        indent++;
    }

    public override void OutAPlusExp(APlusExp node)
    {
        Node leftExpr = node.GetL();
        if (leftExpr is AIdExp idLeft)
        {
            symbolTable.GetNodeFromId(idLeft.GetId().ToString().Trim(), out leftExpr);
        }
        Node rightExpr = node.GetR();
        if (rightExpr is AIdExp idRight)
        {
            symbolTable.GetNodeFromId(idRight.GetId().ToString().Trim(), out rightExpr);
        }
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        switch (leftSymbol)
        {
            case Symbol.Int or Symbol.Pin when rightSymbol is Symbol.Int or Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Int or Symbol.Decimal when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.String when rightSymbol is Symbol.Int or Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.Int or Symbol.String when rightSymbol is Symbol.String:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.Decimal or Symbol.String when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.String);
                break;
            case Symbol.String when rightSymbol is Symbol.String or Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.String);
                break;
            default:
                // UnitTypes?
                if (symbolTable.GetUnit(leftExpr, out var unit1) && symbolTable.GetUnit(rightExpr, out var unit2))
                {
                    if (symbolTable.CompareUnitTypes(unit1, unit2))
                    {
                        symbolTable.AddNodeToUnit(node, unit1);
                        symbolTable.AddNode(node, Symbol.Ok);
                    } 
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Not the same unitTypes used in expression\n");
                    }
                }
                else
                {
                    // not valid input expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Not valid Types used together in expression!\n");
                }
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAMinusExp(AMinusExp node)
    {
        locations.Push(IndentedString($"In minusExpression: {node.GetL() + "-" + node.GetR()}\n"));
        indent++;
    }

    public override void OutAMinusExp(AMinusExp node)
    {
        Node leftExpr = node.GetL();
        if (leftExpr is AIdExp idLeft)
        {
            symbolTable.GetNodeFromId(idLeft.GetId().ToString().Trim(), out leftExpr);
        }
        Node rightExpr = node.GetR();
        if (rightExpr is AIdExp idRight)
        {
            symbolTable.GetNodeFromId(idRight.GetId().ToString().Trim(), out rightExpr);
        }
        Symbol? leftSymbol = symbolTable.GetSymbol(leftExpr);
        Symbol? rightSymbol = symbolTable.GetSymbol(rightExpr);
        switch (leftSymbol)
        {
            case Symbol.Int or Symbol.Pin when rightSymbol is Symbol.Int or Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal when rightSymbol is Symbol.Decimal or Symbol.Int:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Decimal or Symbol.Int when rightSymbol is Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            default:
                // UnitTypes?
                if (symbolTable.GetUnit(leftExpr, out var unit1) && symbolTable.GetUnit(rightExpr, out var unit2))
                {
                    if (symbolTable.CompareUnitTypes(unit1, unit2))
                    {
                        symbolTable.AddNodeToUnit(node, unit1);
                        symbolTable.AddNode(node, Symbol.Ok);
                    } 
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Not the same unitTypes used in expression\n");
                    }
                }
                else
                {
                    // not valid input expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Not valid Types used together in expression!\n");
                }
                break;
        }
        PrintError();
        indent--;
    }

    public override void InARemainderExp(ARemainderExp node)
    {
        locations.Push(IndentedString($"In remainderExpression: {node.GetL() + "%" + node.GetR()}\n"));
        indent++;
    }

    public override void OutARemainderExp(ARemainderExp node)
    {
        Node leftExpr = node.GetL();
        if (leftExpr is AIdExp idLeft)
        {
            symbolTable.GetNodeFromId(idLeft.GetId().ToString().Trim(), out leftExpr);
        }
        Node rightExpr = node.GetR();
        if (rightExpr is AIdExp idRight)
        {
            symbolTable.GetNodeFromId(idRight.GetId().ToString().Trim(), out rightExpr);
        }
        Symbol? left = symbolTable.GetSymbol(leftExpr);
        Symbol? right = symbolTable.GetSymbol(rightExpr);
        switch (left)
        {
            case Symbol.Int or Symbol.Pin:
                if (right is Symbol.Int or Symbol.Pin)
                {
                    symbolTable.AddNode(node, Symbol.Int);
                } else if (right == Symbol.Decimal)
                {
                    symbolTable.AddNode(node, Symbol.Decimal);
                }
                else
                { 
                    symbolTable.AddNode(node, Symbol.NotOk);  
                    tempResult += IndentedString("Only int and decimals are allowed in remainder expression\n");
                }
                break;
            case Symbol.Decimal:
                if (right == Symbol.Int || right == Symbol.Decimal)
                {
                    symbolTable.AddNode(node, Symbol.Decimal);
                }
                else
                { 
                    symbolTable.AddNode(node, Symbol.NotOk);  
                    tempResult += IndentedString("Only decimals are allowed in remainder expression\n");
                }
                break;
            default:
                if (symbolTable.GetUnit(leftExpr, out var unit1) && symbolTable.GetUnit(rightExpr, out var unit2))
                {
                    if (symbolTable.CompareUnitTypes(unit1, unit2))
                    {
                        symbolTable.AddNodeToUnit(node, unit1);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Not same unitTypes\n");
                    } 
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Remainder expressions is only allowed with ints, decimals and units\n");
                }
                break;
        }
        PrintError();
        indent--;
    }

    public override void InATernaryExp(ATernaryExp node)
    {
        locations.Push(IndentedString($"In ternaryExpression: {node.GetCond() + " ? " + node.GetTrue() + " : " + node.GetFalse()}\n"));
        indent++;
    }

    public override void OutATernaryExp(ATernaryExp node)
    {
        Node condExp = node.GetCond();
        if (condExp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out condExp);
        }
        Symbol? condition = symbolTable.GetSymbol(condExp);
        if (condition == Symbol.Bool)
        {
            Node trueExpr = node.GetTrue();
            if (trueExpr is AIdExp idTrue)
            {
                symbolTable.GetNodeFromId(idTrue.GetId().ToString().Trim(), out trueExpr);
            }
            Node falseExpr = node.GetFalse();
            if (falseExpr is AIdExp idFalse)
            {
                symbolTable.GetNodeFromId(idFalse.GetId().ToString().Trim(), out falseExpr);
            }
            var trueSymbol = symbolTable.GetSymbol(trueExpr);
            var falseSymbol = symbolTable.GetSymbol(falseExpr);
            switch (trueSymbol)
            {
                case Symbol.Int:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Int ? Symbol.Int : Symbol.NotOk);
                    tempResult += falseSymbol == Symbol.Int ? "" : IndentedString("True and false exp should be of same type\n");
                    break;
                case Symbol.Decimal:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                    tempResult += falseSymbol == Symbol.Decimal ? "" : IndentedString("True and false exp should be of same type\n");
                    break;
                case Symbol.Bool:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                    tempResult += falseSymbol == Symbol.Bool ? "" : IndentedString("True and false exp should be of same type\n");
                    break;
                case Symbol.Char:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                    tempResult += falseSymbol == Symbol.Char ? "" : IndentedString("True and false exp should be of same type\n");
                    break;
                case Symbol.String:
                    symbolTable.AddNode(node, falseSymbol == Symbol.String ? Symbol.String : Symbol.NotOk);
                    tempResult += falseSymbol == Symbol.String ? "" : IndentedString("True and false exp should be of same type\n");
                    break;
                case Symbol.Pin:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Pin ? Symbol.Pin : Symbol.NotOk);
                    tempResult += falseSymbol == Symbol.Pin ? "" : IndentedString("True and false exp should be of same type\n");
                    break;
                default:
                    // compare custom unit
                    bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(trueExpr);
                    bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(falseExpr);
                    if (leftContainsUnit && rightContainsUnit)
                    {
                        // Tjek at typerne er ens 
                        symbolTable.GetUnit(trueExpr, out var trueUnit); // unit 1
                        symbolTable.GetUnit(falseExpr, out var falseUnit); // unit 2
                        if (symbolTable.CompareUnitTypes(trueUnit, falseUnit))
                        {
                            symbolTable.AddNodeToUnit(node, trueUnit);
                            symbolTable.AddNode(node, Symbol.Ok);
                        }
                        else
                        {
                            symbolTable.AddNode(node, Symbol.NotOk);
                            tempResult += IndentedString("UnitTypes does not match\n");

                        }
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Illegal type used in trueExpression\n");
                    }
                    break;
            }
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("Condition expression is not a boolean\n");
        }
        PrintError();
        indent--;
    }

    public override void InALogicalnotExp(ALogicalnotExp node)
    {
        locations.Push(IndentedString($"In notExpression: {node.GetExp()}\n"));
        indent++;
    }

    public override void OutALogicalnotExp(ALogicalnotExp node)
    {
        Node exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        if (exp == null)
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("Expression is empty\n");
        }
        else if (exp != null)
        {
            Symbol? symbol = symbolTable.GetSymbol(exp);
            symbolTable.AddNode(node, symbol == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
            tempResult += symbol == Symbol.Bool ? "" : IndentedString("Expression should be a boolean\n");
        }
        PrintError();
        indent--;
    }

    public override void InACastExp(ACastExp node)
    {
        locations.Push(IndentedString($"In castExpression: {"(" + node.GetType() + ")" +" " + node.GetExp()}\n"));
        indent++;
    }

    public override void OutACastExp(ACastExp node)
    {
        Symbol? targetExpr = symbolTable.GetSymbol(node.GetType());
        Node exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (targetExpr, expr)
        {
            //typecasting to the same lmao
            case (Symbol.Bool, Symbol.Bool):
                symbolTable.AddNode(node, Symbol.Bool);
                break;
            case (Symbol.Int, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case (Symbol.Decimal, Symbol.Decimal):
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case (Symbol.String, Symbol.String):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.Char, Symbol.Char):
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case (Symbol.Pin, Symbol.Pin):
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            
            /*-- casting to new types --*/
            case (Symbol.Pin, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            case (Symbol.Int, Symbol.Pin):
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case (Symbol.Decimal, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case (Symbol.Int, Symbol.Decimal):
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case (Symbol.String, Symbol.Char):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.String, Symbol.Int):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.String, Symbol.Decimal):
                symbolTable.AddNode(node, Symbol.String);
                break;
            case (Symbol.Char, Symbol.Int):
                symbolTable.AddNode(node, Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("Illegal cast types\n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAOrExp(AOrExp node)
    {
        locations.Push(IndentedString($"In orExpression: {node.GetL() + "||" + node.GetR()}\n"));
        indent++;
    }

    public override void OutAOrExp(AOrExp node)
    {
        Node leftExpr = node.GetL();
        if (leftExpr is AIdExp idLeft)
        {
            symbolTable.GetNodeFromId(idLeft.GetId().ToString().Trim(), out leftExpr);
        }
        Node rightExpr = node.GetR();
        if (rightExpr is AIdExp idRight)
        {
            symbolTable.GetNodeFromId(idRight.GetId().ToString().Trim(), out rightExpr);
        }
        Symbol? leftside = symbolTable.GetSymbol(leftExpr);
        Symbol? rightside = symbolTable.GetSymbol(rightExpr);
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            symbolTable.AddNode(node, Symbol.Bool);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("Both expressions should be booleans\n");
        }
        PrintError();
        indent--;
    }

    public override void InAAndExp(AAndExp node)
    {
        locations.Push(IndentedString($"In andExpression: {node.GetL() + "&&" + node.GetR()}\n"));
        indent++;
    }

    public override void OutAAndExp(AAndExp node)
    {
        Node leftExpr = node.GetL();
        if (leftExpr is AIdExp idLeft)
        {
            symbolTable.GetNodeFromId(idLeft.GetId().ToString().Trim(), out leftExpr);
        }
        Node rightExpr = node.GetR();
        if (rightExpr is AIdExp idRight)
        {
            symbolTable.GetNodeFromId(idRight.GetId().ToString().Trim(), out rightExpr);
        }
        Symbol? leftside = symbolTable.GetSymbol(leftExpr);
        Symbol? rightside = symbolTable.GetSymbol(rightExpr);
        if (leftside == Symbol.Bool && rightside == Symbol.Bool)
        {
            symbolTable.AddNode(node, Symbol.Bool);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
            tempResult += IndentedString("Both expressions should be booleans\n");
        }
        PrintError();
        indent--;
    }

    public override void InASuffixplusplusExp(ASuffixplusplusExp node)
    {
        locations.Push(IndentedString($"In suffixplusplus Expression: {node.GetExp()}\n"));
        indent++;
    }

    public override void OutASuffixplusplusExp(ASuffixplusplusExp node)
    {
        Node exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("can only be int, decimal or char types\n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InASuffixminusminusExp(ASuffixminusminusExp node)
    {
        locations.Push(IndentedString($"In suffixminusminus Expression: {node.GetExp()}\n"));
        indent++;
    }

    public override void OutASuffixminusminusExp(ASuffixminusminusExp node)
    {
        Node exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("can only be int, decimal or char types\n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAUnaryminusExp(AUnaryminusExp node)
    {
        locations.Push(IndentedString($"In unaryminus Expression: {node.GetExp()}\n"));
        indent++;
    }

    public override void OutAUnaryminusExp(AUnaryminusExp node)
    {
        Node exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("can only be int, decimal or char types\n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAPrefixminusminusExp(APrefixminusminusExp node)
    {
        locations.Push(IndentedString($"In prefixminusminus Expression: {node.GetExp()}\n"));
        indent++;
    }

    public override void OutAPrefixminusminusExp(APrefixminusminusExp node)
    {
        Node exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("can only be int, decimal or char types\n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAPrefixplusplusExp(APrefixplusplusExp node)
    {
        locations.Push(IndentedString($"In prefixplusplus Expression: {node.GetExp()}\n"));
        indent++;
    }

    public override void OutAPrefixplusplusExp(APrefixplusplusExp node)
    {
        Node exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        Symbol? expr = symbolTable.GetSymbol(exp);
        switch (expr)
        {
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(node, Symbol.Pin);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                tempResult += IndentedString("can only be int, decimal or char types\n");
                break;
        }
        PrintError();
        indent--;
    }

    public override void InAEqualExp(AEqualExp node)
    {
        locations.Push(IndentedString($"In an equal Expression: {node.GetL() + "==" + node.GetR()}\n"));
        indent++;
    }
    public override void OutAEqualExp(AEqualExp node) => AddBinaryToSymbolTable(node, node.GetL(), node.GetR());
    public override void InANotequalExp(ANotequalExp node)
    {
        locations.Push(IndentedString($"In an notEqual Expression: {node.GetL() + "!=" + node.GetR()}\n"));
        indent++;
    }
    public override void OutANotequalExp(ANotequalExp node) => AddBinaryToSymbolTable(node, node.GetL(), node.GetR());
    public override void InAGreaterExp(AGreaterExp node)
    {
        locations.Push(IndentedString($"In a greater Expression: {node.GetL() + ">" + node.GetR()}\n"));
        indent++;
    }
    public override void OutAGreaterExp(AGreaterExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void InALessExp(ALessExp node)
    {
        locations.Push(IndentedString($"In a less Expression: {node.GetL() + "<" + node.GetR()}\n"));
        indent++;
    }
    public override void OutALessExp(ALessExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void InAGreaterequalExp(AGreaterequalExp node)
    {
        locations.Push(IndentedString($"In a greaterEqual Expression: {node.GetL() + ">=" + node.GetR()}\n"));
        indent++;
    }
    public override void OutAGreaterequalExp(AGreaterequalExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void InALessequalExp(ALessequalExp node)
    {
        locations.Push(IndentedString($"In a lessEqual Expression: {node.GetL() + "<=" + node.GetR()}\n"));
        indent++;
    }
    public override void OutALessequalExp(ALessequalExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    // Equals, Notequal
    private void AddBinaryToSymbolTable(Node Parent, Node L, Node R)
    {
        Node leftExpr = L;
        if (leftExpr is AIdExp idLeft)
        {
            symbolTable.GetNodeFromId(idLeft.GetId().ToString().Trim(), out leftExpr);
        }
        Node rightExpr = R;
        if (rightExpr is AIdExp idRight)
        {
            symbolTable.GetNodeFromId(idRight.GetId().ToString().Trim(), out rightExpr);
        }
        Symbol? left = symbolTable.GetSymbol(leftExpr);
        switch (left)
        {
            case Symbol.Int:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Int ? Symbol.Bool : Symbol.NotOk);
                tempResult += symbolTable.GetSymbol(rightExpr) == Symbol.Int ? "" : IndentedString("Types does not match\n");
                break;
            case (Symbol.Decimal):
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Decimal ? Symbol.Bool : Symbol.NotOk);
                tempResult += symbolTable.GetSymbol(rightExpr) == Symbol.Decimal ? "" : IndentedString("Types does not match\n");
                break;
            case Symbol.Bool:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                tempResult += symbolTable.GetSymbol(rightExpr) == Symbol.Bool ? "" : IndentedString("Types does not match\n");
                break;
            case Symbol.String:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.String ? Symbol.Bool : Symbol.NotOk);
                tempResult += symbolTable.GetSymbol(rightExpr) == Symbol.String ? "" : IndentedString("Types does not match\n");
                break;
            case Symbol.Char:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Char ? Symbol.Bool : Symbol.NotOk);
                tempResult += symbolTable.GetSymbol(rightExpr) == Symbol.Char ? "" : IndentedString("Types does not match\n");
                break;
            case Symbol.Pin:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Pin ? Symbol.Bool : Symbol.NotOk);
                tempResult += symbolTable.GetSymbol(rightExpr) == Symbol.Pin ? "" : IndentedString("Types does not match\n");
                break;
            default:
                bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(rightExpr);
                if (leftContainsUnit && rightContainsUnit)
                {
                    symbolTable.GetUnit(leftExpr, out var unit1);
                    symbolTable.GetUnit(rightExpr, out var unit2);
                    symbolTable.AddNode(Parent, symbolTable.CompareUnitTypes(unit1, unit2) ? Symbol.Bool : Symbol.NotOk);
                    tempResult += symbolTable.CompareUnitTypes(unit1, unit2) ? "" : IndentedString("unitTypes does not match\n");
                }
                else
                {
                    symbolTable.AddNode(Parent, Symbol.NotOk);
                    tempResult += IndentedString("types does not match\n");
                }
                break;
        }
        PrintError();
        indent--;
    }
    // Binary without string, bool
    // Greater, GreaterEqual, Less, LessEqual
    private void AddBinaryNumberToSymbolTable(Node Parent, Node L, Node R)
    {
        Node leftExpr = L;
        if (leftExpr is AIdExp idLeft)
        {
            symbolTable.GetNodeFromId(idLeft.GetId().ToString().Trim(), out leftExpr);
        }
        Node rightExpr = R;
        if (rightExpr is AIdExp idRight)
        {
            symbolTable.GetNodeFromId(idRight.GetId().ToString().Trim(), out rightExpr);
        }
        Symbol? left = symbolTable.GetSymbol(leftExpr);
        switch (left)
        {
            case Symbol.Int:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) is Symbol.Int or Symbol.Decimal or Symbol.Char or Symbol.Pin ? Symbol.Bool : Symbol.NotOk);
                break;
            case (Symbol.Decimal):
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) is Symbol.Int or Symbol.Decimal ? Symbol.Bool : Symbol.NotOk);
                break;
            case Symbol.Char:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) is Symbol.Int or Symbol.Char ? Symbol.Bool : Symbol.NotOk);
                break;
            case Symbol.Pin:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) is Symbol.Int or Symbol.Pin ? Symbol.Bool : Symbol.NotOk);
                break;
            default:
                bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(rightExpr);
                if (leftContainsUnit && rightContainsUnit)
                {
                    symbolTable.GetUnit(leftExpr, out var unit1);
                    symbolTable.GetUnit(rightExpr, out var unit2);
                    symbolTable.AddNode(Parent, symbolTable.CompareUnitTypes(unit1, unit2) ? Symbol.Bool : Symbol.NotOk);
                    tempResult += symbolTable.CompareUnitTypes(unit1, unit2) ? "" : IndentedString("unitTypes does not match\n");
                }
                else
                {
                    symbolTable.AddNode(Parent, Symbol.NotOk);
                    tempResult += IndentedString("types does not match\n");
                }
                break;
        }
        PrintError();
        indent--;
    }
}

