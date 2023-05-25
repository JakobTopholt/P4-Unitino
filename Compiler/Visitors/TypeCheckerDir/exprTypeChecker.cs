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
        Node? exp = node.GetExp();
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
            case Symbol.Ok:
                symbolTable.AddNode(node, Symbol.Ok);
                break;
            default:
                symbolTable.AddNode(node, symbolTable.GetUnit(node.GetExp(), out _) ? Symbol.Ok : Symbol.NotOk);
                // Missing logic, make sure to also save the Customunit to the new node if there is one
                
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

    public override void OutAValueExp(AValueExp node)
    { 
        symbolTable.AddNode(node, Symbol.Decimal);
        // ----- Logic missing here---- (tag stilling til hvad det under betød)
       // symbolTable.AddNode(node, UnitVisitor.StateUnit ? Symbol.Decimal : Symbol.NotOk);
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
                    returnUnit  = parameters[i];
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
                            tempResult += IndentedString("Parameter is not same unitType as Argument\n");
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
                        if (symbolTable.GetUnit(node.GetId().ToString(), out var unit))
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
                    symbolTable.AddNodeToUnit(node, ((List<AUnitdeclGlobal>, List<AUnitdeclGlobal>))unit);
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
            List<AUnitdeclGlobal> nums = new List<AUnitdeclGlobal>();
            nums.Add(unitType);
            List<AUnitdeclGlobal> dens = new List<AUnitdeclGlobal>();
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
            List<AUnitdeclGlobal> nums = new List<AUnitdeclGlobal>();
            nums.Add(unitType);
            List<AUnitdeclGlobal> dens = new List<AUnitdeclGlobal>();
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
        if (leftExpr is ANumberExp or ADecimalExp && rightExpr is AIdExp id)
        {
            AUnitdeclGlobal? unitDecl = symbolTable.GetUnitdeclFromId(id.GetId().ToString().Trim());
            if (unitDecl is null || !symbolTable.GetUnit(unitDecl, out var tuple)) 
                return;
            symbolTable.AddNodeToUnit(node, tuple);
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else
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
                if (symbolTable.NodeToUnit.ContainsKey(leftExpr) && symbolTable.NodeToUnit.ContainsKey(rightExpr))
                {
                    if (!(symbolTable.GetUnit(leftExpr, out (List<AUnitdeclGlobal> num, List<AUnitdeclGlobal> den) left) 
                          && symbolTable.GetUnit(rightExpr, out (List<AUnitdeclGlobal> num, List<AUnitdeclGlobal> den) right)))
                        return;

                    List<AUnitdeclGlobal> unitLeftNums = left.num;
                    List<AUnitdeclGlobal> unitLeftDens = left.den;
                    List<AUnitdeclGlobal> unitRightNums = right.num;
                    List<AUnitdeclGlobal> unitRightDens = right.den;

                    List<AUnitdeclGlobal> numOverlap = unitLeftNums.Intersect(unitRightNums).ToList();
                    List<AUnitdeclGlobal> denOverlap = unitLeftDens.Intersect(unitRightDens).ToList();

                    List<AUnitdeclGlobal> numerators = unitLeftNums.Except(numOverlap).Union(unitRightDens.Except(denOverlap)).ToList();
                    List<AUnitdeclGlobal> denomerators = unitRightNums.Except(numOverlap).Union(unitLeftDens.Except(denOverlap)).ToList();

                    (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unituse = (numerators, denomerators);
                    symbolTable.AddNodeToUnit(node, unituse);
                    symbolTable.AddNode(node, Symbol.Ok);
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unit;
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
                    if (!(symbolTable.GetUnit(leftExpr, out (List<AUnitdeclGlobal> num, List<AUnitdeclGlobal> den) left) 
                          && symbolTable.GetUnit(rightExpr, out (List<AUnitdeclGlobal> num, List<AUnitdeclGlobal> den) right)))
                        return;

                    List<AUnitdeclGlobal> unitLeftNums = left.num;
                    List<AUnitdeclGlobal> unitLeftDens = left.den;
                    List<AUnitdeclGlobal> unitRightNums = right.num;
                    List<AUnitdeclGlobal> unitRightDens = right.den;

                    List<AUnitdeclGlobal> leftNumRightDen = unitLeftNums.Intersect(unitRightDens).ToList();
                    List<AUnitdeclGlobal> leftDenRightNums = unitLeftDens.Intersect(unitRightNums).ToList();

                    List<AUnitdeclGlobal> numerators = unitLeftNums.Except(leftNumRightDen).Union(unitRightDens.Except(leftDenRightNums)).ToList();
                    List<AUnitdeclGlobal> denomerators = unitRightNums.Except(leftNumRightDen).Union(unitLeftDens.Except(leftDenRightNums)).ToList();

                    (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unituse = (numerators, denomerators);
                    symbolTable.AddNodeToUnit(node, unituse);
                    symbolTable.AddNode(node, Symbol.Ok);
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unit;
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
                bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(rightExpr);
                if (leftContainsUnit && rightContainsUnit)
                {
                    // har burgt noget nullable warning?
                    if (!(symbolTable.GetUnit(leftExpr, out (List<AUnitdeclGlobal> num, List<AUnitdeclGlobal> den) left) 
                          && symbolTable.GetUnit(rightExpr, out (List<AUnitdeclGlobal> num, List<AUnitdeclGlobal> den) right)))
                        return;

                    List<AUnitdeclGlobal> a = left.num;
                    List<AUnitdeclGlobal> b = left.den;
                    List<AUnitdeclGlobal> c = right.num;
                    List<AUnitdeclGlobal> d = right.den;

                    var sortedNums1 = a.OrderBy(x => x).ToList();
                    bool isEmptyNums1 = sortedNums1.Count == 0;
                    var sortedNums2 = c.OrderBy(x => x).ToList();
                    bool isEmptyNums2 = sortedNums2.Count == 0;
                    var sortedDens1 = b.OrderBy(x => x).ToList();
                    bool isEmptyDens1 = sortedDens1.Count == 0;
                    var sortedDens2 = d.OrderBy(x => x).ToList();
                    bool isEmptyDens2 = sortedDens2.Count == 0;

                    bool dontCompareNums = isEmptyNums1 || isEmptyNums2;
                    bool dontCompareDens = isEmptyDens1 || isEmptyDens2;

                    if ((dontCompareNums || sortedNums1.SequenceEqual(sortedNums2)) &&
                        (dontCompareDens || sortedDens1.SequenceEqual(sortedDens2)))
                    {
                        // Create a new unitTyple and add it to NodeToUnit and return symbol.ok
                        List<AUnitdeclGlobal> numerators = isEmptyNums1
                            ? isEmptyNums2 ? new List<AUnitdeclGlobal>() : sortedNums2
                            : sortedNums1;
                        List<AUnitdeclGlobal> denomerators = isEmptyDens1
                            ? isEmptyDens2 ? new List<AUnitdeclGlobal>() : sortedDens2
                            : sortedDens1;

                        (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unituse = (numerators, denomerators);
                        symbolTable.AddNodeToUnit(node, unituse);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Not the same unitTypes used in expression\n");

                    }
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unit;
                    if (leftContainsUnit ? symbolTable.GetUnit(leftExpr, out unit) : rightContainsUnit && symbolTable.GetUnit(rightExpr, out unit))
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Not the same unitTypes used in expression!\n");
                    }
                }
                else
                {
                    // not valid input expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Not the same Types used in expression!\n");
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
                // Implement logikken for custom units her
                bool leftContainsUnit = symbolTable.NodeToUnit.ContainsKey(leftExpr);
                bool rightContainsUnit = symbolTable.NodeToUnit.ContainsKey(rightExpr);
                if (symbolTable.NodeToUnit.ContainsKey(leftExpr) && symbolTable.NodeToUnit.ContainsKey(rightExpr))
                {
                    if (!(symbolTable.GetUnit(leftExpr, out (List<AUnitdeclGlobal> num, List<AUnitdeclGlobal> den) left) 
                          && symbolTable.GetUnit(rightExpr, out (List<AUnitdeclGlobal> num, List<AUnitdeclGlobal> den) right)))
                        return;

                    List<AUnitdeclGlobal> a = left.num;
                    List<AUnitdeclGlobal> b = left.den;
                    List<AUnitdeclGlobal> c = right.num;
                    List<AUnitdeclGlobal> d = right.den;

                    var sortedNums1 = a.OrderBy(x => x).ToList();
                    bool isEmptyNums1 = sortedNums1.Count == 0;
                    var sortedNums2 = c.OrderBy(x => x).ToList();
                    bool isEmptyNums2 = sortedNums2.Count == 0;
                    var sortedDens1 = b.OrderBy(x => x).ToList();
                    bool isEmptyDens1 = sortedDens1.Count == 0;
                    var sortedDens2 = d.OrderBy(x => x).ToList();
                    bool isEmptyDens2 = sortedDens2.Count == 0;

                    bool dontCompareNums = isEmptyNums1 || isEmptyNums2;
                    bool dontCompareDens = isEmptyDens1 || isEmptyDens2;

                    if ((dontCompareNums || sortedNums1.SequenceEqual(sortedNums2)) &&
                        (dontCompareDens || sortedDens1.SequenceEqual(sortedDens2)))
                    {
                        // Create a new unitTyple and add it to NodeToUnit and return symbol.ok
                        List<AUnitdeclGlobal> numerators = isEmptyNums1
                            ? isEmptyNums2 ? new List<AUnitdeclGlobal>() : sortedNums2
                            : sortedNums1;
                        List<AUnitdeclGlobal> denomerators = isEmptyDens1
                            ? isEmptyDens2 ? new List<AUnitdeclGlobal>() : sortedDens2
                            : sortedDens1;

                        (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unituse = (numerators, denomerators);
                        symbolTable.AddNodeToUnit(node, unituse);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Not the same unitTypes used in expression!\n");
                    }
                }
                else if ((leftContainsUnit || rightContainsUnit) && (symbolTable.GetSymbol(leftExpr) == Symbol.Int || symbolTable.GetSymbol(leftExpr) == Symbol.Decimal) 
                         || (symbolTable.GetSymbol(rightExpr) == Symbol.Int || symbolTable.GetSymbol(rightExpr) == Symbol.Decimal))
                {
                    // Unitnumber + decimal/int eller decimal/int + UnitNumber
                    (List<AUnitdeclGlobal>, List<AUnitdeclGlobal>) unit;
                    if (leftContainsUnit ? symbolTable.GetUnit(leftExpr, out unit) : rightContainsUnit && symbolTable.GetUnit(rightExpr, out unit))
                    {
                        symbolTable.AddNodeToUnit(node, unit);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                        tempResult += IndentedString("Not the same unitTypes used in expression!\n");
                    }
                }
                else
                {
                    // not valid input expressions to a multiply expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                    tempResult += IndentedString("Not the same Types used in expression!\n");
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

