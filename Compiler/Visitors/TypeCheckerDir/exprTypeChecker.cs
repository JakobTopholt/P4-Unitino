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

    public int bruh = 5;
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
                            matches = false;
                        break;
                    case ADecimalType:
                        if (paramSymbol != Symbol.Decimal)
                            matches = false;
                        break;
                    case ABoolType:
                        if (paramSymbol != Symbol.Bool)
                            matches = false;
                        break;
                    case ACharType:
                        if (paramSymbol != Symbol.Char)
                            matches = false;
                        break;
                    case AStringType:
                        if (paramSymbol != Symbol.String)
                            matches = false;
                        break;
                    case AUnitType argType:
                        if (symbolTable.GetUnit(argType, out var argUnit) && 
                            symbolTable.GetUnit(returnUnit, out var paramUnit) && 
                            !symbolTable.CompareCustomUnits(argUnit, paramUnit))
                        {
                            matches = false;
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
                var symbol = symbolTable.GetReturnFromNode(result);
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
            } 
        }
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
            default:
                if (symbolTable.GetUnit(node.GetId().ToString().Trim(), out var unit))
                {
                    symbolTable.AddNodeToUnit(node, ((List<AUnitdeclGlobal>, List<AUnitdeclGlobal>))unit);
                    symbolTable.AddNode(node, Symbol.Ok);
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                }

                break;
        }
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
        }
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
        }
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
            //throw new Exception(unitD);
            if (unitDecl is null || !symbolTable.GetUnit(unitDecl, out var tuple)) 
                return;
            symbolTable.AddNodeToUnit(node, tuple);
            symbolTable.AddNode(node, Symbol.Ok);
        }
        else
        {
          switch (leftSymbol)
        {
            case Symbol.Int when rightSymbol is Symbol.Int:
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

                    List<AUnitdeclGlobal> a = left.num;
                    List<AUnitdeclGlobal> b = left.den;
                    List<AUnitdeclGlobal> c = right.num;
                    List<AUnitdeclGlobal> d = right.den;

                    List<AUnitdeclGlobal> ac = a.Intersect(c).ToList();
                    List<AUnitdeclGlobal> bd = b.Intersect(d).ToList();

                    List<AUnitdeclGlobal> numerators = a.Except(ac).Union(d.Except(bd)).ToList();
                    List<AUnitdeclGlobal> denomerators = c.Except(ac).Union(b.Except(bd)).ToList();

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
                        symbolTable.AddNode(node, Symbol.NotOk);
                }
                else
                {
                    // not valid input expressions to a divide expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                }
                break;
        }  
        }
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
            case Symbol.Int when rightSymbol is Symbol.Int:
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

                    List<AUnitdeclGlobal> a = left.num;
                    List<AUnitdeclGlobal> b = left.den;
                    List<AUnitdeclGlobal> c = right.num;
                    List<AUnitdeclGlobal> d = right.den;

                    List<AUnitdeclGlobal> ad = a.Intersect(d).ToList();
                    List<AUnitdeclGlobal> bc = b.Intersect(c).ToList();

                    List<AUnitdeclGlobal> numerators = a.Except(ad).Union(d.Except(bc)).ToList();
                    List<AUnitdeclGlobal> denomerators = c.Except(ad).Union(b.Except(bc)).ToList();

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
                        symbolTable.AddNode(node, Symbol.NotOk);
                }
                else
                {
                    // not valid input expressions to a multiply expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                }
                break;
        }
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
            case Symbol.Int when rightSymbol is Symbol.Int:
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
                // Implement logikken for custom units her
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
                    }
                }
                else
                {
                    // not valid input expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                }
                break;
        }
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
            case Symbol.Int when rightSymbol is Symbol.Int:
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
                    }
                }
                else
                {
                    // not valid input expressions to a multiply expression
                    symbolTable.AddNode(node, Symbol.NotOk);
                }
                break;
        }
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
            case Symbol.Int:
                if (right == Symbol.Int)
                {
                    symbolTable.AddNode(node, Symbol.Int);
                } else if (right == Symbol.Decimal)
                {
                    symbolTable.AddNode(node, Symbol.Decimal);
                }
                else
                { 
                    symbolTable.AddNode(node, Symbol.NotOk);  
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
                }
                break;
            default:
                if (symbolTable.GetUnit(leftExpr, out var unit1) && symbolTable.GetUnit(rightExpr, out var unit2))
                {
                    if (symbolTable.CompareCustomUnits(unit1, unit2))
                    {
                        symbolTable.AddNodeToUnit(node, unit1);
                        symbolTable.AddNode(node, Symbol.Ok);
                    }
                    else
                    {
                        symbolTable.AddNode(node, Symbol.NotOk);
                    } 
                }
                else
                {
                    symbolTable.AddNode(node, Symbol.NotOk);
                }
                break;
        }
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
                    break;
                case Symbol.Decimal:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Decimal ? Symbol.Decimal : Symbol.NotOk);
                    break;
                case Symbol.Bool:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                    break;
                case Symbol.Char:
                    symbolTable.AddNode(node, falseSymbol == Symbol.Char ? Symbol.Char : Symbol.NotOk);
                    break;
                case Symbol.String:
                    symbolTable.AddNode(node, falseSymbol == Symbol.String ? Symbol.String : Symbol.NotOk);
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
                        if (symbolTable.CompareCustomUnits(trueUnit, falseUnit))
                        {
                            symbolTable.AddNodeToUnit(node, trueUnit);
                            symbolTable.AddNode(node, Symbol.Ok);
                        }
                        else
                        {
                            symbolTable.AddNode(node, Symbol.NotOk);
                        }
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
        }
    }
    
    public override void OutALogicalnotExp(ALogicalnotExp node)
    {
        Node exp = node.GetExp();
        if (exp is AIdExp id)
        {
            symbolTable.GetNodeFromId(id.GetId().ToString().Trim(), out exp);
        }
        if (exp != null)
        {
            symbolTable.AddNode(node,
                symbolTable.GetSymbol(exp) == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
        }
        else
        {
            symbolTable.AddNode(node, Symbol.NotOk);
        }
        
        
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
            /*-- casting to new types --*/
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
                break;
        }
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
        }
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
        }
    }
    public override void OutAEqualExp(AEqualExp node) => AddBinaryToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutANotequalExp(ANotequalExp node) => AddBinaryToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutAGreaterExp(AGreaterExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutALessExp(ALessExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutAGreaterequalExp(AGreaterequalExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
    public override void OutALessequalExp(ALessequalExp node) => AddBinaryNumberToSymbolTable(node, node.GetL(), node.GetR());
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
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                break;
        }
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
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                break;
        }
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
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                break;
        }
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
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                break;
        }
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
            case Symbol.Decimal:
                symbolTable.AddNode(node, Symbol.Decimal);
                break;
            case Symbol.Int:
                symbolTable.AddNode(node, Symbol.Int);
                break;
            case Symbol.Char:
                symbolTable.AddNode(node, Symbol.Char);
                break;
            default:
                symbolTable.AddNode(node, Symbol.NotOk);
                break;
        }
    }

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
                break;
            case (Symbol.Decimal):
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Decimal ? Symbol.Bool : Symbol.NotOk);
                break;
            case Symbol.Bool:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Bool ? Symbol.Bool : Symbol.NotOk);
                break;
            case Symbol.String:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.String ? Symbol.Bool : Symbol.NotOk);
                break;
            case Symbol.Char:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Int ? Symbol.Bool : Symbol.NotOk);
                break;
            default:
                symbolTable.GetUnit(leftExpr, out var unit1);
                symbolTable.GetUnit(rightExpr, out var unit2);

                symbolTable.AddNode(Parent, symbolTable.CompareCustomUnits(unit1, unit2) ? Symbol.Bool : Symbol.NotOk);
                break;
        }
    }
    // Binary without string, bool and unit
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
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Int ? Symbol.Bool : Symbol.NotOk);
                break;
            case (Symbol.Decimal):
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Decimal ? Symbol.Bool : Symbol.NotOk);
                break;
            case Symbol.Char:
                symbolTable.AddNode(Parent, symbolTable.GetSymbol(rightExpr) == Symbol.Int ? Symbol.Bool : Symbol.NotOk);
                break;
            default:
                symbolTable.AddNode(Parent, Symbol.NotOk);
                break;
        }
    }
}

